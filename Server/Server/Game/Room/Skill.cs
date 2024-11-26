
using Server.Data;
using Server.Game;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;
using Server.Game.Room;
using static System.Net.Mime.MediaTypeNames;
using System.Numerics;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace Server.Game
{

    public class Skill
    {
        List<GameObject> _targetList = new List<GameObject>();
        GameObject _target = null;
        private Queue<(SkillData skillData, GameObject target, int range)> _skillQueue = new Queue<(SkillData, GameObject, int)>();
        Dictionary<int, long> _skillCooldowns = new Dictionary<int, long>();
        public GameObject Owner { get; set; }
        public Skill(GameObject owner)
        {
            Owner = owner;
        }
        public void StartSkill(GameObject user, SkillData skillData, GameObject target = null, int addRange = 0)
        {            
            if (user == null || skillData == null)
                return;
            if (target != null)
            {
                _target = target;
                GameObject exTarget = _targetList.Select(x => x == target) as GameObject;
                if (exTarget != null)
                {
                    _targetList.Remove(exTarget);
                }
                _targetList.Add(target);
            }

            int range = 0;
            if (user is Player)
            {
                if (skillData.shape != null)
                {
                    range = (int)MathF.Max(skillData.shape.range, (Owner as Player).WeaponRange);
                }
                else range = (Owner as Player).WeaponRange;                
            }
            else
            {
                if (skillData.shape != null)
                {
                    range = (int)skillData.shape.range;
                }
            } 
                
            if(addRange != 0)
            {
                range += addRange;
            }
            _skillQueue.Enqueue((skillData, target, range));
            switch (skillData.skillType)
            {
                case SkillType.SkillAttack:
                    HandleAttackSkill(skillData.skillLogicType);
                    break;
                case SkillType.SkillProjectile:
                    HandleProjectileSkill(skillData.skillLogicType);
                    break;
                case SkillType.SkillSpot:
                    HandleSpotSkill(skillData.skillLogicType);
                    break;
                case SkillType.SkillBuff:
                    HandleBuffSkill(skillData);
                    break;
                case SkillType.SkillDebuff:
                    HandleDebuffSkill(skillData);
                    break;
                case SkillType.SkillMove:
                    HandleMoveSkill(skillData);
                    break;
            }
        }
        public bool HandleSkillCool(SkillData skillData, bool attackSpeed = false, float time = 0)
        {
            bool coolDown = true;
            long currentTime = Environment.TickCount64;

            if (_skillCooldowns.TryGetValue(skillData.id, out long cooldownEnd))
            {
                if (cooldownEnd > currentTime)
                {
                    // 쿨타임이 끝나지 않았음
                    coolDown = false;
                    if (time != 0)
                    {
                        _skillCooldowns[skillData.id] += (long)(time * 1000);
                    }
                }
            }

            if (coolDown)
            {
                if (attackSpeed)
                {
                    _skillCooldowns[skillData.id] = currentTime + (long)(1000 / Owner.TotalAttackSpeed);
                }
                else
                {
                    _skillCooldowns[skillData.id] = currentTime + (long)(skillData.coolTime * 1000);
                }
            }

            if ((coolDown == false && time != 0) || coolDown)
            {
                if (Owner is Player)
                {
                    int remainingCoolTime = (int)(_skillCooldowns[skillData.id] - currentTime);
                    (Owner as Player).Session.Send(new S_SkillCool()
                    {
                        SkillId = skillData.id,
                        CoolTime = remainingCoolTime > 0 ? remainingCoolTime : 0
                    });
                }
            }

            return coolDown;
        }
        #region LogicDivide
        public void HandleAttackSkill(SkillLogicType skillLogic)
        {
            _skillQueue.TryDequeue(out var skill);
            SkillData data = skill.skillData;
            if (data.skillLogicType != skillLogic)
            {
                return;
            }
            switch (skillLogic)
            {
                case SkillLogicType.Basicattack:
                    BasicAttakAsync(data, skill.range);
                    return;
                case SkillLogicType.Knockback:
                    KnockBack(data);
                    return;
                case SkillLogicType.Combat:
                    CombatAsync(data);
                    return;
                case SkillLogicType.Pull:
                    Pull(data, skill.range);
                    return;
                case SkillLogicType.Summon:
                    SummonAttack(data, skill.range, skill.target);
                    return;
                case SkillLogicType.Kinetic:
                    KineticAttack(data , skill.range, skill.target);
                    return;
                case SkillLogicType.Invocation:
                    InvokeSkill(data, skill.target);
                    return;
                default: return;
            }
        }
        public async void HandleProjectileSkill(SkillLogicType skillLogic)
        {
            _skillQueue.TryDequeue(out var skill);
            SkillData data = skill.skillData;
            if(data.skillLogicType != skillLogic)
            {
                return;
            }

            switch (skillLogic) 
            {
                case SkillLogicType.Magicball:
                    await Task.Delay((int)(1000*Owner.TotalInvokeSpeed));
                    MagicBall(data);
                    
                    return;
                default: return;
            }            
        }
        public void HandleSpotSkill(SkillLogicType skillLogic)
        {
            _skillQueue.TryDequeue(out var skill);
            SkillData data = skill.skillData;
            List<Vector2Int> skillPos = null;
            if (data.skillLogicType != skillLogic)
            {
                return;
            }
            switch (skillLogic)
            {
                case SkillLogicType.Spotattack:
                    if (data.spot.maxCount - data.spot.minCount > 0)
                    {
                        skillPos = SkillLogic.GetRandomSpots(Owner, data, Owner.Room);
                    }
                    else if(data.spot.maxCount == 1)
                    {
                        skillPos = [skill.target.CellPos];
                    }
                    if (skillPos.Count == 0)
                    {
                        return;
                    }                    
                    SpotAttack(data, skillPos);
                    break;
            }
        }
        public void HandleBuffSkill(SkillData skillData)
        {
            switch (skillData.skillLogicType)
            {
                case SkillLogicType.Block:
                    
                    BlockAsync(skillData);
                    break;
                default:
                    
                    ApplyBuff(skillData);
                    break;
            }
        }
        public void HandleDebuffSkill(SkillData skillData, GameObject target = null)
        {
            switch(skillData.skillLogicType)
            {
                case SkillLogicType.Dot:
                    DOT(skillData, target);
                    break;
                default:
                    ApplyDeBuff(skillData);
                    break;
            }
        }
        public void HandleMoveSkill(SkillData skillData, GameObject target = null)
        {
            switch (skillData.skillLogicType)
            {
                case SkillLogicType.Kinetic:
                    //KeneticMoveSkill(skillData, target);
                    break;
                case SkillLogicType.Invocation:
                    MoveSkill(skillData, target);
                    break;
            }
        }
        private async void ApplyBuff(SkillData data)
        {
            await Task.Delay((int)(1000 * data.term));
            // Buff 적용 로직
            if (data.buff != null)
            {
                // Buff 시작 시 로직
                Console.WriteLine($"Buff {data.buff.name} applied with value {data.buff.value}");
                Owner.ApplyBuff(data.buff);

                // Buff 종료 시 로직
                await Task.Delay(data.buff.duration * 1000);
                Owner.RemoveBuff(data.buff);
                Console.WriteLine($"Buff {data.buff.name} ended");
            }
            else if (data.buffList != null)
            {
                foreach (var buff in data.buffList)
                {
                    // Buff 시작 시 로직
                    Console.WriteLine($"Buff {buff.name} applied with value {buff.value}");
                    Owner.ApplyBuff(buff);

                    // Buff 종료 시 로직
                    await Task.Delay(buff.duration * 1000);
                    Owner.RemoveBuff(buff);
                    Console.WriteLine($"Buff {buff.name} ended");
                }
            }
        }

        private async void ApplyDeBuff(SkillData data)
        {
            await Task.Delay((int)(1000 * data.term));
            if (data.debuff != null)
            {
                // Debuff 시작 시 로직
                Console.WriteLine($"Debuff {data.debuff.name} applied with value {data.debuff.value}");
                Owner.ApplyDebuff(data.debuff);

                // Debuff 종료 시 로직
                await Task.Delay(data.debuff.duration * 1000);
                Owner.RemoveDebuff(data.debuff);
                Console.WriteLine($"Debuff {data.debuff.name} ended");
            }
            else if (data.debuffList != null)
            {
                foreach (var debuff in data.debuffList)
                {
                    // Debuff 시작 시 로직
                    Console.WriteLine($"Debuff {debuff.name} applied with value {debuff.value}");
                    Owner.ApplyDebuff(debuff);

                    // Debuff 종료 시 로직
                    await Task.Delay(debuff.duration * 1000);
                    Owner.RemoveDebuff(debuff);
                    Console.WriteLine($"Debuff {debuff.name} ended");
                }
            }
        }
        #endregion
        #region SkillLogic
        private async void DOT(SkillData data, GameObject target)
        {
            if (data.debuff == null || target == null)
                return;

            int tickInterval = 1000; // 1초 간격
            int totalTicks = data.debuff.duration;

            for (int i = 0; i < totalTicks; i++)
            {
                if (target == null)
                    break;
                target.OnDamaged(Owner, (int)(Owner.TotalAttack*data.debuff.value));

                await Task.Delay(tickInterval);
            }

            Console.WriteLine($"Debuff {data.debuff.name} ended");
        }
        public void MagicBall(SkillData data)
        {
            MagicBall magicBall = ObjectManager.Instance.Add<MagicBall>();
            magicBall.Owner = Owner;
            magicBall.Target = _target;
            magicBall.Owner.Room = Owner.Room;
            magicBall.Data = data;
            magicBall.PosInfo.State = CreatureState.Moving;
            magicBall.PosInfo.MoveDir = Owner.PosInfo.MoveDir;
            magicBall.PosInfo.PosX = Owner.PosInfo.PosX;
            magicBall.PosInfo.PosY = Owner.PosInfo.PosY;
            magicBall.Speed = data.projectile.speed;
            magicBall.DespawnAnim = true;
            magicBall.TemplateId = data.id;
            if (data.debuff != null)
            {
                magicBall.OnHit = (target) => { HandleDebuffSkill(data, target); };
            }
            Owner.Room.Push(Owner.Room.EnterGame, magicBall, false);
        }
        public async void BasicAttakAsync(SkillData data, int range)
        {
            List<Vector2Int> skillPos = new List<Vector2Int>();
            if(Owner.TotalInvokeSpeed > 0)
                await Task.Delay((int)(Owner.TotalInvokeSpeed*1000));
            if (data.shape.shapeType == ShapeType.ShapeBent)
            {
                Vector2Int center = Owner.GetFrontCellPos();   
                skillPos.AddRange(SkillLogic.GetBentAttackTiles(center, Owner.Info.Position.LookDir, range));
                
            }
            else if(data.shape.shapeType == ShapeType.ShapeRect)
            {
                Vector2Int center = Owner.GetFrontCellPos();
                skillPos.AddRange(SkillLogic.GetRectAttackTiles(center, Owner.Info.Position.MoveDir, range));
            }

            foreach (Vector2Int pos in skillPos)
            {
                GameObject target = Owner.Room.Map.Find(pos);
                if (target != null)
                {
                    if (target == Owner)
                    {
                        continue;
                    }
                    if(_target !=null)
                    {
                        if(target != _target)
                            continue;
                    }
                    CalculateDistance(target, () => { target.OnDamaged(Owner, Owner.TotalAttack + data.damage);}, range);
                }
            }
        }
        public void KnockBack(SkillData data)
        {
            //데미지 판정
            _target.OnDamaged(Owner, data.damage + Owner.TotalAttack);

            // 적을 2칸 밀어냄
            Vector2Int direction = (_target.CellPos - Owner.CellPos).normalized;
            Vector2Int destPos = new Vector2Int(_target.CellPos.x + direction.x * 2, _target.CellPos.y + direction.y * 2);

            if (Owner.Room.Map.ApplyMove(_target, destPos, collision: false))
            {
                _target.CellPos = destPos;
                S_ChangePosition changePosition = new S_ChangePosition();
                changePosition.ObjectId = _target.Id;
                changePosition.Position = _target.PosInfo;
                Owner.Room.Broadcast(_target.CellPos, changePosition);
            }
        }
        public async void CombatAsync(SkillData data)
        {
            for(int i = 0; i< data.count; i++)
            {
                if(data.terms!=null && data.terms.Count > 0)
                {
                    if(i >= 1)
                    {
                        await Task.Delay((int)((data.terms[i] - data.terms[i-1])/Owner.TotalAttackSpeed * 1000));
                    }
                    else if(i == 0)
                    {
                        await Task.Delay((int)(data.terms[i]/Owner.TotalAttackSpeed * 1000));
                    }                    
                }
                else if(data.term != 0)
                {
                    await Task.Delay((int)(data.term / Owner.TotalAttackSpeed * 1000));
                }
                List<Vector2Int> targets = SkillLogic.GetTargetsInLine(Owner.CellPos, Owner.Info.Position.MoveDir, (int)data.shape.range);
                foreach(var pos in targets)
                {                    
                    GameObject target = Owner.Room.Map.Find(pos);
                    if (target != null)
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
        public void Pull(SkillData data, int range)
        {
            List<Vector2Int> tiles = SkillLogic.GetAllTargetsInRange(Owner.CellPos, range);
            foreach (Vector2Int tile in tiles)
            {
                if((tile - Owner.CellPos).cellDistanceFromZero > range)
                {
                    continue;
                }
                GameObject target = Owner.Room.Map.Find(tile);
                if (target != null)
                {
                    if (target == Owner)
                        continue;

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
        private async void SpotAttack(SkillData data, List<Vector2Int> skillPos)
        {
            await Task.Delay((int)(1000 * Owner.TotalInvokeSpeed));
            foreach (Vector2Int pos in skillPos)
            {
                if (Owner == null || Owner.Room ==null)
                    return;
                SpotAttack spot = ObjectManager.Instance.Add<SpotAttack>();
                spot.Owner = Owner;
                spot.Owner.Room = Owner.Room;
                spot.Data = data;
                spot.PosInfo.PosX = pos.x;
                spot.PosInfo.PosY = pos.y;
                spot.PosInfo.MoveDir = MoveDir.Down;
                spot.PosInfo.State = CreatureState.Moving;
                spot.Delay = data.spot.delay;
                spot.TemplateId = data.id;
                Owner.Room.Push(Owner.Room.EnterGame, spot, false);

                if(data.term != 0)
                    await Task.Delay((int)(data.term * 1000));
            }
        }
        private async void BlockAsync(SkillData data) 
        {
            await Task.Delay(100);
            float prevReduce = Owner.TotalDamageReduce;
            Owner.TotalDamageReduce = data.buff.value;
            await Task.Delay(data.buff.duration*1000);
            Owner.TotalDamageReduce = prevReduce;
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
                        await Task.Delay((int)((data.terms[i] - data.terms[i - 1]) / Owner.TotalAttackSpeed * 1000));
                    }
                    else if (i == 0)
                    {
                        await Task.Delay((int)((data.terms[i] / Owner.TotalAttackSpeed) * 1000));
                    }
                }
                else if (data.term != 0)
                {
                    await Task.Delay((int)((data.term / Owner.TotalAttackSpeed) * 1000));
                }
                foreach (Vector2Int pos in skillPos)
                {
                    GameObject newTarget = Owner.Room.Map.Find(pos);
                    if (newTarget != null)
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
        private void MoveSkill(SkillData data, GameObject target = null)
        {
            if (data.shape == null)
                return;
            
            Vector2Int dir = GetDir(data.shape.direction);
            Vector2Int destPos = new Vector2Int();
            if (target != null)
            {
                if (data.shape.shapeType == ShapeType.ShapeLine)
                {
                    destPos = target.CellPos - dir;
                }
            }
            else
            {
                if (data.shape.shapeType == ShapeType.ShapeLine)
                {
                    destPos = Owner.CellPos + new Vector2Int((int)(dir.x * data.shape.range), (int)(dir.y * data.shape.range));

                    if (!Owner.Room.Map.CanGo(destPos))
                    {
                        Vector2Int currentPos = Owner.CellPos;
                        while (!Owner.Room.Map.CanGo(currentPos))
                        {
                            currentPos -= dir;
                        }
                        destPos = currentPos;
                    }
                }
            }
            if (Owner.Room.Map.ApplyMove(Owner, destPos))
            {
                Owner.CellPos = destPos;
                S_ChangePosition movePacket = new S_ChangePosition();
                movePacket.ObjectId = Owner.Id;
                movePacket.Position = Owner.PosInfo;
                Owner.Room.Broadcast(Owner.CellPos, movePacket);
            }
        }
        private Vector2Int GetDir(DirectionType type)
        {
            Vector2Int dir = new Vector2Int();
            switch (type)
            {
                case DirectionType.DirectionNone:
                    dir = new Vector2Int(0, 0);
                    break;
                case DirectionType.DirectionUp:
                    dir = new Vector2Int(0, 1);
                    break;
                case DirectionType.DirectionDown:
                    dir = new Vector2Int(0, -1);
                    break;
                case DirectionType.DirectionLeft:
                    dir = new Vector2Int(-1, 0);
                    break;
                case DirectionType.DirectionRight:
                    dir = new Vector2Int(1, 0);
                    break;
                case DirectionType.DirectionBack:
                    if (Owner.Dir == MoveDir.Up)
                    {
                        dir = new Vector2Int(0, -1);
                    }
                    else if (Owner.Dir == MoveDir.Down)
                    {
                        dir = new Vector2Int(0, 1);
                    }
                    else if (Owner.Dir == MoveDir.Left)
                    {
                        dir = new Vector2Int(1, 0);
                    }
                    else if (Owner.Dir == MoveDir.Right)
                    {
                        dir = new Vector2Int(-1, 0);
                    }
                    break;
                case DirectionType.DirectionFront:
                    if (Owner.Dir == MoveDir.Up)
                    {
                        dir = new Vector2Int(0, 1);
                    }
                    else if (Owner.Dir == MoveDir.Down)
                    {
                        dir = new Vector2Int(0, -1);
                    }
                    else if (Owner.Dir == MoveDir.Left)
                    {
                        dir = new Vector2Int(-1, 0);
                    }
                    else if (Owner.Dir == MoveDir.Right)
                    {
                        dir = new Vector2Int(1, 0);
                    }
                    break;
            }
            return dir;
        }
        private async void SummonAttack(SkillData data, int range, GameObject target = null)
        {
            await Task.Delay((int)(1000 * Owner.TotalInvokeSpeed));
            SummonAttackObj summon = ObjectManager.Instance.Add<SummonAttackObj>();
            summon.Owner = Owner;
            summon.Target = _target;
            summon.Owner.Room = Owner.Room;
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
        private async void KineticAttack(SkillData data, int range, GameObject target = null)
        {
            for(int i = 0; i < data.count; i++)
            {
                await Task.Delay((int)(1000 * data.terms[i]));
                MoveSkill(data, target);

                int dist = (Owner.CellPos - target.CellPos).cellDistanceFromZero;
                if (target != null && dist < range)
                {
                    S_Effect effectPacket = new S_Effect();
                    effectPacket.ObjectId = target.Id;
                    effectPacket.Prefab = $"{data.prefabs[i*2 + 1]}";                    
                    Owner.Room.Broadcast(Owner.CellPos, effectPacket);
                    target.OnDamaged(Owner, data.damage + Owner.TotalAttack);
                }
            }
        }
        #endregion
        #region Calculate
        private void CalculateDistance(GameObject target, Action action, int range)
        {
            if (target != null)
            {
                float distance = (Owner.CellPos - target.CellPos).magnitude;

                if (distance <= range)
                {
                    action();
                }
            }
        }
        #endregion
    }
}
