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
using System.Text;
using System.Threading.Tasks;

namespace Server.DB
{
    public partial class DbTransaction : JobSerializer
    {
        public static void EquipItemNoti(Player player, Item item)
        {
            if (player == null || item == null)
            {
                return;
            }

            int? slot = player.Inven.GetEmptySlot();
            if (slot == null)
                return;

            ItemDb itemDb = new ItemDb()
            {
                ItemDbId = item.ItemDbId,
                Equipped = item.Equipped
            };

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(itemDb).State = EntityState.Unchanged;
                    db.Entry(itemDb).Property(nameof(itemDb.Equipped)).IsModified = true;

                    bool success = db.SaveChangesEx();//저장할때 예외처리를 해준다.   
                    if (success)
                    {
                    }
                }
            });
        }
        public static void HpNoti(Player player, int hp)
        {
            if (player == null || hp == 0)
                return;

            PlayerDb playerDb = new PlayerDb()
            {
                PlayerDbId = player.PlayerDbId,
                Hp = hp
            };

            using (AppDbContext db = new AppDbContext())
            {
                db.Entry(playerDb).State = EntityState.Unchanged;
                db.Entry(playerDb).Property(nameof(playerDb.Hp)).IsModified = true;

                bool success = db.SaveChangesEx(); // 저장할 때 예외처리를 해준다.
                if (success)
                {                    
                }
            }
        }
        public static void SavePlayerMap(Player player, MapDb mapDb)
        {
            if (player == null)
                return;

            Instance.Push(async () =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    MapDb exMapDb = db.Maps.FirstOrDefault(m => m.PlayerDbId == player.PlayerDbId && m.TemplateId == mapDb.TemplateId);

                    if (exMapDb == null)
                    {
                        db.Maps.Add(mapDb);
                    }
                    else
                    {
                        exMapDb.PlayerDbId = mapDb.PlayerDbId;
                        exMapDb.TemplateId = mapDb.TemplateId;
                        exMapDb.Scene = mapDb.Scene;
                        exMapDb.MapName = mapDb.MapName;
                    }

                    bool success = db.SaveChangesEx(); // 저장할 때 예외처리를 해준다.
                    if (success)
                    {
                        exMapDb = db.Maps.FirstOrDefault(m => m.PlayerDbId == player.PlayerDbId && m.TemplateId == mapDb.TemplateId);
                        player.MapInfo.MapDbId = exMapDb.MapDbId;
                        player.MapInfo.TemplateId = exMapDb.TemplateId;                        
                        player.MapInfo.Scene = exMapDb.Scene;
                        player.MapInfo.MapName = exMapDb.MapName;

                        PlayerDb playerDb = new PlayerDb()
                        {
                            PlayerDbId = player.PlayerDbId,
                            MapDbId = player.MapInfo.MapDbId,
                            PosX = player.PosInfo.PosX,
                            PosY = player.PosInfo.PosY
                        };
                        await SavePlayerPosDb(player, playerDb);
                    }
                }
            });
        }
        public static void ExpNoti(Player player, int Exp)
        {
            if (player == null)
                return;

            PlayerDb playerDb = new PlayerDb()
            {
                PlayerDbId = player.PlayerDbId,
                Exp = player.Exp + Exp
            };

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(playerDb).State = EntityState.Unchanged;
                    db.Entry(playerDb).Property(nameof(playerDb.Exp)).IsModified = true;

                    bool success = db.SaveChangesEx(); // 저장할 때 예외처리를 해준다.
                    if (success)
                    {
                        player.Room.Push(() =>
                        {
                            player.Exp = playerDb.Exp;
                            S_ChangeExp expPacket = new S_ChangeExp();
                            expPacket.Exp = player.Exp;
                            player.Session.Send(expPacket);
                        });
                    }
                }            
            });
        }
    }
}
