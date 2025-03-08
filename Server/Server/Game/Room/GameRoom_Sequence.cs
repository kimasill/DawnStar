using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using Server.Game.Contents;
using Server.Game.Job;
using Server.Game.Room;
using Server.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using DbTransaction = Server.DB.DbTransaction;

namespace Server.Game
{
    public partial class GameRoom : JobSerializer
    {
        public void HandleRespawn(Player player, RespawnType respawnType)
        {
            if (player == null || player.Room == null)
                return;

            // 플레이어를 게임에서 제거
            LeaveGame(player.Id);

            // Respawn 위치를 결정
            Vector2Int respawnPos = new Vector2Int();
            if (respawnType == RespawnType.Repeat)
            {
                // Repeat 타입은 현재 위치에서 부활
                respawnPos = player.CellPos;
            }
            else if (respawnType == RespawnType.Spot)
            {
                // Spot 타입은 가장 가까운 포탈 근처에서 부활
                MapData mapData = DataManager.MapDict[player.MapInfo.TemplateId];
                int portal = FindClosestPortalId(mapData, player.CellPos);
                UpdatePlayerMapInfo(player, mapData, portal);
            }

            // 플레이어의 상태를 초기화
            player.Stat.Hp = player.Stat.MaxHp;
            player.PosInfo.State = CreatureState.Idle;
            player.PosInfo.MoveDir = MoveDir.Down;

            // 플레이어를 게임에 다시 추가
            EnterGame(player, false);
        }

        private int FindClosestPortalId(MapData mapData, Vector2Int cellPos)
        {
            // 가장 가까운 포탈의 위치를 저장할 변수
            double closestDistance = double.MaxValue;
            int closestPortalId = 0;
            foreach (var portal in mapData.portals)
            {
                // 포탈의 위치를 가져옴
                Vector2Int portalPos = new Vector2Int((int)portal.posX, (int)portal.posY);

                // 현재 위치와 포탈 위치 간의 거리 계산
                double distance = (cellPos - portalPos).magnitude;

                // 가장 가까운 포탈을 업데이트
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPortalId = portal.id;
                }
            }

            return closestPortalId;
        }

        public async void HandleMapChanged(Player player, int portalId)
        {
            MapData mapData = null;
            DataManager.MapDict.TryGetValue(player.MapInfo.TemplateId, out mapData);
            if (mapData == null)
            {
                return;
            }
            int mapId = 0;
            int destPortalId = 0;
            foreach (var portal in mapData.portals)
            {
                if (portal.id == portalId)
                {
                    mapId = portal.mapId;
                    destPortalId = portal.destination;
                }
            }
            MapData nextMapData = null;
            DataManager.MapDict.TryGetValue(mapId, out nextMapData);
            if (nextMapData == null)
            {
                return;
            }

            bool add = false;
            if (nextMapData.type == MapType.Dungeon)
                add = true;
            else if (nextMapData.type == MapType.Field)
                add = false;

            GameRoom room = await GameLogic.Instance.GetRoom(mapId, add);
            HandleMapChanged(player, nextMapData, destPortalId, room);
        }
        public void HandleMapChanged(Player player, MapData map, int destPortalId, GameRoom pRoom)
        {
            if(pRoom == null)
            {
                Console.WriteLine("There is not pRoom");
                return;
            }
            LeaveGame(player.Id, save:false);
            UpdatePlayerMapInfo(player, map, destPortalId);

            MapDb mapDb = new MapDb()
            {                
                PlayerDbId = player.PlayerDbId,
                TemplateId = map.id,
                Scene = map.name,
                MapName = map.name
            };
            DbTransaction.SavePlayerMap(player, mapDb);
            if (pRoom != null)
            {
                player.Room = pRoom;
                pRoom.Push(pRoom.EnterGame, player, false);
            }
        }

