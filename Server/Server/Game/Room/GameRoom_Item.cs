using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using Server.Game.Job;
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
            // 클라이언트에 아이템 구매 패킷 전송
            S_BuyItem buyItemPacket = new S_BuyItem()
            {
                TemplateId = itemData.id,
                Count = count,
                Gold = player.Gold,
                ShopId = buyPacket.ShopId,
            };
            player.Session.Send(buyItemPacket);

            ItemDb itemDb = new ItemDb()
            {
                TemplateId = itemData.id,
                Count = count, //패킷에 개수 추가 
                OwnerDbId = player.PlayerDbId,
                Slot = slot.Value,                
                Options = itemData.options                
            };

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
            DbTransaction.SaveItemDB(player, itemDb, this);
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
                ChestDbId = item.ChestId,
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
            
    }
}
