using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game.Room;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public partial class Skill
    {
        public async void KnockBack(SkillData data, int range)
        {
            await Task.Delay((int)(1000 * Owner.TotalInvokeSpeed));

            Vector2Int destPos = new Vector2Int();
            List<Vector2Int> targetPos = new List<Vector2Int>();
            if (data.shape.shapeType == ShapeType.ShapeLine)
            {
                targetPos = SkillLogic.GetTargetsInLine(Owner.CellPos, Owner.Info.Position.MoveDir, range);

            }
            else if (data.shape.shapeType == ShapeType.ShapeCircle)
            {
                targetPos = SkillLogic.GetAllTargetsInRange(Owner.CellPos, range);
            }
            //데미지 판정
            foreach (Vector2Int pos in targetPos)
            {
                List<GameObject> targets = Owner.Room.Map.Find(pos);
                if(targets.Count>0)
                {
                    foreach (GameObject target in targets)
                    {
                        if (target != null)
                        {
                            if (target == Owner)
                            {
                                continue;
                            }
                            CalculateDistance(target, () =>
                            {
                                target.OnDamaged(Owner, Owner.TotalAttack + data.damage);
                                ApplyAfterEffect(data, target);
                            }, data.range);
                        }
                        Vector2Int direction = (_target.CellPos - Owner.CellPos).normalized;
                        destPos = new Vector2Int(_target.CellPos.x + direction.x * data.range, _target.CellPos.y + direction.y * data.range);
                        if (Owner.Room.Map.ApplyMove(_target, destPos, collision: false))
                        {
                            _target.CellPos = destPos;
                            S_ChangePosition changePosition = new S_ChangePosition();
                            changePosition.ObjectId = _target.Id;
                            changePosition.Position = _target.PosInfo;
                            Owner.Room.Broadcast(_target.CellPos, changePosition);
                        }
                    }
                }
            }
        }

        public void Pull(SkillData data, int range)
        {
            List<Vector2Int> tiles = SkillLogic.GetAllTargetsInRange(Owner.CellPos, range);
            foreach (Vector2Int tile in tiles)
            {
                if ((tile - Owner.CellPos).cellDistanceFromZero > range)
                {
                    continue;
                }
                List<GameObject> targets = Owner.Room.Map.Find(tile);
                foreach (GameObject target in targets)
                {
                    if (target != null)
                    {
                        if (target == Owner)
                        {
                            continue;
                        }
                        Vector2Int direction = (Owner.CellPos - target.CellPos).normalized;
                        Vector2Int newPos = target.CellPos + direction;
                        if (Owner.Room.Map.ApplyMove(target, newPos))
                        {
                            target.CellPos = newPos;
                            S_ChangePosition movePacket = new S_ChangePosition();
                            movePacket.ObjectId = target.Id;
                            movePacket.Position = target.PosInfo;
                            Owner.Room.Broadcast(target.CellPos, movePacket);
                        }
                    }
                }
            }
        }

        private void Block(SkillData data, GameObject target)
        {
            ApplyBuff(data, target);
        }
    }
}
