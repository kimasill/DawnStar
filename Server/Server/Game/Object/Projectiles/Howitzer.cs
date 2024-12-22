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
        public Vector2Int DestPos { get; set; }
        int _moveRange = 0;
        bool _isComplete = false;

        public override void Update()
        {
            if (_isComplete)
                return;
            if (Data == null || Data.spot == null || Data.projectile == null || Owner == null || Room == null)
                return;

            Vector2Int dir = DestPos - CellPos;
            int dist = dir.cellDistanceFromZero;

            PosInfo.State = CreatureState.Moving;
            if (Room.Map.ApplyMove(this, DestPos))
            {
                CellPos = DestPos;
                S_Move movePacket = new S_Move();
                movePacket.ObjectId = Id;
                movePacket.Position = PosInfo;
                Room.Broadcast(CellPos, movePacket);
            }

            int tick = (int)(1000 / Data.projectile.speed);
            Room.PushAfter(tick * dist, Cast);
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
                GameObject target = Room.Map.Find(pos);
                if (target != null && target != Owner)
                {
                    target.OnDamaged(this, Data.damage + Owner.TotalAttack); // 피격 판정
                    OnHit?.Invoke(target);
                }
            }
            DespawnAnim = true;
            Room.Push(Room.LeaveGame, Id);
            _isComplete = true;
        }

        public override GameObject GetOwner()
        {
            return Owner;
        }
    }
}