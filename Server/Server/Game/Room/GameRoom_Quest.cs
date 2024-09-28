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
                    // 퀘스트가 있으면 해당 퀘스트를 할당합니다.
                    QuestInfo questInfo = new QuestInfo()
                    {
                        QuestDbId = questDb.QuestDbId,
                        TemplateId = questDb.TemplateId,
                        Progress = questDb.Progress,
                        Completed = questDb.Completed,
                        QuestType = DataManager.QuestDict[questDb.TemplateId].questType
                    };
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

        public void HandleMapChanged(Player player, int mapId)
        {
            if (player == null)
                return;

            // MapDict에서 포탈 정보 찾기
            MapData mapData = null;

            if (!DataManager.MapDict.TryGetValue(mapId, out mapData))
                return;

            // 플레이어의 이전 맵에서의 위치를 가져옴
            PortalData portalData = null; 
            foreach(var portal in mapData.portals)
            {
                if (portal.name == player.MapInfo.MapName)
                {
                    portalData = portal;
                    break;
                }
            }           

            if (portalData == null)
            {
                Console.WriteLine("포탈 정보를 찾을 수 없습니다.");
                return;
            }

            // 플레이어의 위치를 포탈의 위치로 업데이트
            player.CellPos = new Vector2Int((int)portalData.posX, (int)portalData.posY);            
            player.PosInfo.State = CreatureState.Idle;
            player.MapInfo.TemplateId = mapData.id;
            player.MapInfo.MapName = mapData.name;
            player.MapInfo.Scene = mapData.name;
            player.MapInfo.PortalId = portalData.id;
            // 클라이언트에 맵 이동 정보 전송
            S_MapChange mapChangePacket = new S_MapChange
            {
                MapId = mapId,            
                ObjectInfo = player.Info
            };            
            
            player.Session.Send(mapChangePacket);

            // 플레이어의 위치와 맵 정보를 데이터베이스에 저장
            DbTransaction.SavePlayerStatus_All(player, this);
            DbTransaction.SavePlayerMap(player, player.MapInfo);
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

        public void HandleStatChange(Player player)
        {
            if (player == null)
                return;

            S_ChangeStat statInfoPacket = new S_ChangeStat();
            StatInfo statInfo = new StatInfo()
            {
                Level = player.Stat.Level,
                Hp = player.Stat.Hp,
                MaxHp = player.Stat.MaxHp,
                Attack = player.Stat.Attack,
                Speed = player.Stat.Speed
            };
            statInfoPacket.StatInfo = statInfo;

            player.Session.Send(statInfoPacket);
        }

        public void HandleChangePosition(Player player)
        {
            if (player == null)
                return;

            S_ChangePosition positionPacket = new S_ChangePosition();
            positionPacket.ObjectId = player.Info.ObjectId;
            positionPacket.Position = player.PosInfo;

            player.Session.Send(positionPacket);
        }

        public void HandleSpawnMonster(Player player, C_RequestMonster monsterPacket)
        {
            if (player == null)
                return;

            Monster[] monsters = null;                
            // 몬스터 생성
            foreach (int id in monsterPacket.TemplateId)
            {
                Monster monster = new Monster();
                monster.Init(id);
                monsters.Append(monster);                
            }
            

            // 싱글 모드일 경우 클라이언트에만 몬스터를 추가
            if (player.Session.ServerState == PlayerServerState.ServerStateSingle)
            {                
                S_Spawn spawnPacket = new S_Spawn();
                foreach (Monster monster in monsters)
                {
                    spawnPacket.Objects.Add(monster.Info);                        
                }
                player.Session.Send(spawnPacket);
            }
            else
            {
                // 멀티 모드일 경우 기존 로직 사용
                foreach (Monster monster in monsters)
                {
                    EnterGame(monster, false);
                }
            }
        }
    }
}