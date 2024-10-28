using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using Server.Game.Job;
using Server.Game.Room;
using Server.Migrations;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbTransaction = Server.DB.DbTransaction;

namespace Server.Game
{
    public class Monster : GameObject
    {
        public int TemplateId { get; protected set; }
        public Vector2Int SpawnPosition { get; set; }
        public int SpawnId { get; set; }
        public MonsterType MonsterType { get; protected set; }
        public MonsterGrade MonsterGrade { get; protected set; }

        protected Dictionary<int, long> _skillCooldowns = new Dictionary<int, long>();
        public bool IsDead = false;
        public bool IsFlying = false;
        public bool IsAggressive = false;
        public int _patrolRange = 10;
        Random _rand = new Random();
        public Monster()
        {
            ObjectType = GameObjectType.Monster;
        }
        public static Monster CreateMonster(int templateId)
        {
            MonsterData monsterData = null;
            DataManager.MonsterDict.TryGetValue(templateId, out monsterData);
            switch (monsterData.type)
            {
                case MonsterType.Goblin:
                    return new Goblin(monsterData);
                case MonsterType.Orc:
                    return new Orc(monsterData);
                case MonsterType.Orcmage:
                    return new OrcMage(monsterData);
                // 다른 몬스터 타입 추가 가능
                default:
                    return new Monster { TemplateId = templateId, MonsterType = monsterData.type };
            }
        }
        //FSM (Finite State Machine)
        IJob _job;
        private long _stiffEndTick;
        private int _respawnTime = 60000;
        public override void Update()
        {
            switch (State)
            {
                case CreatureState.Idle:
                    UpdateIdle();
                    break;
                case CreatureState.Moving:
                    UpdateMoving();                   
                    break;
                case CreatureState.Skill:
                    UpdateSkill();
                    break;
                case CreatureState.Stiff:
                    UpdateStiff();
                    break;
                case CreatureState.Dead:
                    UpdateDead();
                    break;
            }

            if(Room != null)
            {
                _job = Room.PushAfter(50, Update);
            }
        }
        protected Player _target;
        Vector2Int _dest;
        int _searchRange = 10;
        int _chaseRange = 20;
        long _nextSearchTick = 0;
        long _nextMoveTick = 0;
        long _nextWaitTick = 0;
        protected virtual void UpdateIdle()
        {
            if (_nextSearchTick > Environment.TickCount64)
                return;
            _nextSearchTick = Environment.TickCount64 + 1000;


            Player target = Room.FindCloesetPlayer(CellPos, _searchRange);
            if (target != null)
            {
                _target = target;
                State = CreatureState.Moving;
                if(MonsterGrade == MonsterGrade.Animal)
                {
                    _dest = new Vector2Int(
                    SpawnPosition.x + _rand.Next(-_patrolRange, _patrolRange + 1),
                    SpawnPosition.y + _rand.Next(-_patrolRange, _patrolRange + 1)
                    );
                }                
                return;
            }            

            if (MonsterGrade == MonsterGrade.Animal)
            {
                if (_rand.NextDouble() < 0.5 && _nextWaitTick<= Environment.TickCount64)
                {
                    State = CreatureState.Moving;
                    _dest = new Vector2Int(
                        SpawnPosition.x + _rand.Next(-_patrolRange, _patrolRange + 1),
                        SpawnPosition.y + _rand.Next(-_patrolRange, _patrolRange + 1)
                    );
                }
                else
                {
                    _nextWaitTick = Environment.TickCount64 + _rand.Next(5000, 20000);
                }
            }
        }
        int _skillRange = 1;
        protected virtual void UpdateMoving()
        {
            if(MonsterType == MonsterType.Flying && IsFlying == false)
            {
                IsFlying = true;
            }
            if (_nextMoveTick > Environment.TickCount64)
                return;
            int moveTick = (int)(1000 / (Speed*4));
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

            List<Vector2Int> path = Room.Map.FindPath(CellPos, _target.CellPos, checkObjects: !IsFlying);
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
            Room.Map.ApplyMove(this, path[1]);

            BroadcastMove();
        }
        public bool HandleSkillCool(SkillData skillData, bool attackSpeed)
        {
            if (_skillCooldowns.TryGetValue(skillData.id, out long cooldownEnd))
            {
                if (cooldownEnd > Environment.TickCount64)
                {
                    // 쿨타임이 끝나지 않았음
                    return false;
                }
            }

            if (attackSpeed)
            {
                _skillCooldowns[skillData.id] = (long)(Environment.TickCount64 + 1000 / TotalAttackSpeed);
            }
            else
            {
                _skillCooldowns[skillData.id] = (long)(Environment.TickCount64 + skillData.coolTime);
            }
            return true;
        }
        protected virtual void UpdatePatrol()
        {
            if (CellPos.Equals(_dest))
            {
                if(_rand.NextDouble() < 0.5)
                {
                    State = CreatureState.Idle;
                    BroadcastMove();
                    return;
                }
                else
                {
                    _dest = new Vector2Int(
                        SpawnPosition.x + _rand.Next(-_patrolRange, _patrolRange + 1),
                        SpawnPosition.y + _rand.Next(-_patrolRange, _patrolRange + 1)
                    );
                }               
            }

            List<Vector2Int> path = Room.Map.FindPath(CellPos, _dest, checkObjects: !IsFlying);
            if (path.Count < 2)
            {
                State = CreatureState.Idle;
                BroadcastMove();
                return;
            }

            UpdateDir(path[1]);
            Room.Map.ApplyMove(this, path[1]);

            BroadcastMove();
        }
        protected void UpdateDir(Vector2Int path)
        {
            Dir = GetDirFromVec(path - CellPos);

            if (Dir == MoveDir.Left)
            {
                LookDir = LookDir.LookLeft;
            }
            else if (Dir == MoveDir.Right)
            {
                LookDir = LookDir.LookRight;
            }
        }
        protected void BroadcastMove()
        {
            S_Move movePacket = new S_Move();
            movePacket.ObjectId = Id;
            movePacket.Position = PosInfo;
            Room.Broadcast(CellPos, movePacket);
        }

