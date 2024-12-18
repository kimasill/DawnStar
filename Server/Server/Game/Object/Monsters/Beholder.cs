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
        private const int LaserSkillId = 30;
        private const int LaserRange = 10;
        private float _currentAngle = 0;

        public Beholder(MonsterData data) : base(data)
        {
            Initialize(data);
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
                SkillData skillData = null;
                DataManager.SkillDict.TryGetValue(LaserSkillId, out skillData);
                if (skillData == null)
                    return;

                S_Skill skillPacket = new S_Skill() { Info = new SkillInfo() };
                skillPacket.ObjectId = Id;
                skillPacket.Info.SkillId = skillData.id;
                Room.Broadcast(CellPos, skillPacket);

                // 레이저 빔 스킬 사용
                UseLaserSkill(skillData);

                // 쿨타임 설정
                int coolTick = (int)(1000 / TotalAttackSpeed);
                _coolTick = Environment.TickCount64 + coolTick;
            }

            if (_coolTick > Environment.TickCount64)
                return;

            _coolTick = 0;
        }

        private void UseLaserSkill(SkillData skillData)
        {
            // 레이저 빔의 각도를 반시계방향으로 회전
            _currentAngle -= 10; // 각도는 필요에 따라 조정 가능
            if (_currentAngle < 0)
                _currentAngle += 360;
        }
    }
}