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
                    if(success)
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
            if(player==null || rewardData == null || room == null)
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
                        } );
                    }
                }
            });
        }
    }
}
