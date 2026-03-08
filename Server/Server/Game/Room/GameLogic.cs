using Google.Protobuf.Protocol;
using Server.Game.Job;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class GameLogic : TaskQueue
    {
        public static GameLogic Instance { get; } = new GameLogic();

        Dictionary<int, GameRoom> _rooms = new Dictionary<int, GameRoom>();
        int _roomId = 1;

        public void Update()
        {
            ExecuteAll();
            var roomsCopy = _rooms.Values.ToList(); // Create a copy of the collection
            foreach (GameRoom room in roomsCopy)
                room.Update();
        }

        public GameRoom Add(int mapId)
        {
            GameRoom gameRoom = new GameRoom();
            gameRoom.Enqueue(gameRoom.Init, mapId, 20);
            
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

        public void UpdateRoom(GameRoom room)
        {
            Instance.EnqueueAfter(1000 * 60 * 5, () =>
            {
                if (room.GetPlayerCount() == 0)
                {
                    Instance.Remove(room.RoomId);
                }
            });
        }

        public async Task<GameRoom> GetRoom(int newMapId, bool add)
        {
            GameRoom newRoom;
            if (add == false)
            {
                newRoom = Instance.FindByMapId(newMapId); // 멀티서버용 룸 찾기
                if (newRoom == null)
                {
                    newRoom = Instance.Add(newMapId);
                    await newRoom.WaitForInitializationAsync();
                }
            }
            else
            {
                newRoom = Instance.Add(newMapId);
                await newRoom.WaitForInitializationAsync();
            }
            return newRoom;
        }
    }
}
