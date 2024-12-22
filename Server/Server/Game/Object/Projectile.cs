using Google.Protobuf.Protocol;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class Projectile : GameObject
    {
        public Data.SkillData Data { get; set; }

        public Action<GameObject> OnHit { get; set; }
        public Projectile()
        {
            ObjectType = GameObjectType.Projectile;
        }
        public override void Update()
        {
        }
    }
}
