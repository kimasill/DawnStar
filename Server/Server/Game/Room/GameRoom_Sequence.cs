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
    public partial class GameRoom : TaskQueue
    {
        public void HandleRespawn(Player player, RespawnType respawnType)
        {
            if (player == null || player.Room == null)
                return;

            LeaveGame(player.Id);

            if (respawnType == RespawnType.Spot)
                TryMoveToNearestPortal(player);

            RestorePlayerStateAfterRespawn(player);
            EnterGame(player, false);
        }

        private void TryMoveToNearestPortal(Player player)
        {
            if (!DataManager.MapDict.TryGetValue(player.MapInfo.TemplateId, out MapData mapData)
                || mapData.portals == null || mapData.portals.Count == 0)
                return;

            int portalId = GetNearestPortalId(mapData, player.CellPos);
            if (portalId != 0)
                UpdatePlayerMapInfo(player, mapData, portalId);
        }

        private void RestorePlayerStateAfterRespawn(Player player)
        {
            player.Stat.Hp = player.Stat.MaxHp;
            player.PosInfo.State = CreatureState.Idle;
            player.PosInfo.MoveDir = MoveDir.Down;
        }

        private int GetNearestPortalId(MapData mapData, Vector2Int cellPos)
        {
            if (mapData?.portals == null || mapData.portals.Count == 0)
                return 0;

            double bestDistance = double.MaxValue;
            int bestPortalId = 0;
            foreach (var portal in mapData.portals)
            {
                Vector2Int portalCell = new Vector2Int((int)portal.posX, (int)portal.posY);
                double distance = (cellPos - portalCell).magnitude;

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestPortalId = portal.id;
                }
            }

            return bestPortalId;
        }

        public async void HandleMapChanged(Player player, int portalId)
        {
            if (player == null)
                return;

            if (!DataManager.MapDict.TryGetValue(player.MapInfo.TemplateId, out MapData currentMap))
                return;

            int destinationMapId = 0;
            int destinationPortalId = 0;
            foreach (var portal in currentMap.portals)
            {
                if (portal.id == portalId)
                {
                    destinationMapId = portal.mapId;
                    destinationPortalId = portal.destination;
                    break;
                }
            }

            if (!DataManager.MapDict.TryGetValue(destinationMapId, out MapData nextMap))
                return;

            bool createRoomIfMissing = nextMap.type == MapType.Dungeon;
            GameRoom destinationRoom = await GameLogic.Instance.GetRoom(destinationMapId, createRoomIfMissing);
            HandleMapChanged(player, nextMap, destinationPortalId, destinationRoom);
        }

        public void HandleMapChanged(Player player, MapData map, int destPortalId, GameRoom destinationRoom)
        {
            if (player == null || destinationRoom == null)
                return;

            LeaveGame(player.Id, save: false);
            UpdatePlayerMapInfo(player, map, destPortalId);

            MapDb mapDb = new MapDb()
            {
                PlayerDbId = player.PlayerDbId,
                TemplateId = map.id,
                Scene = map.name,
                MapName = map.name
            };
            DbTransaction.SavePlayerMap(player, mapDb);
            player.Room = destinationRoom;
            destinationRoom.Enqueue(destinationRoom.EnterGame, player, false);
        }

        public void UpdatePlayerMapInfo(Player player, MapData map, int destPortalId)
        {
            if (player == null)
                return;

            if (map == null || map.portals == null)
                return;

            PortalData portalData = map.portals.FirstOrDefault(p => p != null && p.id == destPortalId);
            if (portalData == null)
                return;

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