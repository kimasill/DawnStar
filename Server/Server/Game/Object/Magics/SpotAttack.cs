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

            Cast();
        }

        private async void Cast()
        {
            if(Delay > 0)
                await Task.Delay((int)Delay * 1000);
            
            Vector2Int destPos = CellPos;
            List<Vector2Int> targetPositions = new List<Vector2Int>();

            if (Data.shape != null)
            {
                targetPositions = SkillLogic.GetAllTargetsInRange(destPos, (int)Data.shape.range);
            }
            else
            {
                targetPositions.Add(destPos);
            }

            foreach (var pos in targetPositions)
            {
                List<GameObject> targets = new List<GameObject>(Room.Map.Find(pos));
                if(targets.Count > 0)
                {
                    foreach (GameObject target in targets)
                    {
                        if (target == Owner)
                            continue;
                        if (target != null && target != Owner)
                        {
                            target.OnDamaged(this, Data.damage + Owner.TotalAttack); //피격판정                    
                        }
                    }
                }
            }
            Room.PushAfter(1000, Room.LeaveGame, Id);
        }
    }
}