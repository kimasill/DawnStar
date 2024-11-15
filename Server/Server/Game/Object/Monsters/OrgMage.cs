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
    internal class OrcMage : Monster
    {
        private int _baseAttackRange = 2;
        private int _skillRange = 10;
        private bool _hasRevived = false;
        private const float SkillInvokeTime = 1.7f;
        public OrcMage(MonsterData data) : base(data)
        {
            Initialize(data);
        }

        public override void Update()
        {
            base.Update();
        }
        long _coolTick = 0;
        Random _rand = new Random();
        protected override void UpdateSkill()
        {
            if(_coolTick == 0)
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
                int skillId = 12;
                int coolTick = 1000 / (int)TotalAttackSpeed;
                SkillData skillData = null;
                if (dir.cellDistanceFromZero <= _baseAttackRange)
                {
                    skillId = 13;
                    DataManager.SkillDict.TryGetValue(skillId, out skillData);
                    if (Skill.HandleSkillCool(skillData) == false)
                    {
                        skillId = 12;
                    }
                }
                if(skillId == 12)
                {
                    DataManager.SkillDict.TryGetValue(skillId, out skillData);
                    coolTick = (int)(1000 * SkillInvokeTime);
                    if (Skill.HandleSkillCool(skillData) == false)
                    {
                        skillId = 11;
                    }
                }
                if (skillId == 11)
                {
                    DataManager.SkillDict.TryGetValue(skillId, out skillData);
                    coolTick = (int)(1000 * SkillInvokeTime);
                    if (Skill.HandleSkillCool(skillData) == false)
                    {
                        State = CreatureState.Moving;
                        BroadcastMove();
                        return;
                    }
                }

                S_Skill skillPacket = new S_Skill() { Info = new SkillInfo() };
                skillPacket.ObjectId = Id;
                skillPacket.Info.SkillId = skillData.id;
                Room.Broadcast(CellPos, skillPacket);

                Skill.StartSkill(this, skillData, _target);
                _coolTick = Environment.TickCount64 + coolTick;
            }
            if (_coolTick > Environment.TickCount64)
                return;
            _coolTick = 0;
        }
        protected override void UpdateMoving()
        {
            if (_nextMoveTick > Environment.TickCount64)
                return;
            int moveTick = (int)(1000 / Speed);
            _nextMoveTick = Environment.TickCount64 + moveTick;

            if (MonsterGrade == MonsterGrade.Animal)
            {
                UpdatePatrol();
                return;
            }

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

            if (dist <= _skillRange && (dir.x == 0 || dir.y == 0))
            {
                _coolTick = 0;
                State = CreatureState.Skill;
                return;
            }
            UpdateDir(path[1]);
            Room.Map.ApplyMove(this, path[1]);
            BroadcastMove();
        }
    }
}