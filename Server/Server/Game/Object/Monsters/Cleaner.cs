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
    internal class Cleaner : Monster
    {
        private const int BasicSkillId = 1;
        const float BasicSkillDuration = 1f;
        const float Combo1Duration = 0.5f;
        const float Combo2Duration = 0.8f;
        private const int ComboSkillId = 28;
        private int _basicSkillCount = 0;
        int _skillId = 0;

        public Cleaner(MonsterData monsterData) : base(monsterData)
        {
            Initialize(monsterData);
        }

        protected override void UpdateSkill()
        {
            if (_coolTick == 0)
            {
                // 유효 타겟 확인
                if (_target == null || _target.Room != Room || _target.Hp == 0)
                {
                    _target = null;
                    State = CreatureState.Moving;
                    BroadcastMove();
                    return;
                }

                // 스킬 사용 가능한지 확인
                Vector2Int dir = _target.CellPos - CellPos;
                int dist = dir.cellDistanceFromZero;
                bool canUseSkill = dist <= SkillRange && (dir.x == 0 || dir.y == 0);
                if (!canUseSkill)
                {
                    State = CreatureState.Moving;
                    BroadcastMove();
                    return;
                }

                LookAt(dir);
                SkillData skillData = null;
                int coolTick = (int)(1000 / TotalAttackSpeed);
                if (_skillId == 0)
                {
                    DataManager.SkillDict.TryGetValue(BasicSkillId, out skillData);
                    if (_basicSkillCount < 2)
                    {
                        _basicSkillCount++;
                    }
                    else
                    {
                        _skillId = ComboSkillId;
                        _basicSkillCount = 0;
                        coolTick = (int)(1000 * Combo1Duration);
                    }
                }
                else
                {
                    DataManager.SkillDict.TryGetValue(ComboSkillId, out skillData);
                    coolTick = (int)(1000 * Combo2Duration);
                    _skillId = 0;
                }

                _coolTick = Environment.TickCount64 + coolTick;

                UseSkill(skillData);
            }
            if (_coolTick > Environment.TickCount64)
                return;

            _coolTick = 0;
        }

        private void UseSkill(SkillData skillData)
        {
            if (skillData == null)
                return;

            S_Skill skillPacket = new S_Skill() { Info = new SkillInfo() };
            skillPacket.ObjectId = Id;
            skillPacket.Info.SkillId = skillData.id;
            Room.Broadcast(CellPos, skillPacket);

            Skill.StartSkill(this, skillData, _target);
        }
    }
}