using Server.Data;
using Server.Game.Room;
using System.Collections.Generic;

namespace Server.Game
{
    internal class Trigger : Interaction
    {
        public bool IsActivated { get; set; }
        public List<int> ActivationItems { get; set; } = new List<int>();
        public Dictionary<int, bool> Conditions { get; set; } = new Dictionary<int, bool>();
        public Trigger(TriggerData triggerData)
        {
            TemplateId = triggerData.id;
            ActivationItems = triggerData.keyItems;
            if (triggerData.targetInteraction != null)
            {
                foreach (var condition in triggerData.targetInteraction)
                {
                    Conditions.Add(condition, false);
                }
            }
            IsActivated = false;
        }
        public override void OnInteraction()
        {
            if (IsActivated)
            {
                Deactivate();
            }
            else
            {
                Activate();
            }
        }
        public void Activate()
        {
            if (Room == null)
            {
                return;
            }
            var keys = new List<int>(Conditions.Keys);
            foreach (var key in keys)
            {
                Conditions[key] = true;
            }
            IsActivated = true;            
        }

        public void Deactivate()
        {
            if (Room == null)
            {
                return;
            }
            var keys = new List<int>(Conditions.Keys);
            foreach (var key in keys)
            {
                Conditions[key] = false;
            }
            IsActivated = false;            
        }
    }
}