
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
    public partial class Skill
    {
        List<GameObject> _targetList = new List<GameObject>();
        GameObject _target = null;
        private List<(SkillData skillData, GameObject target, int range)> _skillList = new List<(SkillData, GameObject, int)>();
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
                Player player = user as Player;
                if (skillData.shape != null)
                {
                    range = (int)MathF.Max(skillData.shape.range, (Owner as Player).WeaponRange);
                }
                else range = player.WeaponRange;
                int up = player.Stat.Up - skillData.unchartedPoint;
                if(up < 0)
                {
                    return;
                }
                player.ChangeUp(up);
            }
            else
            {
                if (skillData.shape != null)
                {
                    range = (int)skillData.shape.range;
                }
            }

            if (addRange != 0)
            {
                range += addRange;
            }
            _skillList.Add((skillData, target, range));

            Update();
        }
        public void Update() // Update 메서드 추가
        {
            for (int i = 0; i < _skillList.Count; i++) // List 순회
            {
                var skill = _skillList[i];
                SkillData data = skill.skillData;
                int range = skill.range;
                GameObject target = skill.target;

                switch (data.skillType)
                {
                    case SkillType.SkillAttack:
                        HandleAttackSkill(data, range, target);
                        break;
                    case SkillType.SkillProjectile:
                        HandleProjectileSkill(data);
                        break;
                    case SkillType.SkillSpot:
                        HandleSpotSkill(data, target);
                        break;
                    case SkillType.SkillBuff:
                        HandleBuffSkill(data, Owner);
                        break;
                    case SkillType.SkillDebuff:
                        HandleDebuffSkill(data, target);
                        break;
                    case SkillType.SkillMove:
                        HandleMoveSkill(data);
                        break;
                }
            }
            _skillList.Clear(); // List 초기화
        }
        public bool HandleSkillCool(SkillData skillData, bool attackSpeed = false, float time = 0, bool peek = false)
        {
            bool coolDown = true;
            long currentTime = Environment.TickCount64;

            if (_skillCooldowns.TryGetValue(skillData.id, out long cooldownEnd))
            {
                if (cooldownEnd > currentTime)
                {
                    coolDown = false;
                    if (time != 0)
                    {
                        _skillCooldowns[skillData.id] += (long)(time * 1000);
                    }
                }
            }
            if (peek)
            {
                return coolDown;
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
        public void HandleAttackSkill(SkillData data, int range, GameObject target)
        {
            switch (data.skillLogicType)
            {
                case SkillLogicType.BasicAttack:
                    BasicAttakAsync(data, range);
                    break;
                case SkillLogicType.KnockBack:
                    KnockBack(data, range);
                    break;
                case SkillLogicType.Combat:
                    CombatAsync(data);
                    break;
                case SkillLogicType.Pull:
                    Pull(data, range);
                    break;
                case SkillLogicType.Summon:
                    SummonAttack(data, range, target);
                    break;
                case SkillLogicType.Kinetic:
                    KineticAttack(data, target);
                    break;
                case SkillLogicType.Invocation:
                    InvokeSkill(data, target);
                    break;
                case SkillLogicType.LoopInvocation:
                    LoopInvocation_Rotate(data, target);
                    break;
                default:
                    BasicAttakAsync(data, range);
                    break;
            }
        }
        public async void HandleProjectileSkill(SkillData data)
        {
            switch (data.skillLogicType)
            {
                case SkillLogicType.MagicBall:
                    await Task.Delay((int)(1000 * Owner.TotalInvokeSpeed));
                    MagicBall(data);

                    return;
                default: return;
            }
        }
        public void HandleSpotSkill(SkillData data, GameObject target = null)
        {
            List<Vector2Int> skillPos = null;
            switch (data.skillLogicType)
            {
                case SkillLogicType.SpotAttack:
                    if (data.spot.isHoming)
                    {
                        skillPos = SkillLogic.GetAllTargetsInRange(Owner.CellPos, data.range);
                    }
                    else
                    {
                        if (data.spot.maxCount - data.spot.minCount > 0)
                        {
                            skillPos = SkillLogic.GetRandomSpots(Owner, data, Owner.Room);
                        }
                        else if (data.spot.maxCount == 1)
                        {
                            skillPos = [target.CellPos];
                        }
                        if (skillPos.Count == 0)
                        {
                            return;
                        }
                    }
                    SpotAttack(data, skillPos);
                    break;
                case SkillLogicType.Curve:
                    ProjectileCurve(data);
                    break;
            }
        }
        public void HandleBuffSkill(SkillData skillData, GameObject target)
        {
            switch (skillData.skillLogicType)
            {
                case SkillLogicType.Block:
                    BlockAsync(skillData);
                    break;
                case SkillLogicType.RealTimeByEnemyNumber:
                    RealTimeByEnemyNum(skillData);
                    break;
                case SkillLogicType.Exchange:
                    BuffExchange(skillData);
                    break;
                default:
                    ApplyBuff(skillData, target);
                    break;
            }
        }
        public void HandleDebuffSkill(SkillData skillData, GameObject target = null)
        {
            switch (skillData.skillLogicType)
            {
                case SkillLogicType.Dot:
                    DOT(skillData, target);
                    break;
                case SkillLogicType.Mark:
                    ApplyDeBuff(skillData, target);
                    break;
                default:
                    break;
            }

            DebuffData debuff = null;
            DataManager.DebuffDict.TryGetValue(skillData.debuff.id, out debuff);
            if (debuff == null)
                return;

            DebuffType debuffType = 0;
            if (debuff.debuffType != 0)
                debuffType = debuff.debuffType;
            else debuffType = DebuffType.Stat;

            switch (debuff.debuffType)
            {
                case DebuffType.Dot:
                    DOT(skillData, target);
                    target.ApplyDebuff(skillData.debuff, suspect:Owner);
                    break;
                case DebuffType.Stat:
                    ApplyDeBuff(skillData, target);
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
        #endregion
    
    }
}
