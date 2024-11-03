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
        int _skillId = 0;
        public OrcMage(MonsterData data) : base(data)
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
        Random _rand = new Random();
        protected override void UpdateSkill()
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

            if (dir.cellDistanceFromZero <= _baseAttackRange)
            {
                _skillId = 11;
            }
            else
            {
                if (_rand.Next(0, 2) == 0)
                {
                    _skillId = 12;
                }
                else
                {
                    _skillId = 13;
                }
            }
            SkillData skillData = null;
            DataManager.SkillDict.TryGetValue(_skillId, out skillData);
            S_Skill skillPacket = new S_Skill() { Info = new SkillInfo() };
            Skill.StartSkill(this, skillData, _target);

            skillPacket.ObjectId = Id;
            skillPacket.Info.SkillId = skillData.id;
            Room.Broadcast(CellPos, skillPacket);
        }
    }
}