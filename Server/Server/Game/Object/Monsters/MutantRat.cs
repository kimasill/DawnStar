using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game.Room;
using System;
using System.Collections.Generic;

namespace Server.Game.Object.Monsters
{
    internal class MutantRat : Monster
    {
        private int _rangeSkillId = 31;
        private int _meleeSkillId = 1;
        private int _skillId = 1;
        public MutantRat(MonsterData data) : base(data)
        {
            Initialize(data);
            SkillRange = 5;
            _skillId = _rangeSkillId;
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
                SkillData skillData = null;

                int skillId = 31;
                SkillRange = 5;
                DataManager.SkillDict.TryGetValue(skillId, out skillData);
                // 31번 스킬의 쿨타임 확인
                if (Skill.HandleSkillCool(skillData, peek:true) == false)
                {
                    skillId = 1;
                    SkillRange = 1;
                    DataManager.SkillDict.TryGetValue(skillId, out skillData);
                }
                // 1번 스킬 사용                
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
                S_Skill skillPacket = new S_Skill() { Info = new SkillInfo() };
                skillPacket.ObjectId = Id;
                skillPacket.Info.SkillId = skillData.id;
                Room.Broadcast(CellPos, skillPacket);

                Skill.StartSkill(this, skillData, target: _target);

                // 쿨타임 설정
                int coolTick = (int)(1000 / TotalAttackSpeed);
                _coolTick = Environment.TickCount64 + coolTick;
            }

            if (_coolTick > Environment.TickCount64)
                return;

            _coolTick = 0;
            
        }
    }
}