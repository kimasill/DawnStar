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
                Level = player.Level,
                Exp = player.Exp,
                PosX = player.CellPos.x,
                PosY = player.CellPos.y,
                Gold = player.Gold
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
                        existingPlayerDb.Gold = playerDb.Gold;
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
                PosY = player.CellPos.y,
                Gold = player.Gold
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

        public static void RewardPlayer(Player player, ItemRewardData rewardData, GameRoom room)
        {
            if (player == null || rewardData == null || room == null)
            {
                return;
            }

            int? slot = player.Inven.GetEmptySlot();
            if (slot == null)
                return;

            ItemDb itemDb = new ItemDb()
            {
                TemplateId = rewardData.itemId,
                Count = rewardData.count,
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
                    db.Items.Add(itemDb);
                    bool success = db.SaveChangesEx();//저장할때 예외처리를 해준다.   
                    if (success)
                    {
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
        public static void SaveRemovedItemDB(Player player, int id, GameRoom room)
        {
            //Info 일치하는 Item Db에서 제거
            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    ItemDb itemDb = db.Items
                        .Where(i=> i.ItemDbId == id)                        
                        .FirstOrDefault();

                    if (itemDb != null)
                    {
                        db.Items.Remove(itemDb);
                        bool success = db.SaveChangesEx();//저장할때 예외처리를 해준다.
                        if(success) {
                            room.Push(() =>
                            {
                                Item removedItem = player.Inven.Find(i => i.ItemDbId == id);
                                if (removedItem != null)
                                {
                                    player.Inven.Remove(removedItem.ItemDbId);
                                }
                            });
                        }
                    }
                }
            });
        }
            
        public static void SaveCompleteQuest(Player player, QuestInfo questInfo)
        {
            if (player == null || questInfo == null)
                return;

            // 퀘스트 데이터 읽기
            Data.QuestData questData = DataManager.QuestDict.GetValueOrDefault(questInfo.TemplateId);
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
                    QuestDb questDb = db.Quests
                        .Where(q => q.QuestDbId == questInfo.QuestDbId)
                        .FirstOrDefault();

                    if (questDb != null)
                    {
                        questDb.Completed = true;
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
                }
            });
        }

        public static void SaveStartQuest(Player player, QuestInfo questInfo)
        {
            if (player == null || questInfo == null)
                return;

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    QuestDb questDb = new QuestDb
                    {
                        OwnerDbId = player.PlayerDbId,
                        TemplateId = questInfo.TemplateId,
                        Progress = questInfo.Progress,
                        Completed = questInfo.Completed
                    };

                    db.Quests.Add(questDb);
                    db.SaveChanges();

                    // 퀘스트 정보 업데이트
                    questInfo.QuestDbId = questDb.QuestDbId;
                }
            });
        }
    }
}