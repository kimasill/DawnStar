using Google.Protobuf.Protocol;
using Server.Game.Room;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class MagicBall : Projectile
    {
        public GameObject Owner { get; set; }
        public GameObject Target { get; set; }
        public Action<GameObject> OnHit { get; set; }
        int _moveRange = 0;
        bool _isComplete = false;

        public override void Update()
        {
            if (_isComplete)
                return;
            if (Data == null || Data.projectile == null || Owner == null || Room == null)
                return;

            int tick = (int)(1000 / Data.projectile.speed);
            Room.PushAfter(tick, Update);

            if (Data.projectile.isHoming && Target != null)
            {
                Vector2Int direction = (Target.CellPos - CellPos).normalized;
                Vector2Int destPos = CellPos + direction;

                if (Room.Map.ApplyMove(this, destPos, false))
                {
                    CellPos = destPos;
                    S_Move movePacket = new S_Move();
                    movePacket.ObjectId = Id;
                    movePacket.Position = PosInfo;
                    Room.Broadcast(CellPos, movePacket);
                    _moveRange += 1;
                }
                else
                {
                    GameObject target = Room.Map.Find(destPos);
                    if (target != null && target != Owner)
                    {
                        target.OnDamaged(this, Data.damage + Owner.TotalAttack); // 피격 판정
                        OnHit?.Invoke(target);
                    }
                    DespawnAnim = true;
                    Room.Push(Room.LeaveGame, Id);
                    _isComplete = true;
                } 
            }
            else
            {
                // 직선으로 날아가는 로직
                Vector2Int destPos = GetFrontCellPos();

                if (Room.Map.ApplyMove(this, destPos))
                {
                    CellPos = destPos;
                    S_Move movePacket = new S_Move();
                    movePacket.ObjectId = Id;
                    movePacket.Position = PosInfo;
                    Room.Broadcast(CellPos, movePacket);
                    _moveRange += 1;
                }
                else
                { 
                    GameObject target = Room.Map.Find(destPos);
                    if (target != null && target != Owner)
                    {
                        target.OnDamaged(this, Data.damage + Owner.TotalAttack); // 피격 판정
                        OnHit?.Invoke(target);
                    }
                    DespawnAnim = true;
                    Room.Push(Room.LeaveGame, Id);
                    _isComplete = true;
                }
            }
        }        

        public override GameObject GetOwner()
        {
            return Owner;
        }
    }
}
