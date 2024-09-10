using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Game;
using Server.Game.Job;
using Server.Game.Room;
using Server.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
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
            PlayerDb playerDb = new PlayerDb();
            playerDb.PlayerDbId = player.PlayerDbId;
            playerDb.Hp = player.Stat.Hp;

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(playerDb).State = EntityState.Unchanged;
                    db.Entry(playerDb).Property(nameof(playerDb.Hp)).IsModified = true;
                    bool success = db.SaveChangesEx();//저장할때 예외처리를 해준다.   
                    if (success)
                    {
                        //room.Push(() => Console.WriteLine($"Hp Saved{playerDb.Hp}"));
                    }
                }
            });


        }

        public static void SavePlayerStatus_Step1(Player player, GameRoom room)
        {
            if (player == null || room == null)
                return;
            //GameRoom
            PlayerDb playerDb = new PlayerDb();
            playerDb.PlayerDbId = player.PlayerDbId;
            playerDb.Hp = player.Stat.Hp;
            playerDb.Level = player.Level;
            playerDb.Exp = player.Exp;
            playerDb.PosX = player.CellPos.x;
            playerDb.PosY = player.CellPos.y;
            Instance.Push<PlayerDb, GameRoom>(SavePlayerStatus_Step2, playerDb, room);
        }

        public static void SavePlayerStatus_Step2(PlayerDb playerDb, GameRoom room)
        {
            using (AppDbContext db = new AppDbContext())
            {
                db.Entry(playerDb).State = EntityState.Unchanged;
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

        public static void RewardPlayer(Player player, RewardData rewardData, GameRoom room)
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
        public static void SaveCompleteQuest(Player player, QuestInfo questInfo)
        {
            if (player == null || questInfo == null)
                return;

            // 퀘스트 데이터 읽기
            Data.QuestData questData = DataManager.QuestDict.GetValueOrDefault(questInfo.TemplateId);
            if (questData == null)
                return;

            // 경험치와 연계 정보 가져오기
            int exp = questData.exp;
            int connection = questData.connection;

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
                            player.HandleLevel(exp);
                            SavePlayerStatus_Step1(player, player.Room);
                            QuestInfo questInfo = new QuestInfo()
                            {
                                QuestDbId = questDb.QuestDbId,
                                TemplateId = questDb.TemplateId,
                                Exp = exp,
                                Connection = connection
                            };
                        S_QuestComplete questCompletePacket = new S_QuestComplete();
                        questCompletePacket.Quest = questInfo;
                        player.Session.Send(questCompletePacket);                        
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