        public void UpdatePlayerMapInfo(Player player, MapData map, int destPortalId)
        {
            if (player == null)
                return;

            if(map == null)
            {
                Console.WriteLine("맵 정보를 찾을 수 없습니다.");
                return;
            }
            PortalData portalData = null;

            foreach (var portal in map.portals)
            {
                if (portal.id == destPortalId)
                {
                    portalData = portal;
                }
            }           

            // 플레이어의 위치를 포탈의 위치로 업데이트
            player.CellPos = new Vector2Int((int)portalData.posX, (int)portalData.posY);
            player.PosInfo.State = CreatureState.Idle;
            player.MapInfo.TemplateId = map.id;
            player.MapInfo.MapName = map.name;
            player.MapInfo.Scene = map.name;
            player.MapInfo.PortalId = portalData.id;
            player.Session.UpdateMapChests(player, map.id);
            player.Session.UpdateMapInteractions(player, map.id);
        }
        public void HandleStatChange(Player player)
        {
            if (player == null)
                return;

            S_ChangeStat statInfoPacket = new S_ChangeStat();
            StatInfo statInfo = new StatInfo()
            {
                Level = player.Stat.Level,
                TotalExp = player.Stat.TotalExp,
                Hp = player.Stat.Hp,
                MaxHp = player.Stat.MaxHp,
                Attack = player.Stat.Attack,
                Speed = player.Stat.Speed,
                StatPoint = player.Stat.StatPoint,                
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
        public void HandleSpawnMonster(Player player = null, List<int> ids = null)
        {
            MapData mapData = null;
            DataManager.MapDict.TryGetValue(Map.MapId, out mapData);

            if (mapData == null || mapData.spawns == null)
                return;

            if (ids == null)
            { 
                ids = mapData.spawns.Select(x => x.id).Distinct().ToList();
            }
            foreach (int id in ids)
            {
                int count = mapData.spawns[id - 1].count;
                int monsterId = mapData.spawns[id - 1].monsterId;
                RandomSpawnMonster(monsterId, count);
            }
        }
        int _spawnCount = 0;
        public void RandomSpawnMonster(int monsterId, int count)
        {
            Random random = new Random();
            List<Vector2Int> spawnPositions = Map.GetSpawnPoints(monsterId);
            if (spawnPositions.Count == 0 || spawnPositions == null)
                return;
            // 랜덤으로 spawnCount만큼의 스폰 포인트를 추출합니다.
            List<Vector2Int> selectedSpawnPositions = spawnPositions
                .OrderBy(x => random.Next())
                .Take(count)
                .ToList();
            foreach (Vector2Int spawnPos in selectedSpawnPositions)
            {
                Monster newMonster = Monster.CreateMonster(monsterId);                                
                newMonster.PosInfo.PosX = spawnPos.x;
                newMonster.PosInfo.PosY = spawnPos.y;
                newMonster.SpawnPosition = newMonster.CellPos;
                newMonster.SpawnId = _spawnCount++;
                
                EnterGame(newMonster, false);
            }
        }
        public void HandleEnterDungeon(Player player, int mapId)
        {
            if (player == null)
                return;            

            if (player.Session.CurrentParty == null)
            {
                player.Session.CreateParty();
            }

            PartyMatchingSystem.Instance.EnterMap(player.Session.CurrentParty, mapId);     
        }

        public void HandleMatching(Player player, int mapId, AdmitType admitType)
        {
            if (player == null) return;
            if(admitType == AdmitType.Matching)
            {
                PartyMatchingSystem.Instance.Register(player.Session, mapId);
            }
            else if(admitType == AdmitType.Cancel)
            {
                PartyMatchingSystem.Instance.Unregister(player.Session, mapId);
            }
        }

        public void HandleChat(Player player, string message)
        {
            if (player == null)
                return;
            S_Chat chatPacket = new S_Chat();
            chatPacket.PlayerId = player.Info.ObjectId;
            chatPacket.PlayerName = player.Info.Name;
            chatPacket.Message = message;
            foreach(var p in player.Room._players.Values)
            {
                p.Session.Send(chatPacket);
            }                        
        }
    }
}