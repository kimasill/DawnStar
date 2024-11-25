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
    internal class SkeletonWarrior : Monster
    {
        private Random _random = new Random();
        private bool _hasShield;
        private int _skillRange = 2;
        private const double StrongAttackProbability = 0.2; // 20% 확률로 강한 공격
        private const double DefenseStanceProbability = 0.3; // 10% 확률로 방어 태세
        private const double DefenseStanceDuration = 2.6; // 방어 태세 지속 시간
        private const double StrongAttackDuration = 1.7;
        public SkeletonWarrior(MonsterData data) : base(data)
        {
            Initialize(data);

            if (TemplateId == 21)
                _hasShield = true;
        }


        long _coolTick = 0;
        protected override void UpdateSkill()
        {
            if (_coolTick > Environment.TickCount64)
                return;

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
            int skillId = 0;
            LookAt(dir);
            if (_random.NextDouble() < StrongAttackProbability)
            {
                skillId = 9;
                _coolTick = Environment.TickCount64 + 1000 * (int)StrongAttackDuration;
            }
            else
            {
                skillId = 1;
                _coolTick = Environment.TickCount64 + (int)(1000 / TotalAttackSpeed);
            }

            if (_hasShield && _random.NextDouble() < DefenseStanceProbability)
            {
                skillId = 16;
                _coolTick = Environment.TickCount64 + (int)(DefenseStanceDuration * 1000);
            }
            {
                SkillData skillData = null;
                DataManager.SkillDict.TryGetValue(skillId, out skillData);
                if (Skill.HandleSkillCool(skillData, attackSpeed: true) == false)
                {
                    _coolTick = Environment.TickCount64 + 1000;
                    State = CreatureState.Moving;
                    BroadcastMove();
                    return;
                }                //스킬 사용

                S_Skill skillPacket = new S_Skill() { Info = new SkillInfo() };
                skillPacket.ObjectId = Id;
                skillPacket.Info.SkillId = skillData.id;
                Room.Broadcast(CellPos, skillPacket);
                Skill.StartSkill(this, skillData, target:_target);
            }
        }
    }
}