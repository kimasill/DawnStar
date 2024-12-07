using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using Server.Game.Job;
using Server.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using DbTransaction = Server.DB.DbTransaction;

namespace Server.Game
{
    public partial class GameRoom : JobSerializer
    {
        public void HandleEquipItem(Player player, C_EquipItem equipPacket)
        {
            if (player == null)
                return;

            player.HandleEquipItem(equipPacket);
        }

        public void HandleBuyItem(Player player, C_BuyItem buyPacket)
        {
            if (player == null)
                return;

            // 데이터 시트에서 아이템 확인
            ItemData itemData = null;
            DataManager.ItemDict.TryGetValue(buyPacket.TemplateId , out itemData);
            if (itemData == null)
                return;
            
            int count = buyPacket.Count;

            int? slot = player.Inven.GetSlot(buyPacket.TemplateId, count);
            if (slot == null)
                return;
            // 골드 확인 및 차감
            if (player.Gold < itemData.price)
            {
                count = 0;
                S_BuyItem refuseItemPacket = new S_BuyItem()
                {
                    TemplateId = itemData.id,
                    Count = count,
                };
                player.Session.Send(refuseItemPacket);
                return;
            }                

            player.Gold -= itemData.price;

            ShopDb shopDb = new ShopDb()
            {
                TemplateId = buyPacket.ShopId,
                PlayerDbId = player.PlayerDbId,
            };
            ShopItemDb shopItemDb = new ShopItemDb()
            {
                ItemId = itemData.id,                
                Count = count,
                Price = itemData.price
            };
            DbTransaction.RewardPlayer(player, itemData, count, this);
            DbTransaction.RemoveShopDb(player, shopDb, shopItemDb, this);
        }
        public void HandleLootItem(Player player, C_LootItem item)
        {
            if (player == null || item == null)
                return;

            //아이템 찾기
            //if (player.Room.Items.ContainsKey(item.ItemDbId) == false)
            //    return;

            ItemData itemData = null;
            DataManager.ItemDict.TryGetValue(item.TemplateId, out itemData);
            if (itemData == null)
                return;
            
            
            // 아이템 보상 처리
            DbTransaction.RewardPlayer(player, itemData, item.Count, this);
        }
        public void HandleOpenChest(Player player, C_OpenChest item)
        {
            if (player == null || item == null)
                return;
            ChestDb chestDb = new ChestDb
            {
                ChestId = item.ChestId,
                TemplateId = item.TemplateId,
                MapDbId = player.MapInfo.MapDbId,
                Opened = true,
                PosX = item.PosX,
                PosY = item.PosY
            };
            DbTransaction.SaveChestDb(player, chestDb, this);
        }
        public void HandleRemoveItem(Player player, C_RemoveItem removeItem)
        {
            if (player == null)
                return;

            if (player.Inven.Items.ContainsKey(removeItem.ItemDbId) == false)
                return;

            Item item = player.Inven.Items[removeItem.ItemDbId];
            if (removeItem.Count <= 0)
                removeItem.Count = 1;

            ItemDb itemDb = new ItemDb()
            {
                TemplateId = item.TemplateId,
                Count = removeItem.Count,
                OwnerDbId = player.PlayerDbId,
                Slot = item.Slot
            };            
            DbTransaction.SaveRemovedItemDB(player, itemDb, this);
            // 인벤토리에서 해당 아이템 제거                 
        }

        public void DropItem(Player player ,S_DropItem dropItem)
        {            
            player.Session.Send(dropItem);
            //룸에 처리
        }
        public void HandleRequestShop(Player player, int shopId)
        {
            if (player == null)
                return;

            DataManager.ShopDict.TryGetValue(shopId, out ShopData shopData);
            if (shopData == null)
                return;
            
            List<ShopItemDb> shopItemDbs = shopData.itemList.Select(x => new ShopItemDb()
            {
                ItemId = x.id,
                Count = x.count,
                Price = x.price,
            }).ToList();

            ShopDb shopDb = new ShopDb()
            {
                TemplateId = shopId,                
                PlayerDbId = player.PlayerDbId,
                ShopItems = shopItemDbs
            };

            DbTransaction.SaveShopDb(player, shopDb, this);
        }
        public void CheckBossRewards(Player player, int rewardId)
        {
            if (player == null)
                return;
            DataManager.AcquireDict.TryGetValue(rewardId, out AcquireData acquireData);
            if (acquireData == null) return;
            using (AppDbContext db = new AppDbContext())
            {
                ChestDb chestDb = db.Chests
                   .Where(s => s.TemplateId == rewardId && s.MapDbId == player.MapInfo.MapDbId)
                   .FirstOrDefault();
                if((chestDb != null && chestDb.Opened == false))
                {
                    S_MakeChest makeChest = new S_MakeChest();
                    makeChest.ChestId = chestDb.ChestId;
                    makeChest.TemplateId = chestDb.TemplateId;
                    player.Session.Send(makeChest);
                }
            }
        }

