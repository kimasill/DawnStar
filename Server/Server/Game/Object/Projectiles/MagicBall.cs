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
        public Vector2Int DestPos { get; set; }
        int _moveRange = 0;
        bool _isComplete = false;

        public override void Update()
        {
            if (_isComplete)
                return;
            if (Data == null || Data.projectile == null || Owner == null || Room == null)
                return;

            int tick = (int)(1000 / Data.projectile.speed);
            _moveRange += 1;
            Room.PushAfter(tick, Update);            
            if (Data.projectile.isHoming && Target != null)
            {   
                if (_moveRange >= Data.projectile.range)
                {
                    ExplosionDamage();
                    return;
                }
                List<Vector2Int> path = Room.Map.FindPath(CellPos, Target.CellPos);
                if (path.Count < 2)
                {
                    ExplosionDamage();
                    return;
                }
                if (Room.Map.ApplyMove(this, path[1], false))
                {
                    CellPos = path[1];
                    S_Move movePacket = new S_Move();
                    movePacket.ObjectId = Id;
                    movePacket.Position = PosInfo;
                    Room.Broadcast(CellPos, movePacket);                    
                }
                else
                {
                    ExplosionDamage();
                }
            }
            else if (!Data.projectile.isHoming && !Data.projectile.isRandom)
            {
                if (_moveRange >= Data.projectile.range)
                {
                    DespawnAnim = true;
                    Room.Push(Room.LeaveGame, Id);
                    _isComplete = true;
                    return;
                }
                // 직선으로 날아가는 로직
                Vector2Int destPos = GetFrontCellPos();

                if (Room.Map.ApplyMove(this, destPos))
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
            else if (Data.projectile.isRandom)
            {
                if (_moveRange >= Data.projectile.range)
                {
                    ExplosionDamage();
                    return;
                }
                List<Vector2Int> path = Room.Map.FindPath(CellPos, DestPos);
                if (path.Count < 2)
                {
                    ExplosionDamage();
                    return;
                }
                if (Room.Map.ApplyMove(this, path[1], false))
                {
                    CellPos = path[1];
                    S_Move movePacket = new S_Move();
                    movePacket.ObjectId = Id;
                    movePacket.Position = PosInfo;
                    Room.Broadcast(CellPos, movePacket);
                }
                else
                {
                    ExplosionDamage();
                }
            }
        }        
        public void ExplosionDamage()
        {
            List<Vector2Int> targetPositions = SkillLogic.GetAllTargetsInRange(CellPos, (int)Data.shape.range);

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
