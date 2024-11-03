
using Server.Data;
using Server.Game.Room;
using Server.Game;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;

namespace Server.Game
{

    public class Skill
    {
        int _invokeDelay = 100;
        List<Vector2Int> _skillPos = new List<Vector2Int>();
        List<GameObject> _targetList = new List<GameObject>();
        GameObject _target = null;
        SkillData _data = null;
        Dictionary<int, long> _skillCooldowns = new Dictionary<int, long>();
        public GameObject Owner { get; set; }
        public GameRoom Room { get; set; }

        public void StartSkill(GameObject user, SkillData skillData, GameObject target = null)
        {            
            if (user == null || skillData == null)
                return;
            Room = user.Room;
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

            _data = skillData;
            switch (_data.skillType)
            {
                case SkillType.SkillAttack:
                    HandleAttackSkill();
                    break;
                case SkillType.SkillProjectile:
                    HandleProjectileSkill();
                    break;
                case SkillType.SkillSpot:
                    HandleSpotSkill();
                    break;
            }
        }
        public bool HandleSkillCool(SkillData skillData, bool attackSpeed)
        {
            if (_skillCooldowns.TryGetValue(skillData.id, out long cooldownEnd))
            {
                if (cooldownEnd > Environment.TickCount64)
                {
                    // 쿨타임이 끝나지 않았음
                    return false;
                }
            }
            if (attackSpeed)
            {
                _skillCooldowns[skillData.id] = (long)(Environment.TickCount64 + 1000 / Owner.TotalAttackSpeed);
            }
            else
            {
                _skillCooldowns[skillData.id] = (long)(Environment.TickCount64 + skillData.coolTime);
            }
            return true;
        }

        public void HandleAttackSkill()
        {
            switch (_data.skillLogicType)
            {
                case SkillLogicType.Basicattack:
                    BasicAttakAsync();
                    return;
                case SkillLogicType.Knockback:
                    KnockBack();
                    return;
                case SkillLogicType.Combat:
                    CombatAsync();
                    return;
                default: return;
            }
        }

        public async void HandleProjectileSkill()
        {
            switch (_data.skillLogicType) 
            {
                case SkillLogicType.Magicball:
                    await Task.Delay((int)(1000*Owner.TotalInvokeSpeed));
                    MagicBall();
                    return;
                default: return;
            }            
        }
        public async void HandleSpotSkill()
        {  
            switch (_data.skillLogicType)
            {
                case SkillLogicType.Spotattack:
                    _skillPos = SkillLogic.GetRandomSpots(Owner, _data, Room);
                    if(_skillPos.Count == 0)
                    {
                        return;
                    }
                    await Task.Delay((int)(1000*Owner.TotalInvokeSpeed));
                    SpotAttack();
                    break;
            }
        }
        public void MagicBall()
        {
            MagicBall magicBall = ObjectManager.Instance.Add<MagicBall>();
            magicBall.Owner = Owner;
            magicBall.Target = _target;
            magicBall.Room = Room;
            magicBall.Data = _data;
            magicBall.PosInfo.State = CreatureState.Moving;
            magicBall.PosInfo.MoveDir = Owner.PosInfo.MoveDir;
            magicBall.PosInfo.PosX = Owner.PosInfo.PosX;
            magicBall.PosInfo.PosY = Owner.PosInfo.PosY;
            magicBall.Speed = _data.projectile.speed;
            magicBall.DespawnAnim = true;
            Room.Push(Room.EnterGame, magicBall, false);
        }
        public async void BasicAttakAsync()
        {
            List<Vector2Int> skillPos = new List<Vector2Int>();
            await Task.Delay((int)(100 / Owner.TotalAttackSpeed));
            if (_data.shape.shapeType == ShapeType.ShapeBent)
            {
                Vector2Int center = Owner.GetFrontCellPos();                
                skillPos.AddRange(SkillLogic.GetBentAttackTiles(center, Owner.Info.Position.LookDir, (int)_data.shape.range));
            }
            foreach (Vector2Int pos in skillPos)
            {
                GameObject target = Room.Map.Find(pos);
                if (target != null)
                {
                    if (target == Owner)
                    {
                        continue;
                    }
                    target.OnDamaged(Owner, Owner.TotalAttack);
                }
            }            
        }
        public void KnockBack()
        {
            //데미지 판정
            _target.OnDamaged(Owner, _data.damage + Owner.TotalAttack);

            // 적을 2칸 밀어냄
            Vector2Int direction = (_target.CellPos - Owner.CellPos).normalized;
            Vector2Int destPos = new Vector2Int(_target.CellPos.x + direction.x * 2, _target.CellPos.y + direction.y * 2);

            if (Room.Map.ApplyMove(_target, destPos, collision: false))
            {
                _target.CellPos = destPos;
                S_ChangePosition changePosition = new S_ChangePosition();
                changePosition.ObjectId = _target.Id;
                changePosition.Position = _target.PosInfo;
                Room.Broadcast(_target.CellPos, changePosition);
            }
        }

        public async void CombatAsync()
        {
            await Task.Delay((int)(100 / Owner.TotalAttackSpeed));
            Vector2Int center = Owner.GetFrontCellPos();
            for (int i = 0; i < _data.shape.range; i++)
            {
                Vector2Int pos = center + new Vector2Int((Owner.CellPos - center).x*i, (Owner.CellPos - center).y*i);
                GameObject target = Room.Map.Find(pos);
                if (target != null)
                {
                    if (target == Owner)
                    {
                        continue;
                    }
                    target.OnDamaged(Owner, Owner.TotalAttack);
                }
            }
        }
        private void SpotAttack()
        {
            int invokeDelay = 0;
            foreach (Vector2Int pos in _skillPos)
            {
                SpotAttack spot = ObjectManager.Instance.Add<SpotAttack>();
                spot.Data = _data;
                spot.Owner = Owner;
                spot.Room = Owner.Room;
                spot.PosInfo.PosX = pos.x;
                spot.PosInfo.PosY = pos.y;
                spot.Delay = _data.spot.delay;
                invokeDelay += _invokeDelay;
                Room.PushAfter(invokeDelay, Room.EnterGame, spot, false);
            }
        }
    }
}
