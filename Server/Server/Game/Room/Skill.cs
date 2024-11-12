
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

namespace Server.Game
{

    public class Skill
    {
        int _invokeDelay = 100;
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
                    CombatAsync(data, skill.range);
                    return;
                case SkillLogicType.Pull:

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
        public async void HandleSpotSkill(SkillLogicType skillLogic)
        {
            _skillQueue.TryDequeue(out var skill);
            SkillData data = skill.skillData;
            if (data.skillLogicType != skillLogic)
            {
                return;
            }
            switch (skillLogic)
            {
                case SkillLogicType.Spotattack:
                    List<Vector2Int> skillPos = SkillLogic.GetRandomSpots(Owner, data, Owner.Room);
                    if(skillPos.Count == 0)
                    {
                        return;
                    }
                    await Task.Delay((int)(1000*Owner.TotalInvokeSpeed));
                    SpotAttack(data, skillPos);
                    break;
            }
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

        public async void CombatAsync(SkillData data, int range)
        {
            await Task.Delay((int)(100 / Owner.TotalAttackSpeed));
            Vector2Int center = Owner.GetFrontCellPos();
            for (int i = 0; i < range; i++)
            {
                Vector2Int pos = center + new Vector2Int((Owner.CellPos - center).x*i, (Owner.CellPos - center).y*i);
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
            foreach (Vector2Int pos in skillPos)
            {
                await Task.Delay(_invokeDelay*100);
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
            }
        }

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
    }
}
