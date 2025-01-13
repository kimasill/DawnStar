using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using Server.Game.Contents;
using Server.Game.Job;
using Server.Game.Room;
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

            //// 클라이언트에 로딩 화면을 띄우라는 신호를 보냅니다.
            //S_Loading loadingPacket = new S_Loading();
            //loadingPacket.Loading = true;
            //player.Session.Send(loadingPacket);

            // LeaveGame을 호출하여 현재 게임에서 플레이어를 제거합니다.
            LeaveGame(player.Id);

            // Respawn 위치를 결정합니다.
            Vector2Int respawnPos;
            if (respawnType == RespawnType.Repeat)
            {
                // Repeat 타입은 현재 위치에서 부활합니다.
                respawnPos = player.CellPos;
            }
            else if (respawnType == RespawnType.Spot)
            {
                // Spot 타입은 가장 가까운 포탈 근처에서 부활합니다.
                MapData mapData = DataManager.MapDict[player.MapInfo.TemplateId];
                respawnPos = FindClosestPortal(mapData, player.CellPos);
            }
            else
            {
                // 기본적으로 현재 위치에서 부활합니다.
                respawnPos = player.CellPos;
            }

            // 플레이어의 상태를 초기화합니다.
            player.Stat.Hp = player.Stat.MaxHp;
            player.PosInfo.State = CreatureState.Idle;
            player.PosInfo.MoveDir = MoveDir.Down;
            player.CellPos = respawnPos;

            // EnterGame을 호출하여 플레이어를 다시 게임에 추가합니다.
            EnterGame(player, false);

            //// 클라이언트에 로딩 화면을 종료하라는 신호를 보냅니다.
            //loadingPacket.Loading = false;
            //player.Session.Send(loadingPacket);
        }

        private Vector2Int FindClosestPortal(MapData mapData, Vector2Int cellPos)
        {
            // 가장 가까운 포탈의 위치를 저장할 변수
            Vector2Int closestPortalPos = new Vector2Int(int.MaxValue, int.MaxValue);
            double closestDistance = double.MaxValue;

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
                    closestPortalPos = portalPos;
                }
            }

            return closestPortalPos;
        }

        public async void HandleMapChanged(Player player, int portalId)
        {
            MapData mapData = null;
            DataManager.MapDict.TryGetValue(player.MapInfo.TemplateId, out  mapData);
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
            DbTransaction.Instance.Push(UpdatePlayerMapInfo, player, map, destPortalId);

            MapDb mapDb = new MapDb()
            {                
                PlayerDbId = player.PlayerDbId,
                TemplateId = map.id,
                Scene = map.name,
                MapName = map.name
            };
            DbTransaction.Instance.Push(DbTransaction.SavePlayerMap,player, mapDb);
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

            Party party = player.Session.CurrentParty;

            if (party == null)
            {
                party = new Party(PartySystem.Instance.CreateParty().PartyId);
                party.AddMember(player);                
            }
            PartyMatchingSystem.Instance.EnterMap(party, mapId);     
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
            Broadcast(player.CellPos, chatPacket);
        }
    }
}