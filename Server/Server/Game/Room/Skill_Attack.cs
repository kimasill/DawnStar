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
        public async void BasicAttakAsync(SkillData data, int range, int distance)
        {
            List<Vector2Int> skillPos = new List<Vector2Int>();
            isExecuting = true;
            if (Owner.TotalInvokeSpeed > 0)
                await Task.Delay((int)(Owner.TotalInvokeSpeed * 1000));

            if (distance > 0)
            {
                Vector2Int center = Owner.GetFrontCellPos();
                Vector2Int direction = SkillLogic.GetOffsetByDirection(Owner.LookDir, distance);
                center += direction;
            }

            if (data.shape.shapeType == ShapeType.ShapeBent)
            {
                Vector2Int center = Owner.GetFrontCellPos();
                skillPos.AddRange(SkillLogic.GetBentAttackTiles(center, Owner.Info.Position.LookDir, range));
            }
            else if (data.shape.shapeType == ShapeType.ShapeRect)
            {
                Vector2Int center = Owner.GetFrontCellPos();
                skillPos.AddRange(SkillLogic.GetRectAttackTiles(center, Owner.Info.Position.MoveDir, range));
            }
            else if (data.shape.shapeType == ShapeType.ShapeCircle)
            {
                Vector2Int center = Owner.GetFrontCellPos();
                skillPos.AddRange(SkillLogic.GetAllTargetsInRange(center, range));
            }
            else if (data.shape.shapeType == ShapeType.ShapeLine)
            {
                skillPos.AddRange(SkillLogic.GetTargetsInLine(Owner.CellPos, Owner.Info.Position.MoveDir, range));
            }
            foreach (Vector2Int pos in skillPos)
            {
                List<GameObject> targets = new List<GameObject>(Owner.Room.Map.Find(pos));
                if (targets.Count > 0)
                {
                    foreach (GameObject target in targets)
                    {
                        if (target == Owner)
                        {
                            continue;
                        }
                        if (_target != null)
                        {
                            if (target != _target)
                                continue;
                        }
                        CalculateDistance(target, () =>
                        {
                            target.OnDamaged(Owner, Owner.TotalAttack + data.damage);
                            ApplyAfterEffect(data, target);
                        }, range);
                    }
                }
            }
            isExecuting = false;
        }
        public async void CombatAsync(SkillData data)
        {
            for (int i = 0; i < data.count; i++)
            {
                if (data.terms != null && data.terms.Count > 0)
                {
                    if (i >= 1)
                    {
                        int delay = (int)((data.terms[i] - data.terms[i - 1]) / Owner.TotalAttackSpeed * 1000);
                        if (delay < 0) delay = 0; // Ensure delay is non-negative
                        await Task.Delay(delay);
                    }
                    else if (i == 0)
                    {
                        int delay = (int)(data.terms[i] / Owner.TotalAttackSpeed * 1000);
                        if (delay < 0)
                        {
                            delay = 0; // or handle the error appropriately
                        }
                        await Task.Delay(delay);
                    }
                }
                else if (data.term != 0)
                {
                    await Task.Delay((int)(data.term / Owner.TotalAttackSpeed * 1000));
                }
                List<Vector2Int> targetPos = SkillLogic.GetTargetsInLine(Owner.CellPos, Owner.Info.Position.MoveDir, (int)data.shape.range);
                foreach (var pos in targetPos)
                {
                    List<GameObject> targets = new List<GameObject>(Owner.Room.Map.Find(pos));
                    if (targets.Count > 0)
                    {
                        foreach (GameObject target in targets)
                        {
                            if (target == Owner)
                            {
                                continue;
                            }
                            target.OnDamaged(Owner, Owner.TotalAttack + data.damage);
                        }
                    }
                }
            }
        }
        private async void SummonAttack(SkillData data, int range, GameObject target = null)
        {
            await Task.Delay((int)(1000 * Owner.TotalInvokeSpeed));
            SummonAttackObj summon = ObjectManager.Instance.Add<SummonAttackObj>();
            summon.Owner = Owner;
            summon.Target = _target;
            summon.Owner.Room = Owner.Room;
            summon.DespawnAnim = data.afterEffect;
            summon.Data = data;
            summon.Range = range;
            summon.PosInfo.State = CreatureState.Moving;
            summon.TemplateId = data.id;
            if (target != null)
            {
                summon.PosInfo.MoveDir = target.PosInfo.MoveDir;
                summon.PosInfo.PosX = target.PosInfo.PosX;
                summon.PosInfo.PosY = target.PosInfo.PosY;
            }
            else
            {
                summon.PosInfo.MoveDir = Owner.PosInfo.MoveDir;
                summon.PosInfo.PosX = Owner.PosInfo.PosX;
                summon.PosInfo.PosY = Owner.PosInfo.PosY;
            }
            if (data.debuff != null)
            {
                summon.OnHit = (target) => { HandleDebuffSkill(data, target); };
            }

            Owner.Room.Push(Owner.Room.EnterGame, summon, false);
        }
        private async void KineticAttack(SkillData data, GameObject target = null)
        {
            for (int i = 0; i < data.count; i++)
            {
                await Task.Delay((int)(1000 * data.terms[i]));
                MoveSkill(data, target);

                int dist = (Owner.CellPos - target.CellPos).cellDistanceFromZero;
                if (target != null && dist < data.range)
                {
                    S_Effect effectPacket = new S_Effect();
                    effectPacket.ObjectId = target.Id;
                    effectPacket.Prefab = $"{data.prefabs[i * 2 + 1]}";
                    Owner.Room.Broadcast(Owner.CellPos, effectPacket);
                    target.OnDamaged(Owner, data.damage + Owner.TotalAttack);
                }
            }
        }
        private async void InvokeSkill(SkillData data, GameObject target)
        {
            List<Vector2Int> skillPos = new List<Vector2Int>();
            switch (data.shape.shapeType)
            {
                case ShapeType.ShapeLine:
                    skillPos = SkillLogic.GetTargetsInLine(Owner.CellPos, Owner.Info.Position.MoveDir, data.range);
                    break;
                case ShapeType.ShapeRect:
                    break;
                case ShapeType.ShapeBent:
                    break;
                case ShapeType.ShapeCircle:
                    break;
            }
            // invokespeed 이후에 스킬 동작
            await Task.Delay((int)(1000 * Owner.TotalInvokeSpeed));
            for (int i = 0; i < data.count; i++)
            {
                if (data.terms != null && data.terms.Count > 0)
                {
                    if (i >= 1)
                    {
                        await Task.Delay((int)((data.terms[i] - data.terms[i - 1]) * 1000));
                    }
                    else if (i == 0)
                    {
                        await Task.Delay((int)(data.terms[i] * 1000));
                    }
                }
                else if (data.term != 0)
                {
                    await Task.Delay((int)(data.term * 1000));
                }
                foreach (Vector2Int pos in skillPos)
                {
                    List<GameObject> targets = new List<GameObject>(Owner.Room.Map.Find(pos));
                    if (targets.Count > 0)
                    {
                        foreach (GameObject newTarget in targets)
                        {
                            if (newTarget == Owner)
                            {
                                continue;
                            }
                            newTarget.OnDamaged(Owner, Owner.TotalAttack + data.damage);
                        }
                    }
                }
            }
        }
        private async void LoopInvocation_Rotate(SkillData data, GameObject target)
        {
            float angle = 0;
            if (Owner.PosInfo.MoveDir == MoveDir.Up)
            {
                angle = 0;
            }
            else if (Owner.PosInfo.MoveDir == MoveDir.Right)
            {
                angle = 90;
            }
            else if (Owner.PosInfo.MoveDir == MoveDir.Down)
            {
                angle = 180;
            }
            else if (Owner.PosInfo.MoveDir == MoveDir.Left)
            {
                angle = 270;
            }

            float angleIncrement = 360f / (data.duration / data.tickInterval);
            while (data.duration > 0)
            {
                List<Vector2Int> skillPos = new List<Vector2Int>();

                if (data.shape.shapeType == ShapeType.ShapeLine)
                {
                    skillPos = SkillLogic.GetTargetsInLaser(Owner.CellPos, angle, data.range);
                }
                else
                {
                    // 다른 shapeType에 대한 로직 추가
                }

                foreach (Vector2Int pos in skillPos)
                {
                    List<GameObject> targets = new List<GameObject>(Owner.Room.Map.Find(pos));
                    if (targets.Count > 0)
                    {
                        foreach (GameObject newTarget in targets)
                        {
                            if (newTarget == Owner)
                            {
                                continue;
                            }
                            newTarget.OnDamaged(Owner, Owner.TotalAttack + data.damage);
                        }
                    }
                }
                angle += angleIncrement;
                data.duration -= data.tickInterval;
                await Task.Delay((int)(data.tickInterval * 1000));
            }
        }
    }
}
