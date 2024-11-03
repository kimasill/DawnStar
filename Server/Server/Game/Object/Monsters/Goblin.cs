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

        public Goblin(MonsterData data) : base(data)
        {
            TemplateId = data.id;
            MonsterType = data.type;
            MonsterGrade = data.grade;
            Info.Name = data.name;
            Info.TemplateId = TemplateId;
            Stat.MergeFrom(data.stat);
            Stat.Hp = data.stat.MaxHp;
            State = CreatureState.Idle;
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
                //방향주시
                MoveDir lookDir = GetDirFromVec(dir);
                int coolTick = (int)(1000 / TotalAttackSpeed);
                if (Dir != lookDir)
                {
                    Dir = lookDir;
                    if (Dir == MoveDir.Left)
                    {
                        LookDir = LookDir.LookLeft;
                    }
                    else if (Dir == MoveDir.Right)
                    {
                        LookDir = LookDir.LookRight;
                    }
                    BroadcastMove();
                }
                if (_basicAttackCount < BasicAttackLimit)
                {
                    AttackBySkill(1);
                    _basicAttackCount++;
                }
                else
                {
                    AttackBySkill(9);
                    _basicAttackCount = 0;
                    coolTick = (int)((int)(1000 / TotalAttackSpeed) * 1.2);
                }

                //쿨타임

                _coolTick += Environment.TickCount64 + coolTick;
            }
            if (_coolTick > Environment.TickCount64)
                return;
            _coolTick = 0;
        }
    }
}