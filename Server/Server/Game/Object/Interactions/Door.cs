using Server.Game.Room;
using System.Collections.Generic;

namespace Server.Game
{
    internal class Door : Interaction
    {
        public int ObjectId { get; set; }
        public bool IsOpen { get; set; }
        public List<int> KeyItems { get; set; } = new List<int>();
        public List<Vector2Int> CellPoses { get; set; } = new List<Vector2Int>();
        public Door(int objectId, bool isOpen, List<int> keyItems)
        {
            ObjectId = objectId;
            IsOpen = isOpen;
            KeyItems = keyItems;
        }

        public void Open()
        {
            if(Room == null)
            {
                return;
            }
            IsOpen = true;
            foreach (var cellPos in CellPoses)
            {
                 Room.Map.SetCollision(cellPos, false);
            }
        }

        public void Close()
        {
            if (Room == null)
            {
                return;
            }
            IsOpen = false;
            foreach (var cellPos in CellPoses)
            {
                 Room.Map.SetCollision(cellPos, true);
            }
        }
    }
}