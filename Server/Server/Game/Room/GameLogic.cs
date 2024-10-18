using Google.Protobuf.Protocol;
using Server.Game.Job;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class GameLogic : JobSerializer
    {
        public static GameLogic Instance { get; } = new GameLogic();

        Dictionary<int, GameRoom> _rooms = new Dictionary<int, GameRoom>();
        int _roomId = 1;

        public void Update()
        {
            Flush();
            foreach (GameRoom room in _rooms.Values)
                room.Update();
        }

        public GameRoom Add(int mapId)
        {
            GameRoom gameRoom = new GameRoom();
            gameRoom.Push(gameRoom.Init, mapId, 20);
            
            gameRoom.RoomId = _roomId;
            _rooms.Add(gameRoom.RoomId, gameRoom);
            _roomId++;

            return gameRoom;
        }
         
        public bool Remove(int roomId)
        {
            return _rooms.Remove(roomId);
        }

        public GameRoom Find(int roomId)
        {
            GameRoom room = null;
            _rooms.TryGetValue(roomId, out room);
            return room;
        }

        public GameRoom FindByMapId(int mapId)
        {
            foreach (var room in _rooms.Values)
            {
                if (room.Map.MapId == mapId)
                    return room;
            }

            return null;
        }

        public void ChangeRoom(Player player, int newMapId, GameRoom room)
        {
            if(room.GetPlayerCount() == 0)
            {
                Instance.Remove(room.RoomId);
            }
            player.Session.ChangeServerState(newMapId);
            // 새로운 맵이 멀티서버용 맵인지 확인
            GameRoom newRoom;
            if (newMapId == 5)
            {
                newRoom = Instance.FindByMapId(newMapId); // 멀티서버용 룸 찾기
                if (newRoom == null)
                {
                    newRoom = Instance.Add(newMapId);
                }
            }
            else
            {
                newRoom = Instance.Add(newMapId);
            }

            if (newRoom != null)
            {
                newRoom.Push(newRoom.EnterGame, player, false);
            }
            else
            {
                Console.WriteLine("알맞은 Room 없음");
            }
        }
    }
}
