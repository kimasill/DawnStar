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
    internal class Satyr : Monster
    {
        private int _skillRange = 3;
        private int _stunDuration = 3; // 기절 지속 시간 (초)
        private int _chargeTime = 2; // 공격 차징 시간 (초)

        public Satyr(MonsterData data) : base(data)
        {
            Initialize(data);
            SkillRange = _skillRange;
        }

        protected override void UpdateSkill()
        {
            if (_coolTick > Environment.TickCount64)
                return;

            _coolTick = Environment.TickCount64 + (int)(1000/TotalAttackSpeed);

            
            if (_target == null || _target.Room != Room || _target.Hp == 0)
            {
                _target = null;
                State = CreatureState.Moving;
                BroadcastMove();
                return;
            }
                //스킬 사용 가능한지
            Vector2Int dir = _target.CellPos - CellPos;
            int dist = dir.cellDistanceFromZero;
            bool canUseSkill = dist <= _skillRange && (dir.x == 0 || dir.y == 0);
            if (canUseSkill == false)
            {
                State = CreatureState.Moving;
                BroadcastMove();
                return;
            }

            LookAt(dir);
            {
                SkillData skillData = null;
                DataManager.SkillDict.TryGetValue(24, out skillData);
                if (Skill.HandleSkillCool(skillData) == false)
                {
                    State = CreatureState.Moving;
                    BroadcastMove();
                    return;
                }                //스킬 사용
                
                S_Skill skillPacket = new S_Skill() { Info = new SkillInfo() };
                skillPacket.ObjectId = Id;
                skillPacket.Info.SkillId = skillData.id;
                Room.Broadcast(CellPos, skillPacket);
                Skill.StartSkill(this, skillData, addRange:1, distance:2);
            }
            if(MonsterGrade == MonsterGrade.Elite)
            {
                SkillData skillData = null;
                DataManager.SkillDict.TryGetValue(14, out skillData);

                EffectSkill(this, skillData, 2, skillData.duration);
            }
        }

        

        public override int OnDamaged(GameObject attacker, int damage)
        {
            if (IsDead)
                return 0;
            if(!Skill.isExecuting)
                State = CreatureState.Stiff;
            return base.OnDamaged(attacker, damage);
        }
    }
}