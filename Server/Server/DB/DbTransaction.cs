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
using System.Collections;
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
                MaxPotion = player.MaxPotion,
                StatPoint = player.StatPoint,
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

        public static void SavePlayerStatDb(Player player, PlayerDb playerDb, GameRoom room)
        {
            if (playerDb == null)
                return;

            using (AppDbContext db = new AppDbContext())
            {
                db.Entry(playerDb).State = EntityState.Unchanged;
                
                var properties = typeof(PlayerDb).GetProperties();
                foreach (var property in properties)
                {
                    if(property.Name == "PlayerDbId")
                        continue;
                    var value = property.GetValue(playerDb);
                    double doubleValue;
                    if (value != null)
                    {
                        if (property.Name == "Realizations")
                        {
                            // Realizations 속성 처리
                            if (playerDb.Realizations != null && playerDb.Realizations.Count > 0)
                            {
                                db.Entry(playerDb).Property(nameof(playerDb.RealizationsJson)).IsModified = true;
                            }
                        }
                        else if (value is IList list && list.Count > 0)
                        {
                            // 일반 리스트 타입이고 요소가 있는 경우
                            db.Entry(playerDb).Property(property.Name).IsModified = true;
                        }
                        else if (property.Name == "StatPoint")
                        {
                            if (playerDb.StatPoint == player.StatPoint)
                            {
                                db.Entry(playerDb).Property(property.Name).IsModified = true;
                            }
                        }
                        else if (double.TryParse(value.ToString(), out doubleValue) && doubleValue != 0 && property.Name != "PosX" && property.Name != "PosY")
                        {
                            // 리스트 타입이 아니고, double로 변환 가능한 경우
                            db.Entry(playerDb).Property(property.Name).IsModified = true;
                        }
                    }
                }
                bool success = db.SaveChangesEx();
                if (success)
                {
                    room.Push(() =>
                    {
                        {
                            S_ChangeStat statPacket = new S_ChangeStat();
                            StatInfo statInfo = new StatInfo();
                            statInfo.MergeFrom(player.Stat);
                            statPacket.StatInfo = statInfo;                            
                            player.Session.Send(statPacket);
                        }                        
                        {
                            S_ChangeHp hpPacket = new S_ChangeHp()
                            {
                                ObjectId = player.Id,
                                Hp = player.Hp,
                            };
                            room.Broadcast(player.CellPos, hpPacket, player);
                        }
                    });
                }
            }            
        }
        public static void SavePlayerPosDb(Player player, PlayerDb playerDb, GameRoom room)
        {
            if (playerDb == null)
                return;

            using (AppDbContext db = new AppDbContext())
            {
                db.Entry(playerDb).State = EntityState.Unchanged;

                var properties = typeof(PlayerDb).GetProperties();
                foreach (var property in properties)
                {
                    if (property.Name == "PlayerDbId")
                        continue;
                    var value = property.GetValue(playerDb);
                    if(property.Name != "PosX" && property.Name != "PosY")
                    {
                        continue;
                    }
                    if (value != null)
                    {
                        db.Entry(playerDb).Property(property.Name).IsModified = true;
                    }
                }
                bool success = db.SaveChangesEx();
                if (success)
                {                    
                }
            }
        }
        public static void RewardPlayer(Player player, ItemData itemData, int count, GameRoom room)
        {
            if (player == null || itemData == null || room == null)
            {
                return;
            }
            int? slot = player.Inven.GetSlot(itemData.id, count);            
            if (slot == null)
                return;
            if(itemData.itemType == ItemType.Consumable)
            {
                if((itemData as ConsumableData).consumableType == ConsumableType.Potion && player.Inven.GetItemCount(itemData.id) + count > player.MaxPotion)
                {
                    //포션 최대 개수 초과 로직
                    return;
                }
            }
            ItemDb itemDb = new ItemDb()
            {
                TemplateId = itemData.id,
                Count = count,
                OwnerDbId = player.PlayerDbId,
                Slot = slot.Value,
                Options = itemData.options
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

                        if (tItemDb == null)
                        {
                            tItemDb = db.Items
                                .Where(i => i.TemplateId == itemDb.TemplateId && i.OwnerDbId == player.PlayerDbId)
                                .FirstOrDefault();
                        }
                        if (tItemDb != null)
                        {
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
                                tItemDb.Count = 0;
                                db.Items.Remove(tItemDb);
                            }
                        }

                        bool success = db.SaveChangesEx(); // 저장할 때 예외 처리를 해준다.
                        if (success)
                        {
                            room.Push(() =>
                            {
                                Item iItem = player.Inven.Find(i => i.ItemDbId == tItemDb.ItemDbId);
                                if (iItem != null)
                                {
                                    iItem.Count = tItemDb.Count;                              
                                    player.Inven.UpdateItem(iItem.ItemDbId, iItem.Count);
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

                    existingQuestDb.Completed = questDb.Completed;
                    existingQuestDb.Progress = questDb.Progress;

                    bool success = db.SaveChangesEx(); // 저장할 때 예외 처리를 해준다.
                    if (success)
                    {
                        room.Push(() =>
                        {
                            player.Gold += gold;
                            ExpNoti(player, exp);
                            RewardInfo rewardInfo = new RewardInfo();
                            foreach (RewardData rewardData in questData.rewards)
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
                        });
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

        public static void SaveChestDb(Player player, ChestDb chestDb, GameRoom room)
        {
            if (player == null || chestDb == null)
                return;
            
            bool reward = false;

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    ChestDb chest = db.Chests
                        .Where(c => c.MapDbId == player.MapInfo.MapDbId && c.ChestId == chestDb.ChestId)
                        .FirstOrDefault();
                    if (chest != null )
                    {
                        if(chest.Opened == true)
                        {
                            reward = false;
                        }
                        else
                        {
                            reward = true;
                        }
                        chest.Opened = chestDb.Opened;
                    }
                    bool success = db.SaveChangesEx();//저장할때 예외처리를 해준다.   
                    if (success && reward)
                    {
                        AcquireData acquireData = DataManager.AcquireDict.GetValueOrDefault(chestDb.TemplateId);
                        if (acquireData == null)
                            return;

                        room.Push(() =>
                        {                            
                            {
                                List<ItemRewardData> datas = ItemLogic.GetRandomReward(acquireData.rewards);
                                foreach (ItemRewardData rewardData in datas)
                                {
                                    S_DropItem dropItem = new S_DropItem();
                                    dropItem.Position = new PositionInfo
                                    {
                                        PosX = chestDb.PosX,
                                        PosY = chestDb.PosY
                                    };
                                    dropItem.TemplateId = rewardData.itemId;
                                    dropItem.Count = ItemLogic.GetRewardCount(rewardData);
                                    player.Session.Send(dropItem);
                                }
                            }
                        });
                    }
                }
            });
        }
        public static void UpdateChestDb(Player player, ChestDb chestDb)
        {
            if (player == null || chestDb == null)
                return;

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    ChestDb chest = db.Chests
                        .Where(c => c.MapDbId == player.MapInfo.MapDbId && c.ChestId == chestDb.ChestId)
                        .FirstOrDefault();
                    if (chest == null)
                    {
                        db.Chests.Add(chestDb);
                    }
                    bool success = db.SaveChangesEx();//저장할때 예외처리를 해준다.
                    if (success)
                    {
                        //Console.WriteLine("success");
                    }
                }
            });
        }
        public static void SaveShopDb(Player player, ShopDb shopDb, GameRoom room)
        {
            if (player == null || shopDb == null)
                return;

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    ShopDb existingShopDb = db.Shops
                        .Where(s => s.TemplateId == shopDb.TemplateId && s.PlayerDbId == player.PlayerDbId)
                        .FirstOrDefault();
                    if (existingShopDb == null)
                    {
                        db.Shops.Add(shopDb);
                        foreach (ShopItemDb shopItemDb in shopDb.ShopItems)
                        {
                            db.ShopItems.Add(shopItemDb);
                        }                        
                    }
                    bool success = db.SaveChangesEx();

                    List<ShopItemDb> shopItemDbs = db.ShopItems
                        .Where(s => s.ShopDbId == existingShopDb.ShopDbId)
                        .ToList();

                    
                    if (success)
                    {
                        room.Push(() =>
                        {
                            {
                                S_ShopList shopPacket = new S_ShopList();
                                shopPacket.ShopId = existingShopDb.TemplateId;
                                shopPacket.ShopDbId = existingShopDb.ShopDbId;                                
                                List<ItemInfo> items = shopItemDbs.Select(x => new ItemInfo()
                                {                                   
                                    ItemDbId = x.ShopItemDbId,
                                    TemplateId = x.ItemId,
                                    Count = x.Count,
                                    Price = x.Price,                                                                      
                                }).ToList();
                                shopPacket.Items.AddRange(items);
                                player.Session.Send(shopPacket);
                            }
                        });
                    }
                }
            });
        }
        public static void RemoveShopDb(Player player, ShopDb shopDb, ShopItemDb shopItemDb, GameRoom room)
        {
            if (player == null || shopDb == null)
                return;

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {                    
                    ShopDb existingShopDb = db.Shops
                        .Where(s => s.TemplateId == shopDb.TemplateId && s.PlayerDbId == player.PlayerDbId)
                        .FirstOrDefault();
                    
                    if (existingShopDb == null)
                    {
                        return;
                    }
                    ShopItemDb exShopItemDb = db.ShopItems.Where(i => i.ItemId == shopItemDb.ItemId && i.ShopDbId == existingShopDb.ShopDbId).FirstOrDefault();
                    if (exShopItemDb == null)
                    {
                        return;
                    }
                    exShopItemDb.Count -= shopItemDb.Count;
                    if (shopItemDb.Count <= 0)
                    {
                        db.ShopItems.Remove(exShopItemDb);
                    }
                    bool success = db.SaveChangesEx();//저장할때 예외처리를 해준다.   

                    

                    if (success)
                    {
                        List<ShopItemDb> shopItemDbs = db.ShopItems
                            .Where(s => s.ShopDbId == existingShopDb.ShopDbId)
                            .ToList();
                        room.Push(() =>
                        {
                            {
                                S_ShopList shopPacket = new S_ShopList();
                                shopPacket.ShopDbId = existingShopDb.ShopDbId;
                                shopPacket.ShopId = existingShopDb.TemplateId;
                                List<ItemInfo> itemInfos = shopItemDbs.Select(x => new ItemInfo()
                                {
                                    ItemDbId = x.ShopItemDbId,
                                    TemplateId = x.ItemId,
                                    Count = x.Count,
                                    Price = x.Price,
                                }).ToList();

                                shopPacket.Items.AddRange(itemInfos);
                                player.Session.Send(shopPacket);
                            }
                        });
                    }
                }
            });
        }
        public static void SaveInteractionDb(Player player, InteractionDb interactionDb, GameRoom room)
        {
            if (player == null || interactionDb == null)
                return;

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    InteractionDb existingInteractionDb = db.Interactions
                        .Where(i => i.TemplateId == interactionDb.TemplateId && i.PlayerDbId == player.PlayerDbId)
                        .FirstOrDefault();
                    if (existingInteractionDb == null)
                    {
                        db.Interactions.Add(interactionDb);
                    }
                    else
                    {
                        existingInteractionDb = interactionDb;
                    }
                    bool success = db.SaveChangesEx();//저장할때 예외처리를 해준다.   
                    if (success)
                    {
                        room.Push(() =>
                        {
                            {
                                S_Interaction interactPacket = new S_Interaction();
                                interactPacket.PlayerId = player.Id;
                                interactPacket.ObjectId = interactionDb.TemplateId;                                
                                interactPacket.Success = true;                                
                                player.Session.Send(interactPacket);
                            }
                        });
                    }
                }
            });
        }
    }

}