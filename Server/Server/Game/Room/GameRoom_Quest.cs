using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using Server.Game.Job;
using Server.Game.Room;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Server.Game.Item;
using DbTransaction = Server.DB.DbTransaction;

namespace Server.Game
{
    public partial class GameRoom : JobSerializer
    {
        public void HandleStartQuest(Player player, int questId = 0)
        {
            if (player == null)
                return;

            S_StartQuest questPacket = new S_StartQuest();

            using (AppDbContext db = new AppDbContext())
            {
                // 주어진 questId가 0이 아니면 해당 퀘스트를 찾고, 0이면 가장 최근 퀘스트를 찾습니다.
                List<QuestDb> quests = db.Quests
                    .Where(q => q.OwnerDbId == player.PlayerDbId && (questId == 0 || q.TemplateId == questId))
                    .OrderByDescending(q => q.QuestDbId)
                    .ToList();
                
                QuestDb questDb = quests.FirstOrDefault();                

                if (questDb == null)
                {
                    if(questId == 0)
                    {
                        // 퀘스트가 없으면 새로 0번 퀘스트를 할당합니다.
                        QuestInfo questInfo = new QuestInfo()
                        {
                            TemplateId = 0,
                            Progress = 0,
                            Completed = false,
                            QuestType = "epic",
                        };
                        DbTransaction.SaveStartQuest(player, questInfo);
                        questPacket.Quest = questInfo;
                    }
                    else
                    {
                        QuestData questData = DataManager.QuestDict.GetValueOrDefault(questId);
                        if (questData == null)
                            return;
                        QuestInfo questInfo = new QuestInfo()
                        {
                            TemplateId = questData.id,
                            Progress = 0,
                            Completed = false,
                            QuestType = questData.questType,
                        };
                        DbTransaction.SaveStartQuest(player, questInfo);
                        questPacket.Quest = questInfo;
                    }
                }
                else
                {
                    if(questDb.Completed)
                        return;
                    
                    // 퀘스트가 있으면 해당 퀘스트를 할당합니다.
                    QuestInfo questInfo = new QuestInfo()
                    {
                        QuestDbId = questDb.QuestDbId,
                        TemplateId = questDb.TemplateId,
                        Progress = questDb.Progress,
                        Completed = questDb.Completed,
                        QuestType = DataManager.QuestDict[questDb.TemplateId].questType
                    };
                    if (player.Quest.TemplateId == questInfo.TemplateId)//메모리에 들고있으면 안함
                        return;
                    player.Quest = questInfo;
                    questPacket.Quest = questInfo;
                }
            }

            player.Session.Send(questPacket);
        }

        public void HandleQuestComplete(Player player, int questId)
        {
            if (player == null)
                return;

            player.HandleQuestComplete(questId);
        }

        public void HandleRequestShop(Player player)
        {
            if (player == null)
                return;

            MapInfo mapInfo = player.MapInfo;
            if (mapInfo == null)
                return;

            ShopData shopData = null;
            foreach (var shop in DataManager.ShopDict)
            {
                if (shop.Value.mapId == mapInfo.TemplateId)
                    shopData = shop.Value;
            }             
            if (shopData != null)
            {
                S_ShopList shopListPacket = new S_ShopList();
                foreach (ShopItemData item in shopData.itemList)
                {
                    ItemInfo itemInfo = new ItemInfo()
                    {
                        TemplateId = item.id,
                        Count = item.count,
                        Price = item.price
                    };
                    shopListPacket.Items.Add(itemInfo);
                }                
                player.Session.Send(shopListPacket);
            }                
        }
    }
}