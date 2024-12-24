using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game.Room;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Server.Game.Object.Monsters
{
    internal class Bat : Monster
    {
        private const int InitialAttackDelay = 600; // 0.6초
        private long _nextAttackTick = 0;

        public Bat(MonsterData monsterData) : base(monsterData)
        {
            Initialize(monsterData);
        }

        protected override void UpdateSkill()
        {
            if (_nextAttackTick == 0)
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
                UseSkill(1);
                // 초기 공격 딜레이 설정
                _nextAttackTick = Environment.TickCount64 + InitialAttackDelay;
            }
            else if (_nextAttackTick > Environment.TickCount64)
            {
                return;
            }
            else
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
                UseSkill(1);
                int attackDelay = (int)(1000 / TotalAttackSpeed);
                _nextAttackTick = Environment.TickCount64 + attackDelay;
            }
        }
        protected override void UpdateMoving()
        {
            if (_moveTick > Environment.TickCount64)
                return;
            int moveTick = (int)(1000 / TotalSpeed);
            _moveTick = Environment.TickCount64 + moveTick;
            if (_target == null || _target.Room != Room)
            {
                _target = null;
                State = CreatureState.Idle;
                BroadcastMove();
                return;
            }

            Vector2Int dir = _target.CellPos - CellPos;
            int dist = dir.cellDistanceFromZero;
            if (dist == 0 || dist > _chaseRange)
            {
                _target = null;
                State = CreatureState.Idle;
                BroadcastMove();
                return;
            }

            List<Vector2Int> path = Room.Map.FindPath(CellPos, _target.CellPos);
            if (path.Count < 2 || path.Count > _chaseRange)
            {
                _target = null;
                State = CreatureState.Idle;
                BroadcastMove();
                return;
            }

            if (dist <= SkillRange && (dir.x == 0 || dir.y == 0))
            {
                _nextAttackTick = 0;
                State = CreatureState.Skill;
                return;
            }
            UpdateDir(path[1]);
            Room.Map.ApplyMove(this, path[1]);
            BroadcastMove();
        }
        private void UseSkill(int skillId, bool term = false)
        {
            SkillData skillData = null;
            DataManager.SkillDict.TryGetValue(skillId, out skillData);
            if (skillData == null)
                return;

            S_Skill skillPacket = new S_Skill() { Info = new SkillInfo() };
            skillPacket.ObjectId = Id;
            skillPacket.Info.SkillId = skillData.id;
            Room.Broadcast(CellPos, skillPacket);

            Skill.StartSkill(this, skillData, target: _target);
        }
    }
    
}