using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Server.Data;
using Server.DB;
using Server.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class Inventory
    {
        public Dictionary<int, Item> Items { get; } = new Dictionary<int, Item>();

        public void Add(Item item)
        {
            Item existingItem = Get(item.ItemDbId);
            if (existingItem != null)
            {
                existingItem.Count += item.Count;
            }
            else
            {
                Items.Add(item.ItemDbId, item);
            }
        }

        public void Remove(int itemId)
        {
            Items.Remove(itemId);
        }

        public void Remove(int itemId, int count)
        {
            Item item = Get(itemId);
            if (item != null)
            {
                item.Count -= count;
                if (item.Count <= 0)
                {
                    Remove(itemId);
                }
            }
        }

        public void UpdateItem(int itemId, int count)
        {
            Item item = Get(itemId);
            if (item != null)
            {
                if(item.Count > 0)
                {
                    item.Count = count;
                }                    
                else if (item.Count <= 0)
                {
                    Remove(itemId);
                    return;
                }
            }
        }

        public void UpdateItemInfo(Item item)
        {
            Item existingItem = Get(item.ItemDbId);
            if (existingItem != null)
            {
                Remove(existingItem.ItemDbId);
                Add(item);
            }
        }

        public Item Get(int itemId)
        {
            Item item = null;
            Items.TryGetValue(itemId, out item);
            return item;
        }

        public Item Find(Func<Item, bool> condition)
        {
            foreach(var item in Items.Values)
            {
                if (condition.Invoke(item))
                    return item;
            }
            return null;
        }
        public Item FindByTemplateId(int templateId)
        {
            Item item = null;
            item = Items.Values.FirstOrDefault(item => item.TemplateId == templateId);
            return item;
        }

        public int? GetSlot(int itemId, int count)
        {
            var existingItem = Find(i => i.TemplateId == itemId && (i.ItemType == ItemType.Goods || i.ItemType == ItemType.Material|| i.ItemType == ItemType.Consumable));
            if (existingItem != null)
            {
                if (existingItem.Stackable)
                {
                    if (existingItem.ItemType == ItemType.Goods)
                    {
                        GoodsData goodsData = DataManager.ItemDict[itemId] as GoodsData;
                        if (goodsData == null)
                            return null;
                        if (existingItem.Count + count > goodsData.maxCount)
                        {
                            return GetEmptySlot();
                        }
                        else
                        {
                            return existingItem.Slot;
                        }
                    }
                    else if (existingItem.ItemType == ItemType.Material)
                    {
                        MaterialData materialData = DataManager.ItemDict[itemId] as MaterialData;
                        if (materialData == null)
                            return null;
                        if (existingItem.Count + count > materialData.maxCount)
                        {
                            return GetEmptySlot();
                        }
                        else
                        {
                            return existingItem.Slot;
                        }
                    }
                    else if(existingItem.ItemType == ItemType.Consumable)
                    {
                        ConsumableData consumableData = DataManager.ItemDict[itemId] as ConsumableData;
                        if (consumableData == null)
                            return null;
                        if (existingItem.Count + count > consumableData.maxCount)
                        {
                            return GetEmptySlot();
                        }
                        else
                        {
                            return existingItem.Slot;
                        }
                    }
                }
            }
            return GetEmptySlot();
        }
        public int? GetItemCount(int itemId)
        {
            Item item = Find(i => i.TemplateId == itemId);
            if (item != null)
                return item.Count;
            return null;
        }
        public int? GetEmptySlot()
        {
            for (int slot = 0; slot<100; slot++)
            {
                Item item = Items.Values.FirstOrDefault(i=> i.Slot == slot);
                if (item == null)
                    return slot;
            }
            return null;
        }
        public int GetInvenProperty(int templateId)
        {
            int total = 0;

            foreach (Item item in Items.Values)
            {
                if (item.TemplateId == templateId)
                {
                    total += item.Count;
                }
            }
            return total;
        }

        public void SetInvenProperty(int count, int templateId, Player player)
        {
            GoodsData goodsData = DataManager.ItemDict[templateId] as GoodsData;
            if(goodsData == null)
                return;

            int? slot = GetSlot(templateId, count);
            if(slot == null)
                return;

            int remainingCount = count - GetInvenProperty(templateId);
            int amount = (int)MathF.Abs(remainingCount);

            ItemDb itemDb = new ItemDb()
            {
                TemplateId = templateId,
                Count = amount,
                OwnerDbId = player.PlayerDbId,
                Slot = slot.Value
            };
            if (remainingCount > 0)
            {
                DbTransaction.SaveAddItemDB(player, itemDb, player.Room);
            }
            else if(remainingCount < 0)
            {
                DbTransaction.SaveRemovedItemDB(player, itemDb, player.Room);
            }
        }
        public void SortInven(Player player)
        {
            ConsolidateItems(player);
            SortItemSlots();
            
            List<ItemDb> items = new List<ItemDb>();
            foreach (var item in Items.Values)
            {
                ItemDb itemDb = new ItemDb()
                {
                    ItemDbId = item.ItemDbId,
                    TemplateId = item.TemplateId,
                    Count = item.Count,
                    Slot = item.Slot
                };
                items.Add(itemDb);
            }
            DbTransaction.SaveItemSlotAndCount(player, items, player.Room);
        }
            
        private void SortItemSlots()
        {
            // 아이템을 TemplateId 기준으로 정렬
            var sortedItems = Items.Values.OrderBy(item => item.TemplateId).ToList();

            // 정렬된 아이템을 새로운 슬롯에 배치
            Items.Clear();
            int slot = 0;
            foreach (var item in sortedItems)
            {
                item.Slot = slot++;
                Items.Add(item.ItemDbId, item);
            }
        }
        private void ConsolidateItems(Player player)
        {
            var groupedItems = Items.Values
                .Where(item => item.Stackable)
                .GroupBy(item => item.TemplateId)
                .ToList();

            foreach (var group in groupedItems)
            {
                var items = group.ToList();
                if (items.Count <= 1)
                    continue;

                var firstItem = items[0];
                for (int i = 1; i < items.Count; i++)
                {
                    var currentItem = items[i];
                    int maxCount = GetMaxCount(currentItem);

                    if (firstItem.Count < maxCount)
                    {
                        int transferAmount = Math.Min(currentItem.Count, maxCount - firstItem.Count);
                        firstItem.Count += transferAmount;
                        currentItem.Count -= transferAmount;

                        if (currentItem.Count <= 0)
                        {
                            ItemDb itemDb = new ItemDb()
                            {
                                ItemDbId = currentItem.ItemDbId,
                                TemplateId = currentItem.TemplateId,
                                Count = currentItem.Count,
                                Slot = currentItem.Slot
                            };
                            DbTransaction.SaveRemoveItemStateDb(player, itemDb, player.Room);
                        }
                    }
                }
            }
        }

        private int GetMaxCount(Item item)
        {
            switch (item.ItemType)
            {
                case ItemType.Goods:
                    return (DataManager.ItemDict[item.TemplateId] as GoodsData)?.maxCount ?? int.MaxValue;
                case ItemType.Material:
                    return (DataManager.ItemDict[item.TemplateId] as MaterialData)?.maxCount ?? int.MaxValue;
                case ItemType.Consumable:
                    return (DataManager.ItemDict[item.TemplateId] as ConsumableData)?.maxCount ?? int.MaxValue;
                default:
                    return int.MaxValue;
            }
        }
    }
}
