using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using Server.Game.Job;
using Server.Game.Room;
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
        public int TemplateId { get; private set; }
        public Monster()
        {
            ObjectType = GameObjectType.Monster;
        }

        public void Init(int templateId)
        {
            TemplateId = templateId;

            MonsterData monsterData = null;
            DataManager.MonsterDict.TryGetValue(TemplateId, out monsterData);
            Stat.MergeFrom(monsterData.stat);
            Stat.Hp = monsterData.stat.MaxHp;
            State = CreatureState.Idle;
        }
        //FSM (Finite State Machine)
        IJob _job;
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
                case CreatureState.Dead:
                    UpdateDead();
                    break;
            }

            if(Room != null)
            {
                _job = Room.PushAfter(200, Update);
            }
        }
        Player _target;
        int _searchRange = 10;
        int _chaseRange = 20;
        long _nextSearchTick = 0;
        long _nextMoveTick = 0;
        
        protected virtual void UpdateIdle() 
        {
            if (_nextSearchTick > Environment.TickCount64)
                return;
            _nextSearchTick = Environment.TickCount64 + 1000;

            Player target = Room.FindCloesetPlayer(CellPos, _searchRange);
            

            if (target == null)
                return;

            _target = target;
            State = CreatureState.Moving;
        }
        int _skillRange = 1;
        protected virtual void UpdateMoving() 
        {
            if(_nextMoveTick > Environment.TickCount64)
                return;
            int moveTick = (int)(1000 / Speed);
            _nextMoveTick = Environment.TickCount64 + moveTick;

            if(_target == null || _target.Room != Room)
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
            List<Vector2Int> path = Room.Map.FindPath(CellPos, _target.CellPos, checkObjects: true);
            if (path.Count < 2 || path.Count > _chaseRange)
            {
                _target = null;
                State = CreatureState.Idle;
                BroadcastMove();
                return;
            }
            if(dist <= _skillRange && (dir.x == 0 || dir.y == 0))
            {
                _coolTick = 0;
                State = CreatureState.Skill;
                return;
            }

            //이동
            Dir = GetDirFromVec(path[1] - CellPos);

            if (Dir == MoveDir.Left)
            {
                LookDir = LookDir.LookLeft;
            }
            else if (Dir == MoveDir.Right)
            {
                LookDir = LookDir.LookRight;
            }

            Room.Map.ApplyMove(this, path[1]);

            BroadcastMove();
        }

        void BroadcastMove()
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
                //방향주시
                MoveDir lookDir = GetDirFromVec(dir);
                if (Dir != lookDir)
                {
                    Dir = lookDir;
                    if(Dir == MoveDir.Left)
                    {
                        LookDir = LookDir.LookLeft;
                    }
                    else if(Dir == MoveDir.Right)
                    {
                        LookDir = LookDir.LookRight;
                    }
                    BroadcastMove();
                }
                Skill skillData = null;
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
        protected virtual void UpdateDead() { }

        public override void Ondead(GameObject attacker)
        {
            //죽은 상태로 변경 : job 취소
            if (_job != null)
            {
                _job.Cancel = true;
                _job = null;
            }

            base.Ondead(attacker);
            //Todo : 경험치, 아이템 드랍
            GameObject owner = attacker.GetOwner();
            if (owner.ObjectType == GameObjectType.Player)
            {
                ItemRewardData rewardData = GetRandomReward();
                if (rewardData != null)
                {
                    Player player = (Player)owner;
                    DbTransaction.RewardPlayer(player, rewardData, Room);
                }
            }   
        }

        ItemRewardData GetRandomReward()
        {
            MonsterData monsterData = null;
            DataManager.MonsterDict.TryGetValue(TemplateId, out monsterData);

            int rand = new Random().Next(0, 101);
            int sum = 0;
            foreach(ItemRewardData rewardData in monsterData.rewards)
            {
                sum+=rewardData.probability;
                if(rand <= sum)
                {
                    return rewardData;
                }
            }
            return null;
        }
    }
}