        long _coolTick = 0;
        protected virtual void UpdateSkill() 
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
                if (canUseSkill==false)
                {
                    State = CreatureState.Moving;
                    BroadcastMove();
                    return;
                }
                LookAt(dir);
                SkillData skillData = null;
                DataManager.SkillDict.TryGetValue(1, out skillData);
                //데미지 판정
                _target.OnDamaged(this, skillData.damage + TotalAttack);
                
                //스킬 사용
                S_Skill skillPacket = new S_Skill() { Info = new SkillInfo()};
                skillPacket.ObjectId = Id;
                skillPacket.Info.SkillId = skillData.id;
                Room.Broadcast(CellPos, skillPacket);

                //쿨타임
                int coolTick = (int)(skillData.coolTime * 1000);
                _coolTick += Environment.TickCount64 + coolTick;
            }
            if (_coolTick > Environment.TickCount64)
                return;
            _coolTick = 0;
        }
        protected virtual void UpdateStiff()
        {
            if (_stiffEndTick == 0)
            {
                _stiffEndTick = Environment.TickCount64 + 1000;
            }
            if (_stiffEndTick > Environment.TickCount64)
                return;
            _stiffEndTick = 0;
            State = CreatureState.Idle;
        }
        protected virtual void UpdateDead() { }

        protected virtual void LookAt(Vector2Int dir)
        {
            MoveDir lookDir = GetDirFromVec(dir);
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
        }
        protected virtual void AttackBySkill(int skillId)
        {
            SkillData skillData = null;
            DataManager.SkillDict.TryGetValue(skillId, out skillData);
            //데미지 판정
            _target.OnDamaged(this, skillData.damage + TotalAttack);

            //스킬 사용
            S_Skill skillPacket = new S_Skill() { Info = new SkillInfo() };
            skillPacket.ObjectId = Id;
            skillPacket.Info.SkillId = skillData.id;
            Room.Broadcast(CellPos, skillPacket);
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
            IsDead = true;

            //죽은 상태로 변경 : job 취소
            if (_job != null)
            {
                _job.Cancel = true;
                _job = null;
            }
            if (Room == null)
                return;

            S_Die diePacket = new S_Die();
            diePacket.ObjectId = Id;
            diePacket.AttackerId = attacker.Id;
            Room.Broadcast(CellPos, diePacket);

            GameRoom room = Room;//Room이 null이 될 수 있으므로 미리 저장  
            GameObject owner = attacker.GetOwner();
            if (owner.ObjectType == GameObjectType.Player)
            {
                MonsterData monsterData = null;
                DataManager.MonsterDict.TryGetValue(TemplateId, out monsterData);
                List<ItemRewardData> rewardData = ItemLogic.GetRandomReward(monsterData.rewards);
                if (rewardData != null)
                {
                    foreach (var item in rewardData)
                    {
                        int count = ItemLogic.GetRewardCount(item);
                        Player player = (Player)owner;
                        S_DropItem dropItemPacket = new S_DropItem()
                        {
                            TemplateId = item.itemId,
                            Count = count,
                            Position = PosInfo
                        };
                        room.Push(room.DropItem, player, dropItemPacket);
                        DbTransaction.ExpNoti(player, Stat.TotalExp);
                        S_ChangeExp changeExpPacket = new S_ChangeExp()
                        {
                            Exp = Stat.TotalExp
                        };
                        player.Session.Send(changeExpPacket);
                    }                    
                }
            }
            
            room.PushAfter(1100, () =>
            {
                if (room != null)
                {
                    room.LeaveGame(Id);
                }
            });

            Stat.Hp = Stat.MaxHp;
            PosInfo.State = CreatureState.Idle;
            PosInfo.MoveDir = MoveDir.Down;            
            room.PushAfter(_respawnTime, () =>
            {
                if (room != null)
                {
                    room.RandomSpawnMonster(TemplateId, 1);
                }
            });
        }
    }
}
