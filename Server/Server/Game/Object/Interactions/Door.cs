using Server.Data;
using Server.Game.Room;
using System.Collections.Generic;

namespace Server.Game
{
    internal class Door : Interaction
    {
        public bool IsOpen { get; set; }
        public List<int> KeyItems { get; set; } = new List<int>();
        public Dictionary<int, bool> Triggers { get; set; } = new Dictionary<int, bool>();
        public List<Vector2Int> CellPoses { get; set; } = new List<Vector2Int>();
        public Door(DoorData doorData)
        {
            TemplateId = doorData.id;
            KeyItems = doorData.keyItems;
            foreach (var cell in doorData.cells)
            {
                CellPoses.Add(new Vector2Int(cell.x, cell.y));
            }
            if (doorData.triggers != null)
            {
                foreach (var trigger in doorData.triggers)
                {
                    Triggers.Add(trigger, false);
                }
            }
            IsOpen = false;
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

        public override void OnTriggerEnter(int id)
        {
            if (Triggers.ContainsKey(id))
            {
                Triggers[id] = !Triggers[id];
            }
        }
    }
}