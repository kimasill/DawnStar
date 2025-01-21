using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game.Room;
using System;
using System.Collections.Generic;
using static Google.Protobuf.Compiler.CodeGeneratorResponse.Types;

namespace Server.Game.Object.Monsters
{
    internal class Beholder : Monster
    {
        private const int LaserSkillId = 33;
        private const int LaserRange = 10;
        private const int KnockbackSkillId = 34;
        private const int KnockbackRange = 2;
        private const int MagicSkillId = 35;
        private float _currentAngle = 0;

        public Beholder(MonsterData data) : base(data)
        {
            Initialize(data);
            SkillRange = LaserRange;
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

                // 쿨타임 설정
                float coolTick = (int)(1000 / TotalAttackSpeed);
                int skillId = MagicSkillId;
                AdditionalInvokeSpeed = 0;
                SkillRange = LaserRange;
                SkillData skillData = null;
                DataManager.SkillDict.TryGetValue(skillId, out skillData);
                if (skillData == null)
                    return;

                if (Skill.HandleSkillCool(skillData) == false)
                {
                    Vector2Int sDir = _target.CellPos - CellPos;
                    int sDist = sDir.cellDistanceFromZero;
                    if(sDist <= KnockbackRange)
                    {
                        skillId = KnockbackSkillId;
                    }
                    else
                    {
                        skillId = LaserSkillId;
                    }
                }
                if(skillId == KnockbackSkillId)
                {
                    DataManager.SkillDict.TryGetValue(skillId, out skillData);
                    SkillRange = skillData.range;    
                    if (Skill.HandleSkillCool(skillData) == false)
                    {
                        skillId = LaserSkillId;
                    }
                }
                if (skillId == LaserSkillId)
                {
                    DataManager.SkillDict.TryGetValue(skillId, out skillData);
                    SkillRange = LaserRange;
                    if (Skill.HandleSkillCool(skillData) == false)
                    {
                        State = CreatureState.Moving;
                        BroadcastMove();
                        return;
                    }
                }

                // 스킬 사용 가능한지 확인
                Vector2Int dir = _target.CellPos - CellPos;
                int dist = dir.cellDistanceFromZero;
                bool canUseSkill = dist <= SkillRange;
                if (!canUseSkill)
                {
                    State = CreatureState.Moving;
                    BroadcastMove();
                    return;
                }

                coolTick = 0;
                foreach (var term in skillData.terms)
                {
                    coolTick += term;
                }
                LookAt(dir);
                AdditionalInvokeSpeed = skillData.terms[0];
                S_Skill skillPacket = new S_Skill() { Info = new SkillInfo() };
                skillPacket.ObjectId = Id;
                skillPacket.Info.SkillId = skillData.id;
                Room.Broadcast(CellPos, skillPacket);
                Skill.StartSkill(this, skillData, target: _target);
                _coolTick = (long)(Environment.TickCount64 + (coolTick * 1000));
            }
            if (_coolTick > Environment.TickCount64)
                return;

            _coolTick = 0;
        }
    }
}