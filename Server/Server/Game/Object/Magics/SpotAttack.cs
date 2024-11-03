using Server.Game.Room;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class SpotAttack : Magic
    {
        public GameObject Owner { get; set; }
        private int _damage;
        private float _delay;        
        public float Delay 
        {
            get { return _delay; }
            set { _delay = value; }
        }
        private GameRoom _room;
        public override void Update()
        {
            if (Data == null || Data.spot == null || Owner == null || Room == null)
                return;
            Task.Delay((int)Delay);
            Vector2Int destPos = CellPos;
            GameObject target = Room.Map.Find(destPos);
            if (target != null)
            {
                target.OnDamaged(this, Data.damage + Owner.TotalAttack); //피격판정                    
            }
            Room.Push(Room.LeaveGame, Id);
        }
    }
}