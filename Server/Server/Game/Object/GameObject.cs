using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game.Room;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static System.Net.Mime.MediaTypeNames;

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
        public Dictionary<int, float> Buffs { get; set; } = new Dictionary<int, float>();
        public Dictionary<int, float> Debuffs { get; set; } = new Dictionary<int, float>();
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
        protected int _additionalHpRegen;
        public virtual int AdditionalHpRegen { get; set; }
        protected int _additionalUpRegen;
        public virtual int AdditionalUpRegen { get; set; }
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
        public int Up
        {
            get { return Stat.Up + AdditionalUp; }
            set { Stat.Up = Math.Clamp(value, 0, MaxUp); }
        }
        public int MaxUp
        {
            get { return Stat.MaxUp + AdditionalUp; }
        }
        public int HpRegen
        {
            get { return Stat.HpRegen + AdditionalHpRegen; }
            set { Stat.HpRegen = value; }
        }
        public int UpRegen
        {
            get { return Stat.UpRegen + AdditionalUpRegen; }
            set { Stat.UpRegen = value; }
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
            UpdateHp();
            UpdateUp();
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
            int hp = Math.Max(Stat.Hp - damage, 0);
            ChangeHp(hp); 

            if (Stat.Hp <= 0)
            {
                if(ObjectType == GameObjectType.Monster)
                {
                    if (attacker is not Player)
                    {
                        Stat.Hp = 0;
                        return damage;
                    }
                }
                Ondead(attacker);
                IsDead = true;
            }
            return damage;
        }
        public virtual void UpdateHp()
        {
            if(Room == null)
                return;
            int hp = Math.Min(Hp + HpRegen, MaxHp);
            ChangeHp(hp);
            Room.PushAfter(1000, () => UpdateHp());
        }
        public virtual void UpdateUp()
        {
            if (Room == null)
                return;
            int up = Math.Min(Up + UpRegen, MaxUp);
            ChangeUp(up);
            Room.PushAfter(1000, () => UpdateUp());
        }
        public virtual void ChangeHp(int hp)
        {
            if (Room == null)
                return;

            Stat.Hp = hp;
            S_ChangeHp changePacket = new S_ChangeHp();
            changePacket.ObjectId = Id;
            changePacket.Hp = Stat.Hp;
            Room.Broadcast(CellPos, changePacket);
        }
        public virtual void ChangeUp(int up)
        {
            if (Room == null)
                return;

            Stat.Up = up;
            S_ChangeUp changePacket = new S_ChangeUp();
            changePacket.ObjectId = Id;
            changePacket.Up = Stat.Up;
            Room.Broadcast(CellPos, changePacket);
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
        public void ApplyBuff(BuffInfo buff, int count = 1)
        {
            if (buff == null)
                return;
            for(int i = 0; i < count; i++)
            {
                float afterValue = 0;
                afterValue = ApplyEffect(buff.name, buff.value, true);
                if (!Buffs.ContainsKey(buff.id))
                {
                    Buffs[buff.id] = 0;
                }
                Buffs[buff.id] += afterValue;

                float tValue = 0;
                Buffs.TryGetValue(buff.id, out tValue);

                S_Buff buffPacket = new S_Buff() 
                {
                    ObjectId = Id,
                    BuffId = buff.id,
                    Value = tValue
                };
                Room.Broadcast(CellPos, buffPacket);
                Console.WriteLine($"Buff {buff.name} applied with value {afterValue}");
                Room.PushAfter(buff.duration * 1000, () =>
                {
                    RemoveBuff(buff, afterValue, count);
                });
            }
        }
        public void RemoveBuff(BuffInfo buff, float value,  int count = 1)
        {
            if (Buffs.ContainsKey(buff.id) == false)
                return;

            for (int i = 0; i < count; i++)
            {
                float tValue = 0;
                Buffs.TryGetValue(buff.id, out tValue);
                tValue -= value;
                ApplyEffect(buff.name, value, false);

                if(tValue <= 0)
                    Buffs.Remove(buff.id);

                S_Buff buffPacket = new S_Buff()
                {
                    ObjectId = Id,
                    BuffId = buff.id,
                    Value = tValue
                };
                Console.WriteLine($"Buff {buff.id} removed");
            }
        }
        public void ApplyDebuff(DebuffInfo debuff, int count = 1, GameObject suspect = null)
        {
            if (debuff == null)
                return;

            for (int i = 0; i < count; i++)
            {
                float value = 0;
                if (suspect != null)
                {
                    value = suspect.TotalAttack * debuff.value;
                }
                else value = ApplyEffect(debuff.name, debuff.value, true);

                if (!Debuffs.ContainsKey(debuff.id))
                {
                    Debuffs[debuff.id] = 0;
                }
                Debuffs[debuff.id] += value;

                float tValue = 0;
                Debuffs.TryGetValue(debuff.id, out tValue);

                S_Buff buffPacket = new S_Buff()
                {
                    ObjectId = Id,
                    DebuffId = debuff.id,
                    Value = tValue
                };
                Console.WriteLine($"Debuff {debuff.name} applied with value {debuff.value}");
                Room.PushAfter(debuff.duration * 1000, () =>
                {
                    RemoveDebuff(debuff, value, count);
                });
            }
        }
        public void RemoveDebuff(DebuffInfo debuff, float value, int count = 1)
        {
            if (Debuffs.ContainsKey(debuff.id) ==false)
                return;

            for (int i = 0; i < count; i++)
            {
                float tValue = 0;
                Debuffs.TryGetValue(debuff.id, out tValue);
                ApplyEffect(debuff.name, value, false);

                if (tValue <= 0)
                    Debuffs.Remove(debuff.id);

                S_Buff buffPacket = new S_Buff()
                {
                    ObjectId = Id,
                    DebuffId = debuff.id,
                    Value = tValue
                };
                Console.WriteLine($"Debuff {debuff.id} removed");
            }
        }
        private float ApplyEffect(string buff, float value, bool isApplying)
        {
            int multiplier = isApplying ? 1 : -1;
            float applyValue = 0;
            if(buff == "부패")
            {
                buff = "방어력";
            }
            switch (buff)
            {
                case "공격력":
                    applyValue = isApplying ? TotalAttack * value * multiplier : value * multiplier;
                    AdditionalAttack += (int)applyValue;
                    break;
                case "방어력":
                    applyValue = isApplying ? TotalDefense * value * multiplier : value * multiplier;
                    AdditionalDefense += (int)applyValue;
                    break;
                case "공격 속도":
                    applyValue = value * multiplier * 100;
                    AdditionalAttackSpeed += applyValue;
                    break;
                case "치명타 확률":
                    applyValue = value * multiplier * 100;
                    AdditionalCriticalChance += (int)applyValue;
                    break;
                case "치명타 피해":
                    applyValue = value * multiplier * 100;
                    AdditionalCriticalDamage += (int)applyValue;
                    break;
                case "회피":
                    applyValue = value * multiplier * 100;
                    AdditionalAvoidance += (int)applyValue;
                    break;
                case "명중":
                    applyValue = value * multiplier * 100;
                    AdditionalAccuracy += (int)applyValue;
                    break;
                case "이동 속도":
                    applyValue = isApplying ? TotalSpeed * value * multiplier : value * multiplier;
                    AdditionalSpeed += applyValue;
                    break;
                case "체력":
                    applyValue = isApplying ? MaxHp * value * multiplier : value * multiplier;
                    AdditionalHp += (int)applyValue;
                    break;
                case "회복":
                    if (isApplying)
                        OnHealed((int)value, this);
                    break;
                case "데미지 감소":
                    applyValue = isApplying ? TotalDamageReduce * value * multiplier : value * multiplier;
                    TotalDamageReduce += applyValue;
                    break;
                default:
                    applyValue = value;
                    break;
            }
            return applyValue;

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
