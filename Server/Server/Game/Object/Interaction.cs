using Google.Protobuf.Protocol;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class Interaction : GameObject
    {
        public Data.SkillData Data { get; set; }
        public Interaction()
        {
            ObjectType = GameObjectType.Interaction;
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
