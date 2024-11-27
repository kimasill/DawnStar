using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game.Room;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Server.Game.Object.Monsters
{
    internal class PrisonKeeper : Monster
    {
        private Random _random = new Random();

        private const double PhaseTwoThreshold = 0.7; // 70%
        private bool _isInPhaseTwo = false;

        private int _skillRange = 5;
        private const double SkillProbability = 0.3; // 30% 확률로 스킬 사용
        private const double SkillCooldown = 2.0; // 스킬 쿨다운 시간 (초)
        private const double AssassinateInvokeTime = 1.0;
        private const double BuffInvokeTime = 3.0;
        private const double SkillInvokeTime = 0.5;
        private int _coolTick = 0;

        private const int AssassinateSkillId = 19;
        private const int EnhanceSkillId = 18;
        private const int StepBackSkillId = 22;

        private bool _isUsingSkill = false;

        public PrisonKeeper(MonsterData data) : base(data)
        {
            Initialize(data);
        }

        public override int OnDamaged(GameObject attacker, int damage)
        {
            int resultDamage = base.OnDamaged(attacker, damage);
            if (_isInPhaseTwo)
            {
                if (_random.NextDouble() < SkillProbability)
                {
                    UseSkill(StepBackSkillId);
                    State = CreatureState.Moving;
                }
            }
            if (Stat.Hp <= Stat.MaxHp * PhaseTwoThreshold && !_isInPhaseTwo)
            {
                EnterPhaseTwo();
            }
            return resultDamage;
        }

        private void EnterPhaseTwo()
        {
            _isUsingSkill = true;
            UseSkill(StepBackSkillId, term: true); // term 매개변수 추가
            UseSkill(EnhanceSkillId, term: true); // term 매개변수 추가
            SkillRange = _skillRange;
            _isInPhaseTwo = true;
            _isUsingSkill = false;
        }

        protected override void UpdateIdle()
        {
            _isUsingSkill = false; // _isUsingSkill 변수 초기화

            if (_nextSearchTick > Environment.TickCount64 || _isUsingSkill)
                return;

            _nextSearchTick = Environment.TickCount64 + 1000;

            _dest = CellPos;

            Player target = Room.FindCloesetPlayer(CellPos, _searchRange);
            if (target != null)
            {
                _target = target;
                State = CreatureState.Moving;
                return;
            }
        }

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

            Vector2Int dir = _target.CellPos - CellPos;
            int dist = dir.cellDistanceFromZero;
            
            bool canUseSkill = dist <= SkillRange;
            if (canUseSkill == false)
            {
                State = CreatureState.Moving;
                BroadcastMove();
                return;
            }

            LookAt(dir);
            int skillId = 1;
            SkillData skillData = null;
            if (_isInPhaseTwo)
            {
                skillId = 17;
                AdditionalInvokeSpeed = (float)(SkillInvokeTime - TotalInvokeSpeed);
                DataManager.SkillDict.TryGetValue(skillId, out skillData);
                if (skillData == null || Skill.HandleSkillCool(skillData) == false) // skillData null 확인
                {
                    skillId = 1;
                }
            }
            if (skillId == 1)
            {
                AdditionalInvokeSpeed = 0;
                DataManager.SkillDict.TryGetValue(skillId, out skillData);
                if (skillData == null || Skill.HandleSkillCool(skillData) == false) // skillData null 확인
                {
                    State = CreatureState.Moving;
                    BroadcastMove();
                    return;
                }
            }
            S_Skill skillPacket = new S_Skill() { Info = new SkillInfo() };
            skillPacket.ObjectId = Id;
            skillPacket.Info.SkillId = skillData.id;
            Room.Broadcast(CellPos, skillPacket);

            if (_target != null) // _target null 확인
            {
                Skill.StartSkill(this, skillData, _target);
            }

            _coolTick = (int)(Environment.TickCount64 + (1000 / TotalAttackSpeed));
        }

        // ... (기존 코드) ...

        private void UseSkill(int skillId, int phase = 0, bool term = false)
        {
            SkillData skillData = null;
            DataManager.SkillDict.TryGetValue(skillId, out skillData);
            if (skillData == null || Skill.HandleSkillCool(skillData) == false) // skillData null 확인
            {
                State = CreatureState.Moving;
                BroadcastMove();
                return;
            }

            S_Skill skillPacket = new S_Skill() { Info = new SkillInfo() };
            skillPacket.ObjectId = Id;
            skillPacket.Info.SkillId = skillData.id;
            if (phase != 0)
            {
                skillPacket.Phase = phase;
            }
            Room.Broadcast(CellPos, skillPacket);
            Skill.StartSkill(this, skillData, _target);
            if (term)
            {
                _coolTick = (int)(Environment.TickCount64 + (1000 * skillData.terms[0]));
            }
            else
            {
                _coolTick = (int)(Environment.TickCount64 + (1000 / TotalAttackSpeed));
            }
        }
    }
}