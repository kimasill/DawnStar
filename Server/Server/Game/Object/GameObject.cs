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
    public class GameObject
    {
        public ObjectInfo Info { get; private set;} = new ObjectInfo();
        public GameRoom Room { get; set; }
        public Skill Skill { get; set; }
        public GameObjectType ObjectType { get; protected set; } = GameObjectType.None;
        public int Id 
        { 
            get { return Info.ObjectId; } 
            set { Info.ObjectId = value; } 
        }
        public PositionInfo PosInfo { get; private set;} = new PositionInfo();
        public StatInfo Stat { get; private set; } = new StatInfo();
        public MapInfo MapInfo { get; set; } = new MapInfo();
        public virtual int Level
        {
            get { return Stat.Level; }
            set { Stat.Level = value; }
        }
        public virtual float TotalInvokeSpeed { get { return Stat.InvokeSpeed; } }
        public virtual int TotalAttack { get { return Stat.Attack; } }
        public virtual int TotalDefense { get { return 0; } }
        public virtual float TotalAttackSpeed { get { return Stat.AttackSpeed; } }
        public virtual int TotalAvoidance { get { return Stat.Avoid; } } 
        public virtual int TotalAccuracy { get { return Stat.Accuracy; } }

        public int Hp
        {
            get { return Stat.Hp; }
            set { Stat.Hp = Math.Clamp(value, 0, Stat.Hp); }
        }
        public MoveDir Dir
        {
            get { return PosInfo.MoveDir; }
            set { PosInfo.MoveDir = value; }
        }

        public LookDir LookDir
        {
            get { return PosInfo.LookDir; }
            set { PosInfo.LookDir = value; }
        }

        public float Speed
        {
            get { return Stat.Speed; }
            set { Stat.Speed = value; }
        }
        public CreatureState State
        {
           get { return PosInfo.State; }
           set { PosInfo.State = value; }
        }
        public Vector2Int CellPos
        {
            get
            {
                return new Vector2Int(PosInfo.PosX, PosInfo.PosY);
            }

            set
            {
                PosInfo.PosX = value.x;
                PosInfo.PosY = value.y;
            }
        }
        public bool DespawnAnim { get; set; } = false;
        public GameObject()
        {
            Info.Position = PosInfo;
            Info.StatInfo = Stat;
        }

        public virtual void Update()
        {

        }
        public Vector2Int GetFrontCellPos()
        {
            return GetFrontCellPos(PosInfo.MoveDir);
        }
        public Vector2Int GetFrontCellPos(MoveDir dir)
        {
            Vector2Int cellPos = CellPos;

            switch (dir)
            {
                case MoveDir.Up:
                    cellPos += Vector2Int.up;
                    break;
                case MoveDir.Down:
                    cellPos += Vector2Int.down;
                    break;
                case MoveDir.Left:
                    cellPos += Vector2Int.left;
                    break;
                case MoveDir.Right:
                    cellPos += Vector2Int.right;
                    break;
            }
            return cellPos;
        }

        public static MoveDir GetDirFromVec(Vector2Int dir)
        {
            if (dir.x > 0)
                return MoveDir.Right;
            else if (dir.x < 0)
                return MoveDir.Left;
            else if (dir.y > 0)
                return MoveDir.Up;
            else
                return MoveDir.Down;
        }

        public Vector2Int GetPosFromLookDir(Vector2Int pos, LookDir lookDir)
        {
            switch (lookDir)
            {
                case LookDir.LookLeft:
                    return pos + Vector2Int.left;
                case LookDir.LookRight:
                    return pos + Vector2Int.right;
            }
            return pos;
        }

        public virtual int OnDamaged(GameObject attacker, int damage)
        {
            if (Room == null)
                return 0;
            damage = Room.CalculateDamage(attacker,Id, damage);            
            Stat.Hp = Math.Max(Stat.Hp - damage, 0);
            Stat.Hp -= damage;
            S_ChangeHp changePacket = new S_ChangeHp();
            changePacket.ObjectId = Id;
            changePacket.Hp = Stat.Hp;
            Console.WriteLine($"OnDamaged : {Id}, {damage}");
            Room.Broadcast(CellPos, changePacket);
            if (Stat.Hp <= 0)
            {
                Ondead(attacker);
            }
            return damage;
        }

        public virtual void OnHealed(int heal, GameObject healer)
        {
            if (Room == null)
                return;

            Stat.Hp = Math.Min(Stat.Hp + heal, Stat.MaxHp);
            S_ChangeHp changePacket = new S_ChangeHp();
            changePacket.ObjectId = Id;
            changePacket.Hp = Stat.Hp;
            Room.Broadcast(CellPos, changePacket);
        }

        public virtual void OnBuffed(SkillData skillData)
        {
            if (Room == null)
                return;

            // TODO : 버프 적용
        }

        public virtual void OnDebuffed(SkillData skillData)
        {
            if (Room == null)
                return;

            // TODO : 디버프 적용
        }

        public virtual void Ondead(GameObject attacker)
        {
            if (Room == null)
                return;

            S_Die diePacket = new S_Die();
            diePacket.ObjectId = Id;
            diePacket.AttackerId = attacker.Id;            
            Room.Broadcast(CellPos, diePacket);

            GameRoom room = Room;//Room이 null이 될 수 있으므로 미리 저장  
            
            room.LeaveGame(Id);
            Stat.Hp = Stat.MaxHp;
            PosInfo.State = CreatureState.Idle;
            PosInfo.MoveDir = MoveDir.Down;

            room.EnterGame(this, false);
        }

        public virtual GameObject GetOwner()
        {
            return this;
        }
    }
}
