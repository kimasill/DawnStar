using Google.Protobuf;
using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Game;
using Server.Game.Job;
using Server.Migrations;
using Server.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Server.Game.Item;

namespace Server.DB
{
    public partial class DbTransaction : TaskQueue
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
                MaxPotion = player.MaxPotion,
                StatPoint = player.StatPoint,
            };

            Instance.Enqueue(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    PlayerDb existingPlayerDb = db.Players.FirstOrDefault(p => p.PlayerDbId == playerDb.PlayerDbId);
                    if (existingPlayerDb != null)
                    {                        
                        existingPlayerDb.Hp = playerDb.Hp;
                        existingPlayerDb.Level = playerDb.Level;
                        existingPlayerDb.Exp = playerDb.Exp;
                        existingPlayerDb.MaxPotion = playerDb.MaxPotion;
                        existingPlayerDb.StatPoint = playerDb.StatPoint;
                        existingPlayerDb.MaxHp = playerDb.MaxHp;
                        existingPlayerDb.Attack = playerDb.Attack;
                        existingPlayerDb.Defense = playerDb.Defense;
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
            
            Instance.Enqueue<PlayerDb, GameRoom>(SavePlayerStatus_Step2, playerDb, room);
        }

        public static void SavePlayerStatus_Step2(PlayerDb playerDb, GameRoom room)
        {
            using (AppDbContext db = new AppDbContext())
            {
                db.Entry(playerDb).State = EntityState.Unchanged;
                db.Entry(playerDb).Property(nameof(playerDb.Hp)).IsModified = true;
                bool success = db.SaveChangesEx();//??ν븷???덉쇅泥섎━瑜??댁???   
                if (success)
                {
                    room.Enqueue(SavePlayerStatus_Step3, playerDb.Hp);
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
                            // Realizations ?띿꽦 泥섎━
                            if (playerDb.Realizations != null && playerDb.Realizations.Count > 0)
                            {
                                db.Entry(playerDb).Property(nameof(playerDb.RealizationsJson)).IsModified = true;
                            }
                        }
                        else if (value is IList list && list.Count > 0)
                        {
                            // ?쇰컲 由ъ뒪????낆씠怨??붿냼媛 ?덈뒗 寃쎌슦
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
                            // 由ъ뒪????낆씠 ?꾨땲怨? double濡?蹂??媛?ν븳 寃쎌슦
                            db.Entry(playerDb).Property(property.Name).IsModified = true;
                        }
                    }
                }
                bool success = db.SaveChangesEx();
                if (success)
                {
                    room.Enqueue(() =>
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
        public static async Task SavePlayerPosDb(Player player, PlayerDb playerDb)
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
                    if (property.Name == "MapDbId")
                    {
                        db.Entry(playerDb).Property(property.Name).IsModified = true;
                    }
                    else if (property.Name == "PosX" || property.Name == "PosY")
                    {
                        db.Entry(playerDb).Property(property.Name).IsModified = true;
                    }
                }
                await db.SaveChangesExAsync();
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

            ItemDb itemDb = new ItemDb()
            {
                TemplateId = itemData.id,
                Count = count,
                Enhance = 0,
                Enchant = 0,
                Grade = itemData.grade.ToString(),
                OwnerDbId = player.PlayerDbId,
                Slot = slot.Value,
                Options = itemData.options
            };
            SaveAddItemDB(player, itemDb, room);
        }
        public static void SaveEnchantItem(Player player, ItemDb itemDb, GameRoom room)
        {
            if (itemDb == null)
                return;

            using (AppDbContext db = new AppDbContext())
            {
                db.Entry(itemDb).State = EntityState.Unchanged;
                db.Entry(itemDb).Property(nameof(itemDb.Enchant)).IsModified = true;
                db.Entry(itemDb).Property(nameof(itemDb.OptionsJson)).IsModified = true;
                bool success = db.SaveChangesEx();
                if (success)
                {
                    room.Enqueue(() =>
                    {
                        Item iItem = player.Inven.Find(i => i.ItemDbId == itemDb.ItemDbId);
                        if (iItem != null)
                        {
                            iItem.Enchant = itemDb.Enchant;
                            iItem.Options.Clear();
                            foreach (var option in itemDb.Options)
                            {
                                iItem.Options.Add(option.Key, option.Value);
                            }

                            S_Enchant enchantPacket = new S_Enchant();
                            enchantPacket.ItemInfo = iItem.Info;
                            enchantPacket.Success = true;
                            player.Session.Send(enchantPacket);
                        }
                    });
                }
            }
        }
        public static void SaveAddItemDB(Player player, ItemDb itemDb, GameRoom room)
        {
            Instance.Enqueue(() =>
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
                    bool success = db.SaveChangesEx();//??ν븷???덉쇅泥섎━瑜??댁???   
                    
                    if (success)
                    {                        
                        itemDb.ItemDbId = db.Items.FirstOrDefault(i => i.OwnerDbId == itemDb.OwnerDbId && i.Slot == itemDb.Slot).ItemDbId;
                        room.Enqueue(() =>
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
            Instance.Enqueue(() =>
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
                            if (tItemDb.Count < remainingCount)
                            {   
                                S_SystemNotice systemNotice = new S_SystemNotice();
                                systemNotice.Message = "The item count to be removed exceeds the stored item count.";
                                player.Session.Send(systemNotice);
                                return;
                            }
                            if (tItemDb.Count > remainingCount)
                            {
                                // Count瑜?媛먯냼?쒗궓??
                                tItemDb.Count -= remainingCount;
                                remainingCount = 0;
                            }
                            else
                            {
                                // Count媛 0 ?댄븯媛 ?섎㈃ ?꾩씠?쒖쓣 ?쒓굅?쒕떎.
                                remainingCount -= tItemDb.Count;
                                tItemDb.Count = 0;
                                db.Items.Remove(tItemDb);
                            }
                        }

                        bool success = db.SaveChangesEx(); // ??ν븷 ???덉쇅 泥섎━瑜??댁???
                        if (success)
                        {
                            room.Enqueue(() =>
                            {
                                Item iItem = player.Inven.Find(i => i.ItemDbId == tItemDb.ItemDbId);
                                if (iItem != null)
                                {
                                    iItem.Count = tItemDb.Count;                              
                                    player.Inven.UpdateItem(iItem.ItemDbId, iItem.Count);
                                }
                                Item rItem = Item.MakeItem(tItemDb);
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
        public static void SaveRemoveItemStateDb(Player player, ItemDb itemDb, GameRoom room)
        {
            if (player == null || itemDb == null || room == null)
                return;

            Instance.Enqueue(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    try
                    {
                        ItemDb tItemDb = db.Items
                            .FirstOrDefault(i => i.ItemDbId == itemDb.ItemDbId && i.OwnerDbId == player.PlayerDbId);
                        if (tItemDb != null)
                        {
                            db.Entry(tItemDb).State = EntityState.Deleted;
                            bool success = db.SaveChangesEx(); // ??ν븷 ???덉쇅 泥섎━瑜??댁???
                            if (success)
                            {
                                room.Enqueue(() =>
                                {
                                    player.Inven.Remove(itemDb.ItemDbId);
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // ?덉쇅 泥섎━ 濡쒖쭅 異붽?
                        Console.WriteLine($"Error removing item: {ex.Message}");
                    }
                }
            });
        }
        public static void SaveItemSlotAndCount(Player player, List<ItemDb> itemDb, GameRoom room)
        {
            if (player == null || itemDb == null || room == null)
                return;

            Instance.Enqueue(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    foreach(var item in itemDb)
                    {
                        db.Entry(item).State = EntityState.Unchanged;
                        db.Entry(item).Property(nameof(item.Count)).IsModified = true;
                        db.Entry(item).Property(nameof(item.Slot)).IsModified = true;
                    }
                    bool success = db.SaveChangesEx();

                    if (success)
                    {
                        room.Enqueue(() =>
                        {
                            S_ItemList itemList = new S_ItemList();
                            foreach (Item item in player.Inven.Items.Values)
                            {                                
                                ItemInfo info = new ItemInfo();
                                info.MergeFrom(item.Info);
                                itemList.Items.Add(info);                                
                            }
                            player.Session.Send(itemList);
                        });
                    }
                }
            });
        }
        public static void SaveEnhancedItemDB(Player player, ItemDb itemDb, GameRoom room)
        {
            Instance.Enqueue(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(itemDb).State = EntityState.Unchanged;
                    db.Entry(itemDb).Property(nameof(itemDb.Enhance)).IsModified = true;
                    db.Entry(itemDb).Property(nameof(itemDb.Damage)).IsModified = true;
                    db.Entry(itemDb).Property(nameof(itemDb.Defense)).IsModified = true;
                    db.Entry(itemDb).Property(nameof(itemDb.OptionsJson)).IsModified = true;                   

                    bool success = db.SaveChangesEx();//??ν븷???덉쇅泥섎━瑜??댁???   
                    if (success)
                    {
                        room.Enqueue(() =>
                        {
                            Item newItem = Item.MakeItem(itemDb);
                            Item iItem = player.Inven.Find(i => i.ItemDbId == itemDb.ItemDbId);
                            if (iItem != null)
                            {
                                player.Inven.UpdateItemInfo(newItem);

                                S_Enhance itemPacket = new S_Enhance();
                                ItemInfo info = new ItemInfo();
                                info.MergeFrom(newItem.Info);
                                itemPacket.ItemInfo = info;
                                itemPacket.Success = true;
                                player.Session.Send(itemPacket);
                            }
                        });
                    }
                }
            });
        }

        public static void SaveCompleteQuest(Player player, QuestDb questDb, GameRoom room)
        {
            if (player == null || questDb == null)
                return;

            // ?섏뒪???곗씠???쎄린
            Data.QuestData questData = DataManager.QuestDict.GetValueOrDefault(questDb.TemplateId);
            if (questData == null)
                return;

            // ?곌퀎 ?뺣낫 媛?몄삤湲?
            int exp = questData.rewards.FirstOrDefault(r => r.rewardType == RewardType.Exp)?.amount ?? 0;
            int connection = questData.connection;
            int gold = questData.rewards.FirstOrDefault(r => r.rewardType == RewardType.Gold)?.amount ?? 0;

            // ?섏뒪???꾨즺 泥섎━
            Instance.Enqueue(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(questDb).State = EntityState.Unchanged;
                    db.Entry(questDb).Property(nameof(questDb.Progress)).IsModified = true;
                    db.Entry(questDb).Property(nameof(questDb.Completed)).IsModified = true;
                    
                    bool success = db.SaveChangesEx();//??ν븷???덉쇅泥섎━瑜??댁???   
                    if (success)
                    {
                        room.Enqueue(() =>
                        {
                            player.Gold += gold;
                            ExpNoti(player, exp);
                            RewardInfo rewardInfo = new RewardInfo();
                            foreach (RewardData rewardData in questData.rewards)
                            {
                                rewardInfo.RewardType = rewardData.rewardType;
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

            Instance.Enqueue(() =>
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
                    bool success = db.SaveChangesEx();//??ν븷???덉쇅泥섎━瑜??댁???   
                    if (success)
                    {
                        room.Enqueue(() =>
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
        public static void UpdateQuestProgress(Player player, QuestDb questDb, GameRoom room)
        {
            if (player == null || questDb == null)
                return;

            using (AppDbContext db = new AppDbContext())
            {
                db.Entry(questDb).State = EntityState.Unchanged;
                db.Entry(questDb).Property(nameof(questDb.Progress)).IsModified = true;
                bool success = db.SaveChangesEx();//??ν븷???덉쇅泥섎━瑜??댁???   
                if (success)
                {                    
                }
            }
        }
        public static void SaveChestDb(Player player, ChestDb chestDb, GameRoom room)
        {
            if (player == null || chestDb == null)
                return;
            
            bool reward = false;

            Instance.Enqueue(() =>
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
                    bool success = db.SaveChangesEx();//??ν븷???덉쇅泥섎━瑜??댁???   
                    if (success && reward)
                    {
                        AcquireData acquireData = DataManager.AcquireDict.GetValueOrDefault(chestDb.TemplateId);
                        if (acquireData == null)
                            return;

                        room.Enqueue(() =>
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

            Instance.Enqueue(() =>
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
                    bool success = db.SaveChangesEx();//??ν븷???덉쇅泥섎━瑜??댁???
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

            Instance.Enqueue(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    ShopDb existingShopDb = db.Shops
                        .Where(s => s.TemplateId == shopDb.TemplateId && s.PlayerDbId == player.PlayerDbId)
                        .FirstOrDefault();
                    if (existingShopDb == null)
                    {
                        db.Shops.Add(shopDb);
                        db.SaveChangesEx();
                        existingShopDb = shopDb;
                        existingShopDb.ShopDbId = db.Entry(shopDb).Property(s => s.ShopDbId).CurrentValue;
                    }
                    foreach (ShopItemDb shopItemDb in shopDb.ShopItems)
                    {
                        if (db.ShopItems.Where(i => i.ItemId == shopItemDb.ItemId && i.ShopDbId == existingShopDb.ShopDbId).FirstOrDefault() == null)
                        {
                            shopItemDb.ShopDbId = existingShopDb.ShopDbId; // ShopItemDb??ShopDbId瑜??ㅼ젙?⑸땲??
                            db.ShopItems.Add(shopItemDb);
                        }
                    }
                    bool success = db.SaveChangesEx();
                    
                    if (success)
                    {
                        List<ShopItemDb> shopItemDbs = db.ShopItems
                       .Where(s => s.ShopDbId == existingShopDb.ShopDbId)
                       .ToList();

                        room.Enqueue(() =>
                        {
                            {
                                S_ShopList shopPacket = new S_ShopList();
                                shopPacket.ShopId = shopDb.TemplateId;
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

            Instance.Enqueue(() =>
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
                    bool success = db.SaveChangesEx();//??ν븷???덉쇅泥섎━瑜??댁???   

                    

                    if (success)
                    {
                        List<ShopItemDb> shopItemDbs = db.ShopItems
                            .Where(s => s.ShopDbId == existingShopDb.ShopDbId)
                            .ToList();
                        room.Enqueue(() =>
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

            Instance.Enqueue(() =>
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
                        existingInteractionDb.Completed = interactionDb.Completed;
                    }
                    bool success = db.SaveChangesEx();//??ν븷???덉쇅泥섎━瑜??댁???   
                    if (success)
                    {
                        room.Enqueue(() =>
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