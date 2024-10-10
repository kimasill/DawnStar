using Google.Protobuf;
using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Game;
using Server.Game.Job;
using Server.Game.Room;
using Server.Migrations;
using Server.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Server.DB
{
    public partial class DbTransaction : JobSerializer
    {
        public static DbTransaction Instance { get; } = new DbTransaction();

        public static void SavePlayerStatus_All(Player player, GameRoom room)
        {
            if (player == null || room == null)
                return;
            //GameRoom
            PlayerDb playerDb = new PlayerDb()
            {
                PlayerDbId = player.PlayerDbId,
                Hp = player.Stat.Hp,
                MaxHp = player.Stat.MaxHp,
                Level = player.Level,
                Attack = player.Stat.Attack,                
                Defense = player.Stat.Defense,
                Exp = player.Exp,
                PosX = player.CellPos.x,
                PosY = player.CellPos.y,
            };

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    PlayerDb existingPlayerDb = db.Players.FirstOrDefault(p => p.PlayerDbId == playerDb.PlayerDbId);
                    if (existingPlayerDb != null)
                    {                        
                        existingPlayerDb.Hp = playerDb.Hp;
                        existingPlayerDb.Level = playerDb.Level;
                        existingPlayerDb.Exp = playerDb.Exp;
                        existingPlayerDb.PosX = playerDb.PosX;
                        existingPlayerDb.PosY = playerDb.PosY;
                    }
                    else
                    {
                        db.Players.Add(playerDb);
                    }
                    db.SaveChangesEx();
                }
            });
        }


        public static void SavePlayerStatus_Step1(Player player, GameRoom room)
        {
            if (player == null || room == null)
                return;
            //GameRoom
            PlayerDb playerDb = new PlayerDb()
            {
                PlayerDbId = player.PlayerDbId,
                Hp = player.Stat.Hp,
                Level = player.Level,
                Exp = player.Exp,
                PosX = player.CellPos.x,
                PosY = player.CellPos.y
            };
            
            Instance.Push<PlayerDb, GameRoom>(SavePlayerStatus_Step2, playerDb, room);
        }

        public static void SavePlayerStatus_Step2(PlayerDb playerDb, GameRoom room)
        {
            using (AppDbContext db = new AppDbContext())
            {
                db.Entry(playerDb).State = EntityState.Modified;
                db.Entry(playerDb).Property(nameof(playerDb.Hp)).IsModified = true;
                bool success = db.SaveChangesEx();//저장할때 예외처리를 해준다.   
                if (success)
                {
                    room.Push(SavePlayerStatus_Step3, playerDb.Hp);
                }
            }
        }

        public static void SavePlayerStatus_Step3(int hp)
        {
            //Console.WriteLine($"save hp{hp}");
        }

        public static void RewardPlayer(Player player, ItemRewardData rewardData, int count, GameRoom room)
        {
            if (player == null || rewardData == null || room == null)
            {
                return;
            }
            int? slot = player.Inven.GetSlot(rewardData.itemId, count);            
            if (slot == null)
                return;

            ItemDb itemDb = new ItemDb()
            {
                TemplateId = rewardData.itemId,
                Count = count,
                OwnerDbId = player.PlayerDbId,
                Slot = slot.Value
            };
            SaveItemDB(player, itemDb, room);
        }

        public static void SaveItemDB(Player player, ItemDb itemDb, GameRoom room)
        {
            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    ItemDb existingItemDb = db.Items
                        .FirstOrDefault(i => i.OwnerDbId == itemDb.OwnerDbId && i.Slot == itemDb.Slot);
                    if (existingItemDb != null)
                    {
                        existingItemDb.Count += itemDb.Count;
                    }
                    else
                    {
                        db.Items.Add(itemDb);
                    }                    
                    bool success = db.SaveChangesEx();//저장할때 예외처리를 해준다.   
                    
                    if (success)
                    {                        
                        itemDb.ItemDbId = db.Items.FirstOrDefault(i => i.OwnerDbId == itemDb.OwnerDbId && i.Slot == itemDb.Slot).ItemDbId;
                        room.Push(() =>
                        {
                            Item newItem = Item.MakeItem(itemDb);
                            player.Inven.Add(newItem);

                            //client noti
                            {
                                S_AddItem itemPacket = new S_AddItem();
                                ItemInfo info = new ItemInfo();
                                info.MergeFrom(newItem.Info);
                                itemPacket.Items.Add(info);

                                player.Session.Send(itemPacket);
                            }
                        });
                    }
                }
            });
        }
        public static void SaveRemovedItemDB(Player player, ItemDb itemDb, GameRoom room)
        {
            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    int remainingCount = itemDb.Count;
                    while (remainingCount > 0)
                    {
                        ItemDb tItemDb = db.Items
                            .Where(i => i.ItemDbId == itemDb.ItemDbId && i.OwnerDbId == player.PlayerDbId)
                            .FirstOrDefault();

                        if (itemDb == null)
                        {
                            tItemDb = db.Items
                                .Where(i => i.TemplateId == itemDb.TemplateId && i.OwnerDbId == player.PlayerDbId)
                                .FirstOrDefault();
                            break;
                        }

                        if (tItemDb.Count > remainingCount)
                        {
                            // Count를 감소시킨다.
                            tItemDb.Count -= remainingCount;
                            remainingCount = 0;
                        }
                        else
                        {
                            // Count가 0 이하가 되면 아이템을 제거한다.
                            remainingCount -= tItemDb.Count;
                            db.Items.Remove(tItemDb);
                        }

                        bool success = db.SaveChangesEx(); // 저장할 때 예외 처리를 해준다.
                        if (success)
                        {
                            room.Push(() =>
                            {
                                Item iItem = player.Inven.Find(i => i.ItemDbId == tItemDb.ItemDbId);
                                if (iItem != null)
                                {
                                    if (iItem.Count > remainingCount)
                                    {
                                        int rCount = iItem.Count - remainingCount;
                                        player.Inven.UpdateItem(iItem.ItemDbId, rCount);
                                    }
                                    else
                                    {
                                        player.Inven.Remove(iItem.ItemDbId);
                                    }
                                }
                                Item rItem = Item.MakeItem(tItemDb);
                                //client noti
                                {
                                    S_RemoveItem itemPacket = new S_RemoveItem();
                                    ItemInfo info = new ItemInfo();
                                    info.MergeFrom(rItem.Info);
                                    itemPacket.Items.Add(info);
                                    player.Session.Send(itemPacket);
                                }
                            });
                        }
                    }
                }
            });
        }
            
        public static void SaveCompleteQuest(Player player, QuestDb questDb, GameRoom room)
        {
            if (player == null || questDb == null)
                return;

            // 퀘스트 데이터 읽기
            Data.QuestData questData = DataManager.QuestDict.GetValueOrDefault(questDb.TemplateId);
            if (questData == null)
                return;

            // 연계 정보 가져오기
            int exp = questData.rewards.FirstOrDefault(r => r.type == RewardType.RewardExp)?.amount ?? 0;
            int connection = questData.connection;
            int gold = questData.rewards.FirstOrDefault(r => r.type == RewardType.RewardGold)?.amount ?? 0;

            // 퀘스트 완료 처리
            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    QuestDb existingQuestDb = db.Quests
                        .Where(q => q.QuestDbId == questDb.QuestDbId)
                        .FirstOrDefault();

                    if (existingQuestDb == null || existingQuestDb.Completed)
                        return;

                    existingQuestDb.Completed = true;

                    bool success = db.SaveChangesEx(); // 저장할 때 예외 처리를 해준다.
                    if (success)
                    {
                        player.Gold += gold;
                        player.Exp += exp;                            
                        RewardInfo rewardInfo = new RewardInfo();
                        foreach(RewardData rewardData in questData.rewards)
                        {
                            rewardInfo.RewardType = rewardData.type;
                            rewardInfo.RewardValue = rewardData.amount;
                        }
                        QuestInfo questInfo = new QuestInfo()
                        {
                            QuestDbId = questDb.QuestDbId,
                            TemplateId = questDb.TemplateId,
                            Connection = connection,
                            Rewards = rewardInfo,
                        };
                        S_QuestComplete questCompletePacket = new S_QuestComplete();
                        questCompletePacket.Quest = questInfo;
                        player.Session.Send(questCompletePacket);

                        SavePlayerStatus_All(player, player.Room);
                    }                    
                }
            });
        }

        public static void SaveStartQuest(Player player, QuestDb questDb, GameRoom room)
        {
            if (player == null || questDb == null)
                return;

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    QuestDb existingQuestDb = db.Quests.FirstOrDefault(q => q.TemplateId == questDb.TemplateId && q.OwnerDbId == player.PlayerDbId);
                    if (existingQuestDb != null)
                    {
                        existingQuestDb.Progress = questDb.Progress;
                    }
                    else
                    {
                        db.Quests.Add(questDb);
                    }                    
                    bool success = db.SaveChangesEx();//저장할때 예외처리를 해준다.   
                    if (success)
                    {
                        room.Push(() =>
                        {
                            Quest newQuest = Quest.MakeQuest(questDb);
                            player.Quest.Add(newQuest);
                            player.Quest.CurrentQuest = newQuest;                            

                            //client noti
                            {
                                S_StartQuest questPacket = new S_StartQuest();
                                QuestInfo info = new QuestInfo();
                                info.MergeFrom(newQuest.Info);
                                questPacket.Quest = info;

                                player.Session.Send(questPacket);
                            }
                        });
                    }       
                }
            });
        }

        public static void SaveQuestDB(Player player, QuestDb questDb, GameRoom room)
        {
            if (player == null || questDb == null)
                return;

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    QuestDb existingQuestDb = db.Quests.FirstOrDefault(q => q.TemplateId == questDb.TemplateId && q.OwnerDbId == player.PlayerDbId);
                    if (existingQuestDb != null)
                    {
                        existingQuestDb = questDb;
                    }
                    else
                    {
                        db.Quests.Add(questDb);
                    }
                    bool success = db.SaveChangesEx();//저장할때 예외처리를 해준다.   
                    if (success)
                    {
                        room.Push(() =>
                        {
                            Quest newQuest = Quest.MakeQuest(questDb);
                            newQuest.Progress = questDb.Progress;
                            player.Quest.Add(newQuest);
                            //client noti
                            {
                                S_QuestList questPacket = new S_QuestList();
                                QuestInfo info = new QuestInfo();
                                info.MergeFrom(newQuest.Info);
                                questPacket.Quests.Add(info);

                                player.Session.Send(questPacket);
                            }
                        });
                    }
                }
            });
        }
    }
}