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
            for (int slot = 0; slot<20; slot++)
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
                DbTransaction.SaveItemDB(player, itemDb, player.Room);
            }
            else if(remainingCount < 0)
            {
                DbTransaction.SaveRemovedItemDB(player, itemDb, player.Room);
            }
        }
    }
}
