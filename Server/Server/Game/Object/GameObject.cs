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
        #region Properties
        public ObjectInfo Info { get; private set;} = new ObjectInfo();
        public GameRoom Room { get; set; }
        public Skill Skill { get; set; }
        public GameObjectType ObjectType { get; protected set; } = GameObjectType.None;
        public bool IsDead { get; set; }
        public int Id 
        { 
            get { return Info.ObjectId; } 
            set { Info.ObjectId = value; } 
        }
        public int TemplateId 
        { 
            get { return Info.TemplateId; }
            set { Info.TemplateId = value; }
        }
        public PositionInfo PosInfo { get; private set;} = new PositionInfo();
        public StatInfo Stat { get; private set; } = new StatInfo();
        public MapInfo MapInfo { get; set; } = new MapInfo();
        public List<BuffInfo> Buffs { get; set; } = new List<BuffInfo>();
        public List<DebuffInfo> Debuffs { get; set; } = new List<DebuffInfo>();
        public virtual int Level
        {
            get { return Stat.Level; }
            set { Stat.Level = value; }
        }
        public virtual float TotalInvokeSpeed { get { return Stat.InvokeSpeed + AdditionalInvokeSpeed; } }
        public virtual int TotalAttack { get { return Stat.Attack + AdditionalAttack;  } }
        public virtual int TotalDefense { get { return Stat.Defense + AdditionalDefense; } }
        public virtual int TotalAvoidance { get { return Stat.Avoid + AdditionalAvoidance; } }
        public virtual int TotalAccuracy { get { return Stat.Accuracy + AdditionalAccuracy; } }
        public virtual float TotalAttackSpeed { get { return Stat.AttackSpeed + AdditionalAttackSpeed; } }
        public virtual int TotalCriticalChance { get { return Stat.CriticalChance + AdditionalCriticalChance; } }
        public virtual int TotalCriticalDamage { get { return (int)MathF.Max(Stat.CriticalDamage + AdditionalCriticalDamage, 2); } }
        public virtual int TotalSpeed { get { return (int)(Stat.Speed + AdditionalSpeed); } }
        public virtual float TotalDamageReduce { get; set; }

        protected int _additionalAttack;
        public virtual int AdditionalAttack { get; set; }

        protected int _additionalDefense;
        public virtual int AdditionalDefense { get; set; }

        protected float _additionalInvokeSpeed;
        public virtual float AdditionalInvokeSpeed { get; set; }

        protected float _additionalCoolTime;
        public virtual float AdditionalCoolTime { get; set; }

        protected int _additionalCriticalChance;
        public virtual int AdditionalCriticalChance { get; set; }

        protected int _additionalCriticalDamage;
        public virtual int AdditionalCriticalDamage { get; set; }

        protected int _additionalAvoidance;
        public virtual int AdditionalAvoidance { get;  set; }

        protected int _additionalAccuracy;
        public virtual int AdditionalAccuracy { get; set; }

        protected float _additionalAttackSpeed;
        public virtual float AdditionalAttackSpeed { get; set; }

        protected float _additionalSpeed;
        public virtual float AdditionalSpeed { get; set; }

        protected int _additionalHp;
        public virtual int AdditionalHp { get; set; }

        protected int _additionalUp;
        public virtual int AdditionalUp { get; set; }
        public virtual float TotalStiffTime { get { return Stat.StiffTime; }}
        public int Hp
        {
            get { return Stat.Hp; }
            set { Stat.Hp = Math.Clamp(value, 0, MaxHp); }
        }

        public int MaxHp
        {
            get { return Stat.MaxHp + AdditionalHp; }            
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
        public bool DespawnAnim { get; set; }
        public GameObject()
        {
            Info.Position = PosInfo;
            Info.StatInfo = Stat;
        }
        #endregion
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
            damage = Room.CalculateDamage(attacker,Id,damage,this);            
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
                IsDead = true;
            }
            return damage;
        }
        public virtual void OnHealed(int heal, GameObject healer)
        {
            if (Room == null)
                return;

            Stat.Hp = Math.Min(Stat.Hp + heal, MaxHp);
            S_ChangeHp changePacket = new S_ChangeHp();
            changePacket.ObjectId = Id;
            changePacket.Hp = Stat.Hp;
            Room.Broadcast(CellPos, changePacket);
        }
        public virtual void OnBuffed(SkillData skillData)
        {
            if (Room == null)
                return;

            ApplyBuff(skillData.buff);
        }
        public virtual void OnDebuffed(SkillData skillData)
        {
            if (Room == null)
                return;

            ApplyDebuff(skillData.debuff);
        }
        public void ApplyBuff(BuffInfo buff)
        {
            if (buff == null)
                return;

            ApplyEffect(buff.name, buff.value, true);
            Buffs.Add(buff);
            Console.WriteLine($"Buff {buff.name} applied with value {buff.value}");
        }
        public void RemoveBuff(BuffInfo buff)
        {
            if (buff == null)
                return;

            ApplyEffect(buff.name, buff.value, false);
            Buffs.Remove(buff);
            Console.WriteLine($"Buff {buff.name} removed");
        }
        public void ApplyDebuff(DebuffInfo debuff)
        {
            if (debuff == null)
                return;

            ApplyEffect(debuff.name, debuff.value, true);
            Debuffs.Add(debuff);
            Console.WriteLine($"Debuff {debuff.name} applied with value {debuff.value}");
        }
        public void RemoveDebuff(DebuffInfo debuff)
        {
            if (debuff == null)
                return;

            ApplyEffect(debuff.name, debuff.value, false);
            Debuffs.Remove(debuff);
            Console.WriteLine($"Debuff {debuff.name} removed");
        }
        private void ApplyEffect(string name, float value, bool isApplying)
        {
            int multiplier = isApplying ? 1 : -1;

            switch (name)
            {
                case "공격력":
                    AdditionalAttack += (int)(TotalAttack * value) * multiplier;
                    break;
                case "방어력":
                    AdditionalDefense += (int)(TotalDefense * value) * multiplier;
                    break;
                case "이동속도":
                    AdditionalSpeed += (int)(TotalSpeed * value) * multiplier;
                    break;
                case "공격속도":
                    AdditionalAttackSpeed += TotalAttackSpeed * value * multiplier;
                    break;
                case "치명타 확률":
                    AdditionalCriticalChance += (int)(TotalCriticalChance * value) * multiplier;
                    break;
                case "치명타 피해":
                    AdditionalCriticalDamage += (int)(TotalCriticalDamage * value) * multiplier;
                    break;
                case "회피율":
                    AdditionalAvoidance += (int)(TotalAvoidance * value) * multiplier;
                    break;
                case "명중률":
                    AdditionalAccuracy += (int)(TotalAccuracy * value) * multiplier;
                    break;
                case "생명력":
                    AdditionalHp += (int)(Stat.MaxHp * value) * multiplier;
                    break;
                case "회복":
                    if (isApplying)
                        OnHealed((int)(Stat.MaxHp * value), this);
                    break;
            }
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
