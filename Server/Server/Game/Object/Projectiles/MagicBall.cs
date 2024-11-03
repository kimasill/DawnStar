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
        public override void Update()
        {
            if (Data == null || Data.projectile == null || Owner == null || Room == null)
                return;

            int tick = (int)(1000 / Data.projectile.speed);
            Room.PushAfter(tick, Update);
            if (Data.projectile.isHoming)
            {
                // 유도하여 날아가는 로직
                if (Target != null)
                {
                    Vector2Int direction = (Target.CellPos - CellPos).normalized;
                    Vector2Int destPos = CellPos + direction;

                    if (Room.Map.ApplyMove(this, destPos, collision: false))
                    {
                        CellPos = destPos;
                        S_Move movePacket = new S_Move();
                        movePacket.ObjectId = Id;
                        movePacket.Position = PosInfo;
                        Room.Broadcast(CellPos, movePacket);
                    }
                    else
                    {
                        GameObject target = Room.Map.Find(destPos);
                        if (target != null)
                        {
                            target.OnDamaged(this, Data.damage + Owner.TotalAttack); // 피격 판정
                        }
                        Room.Push(Room.LeaveGame, Id);
                    }
                }
            }
            else
            {
                // 직선으로 날아가는 로직
                Vector2Int destPos = GetFrontCellPos();
                if (Room.Map.ApplyMove(this, destPos, collision: false))
                {
                    CellPos = destPos;
                    S_Move movePacket = new S_Move();
                    movePacket.ObjectId = Id;
                    movePacket.Position = PosInfo;
                    Room.Broadcast(CellPos, movePacket);
                }
                else
                {
                    GameObject target = Room.Map.Find(destPos);
                    if (target != null)
                    {
                        target.OnDamaged(this, Data.damage + Owner.TotalAttack); // 피격 판정
                    }
                    Room.Push(Room.LeaveGame, Id);
                }
            }

        }

        public override GameObject GetOwner()
        {
            return Owner;
        }
    }
}
