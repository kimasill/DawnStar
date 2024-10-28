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
        private int _skillRange = 7;
        private bool _hasRevived = false;

        public OrcMage(MonsterData data)
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
        protected override async void UpdateSkill()
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

            if(dir.cellDistanceFromZero <= _baseAttackRange)
            {
                BasicAttack();
            }
            else
            {
                if(_rand.Next(0,2) == 0)
                {
                    UseMagicBall();
                }
                else
                {
                    await UseSpotAttack();
                }
            }
        }

        private void BasicAttack()
        {
            // 기본 공격 처리 로직
            SkillData skillData = null;
            DataManager.SkillDict.TryGetValue(13, out skillData);
            if (!HandleSkillCool(skillData, true))
            {
                return;
            }
            //데미지 판정
            _target.OnDamaged(this, skillData.damage + TotalAttack);

            //스킬 사용
            S_Skill skillPacket = new S_Skill() { Info = new SkillInfo() };
            skillPacket.ObjectId = Id;
            skillPacket.Info.SkillId = skillData.id;
            Room.Broadcast(CellPos, skillPacket);
            // 적을 2칸 밀어냄
            Vector2Int direction = (_target.CellPos - CellPos).normalized;
            Vector2Int destPos = new Vector2Int(_target.CellPos.x + direction.x * 2, _target.CellPos.y + direction.y * 2);

            if (Room.Map.ApplyMove(_target, destPos, collision: false))
            {
                _target.CellPos = destPos;
                S_ChangePosition changePosition = new S_ChangePosition();
                changePosition.ObjectId = _target.Id;
                changePosition.Position = _target.PosInfo;
                Room.Broadcast(_target.CellPos, changePosition);
            }
        }

        private void UseMagicBall()
        {
            SkillData skillData = null;
            DataManager.SkillDict.TryGetValue(11, out skillData); 
            S_Skill skillPacket = new S_Skill() { Info = new SkillInfo() };
            skillPacket.ObjectId = Id;
            skillPacket.Info.SkillId = skillData.id;
            Room.Broadcast(CellPos, skillPacket);
            // 스킬 11인 MagicBall 사용
            MagicBall magicBall = new MagicBall();
            magicBall.Owner = this;
            magicBall.Target = _target;
            magicBall.Room = Room; 
            magicBall.Data = skillData;
            magicBall.PosInfo.State = CreatureState.Moving;
            magicBall.PosInfo.MoveDir = PosInfo.MoveDir;
            magicBall.PosInfo.PosX = PosInfo.PosX;
            magicBall.PosInfo.PosY = PosInfo.PosY;
            magicBall.Speed = skillData.projectile.speed;
            Room.Push(Room.EnterGame, magicBall, false);
        }

        // ...

        private async Task UseSpotAttack()
        {
            SkillData skillData = null;
            DataManager.SkillDict.TryGetValue(12, out skillData);
            S_Skill skillPacket = new S_Skill() { Info = new SkillInfo() };
            skillPacket.ObjectId = Id;
            skillPacket.Info.SkillId = skillData.id;            

            List<Vector2Int> skillPos = SkillLogic.GetRandomSpots(this, skillData, Room);
            Room.Broadcast(CellPos, skillPacket);
            foreach (Vector2Int pos in skillPos)
            {
                SpotAttack spot = ObjectManager.Instance.Add<SpotAttack>();
                spot.Data = skillData;
                spot.Owner = this;
                spot.Room = Room;
                spot.PosInfo.PosX = pos.x;
                spot.PosInfo.PosY = pos.y;
                spot.Delay = skillData.spot.delay;
                await Task.Delay(100);
                Room.Push(Room.EnterGame, spot, false);
            }
        }

        public override int OnDamaged(GameObject attacker, int damage)
        {
            if (IsDead)
                return 0;

            State = CreatureState.Stiff;
            return base.OnDamaged(attacker, damage);
        }

        public override void Ondead(GameObject attacker)
        {
            if (_hasRevived)
            {
                base.Ondead(attacker);
                return;
            }

            _hasRevived = true;
            IsDead = false;
            Stat.Hp = Stat.MaxHp / 2; // 부활 시 체력 절반으로 회복
            State = CreatureState.Idle;

            // 부활 패킷 전송
            //S_Revive revivePacket = new S_Revive();
            //revivePacket.ObjectId = Id;
            //Room.Broadcast(CellPos, revivePacket);
        }
    }
}