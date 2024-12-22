using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game.Room;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Server.Game.Object.Monsters
{
    internal class Pot : Monster
    {
        bool isRevealed = false;
        private float _revealingTime = 1.2f;
        private const int CannonSkillId = 37;
        private const int MeleeSkillId = 13;
        private int _cannonRange = 10; // Cannon 사거리
        private int _baseAttackRange = 2; // 기본 공격 사거리
        private float _cannonInvokeTime = 0.3f;
        int skillId = 0;

        public Pot(MonsterData monsterData) : base(monsterData)
        {
            Initialize(monsterData);
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

            if (isRevealed == false)
            {
                BroadcastMove();
                moveTick = (int)(1000 * _revealingTime);
                _moveTick = Environment.TickCount64 + moveTick;
                isRevealed = true;
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

            if(dist <= SkillRange)
            {
                _coolTick = 0;
                if (dist <= _cannonRange && dist > _baseAttackRange)
                    skillId = CannonSkillId;
                else
                {
                    SkillRange = _cannonRange;
                    skillId = MeleeSkillId;
                }
                    
                State = CreatureState.Skill;
                return;
            }
            UpdateDir(path[1]);
            Room.Map.ApplyMove(this, path[1]);
            BroadcastMove();
        }

        protected override void UpdateSkill()
        {
            if (_coolTick == 0)
            {
                if (_target == null || _target.Room != Room || _target.Hp == 0)
                {
                    _target = null;
                    State = CreatureState.Moving;
                    return;
                }

                Vector2Int dir = _target.CellPos - CellPos;
                int dist = dir.cellDistanceFromZero;
                SkillData skillData = null;
                DataManager.SkillDict.TryGetValue(skillId, out skillData);

                if (Skill.HandleSkillCool(skillData, peek: true))
                {
                    UseSkill(skillId);
                }
                else
                {
                    SkillRange = _baseAttackRange;
                    State = CreatureState.Moving;
                }
            }
        }

        private void UseSkill(int skillId)
        {
            SkillData skillData;
            DataManager.SkillDict.TryGetValue(skillId, out skillData);

            S_Skill skillPacket = new S_Skill() { Info = new SkillInfo() };
            skillPacket.ObjectId = Id;
            skillPacket.Info.SkillId = skillId;
            if (skillId == CannonSkillId)
            {
                Skill.HandleSkillCool(skillData);
                AdditionalInvokeSpeed = _cannonInvokeTime;
                _coolTick = (int)(Environment.TickCount64 + (1000 * skillData.terms[0]));
            }   
            else
            {
                AdditionalInvokeSpeed = 0;
                _coolTick = (int)(Environment.TickCount64 + (1000/TotalAttackSpeed));
            }
                
            Room.Broadcast(CellPos, skillPacket);
            Skill.StartSkill(this, skillData, _target);
        }
    }
}