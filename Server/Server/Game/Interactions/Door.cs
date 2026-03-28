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
        public Door(DoorData doorData) : base(doorData)
        {
            TemplateId = doorData.id;
            if(doorData.keyItems != null)
                KeyItems.AddRange(doorData.keyItems);            
            if (doorData.triggers != null)
            {
                foreach (var trigger in doorData.triggers)
                {
                    Triggers.Add(trigger, false);
                }
            }
            IsOpen = false;
        }
        public override void OnInteraction()
        {
            if (IsOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }
        public void Open()
        {
            if (Room == null)
            {
                return;
            }
            
            foreach (var cellPos in Cells)
            {
                Room?.Map?.SetCollision(cellPos, false);
            }
            IsOpen = true;
        }

        public void Close()
        {
            if (Room == null)
            {
                return;
            }
            foreach (var cellPos in Cells)
            {
                Room?.Map?.SetCollision(cellPos, true);
            }

            IsOpen = false;
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