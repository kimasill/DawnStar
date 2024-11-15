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
        private int _skillRange = 10;              
        private const double _blasterMinRange = 3;
        private const double _blasterDuration = 2.2;
        private const double _summonInvokeAdd = 0.5f;
        public SkeletonMage(MonsterData data) : base(data)
        {
            Initialize(data);
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
            if (_blasterMinRange <= dist)
            {
                skillId = 17;
                _coolTick = Environment.TickCount64 + (int)(_blasterDuration * 1000);
            }
            else
            {
                skillId = 18;
                _coolTick = Environment.TickCount64 + 1000 / (int)TotalAttackSpeed;
            }
            SkillData skillData = null;
            DataManager.SkillDict.TryGetValue(skillId, out skillData);
            AdditionalInvokeSpeed = 0;
            if (Skill.HandleSkillCool(skillData) == false)
            {
                skillId = 18;

                if (Skill.HandleSkillCool(skillData) == false)
                {
                    _coolTick = Environment.TickCount64 + 1000/(int)TotalAttackSpeed;
                    State = CreatureState.Moving;
                    BroadcastMove();
                    return;
                }
            }
            if(skillId == 18)
            {
                AdditionalInvokeSpeed = (float)_summonInvokeAdd;
            }
            S_Skill skillPacket = new S_Skill() { Info = new SkillInfo() };
            skillPacket.ObjectId = Id;
            skillPacket.Info.SkillId = skillData.id;
            Room.Broadcast(CellPos, skillPacket);
            Skill.StartSkill(this, skillData);            
        }
    }
}
