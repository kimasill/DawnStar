using Google.Protobuf.Protocol;
using Server.Game.Room;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class Howitzer : Magic
    {
        public GameObject Owner { get; set; }
        GameObject attacker = null;
        public Vector2Int DestPos { get; set; }
        int _moveRange = 0;
        GameRoom room = null;
        public override void Update()
        {
            if(room != null && IsComplete)
            {
                room.Push(room.LeaveGame, Id);
                return;
            }
            if (Data == null || Data.spot == null || Data.projectile == null || Room == null)
                return;

            room = Room;   
            
            Vector2Int dir = DestPos - CellPos;
            int dist = dir.cellDistanceFromZero;

            int tick = (int)(1000 / Data.projectile.speed);
            attacker = Owner;
            room.PushAfter(tick * dist, Cast);
        }
        public void Cast()
        {
            
            List<Vector2Int> targetPositions = new List<Vector2Int>();
            if (Data.shape != null)
            {
                targetPositions = SkillLogic.GetAllTargetsInRange(CellPos, (int)Data.shape.range);
            }
            else
            {
                targetPositions.Add(CellPos);
            }

            foreach (Vector2Int pos in targetPositions)
            {
                if (room == null)
                    return;
                List<GameObject> targets = new List<GameObject>(room.Map.Find(pos));
                if (targets.Count > 0)
                {
                    foreach (GameObject target in targets)
                    {
                        if (target == attacker)
                            continue;
                        if (target != null)
                        {
                            target.OnDamaged(this, Data.damage + attacker.TotalAttack); // 피격 판정
                            OnHit?.Invoke(target);                  
                        }
                    }
                }
            }
            DespawnAnim = true;
            if (room != null)
                room.Push(room.LeaveGame, Id);
            IsComplete = true;
            room.Push(Update);
        }

        public override GameObject GetOwner()
        {
            return Owner;
        }
    }
}