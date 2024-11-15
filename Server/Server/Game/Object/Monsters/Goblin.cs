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
    internal class Goblin : Monster
    {
        private int _basicAttackCount = 0;
        private const int BasicAttackLimit = 5;
        private int _skillRange = 1;
        private const double StrongAttackTime = 1.5;
        private const double StrongAttackInvokeAdd = 0.4;

        public Goblin(MonsterData data) : base(data)
        {            
            Initialize(data);
        }

        public override void Update()
        {
            base.Update();
        }

        long _coolTick = 0;
        protected override void UpdateSkill()
        {
            if (_coolTick == 0)
            {
                //유효 타겟
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
                int skillId = 0;
                int coolTick = (int)(1000/TotalAttackSpeed);
                SkillData skillData = null;
                AdditionalInvokeSpeed = 0;
                if (_basicAttackCount < BasicAttackLimit)
                {            
                    skillId = 1;
                    DataManager.SkillDict.TryGetValue(skillId, out skillData);
                    if(Skill.HandleSkillCool(skillData, true) == false)
                    {
                        State = CreatureState.Moving;
                        BroadcastMove();
                        return;
                    }
                    _basicAttackCount += 1;                     
                }
                else
                {
                    skillId = 9;
                    DataManager.SkillDict.TryGetValue(skillId, out skillData);
                    if (Skill.HandleSkillCool(skillData) == false)
                    {
                        State = CreatureState.Moving;
                        BroadcastMove();
                        return;
                    }
                    _basicAttackCount = 0;
                    coolTick = (int)(1000 * StrongAttackTime);
                    AdditionalInvokeSpeed = (float)StrongAttackInvokeAdd;
                }
                
                Skill.StartSkill(this, skillData, _target);                
                S_Skill skillPacket = new S_Skill() { Info = new SkillInfo() };
                skillPacket.ObjectId = Id;
                skillPacket.Info.SkillId = skillData.id;
                Room.Broadcast(CellPos, skillPacket);
                
                _coolTick = Environment.TickCount64 + coolTick;
            }
            if (_coolTick > Environment.TickCount64)
                return;
            _coolTick = 0;
        }
    }
}