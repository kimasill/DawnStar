using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game.Room;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game.Object.Monsters
{
    internal class ToothKing : Monster
    {
        private const int AttackSkillId = 9;
        private const float BasicSkillDuration = 1.3f;
        private const float ComboDuration = 1.6f;
        private const int MagicSkillId = 39;
        private const int PoolingSkillId = 14;
        private const int BuffSkillId = 40;
        private const float RevealTime = 1.2f;
        private bool _isInPhaseTwo = false;
        private int _basicSkillCount = 0;
        private bool _hasRevealed = false;

        public ToothKing(MonsterData monsterData) : base(monsterData)
        {
            Initialize(monsterData);
        }

        protected override void UpdateIdle()
        {
            if (!_hasRevealed)
            {
                _hasRevealed = true;
                Room.PushAfter((int)(RevealTime * 1000), () =>
                {
                    if (State == CreatureState.Idle)
                    {
                        State = CreatureState.Moving;
                    }
                });
            }
            else
            {
                base.UpdateIdle();
            }
        }

        protected override void UpdateSkill()
        {
            if (_coolTick == 0)
            {
                if (_isInPhaseTwo)
                {
                    UseSkill(MagicSkillId);
                    _coolTick = (int)(1000 * ComboDuration);
                }
                else
                {
                    if (_basicSkillCount < 2)
                    {
                        UseSkill(AttackSkillId);
                        _basicSkillCount++;
                        _coolTick = (int)(1000 * BasicSkillDuration);
                    }
                    else
                    {
                        UseSkill(PoolingSkillId, true);
                        UseSkill(AttackSkillId, true);
                        _basicSkillCount = 0;
                        _coolTick = (int)(1000 * ComboDuration);
                    }
                }
            }
        }

        public override int OnDamaged(GameObject attacker, int damage)
        {
            int resultDamage = base.OnDamaged(attacker, damage);
            if (Stat.Hp <= Stat.MaxHp * 0.7 && !_isInPhaseTwo)
            {
                EnterPhaseTwo();
            }
            return resultDamage;
        }

        private void EnterPhaseTwo()
        {
            _isInPhaseTwo = true;
            UseSkill(BuffSkillId);
        }

        private void UseSkill(int skillId, bool reverseDirection = false)
        {
            SkillData skillData;
            if (!DataManager.SkillDict.TryGetValue(skillId, out skillData))
                return;
            S_Skill skillPacket = new S_Skill() { Info = new SkillInfo() };
            skillPacket.ObjectId = Id;
            skillPacket.Info.SkillId = skillData.id;
            Room.Broadcast(CellPos, skillPacket);

            Skill.StartSkill(this, skillData, _target);
        }
    }
}