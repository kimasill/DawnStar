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
        public void HandleRespawn(Player player, RespawnType respawnType)
        {
            if (player == null || player.Room == null)
                return;

            // 클라이언트에 로딩 화면을 띄우라는 신호를 보냅니다.
            S_Loading loadingPacket = new S_Loading();
            loadingPacket.Loading = true;
            player.Session.Send(loadingPacket);

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

            // 클라이언트에 로딩 화면을 종료하라는 신호를 보냅니다.
            loadingPacket.Loading = false;
            player.Session.Send(loadingPacket);
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
                double distance = (cellPos- portalPos).magnitude;

                // 가장 가까운 포탈을 업데이트
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPortalPos = portalPos;
                }
            }

            return closestPortalPos;
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
            foreach (var portal in mapData.portals)
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
        public void HandleSpawnMonster(Player player, List<int> ids)
        {
            if (player == null)
                return;

            DataManager.MapDict.TryGetValue(player.MapInfo.TemplateId, out MapData mapData);
            if (mapData == null)
                return;

            foreach (int id in ids)
            {
                Monster monster = ObjectManager.Instance.Add<Monster>();
                if (monster == null)
                    return;

                int templateId = mapData.spawns[id - 1].objectId;
                monster.Init(templateId);
                monster.PosInfo.PosX = (int)mapData.spawns[id - 1].posX;
                monster.PosInfo.PosY = (int)mapData.spawns[id - 1].posY;
                monster.SpawnPosition = monster.CellPos;
                monster.SpawnId = id;
                player.Room.Push(EnterGame, monster, false);
            }
        }
    }
}