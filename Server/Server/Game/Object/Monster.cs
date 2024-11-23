using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using Server.Game.Job;
using Server.Game.Object.Monsters;
using Server.Game.Room;
using Server.Migrations;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DbTransaction = Server.DB.DbTransaction;

namespace Server.Game
{
    public class Monster : GameObject
    {
        public Vector2Int SpawnPosition { get; set; }
        public int SpawnId { get; set; }
        public MonsterType MonsterType { get; protected set; }
        public MonsterGrade MonsterGrade { get; protected set; }
        protected Dictionary<int, long> _skillCooldowns = new Dictionary<int, long>();
        public bool IsFlying = false;
        public int _patrolRange = 10;
        private const int PushTime = 200;
        Random _rand = new Random();
        public Monster(MonsterData monsterData)
        {
            Initialize(monsterData);
        }
        protected void Initialize(MonsterData monsterData)
        {
            ObjectType = GameObjectType.Monster;
            MonsterGrade = monsterData.grade;
            MonsterType = monsterData.type;
            Info.Name = monsterData.name;
            TemplateId = monsterData.id;
            Stat.MergeFrom(monsterData.stat);
            Stat.Hp = monsterData.stat.MaxHp;
            State = CreatureState.Idle;
            Skill = new Skill(this);

            if (MonsterType == MonsterType.Flying)
            {
                IsFlying = true;
            }
            IsDead = false;
        }
        public static Monster CreateMonster(int templateId)
        {
            MonsterData monsterData = null;
            Monster monster = null;
            DataManager.MonsterDict.TryGetValue(templateId, out monsterData);            
            switch (monsterData.type)
            {
                case MonsterType.Goblin:
                    monster = new Goblin(monsterData);
                    break;
                case MonsterType.Orc:
                    monster = new Orc(monsterData);
                    break;
                case MonsterType.Orcmage:
                    monster = new OrcMage(monsterData);
                    break;
                case MonsterType.Satyr:
                    monster = new Satyr(monsterData);
                    break;
                case MonsterType.SkeletonWarrior:
                    monster = new SkeletonWarrior(monsterData);
                    break;
                case MonsterType.SkeletonMage:
                    monster = new SkeletonMage(monsterData);
                    break;
                case MonsterType.PrisonKeeper:
                    monster = new PrisonKeeper(monsterData);
                    break;
                default:
                    monster = new Monster(monsterData);
                    break;
            }
            monster.ObjectType = GameObjectType.Monster;
            monster.Id = ObjectManager.Instance.GenerateId(GameObjectType.Monster);
            return monster;
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
                _job = Room.PushAfter(PushTime, Update);
            }
        }
        protected Player _target;
        protected Vector2Int _dest;
        protected int _searchRange = 10;
        protected int _chaseRange = 20;
        protected long _nextSearchTick = 0;
        protected long _nextMoveTick = 0;
        protected long _nextWaitTick = 0;
        protected virtual void UpdateIdle()
        {
            if (_nextSearchTick > Environment.TickCount64)
                return;
            _nextSearchTick = Environment.TickCount64 + 1000;

            _dest = CellPos;

            Player target = Room.FindCloesetPlayer(CellPos, _searchRange);
            if (target != null)
            {
                _target = target;                
                State = CreatureState.Moving;
                return;                
            }
        }
        int _skillRange = 1;
        protected virtual void UpdateMoving()
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
        protected virtual void UpdatePatrol()
        {
            if (_dest.Equals(default(Vector2Int)) || _dest.Equals(CellPos))
            {
                // 새로운 목적지를 설정
                while (true)
                {
                    _dest = new Vector2Int(
                        SpawnPosition.x + _rand.Next(-_patrolRange, _patrolRange + 1),
                        SpawnPosition.y + _rand.Next(-_patrolRange, _patrolRange + 1)
                    );
                    if (Room.Map.CanGo(_dest, false))
                    {
                        break;
                    }
                }
            }
            if (_target == null || _target.Room != Room)
            {
                _target = null;
                State = CreatureState.Idle;
                BroadcastMove();
                return;
            }

            Vector2Int dir = _dest - CellPos;
            int dist = dir.cellDistanceFromZero;
            if (dist == 0 || dist > _chaseRange)
            {
                State = CreatureState.Idle;
                BroadcastMove();
                return;
            }
            List<Vector2Int> path = Room.Map.FindPath(CellPos, _dest);
            if (path.Count < 2 || path.Count > _chaseRange)
            {
                _target = null;
                State = CreatureState.Idle;
                BroadcastMove();
                return;
            }
            UpdateDir(path[1]);
            Room.Map.ApplyMove(this, path[1], checkObjects:false);

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
                int coolTick = (int)(1000/TotalAttackSpeed);
                _coolTick = Environment.TickCount64 + coolTick;
            }
            if (_coolTick > Environment.TickCount64)
                return;
            _coolTick = 0;
        }
        protected virtual void UpdateStiff()
        {
            if (_stiffEndTick == 0)
            {
                _stiffEndTick = (long)(Environment.TickCount64 + 1000*TotalStiffTime - PushTime);
            }

            if (_stiffEndTick > Environment.TickCount64)
            {
                return;
            }

            // 1초가 지나면 Idle 상태로 전환
            _stiffEndTick = 0;
            State = CreatureState.Idle;
            BroadcastMove();
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
            MonsterData monsterData = null;
            DataManager.MonsterDict.TryGetValue(TemplateId, out monsterData);
            if (owner.ObjectType == GameObjectType.Player)
            {
                Player player = (Player)owner;
                DbTransaction.ExpNoti(player, Stat.TotalExp);

                List<ItemRewardData> rewardData = ItemLogic.GetRandomReward(monsterData.rewards);
                if (rewardData != null)
                {
                    foreach (var item in rewardData)
                    {
                        int count = ItemLogic.GetRewardCount(item);                        
                        S_DropItem dropItemPacket = new S_DropItem()
                        {
                            TemplateId = item.itemId,
                            Count = count,
                            Position = PosInfo
                        };
                        room.Push(room.DropItem, player, dropItemPacket);
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
            if(monsterData.grade == MonsterGrade.Elite)
            {
                room.CheckBossRewards(_target, monsterData.rewardBoxId);
                _respawnTime = _respawnTime * 10;
            }                
            else if (monsterData.grade == MonsterGrade.Boss)
            {
                room.CheckBossRewards(_target, monsterData.rewardBoxId);
                return;
            }
                
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
