using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class Magic : GameObject
    {
        public Data.SkillData Data { get; set; }

        public Action<GameObject> OnHit { get; set; }
        public bool IsComplete { get; set; }
        public Magic()
        {
            ObjectType = GameObjectType.Magic;
        }
        public override void Update()
        {
            Room.EnqueueAfter(200, Update);
        }
    }
}
