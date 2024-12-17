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
    internal class Zombie : Monster
    {
        public Zombie(MonsterData data) : base(data)
        {
            Initialize(data);
        }

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
                bool canUseSkill = dist <= SkillRange && (dir.x == 0 || dir.y == 0);
                if (canUseSkill == false)
                {
                    State = CreatureState.Moving;
                    BroadcastMove();
                    return;
                }
                LookAt(dir);
                SkillData skillData = null;
                DataManager.SkillDict.TryGetValue(30, out skillData);
                if (skillData == null)
                    return;

                S_Skill skillPacket = new S_Skill() { Info = new SkillInfo() };
                skillPacket.ObjectId = Id;
                skillPacket.Info.SkillId = skillData.id;
                Room.Broadcast(CellPos, skillPacket);

                Skill.StartSkill(this, skillData, target: _target);
                //쿨타임
                int coolTick = (int)(1000 / TotalAttackSpeed);
                _coolTick = Environment.TickCount64 + coolTick;
            }
            if (_coolTick > Environment.TickCount64)
                return;
            _coolTick = 0;
        }
    }
}
