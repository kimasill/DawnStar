using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using Server.Game.Job;
using Server.Game.Room;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using static Server.Game.Item;
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

            //TODO: 아이템 구매 개수 확인. 현재는 상점에 있는거 다삼
            int count = 1;

            int? slot = player.Inven.GetEmptySlot();
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

            // 아이템 생성 및 데이터베이스에 추가
            ItemDb itemDb = new ItemDb()
            {
                TemplateId = itemData.id,
                Count = count, //패킷에 개수 추가 
                OwnerDbId = player.PlayerDbId,
                Slot = slot.Value
            };
            DbTransaction.SaveItemDB(player, itemDb, this);

            // 클라이언트에 아이템 구매 패킷 전송
            S_BuyItem buyItemPacket = new S_BuyItem()
            {
                TemplateId = itemData.id,
                Count = count,
                Gold = player.Gold
            };
            player.Session.Send(buyItemPacket);
        }
        public void HandleLootItem(Player player, C_LootItem item)
        {
            if (player == null || item == null)
                return;

            //아이템 찾기
            //if (player.Room.Items.ContainsKey(item.ItemDbId) == false)
            //    return;

            ItemRewardData rewardData = new ItemRewardData();
            rewardData.itemId = item.TemplateId;
            rewardData.count = item.Count;           

            // 아이템 보상 처리
            DbTransaction.RewardPlayer(player, rewardData, this);
        }

        public void HandleRemoveItem(Player player, int id)
        {
            if (player == null)
                return;

            if (player.Inven.Items.ContainsKey(id) == false)
                return;

            DbTransaction.SaveRemovedItemDB(player, id, this);
            // 인벤토리에서 해당 아이템 제거                 
        }
    }
}
