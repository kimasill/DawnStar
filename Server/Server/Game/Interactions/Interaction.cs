using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game.Room;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class Interaction : GameObject
    {
        public SkillData Data { get; set; }
        public List<Vector2Int> Cells { get; set; } = new List<Vector2Int>();
        public Interaction(InteractionData data)
        {
            ObjectType = GameObjectType.Interaction;
        }
        public static Interaction CreateInteraction(InteractionData data)
        {
            if (data == null)
            {
                return null;
            }
            Interaction interaction = null;
            switch (data.interactionType)
            {
                case InteractionType.None:
                    break;
                case InteractionType.Door:
                    interaction = new Door((DoorData)data);
                    break;
                case InteractionType.Trigger:
                    interaction = new Trigger((TriggerData)data);
                    break;
                case InteractionType.ItemTable:
                    return null;
                default:
                    return null;
            }
            interaction.ObjectType = GameObjectType.Interaction;
            interaction.Id = EntityRegistry.Instance.GenerateId(GameObjectType.Interaction);
            return interaction;
        }
        public override void Update()
        {
        }
        public virtual void OnTriggerEnter(int id)
        {
        }
        public virtual void OnInteraction()
        {
        }
    }
}
