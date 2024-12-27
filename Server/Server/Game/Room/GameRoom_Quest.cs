using Google.Protobuf;
using Google.Protobuf.Protocol;
using Microsoft.VisualBasic;
using Server.Data;
using Server.DB;
using Server.Game.Job;
using Server.Game.Room;
using Server.Migrations;
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

            Quest quest = player.Quest.Get(questId);
            if(quest != null)
            {
                if(quest.Progress == 1)
                {
                    return;
                }
            }

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
                        questDb = new QuestDb()
                        {
                            OwnerDbId = player.PlayerDbId,
                            TemplateId = 1,
                            Progress = 1,
                            Completed = false,                            
                        };
                    }
                    else                                                                                                                                                                                                                                                                                                                                                          
                    {
                        QuestData questData = DataManager.QuestDict.GetValueOrDefault(questId);
                        if (questData == null)
                            return;
                        questDb = new QuestDb()
                        {
                            OwnerDbId = player.PlayerDbId,
                            TemplateId = questData.id,
                            Progress = 1,
                            Completed = false,                            
                        };
                    }
                }
                else
                {
                    if(questDb.Completed)
                        return;

                    if (player.Quest.CurrentQuest.TemplateId == questDb.TemplateId && player.Quest.CurrentQuest.Progress == 1)//메모리에 들고있으면 안함
                        return;               
                    questDb.Progress = 1;
                    player.Quest.CurrentQuest = Quest.MakeQuest(questDb);
                }
                DbTransaction.SaveStartQuest(player, questDb, player.Room);
            }
        }

        public void HandleUpdateQuest(Player player, int questId, int progress)
        {
            if (player == null)
                return;

            // 퀘스트 진행 상태 업데이트
            Quest quest = player.Quest.CurrentQuest; // 현재 퀘스트 정보 가져오기
            if (quest == null || quest.TemplateId != questId)
                return;

            // 퀘스트 진행 상태 업데이트
            quest.Progress = progress;

            QuestDb questDb = new QuestDb()
            {
                QuestDbId = quest.QuestDbId,
                OwnerDbId = player.PlayerDbId,
                TemplateId = quest.TemplateId,
                Progress = quest.Progress,
                Completed = quest.IsCompleted,
            };

            // DB에 퀘스트 진행 상태 저장
            DbTransaction.UpdateQuestProgress(player, questDb, player.Room);
        }

        public void HandleQuestComplete(Player player, int questId)
        {
            if (player == null)
                return;

            // 퀘스트 완료 처리
            Quest quest = player.Quest.Get(questId);
            if (quest == null || quest.TemplateId != questId)
                return;

            // 퀘스트 완료 상태로 변경
            if (quest.IsCompleted == false)
                quest.IsCompleted = true;
            quest.Progress = 100;

            QuestDb questDb = new QuestDb()
            {
                OwnerDbId = player.PlayerDbId,
                QuestDbId = quest.QuestDbId,
                TemplateId = quest.TemplateId,
                Progress = quest.Progress,
                Completed = quest.IsCompleted,
            };

            // DB에 퀘스트 완료 상태 저장
            DbTransaction.SaveCompleteQuest(player, questDb, player.Room);

            // 플레이어의 퀘스트 정보를 QuestInfo 리스트로 반환
            List<QuestInfo> questInfos = new List<QuestInfo>();
            foreach (Quest q in player.Quest.Quests.Values)
            {
                questInfos.Add(q.Info);
            }

            S_QuestList questListPacket = new S_QuestList();
            questListPacket.Quests.AddRange(questInfos);
            player.Session.Send(questListPacket);
        }
        public void HandleSelectStat(Player player, int statId)
        {
            if (player == null)
                return;

            if (player.StatPoint <= 0)
                return;

            
            RealizationData realization = null; 
            DataManager.RealizationDict.TryGetValue(statId, out realization);
            if (realization == null)
                return;
            PlayerDb playerDb = new PlayerDb()
            {
                PlayerDbId = player.PlayerDbId
            };
            player.StatPoint--;
            if (realization != null)
            {
                int index = realization.id - 1;
                int count = 0;
                player.Stat.Realizations[index] += 1;
                count = player.Stat.Realizations[index];
                playerDb.Realizations = player.Stat.Realizations.ToList();
                
                // 소인수 분해를 통해 추가 스탯 적용
                List<int> factors = GetFactors(count);
                foreach (int factor in factors)
                {
                    foreach(var specialStat in realization.specialStatDatas)
                    {
                        if(specialStat == null)
                            continue;
                        if (specialStat.point == factor)
                        {
                            playerDb = ApplySpecialStat(player, playerDb, specialStat);
                        }
                    }
                }
            }
            playerDb.StatPoint = player.StatPoint;
            DbTransaction.SavePlayerStatDb(player, playerDb, player.Room);
        }
        private List<int> GetFactors(int number)
        {
            List<int> factors = new List<int>();
            for (int i = 1; i <= number; i++)
            {
                if (number % i == 0)
                {
                    factors.Add(i);
                }
            }
            return factors;
        }

        private PlayerDb ApplySpecialStat(Player player, PlayerDb playerDb, SpecialStatData specialStatData)
        {
            switch (specialStatData.name)
            {
                case "생명력":
                    player.Stat.MaxHp += (int)specialStatData.value;
                    playerDb.MaxHp = player.Stat.MaxHp;
                    break;
                case "공격력":
                    player.Stat.Attack += (int)specialStatData.value;
                    playerDb.Attack = player.Stat.Attack;
                    break;
                case "방어력":
                    player.Stat.Defense += (int)specialStatData.value;
                    playerDb.Defense = player.Stat.Defense;
                    break;
                case "공격속도":
                    player.Stat.AttackSpeed += specialStatData.value/100;
                    playerDb.AttackSpeed = player.Stat.AttackSpeed;
                    break;
                case "회피율":
                    player.Stat.Avoid += (int)specialStatData.value;
                    playerDb.Avoid = player.Stat.Avoid;
                    break;
                case "명중률":
                    player.Stat.Accuracy += (int)specialStatData.value;
                    playerDb.Accuracy = player.Stat.Accuracy;
                    break;
                case "이동속도":
                    player.Stat.Speed += specialStatData.value;
                    playerDb.Speed = player.Stat.Speed;
                    break;
                case "치명타 확률":
                    player.Stat.CriticalChance += (int)specialStatData.value;
                    playerDb.CriticalChance = player.Stat.CriticalChance;
                    break;
                case "치명타 피해량":
                    player.Stat.CriticalDamage += (int)specialStatData.value;
                    playerDb.CriticalDamage = player.Stat.CriticalDamage;
                    break;
                case "최대 회복제":
                    player.Stat.MaxPotion += (int)specialStatData.value;    
                    playerDb.MaxPotion = player.Stat.MaxPotion;
                    break;
                case "회복제 성능":
                    player.Stat.PotionPerformance += (int)specialStatData.value;
                    playerDb.PotionPerformance = player.Stat.PotionPerformance;
                    break;
                case "미지의 기운":
                    player.Stat.Up += (int)specialStatData.value;
                    playerDb.Up = player.Stat.Up;
                    break;
                case "미지의 기운 회복":
                    player.Stat.UpRegen += (int)specialStatData.value;
                    playerDb.UpRegen = (int)player.Stat.UpRegen;
                    break;
            }
            return playerDb;
        }
        public void HandleDoorInteraction(Player player, int doorId)
        {
            if (player == null || player.Room == null)
                return;

            Door door = Map.GetInteraction(doorId) as Door;

            bool success = true;
            

            if(door.IsOpen == false)
            {
                if (door.Triggers.Count > 0)
                {
                    foreach (var trigger in door.Triggers.Values)
                    {
                        if (trigger == false)
                        {
                            success = false;
                            break;
                        }
                    }
                }
                if (door.KeyItems.Count > 0)
                {
                    foreach (var keyId in door.KeyItems)
                    {
                        Item key = player.Inven.FindByTemplateId(keyId);
                        if (key == null)
                        {
                            success = false;
                            break;
                        }
                    }
                    if(success)
                    {
                        foreach (var keyId in door.KeyItems)
                        {
                            Item key = player.Inven.FindByTemplateId(keyId);
                            ItemDb itemDb = new ItemDb()
                            {
                                TemplateId = key.TemplateId,
                                Count = 1,
                                OwnerDbId = player.PlayerDbId,
                                Slot = key.Slot
                            };
                            DbTransaction.SaveRemovedItemDB(player, itemDb, player.Room);
                            player.Inven.Remove(key.ItemDbId, 1);
                        }
                    }
                }                
            }
            S_Interaction interactionPacket = new S_Interaction()
            {
                Success = success,
                ObjectId = doorId,
                PlayerId = player.Id,
                InteractionType = InteractionType.Door
            };
            if (interactionPacket.Success)
            {
                door.OnInteraction();
                player.Room.Broadcast(player.CellPos, interactionPacket);
            }
            else
            {
                player.Session.Send(interactionPacket);
            }
            Console.WriteLine($"Door Interaction - {interactionPacket.InteractionType}, ID:{doorId}, Success:{interactionPacket.Success}");
        }

        public void HandleTriggerInteraction(Player player, int triggerId)
        {
            if (player == null || player.Room == null)
                return;

            Trigger trigger = Map.GetInteraction(triggerId) as Trigger;
            if (trigger == null)
            {
                return;
            }
            bool success = true;
            if (trigger.ActivationItems != null && trigger.IsActivated==false)
            {
                foreach (var keyId in trigger.ActivationItems)
                {
                    Item key = player.Inven.FindByTemplateId(keyId);
                    if (key == null)
                    {
                        success = false;
                    }
                }
            }
            List<int> targetIds = new List<int>();
            if (trigger.Conditions.Count > 0 && success)
            {
                foreach (var targetInteraction in trigger.Conditions.Keys)
                {
                    targetIds.Add(targetInteraction);
                    Map.GetInteraction(targetInteraction).OnTriggerEnter(triggerId);                    
                }
            }

            S_Interaction interactionPacket = new S_Interaction()
            {
                Success = success,
                ObjectId = triggerId,
                PlayerId = player.Id,                
                InteractionType = InteractionType.Trigger
            };
            interactionPacket.TargetId.AddRange(targetIds);
            if (success)
            {
                trigger.OnInteraction();                
                player.Room.Broadcast(player.CellPos, interactionPacket);
            }
            else
            {
                player.Session.Send(interactionPacket);
            }
            Console.WriteLine($"Trigger Interaction - {interactionPacket.InteractionType}, ID:{triggerId}, Success:{interactionPacket.Success}");
        }

        public void HandleItemTableInteraction(Player player, int itemTableId)
        {
            if (player == null)
                return;

            DataManager.InteractionDict.TryGetValue(itemTableId, out InteractionData interactionData);
            if (interactionData == null)
                return;
            ItemTableData itemTableData = interactionData as ItemTableData;

            foreach (int id in itemTableData.itemIds)
            {
                DataManager.ItemDict.TryGetValue(id, out ItemData itemData);
                if (itemData == null)
                    return;
                DbTransaction.RewardPlayer(player, itemData, 1, this);
            }
            // 인터랙션 성공 여부를 DbTransaction에 저장합니다.
            InteractionDb interactionDb = new InteractionDb
            {
                PlayerDbId = player.PlayerDbId,
                TemplateId = itemTableId,
                MapDbId = player.MapInfo.MapDbId,
                Completed = true
            };

            DbTransaction.SaveInteractionDb(player, interactionDb, this);
        }

        public void HandleQuestSignInteraction(Player player, int questSignId)
        {
            if (player == null) return;

            DataManager.InteractionDict.TryGetValue(questSignId, out InteractionData interactionData);
            if (interactionData == null)
                return;
            QuestSignData questData = interactionData as QuestSignData;

            if(questData == null)
            {
                return;
            }

            if(questData.startId > 0)
            {
                HandleStartQuest(player, questData.startId);
            }
            else if (questData.endId > 0)
            {
                HandleQuestComplete(player, questData.endId);
            }

            // 인터랙션 성공 여부를 DbTransaction에 저장합니다.
            InteractionDb interactionDb = new InteractionDb
            {
                PlayerDbId = player.PlayerDbId,
                TemplateId = questData.id,
                MapDbId = player.MapInfo.MapDbId,
                Completed = true
            };

            DbTransaction.SaveInteractionDb(player, interactionDb, this);
        }
    }
}
