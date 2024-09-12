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
        public void HandleStartQuest(Player player, int questId)
        {
            if (player == null)
                return;

            player.HandleStartQuest(questId);
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
            // 클라이언트에 맵 이동 정보 전송
            S_MapChange mapChangePacket = new S_MapChange
            {
                MapId = mapId,
                ObjectInfo = player.Info,
            };
            player.Session.Send(mapChangePacket);

            // 플레이어를 해당 맵에 소환하는 패킷 전송
            

            // 플레이어의 위치와 맵 정보를 데이터베이스에 저장
            DbTransaction.SavePlayerPositionAndMap(player, mapId);
        }
    }
}