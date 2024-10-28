
using Server.Data;
using Server.Game.Room;
using Server.Game;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using System.Collections.Generic;
using System.Linq;

namespace Server.Game
{

    public class Skill
    {
        int _invokeDelay = 100;
        List<Vector2Int> _skillPos = new List<Vector2Int>();
        List<GameObject> _target = new List<GameObject>();
        public GameObject Owner { get; set; }
        public GameRoom Room { get; set; }

        public async void StartSkill(GameObject user, SkillData skillData, GameObject target = null)
        {
            if (target != null)
                _target.Add(target);
            
            switch (skillData.skillType)
            {
                case SkillType.SkillAttack:
                    HandleAttackSkill(skillData);
                    break;
                case SkillType.SkillProjectile:
                    await HandleProjectileSkill(skillData);
                    break;
                case SkillType.SkillSpot:
                    HandleSpotSkill(skillData);
                    break;
            }
        }

        public GameObject HandleAttackSkill(SkillData skillData)
        {
            switch (skillData.skillLogicType)
            {
                case SkillLogicType.Basicattack:
                    BasicAttak(skillData);
                    return null;
                default: return null;
            }
        }

        public async Task<GameObject> HandleProjectileSkill(SkillData skillData)
        {
            switch (skillData.skillLogicType) 
            {
                case SkillLogicType.Magicball:
                    await Task.Delay(Owner.TotalInvokeSpeed);
                    return MagicBall(skillData);
                default: return null;
            }            
        }
        public async void HandleSpotSkill(SkillData skillData)
        {            
            
            switch (skillData.skillLogicType)
            {
                case SkillLogicType.Spotattack:
                    _skillPos = SkillLogic.GetRandomSpots(Owner, skillData, Room);
                    if(_skillPos.Count == 0)
                    {
                        return;
                    }
                    await Task.Delay(Owner.TotalInvokeSpeed);
                    SpotAttack(Owner, skillData);
                    break;
            }
        }
        public MagicBall MagicBall(SkillData skillData)
        {
            MagicBall magicBall = ObjectManager.Instance.Add<MagicBall>();
            magicBall.Owner = Owner;
            magicBall.Target = _target.LastOrDefault();
            magicBall.Room = Room;
            magicBall.Data = skillData;
            magicBall.PosInfo.State = CreatureState.Moving;
            magicBall.PosInfo.MoveDir = Owner.PosInfo.MoveDir;
            magicBall.PosInfo.PosX = Owner.PosInfo.PosX;
            magicBall.PosInfo.PosY = Owner.PosInfo.PosY;
            magicBall.Speed = skillData.projectile.speed;
            return magicBall;
        }
        public void BasicAttak(SkillData skillData)
        {
            List<Vector2Int> skillPos = new List<Vector2Int>();
            if (skillData.shape.shapeType == ShapeType.ShapeBent)
            {
                Vector2Int center = Owner.GetFrontCellPos();
                Vector2Int lookDir = Owner.GetPosFromLookDir(Owner.CellPos, Owner.Info.Position.LookDir);
                skillPos.AddRange(SkillLogic.GetBentAttackTiles(center, lookDir, (int)skillData.shape.range));
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

        private void SpotAttack(GameObject user, SkillData skillData)
        {
            int invokeDelay = 0;
            foreach (Vector2Int pos in _skillPos)
            {
                SpotAttack spot = ObjectManager.Instance.Add<SpotAttack>();
                spot.Data = skillData;
                spot.Owner = user;
                spot.Room = user.Room;
                spot.PosInfo.PosX = pos.x;
                spot.PosInfo.PosY = pos.y;
                spot.Delay = skillData.spot.delay;
                invokeDelay += _invokeDelay;
                user.Room.PushAfter(invokeDelay, user.Room.EnterGame, spot, false);
            }
        }
    }
}
