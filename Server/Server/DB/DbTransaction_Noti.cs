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
        public static void SavePlayerMap(Player player, MapInfo map)
        {
            if (player == null)
                return;

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    MapDb mapDb = db.Maps.FirstOrDefault(m => m.PlayerDbId == player.PlayerDbId);
                    if (mapDb == null)
                    {
                        mapDb = new MapDb()
                        {
                            TemplateId = map.TemplateId,
                            Scene = map.Scene,
                            PlayerDbId = player.PlayerDbId,
                            MapName = map.MapName
                        };
                        db.Maps.Add(mapDb);
                    }
                    else
                    {
                        mapDb.TemplateId = map.TemplateId;
                        mapDb.Scene = map.Scene;
                        mapDb.MapName = map.MapName;
                    }

                    bool success = db.SaveChangesEx(); // 저장할 때 예외처리를 해준다.
                    if (success)
                    {
                        // 성공적으로 저장된 경우 추가 작업을 수행할 수 있습니다.
                    }
                }
            });
        }

    }
}
