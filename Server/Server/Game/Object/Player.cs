using Google.Protobuf.Protocol;
using Server.Game.Room;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class Player: GameObject
    {
       
        public ClientSession Session { get; set; }

        public Player()
        {
            ObjectType = GameObjectType.Player;
            Speed = 10.0f;
        }

        public override void OnDamaged(GameObject target, int damage)
        {
            base.OnDamaged(target, damage);
        }

        public override void Ondead(GameObject attacker)
        {
            base.Ondead(attacker);
        }
    }
}
