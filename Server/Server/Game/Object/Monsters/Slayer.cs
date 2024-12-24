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
    internal class Slayer : Monster
    {
        private const int BasicSkillId = 1;
        private const int PowerSkillId = 9;
        private const float PowerSkillInvokeTime = -0.4f;
        private const int skillRange = 2;
        int _skillId = 0;
        public Slayer(MonsterData monsterData) : base(monsterData)
        {
            Initialize(monsterData);
            SkillRange = skillRange;
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

                _skillId = PowerSkillId;
                AdditionalInvokeSpeed = PowerSkillInvokeTime;
                DataManager.SkillDict.TryGetValue(PowerSkillId, out skillData);
                if (Skill.HandleSkillCool(skillData) == false)
                {
                    AdditionalInvokeSpeed = 0;
                    DataManager.SkillDict.TryGetValue(BasicSkillId, out skillData);
                }

                _coolTick = coolTick;
                UseSkill(skillData);
            }
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
