using Azure;
using Server.Game.Room;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Server.Game
{
    class SummonAttackObj : Magic
    {
        public GameObject Owner { get; set; }
        private int _damage;
        private float _delay;     
        public float Range { get; set; }
        public GameObject Target { get; set; }                
        int _count = 0;
        public Action<GameObject> OnHit { get; set; }
        public float Delay
        {
            get { return _delay; }
            set { _delay = value; }
        }
        private GameRoom _room;

        private int _coolTick = 0;
        public override void  Update()
        {
            if (Data == null || Data.spot == null || Owner == null || Room == null)
                return;

            if (Delay <= 0)
                return;

            int tick = (int)(1000 / Delay);
            Room.PushAfter(tick, Update);

            if (_count == Data.count)
            {
                Room.Push(Room.LeaveGame, Id);
                return;
            }

            Vector2Int summonPos = Target != null ? Target.CellPos : Owner.CellPos;
            List<Vector2Int> attackTiles = SkillLogic.GetAllTargetsInRange(summonPos, (int)Range);
            foreach (Vector2Int pos in attackTiles)
            {
                GameObject target = Owner.Room.Map.Find(pos);
                if (target != null && target != Owner)
                {
                    target.OnDamaged(Owner, Data.damage);
                }
            }                
            _count++;
        }
    }
}