        public void HandleUseItem(Player player, C_UseItem useItemPacket)
        {
            if (player == null)
                return;

            Item item = player.Inven.Get(useItemPacket.ItemDbId);
            if (item == null)
                return;

            ItemData itemData = null;
            DataManager.ItemDict.TryGetValue(item.TemplateId, out itemData);
            if (itemData == null)
                return;

            // 아이템 타입에 따라 처리
            switch (item.ItemType)
            {
                case ItemType.Consumable:
                    UseConsumableItem(player, item, itemData);
                    break;
                case ItemType.Material:
                    // 재료 아이템은 사용 불가
                    break;
                case ItemType.Goods:
                    // 기타 아이템은 사용 불가
                    break;
                default:
                    break;
            }
        }

        private void UseConsumableItem(Player player, Item item, ItemData itemData)
        {
            foreach (var option in itemData.options)
            {
                if (option.Key == "Heal")
                {
                    int healAmount = int.Parse(option.Value);
                    player.Hp = player.Stat.Hp + healAmount;
                    S_ChangeHp changeHpPacket = new S_ChangeHp()
                    {
                        ObjectId = player.Id,
                        Hp = player.Hp
                    };
                    Broadcast(player.CellPos, changeHpPacket);
                    player.Inven.Remove(item.ItemDbId, 1);
                    ItemDb itemDb = new ItemDb()
                    {
                        TemplateId = item.TemplateId,
                        Count = 1,
                        OwnerDbId = player.PlayerDbId,
                        Slot = item.Slot
                    };
                    DbTransaction.SaveRemovedItemDB(player, itemDb, this);
                }
            }
        }

        public void HandleEnhanceItem(Player player, C_Enhance enhancePacket)
        {
            if (player == null)
                return;

            Item item = player.Inven.Get(enhancePacket.ItemDbId);
            if (item == null)
                return;

            ItemData itemData = null;
            DataManager.ItemDict.TryGetValue(item.TemplateId, out itemData);
            if (itemData == null)
                return;
            
            EnhanceData enhanceData = DataManager.EnhanceDict.Select(x => x.Value).Where(x => x.rank == item.Rank && x.itemType == item.ItemType).FirstOrDefault();
            List<Item> costItems = new List<Item>();
            foreach (CostData cost in enhanceData.costData)
            {
                Item costItem = player.Inven.FindByTemplateId(cost.templateId);
                if (costItem.Count < cost.count || costItem == null)
                    return;

                costItems.Add(costItem);
            }

            foreach (Item costItem in costItems)
            {                
                ItemDb itemDb = new ItemDb()
                {
                    TemplateId = costItem.TemplateId,
                    Count = costItem.Count,
                    OwnerDbId = player.PlayerDbId,
                    Slot = costItem.Slot
                };
                DbTransaction.SaveRemovedItemDB(player, itemDb, this);
            }

            //강화실행
            bool success = true;
            ItemDb newItemDb = ItemLogic.EnhanceItem(player, item, enhanceData);
            if (newItemDb == null)
                success = false;            
            
            if (success == false)
            {
                S_Enhance enhanceResultPacket = new S_Enhance()
                {
                    ItemInfo = item.Info,
                    Success = success
                };
                player.Session.Send(enhanceResultPacket);
            }
            else
            {
                DbTransaction.SaveEnhancedItemDB(player, newItemDb, this); // DB Save -> Client Notify
            }
        }
    }
}
