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
    internal class SkeletonMage : Monster
    {
        private int _baseAttackRange = 2;
        int _skillRange = 10;              
        private const double _blasterMinRange = 3;
        private const double _blasterDuration = 2;
        private const double _blasterAttackDelay = 0.4f;
        private const double _summonInvokeAdd = 0.5f;
        public SkeletonMage(MonsterData data) : base(data)
        {
            Initialize(data);
            SkillRange = _skillRange;
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
            bool canUseSkill = dist <= SkillRange && (dir.x == 0 || dir.y == 0);
            if (canUseSkill == false)
            {
                State = CreatureState.Moving;
                BroadcastMove();
                return;
            }
            int skillId = 23;
            LookAt(dir);
            SkillData skillData = null;
            if (_blasterMinRange <= dist)
            {
                skillId = 20;
                DataManager.SkillDict.TryGetValue(skillId, out skillData);
                AdditionalInvokeSpeed = (float)_blasterAttackDelay;
                _coolTick = Environment.TickCount64 + (int)((_blasterDuration + Stat.InvokeSpeed + (int)(1 / TotalAttackSpeed)) * 1000);
                if (Skill.HandleSkillCool(skillData) == false)
                {                    
                    skillId = 23;
                }
            }
            if(skillId == 23)
            {
                DataManager.SkillDict.TryGetValue(skillId, out skillData);

                AdditionalInvokeSpeed = (float)_summonInvokeAdd;
                _coolTick = Environment.TickCount64 + (int)(1000 / TotalAttackSpeed);
                if (dist>skillData.range || Skill.HandleSkillCool(skillData) == false)
                {
                    State = CreatureState.Moving;
                    BroadcastMove();
                    return;
                }
            }
            
            S_Skill skillPacket = new S_Skill()
            {
                ObjectId = Id,
                Info = new SkillInfo()
                {
                    SkillId = skillData.id,                    
                },                
            };
            Room.Broadcast(CellPos, skillPacket);
            Skill.StartSkill(this, skillData, target:_target);            
        }
    }
}
