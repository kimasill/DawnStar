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
    internal class Orc : Monster
    {
        private int _basicAttackCount = 0;
        private const int BasicAttackLimit = 5;
        private int _skillRange = 2;

        public Orc(MonsterData data) : base(data)
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
                float dist = dir.cellDistanceFromZero;
                bool canUseSkill = dist <= _skillRange && (dir.x == 0 || dir.y == 0);
                if (canUseSkill == false)
                {
                    State = CreatureState.Moving;
                    BroadcastMove();
                    return;
                }
                //방향주시
                LookAt(dir);
                int coolTick = (int)(1000 / TotalAttackSpeed);

                if (Hp < Stat.MaxHp / 2)
                {
                    SkillData skillData = null;
                    DataManager.SkillDict.TryGetValue(10, out skillData);
                    //데미지 판정
                    S_Skill skillPacket = new S_Skill() { Info = new SkillInfo() };
                    skillPacket.ObjectId = Id;
                    skillPacket.Info.SkillId = skillData.id;
                    Room.Broadcast(CellPos, skillPacket);

                    S_ChangePosition changePacket = new S_ChangePosition();
                    changePacket.ObjectId = Id;
                    Vector2Int targetDir = _target.CellPos - CellPos;
                    float minAbs = Math.Min(targetDir.magnitude, 3);
                    Vector2Int moveDir = targetDir.normalized;

                    Task.Run(async () =>
                    {
                        // 첫 데미지를 주기 전에 0.5초 대기
                        await Task.Delay(500);

                        for (int i = 0; i < 3; i++)
                        {
                            Vector2Int targetDir = _target.CellPos - CellPos;
                            if (_target != null && targetDir.magnitude <= skillData.shape.range)
                            {
                                int damage = _target.OnDamaged(this, skillData.damage + TotalAttack);
                                Hp += damage / 2;
                            }

                            // 타겟 방향으로 이동
                            CellPos += moveDir;
                            changePacket.Position = PosInfo;   
                            Room.Broadcast(CellPos, changePacket);
                            await Task.Delay(1000);
                        }
                    });
                    coolTick = (int)(1000 / TotalAttackSpeed) * 3;
                }
                else
                {
                    SkillData skillData = null;
                    DataManager.SkillDict.TryGetValue(1, out skillData);
                    S_Skill skillPacket = new S_Skill() { Info = new SkillInfo() };
                    skillPacket.ObjectId = Id;
                    skillPacket.Info.SkillId = skillData.id;
                    Room.Broadcast(CellPos, skillPacket);
                    Skill.StartSkill(this, skillData, target:_target, range:2);
                }
                _coolTick = Environment.TickCount64 + coolTick;
            }
            if (_coolTick > Environment.TickCount64)
                return;
            _coolTick = 0;
        }
    }
}