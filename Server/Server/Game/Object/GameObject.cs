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
        private readonly object _debuffsLock = new object();
        private readonly object _buffsLock = new object();
        public Action DamageAction;
        public Dictionary<string, Action> DamageEffects { get; private set; } = new Dictionary<string, Action>();
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
        public float DefenseRate
        {
            get
            {
                float defenseFactor = (float)Math.Pow(TotalDefense, 0.75f);
                return defenseFactor / (defenseFactor + 125);
            }
        }
        public virtual int TotalAvoidance { get { return Stat.Avoid + AdditionalAvoidance; } }
        public float AvoidanceRate 
        { 
            get {
                float avoidFactor = (float)Math.Pow(TotalAvoidance, 0.65f);
                return avoidFactor / (avoidFactor + 100);                
            } 
        }
        public virtual int TotalAccuracy { get { return Stat.Accuracy + AdditionalAccuracy; } }
        public float AccuracyRate
        {
            get
            {
                float accuracyFactor = (float)Math.Pow(TotalAccuracy, 0.55f);
                return accuracyFactor / (accuracyFactor + 100);
            }
        }
        public virtual float TotalAttackSpeed { get { return Stat.AttackSpeed + AdditionalAttackSpeed; } }
        public virtual int TotalCriticalChance { get { return Stat.CriticalChance + AdditionalCriticalChance; } }
        public virtual int TotalCriticalDamage { get { return Stat.CriticalDamage + AdditionalCriticalDamage; } }
        public virtual int TotalSpeed { get { return (int)(Stat.Speed + AdditionalSpeed); } }
        public virtual float TotalDamageReduce { get; set; }

        protected int _additionalAttack;
        protected int _buffedDamage;        
        protected int _additionalDefense;
        protected float _buffedDefense;
        protected float _additionalInvokeSpeed;
        protected float _additionalCoolTime;
        protected int _additionalCriticalChance;
        protected int _buffedCriticalChance;
        protected int _additionalCriticalDamage;
        protected int _buffedCriticalDamage;
        protected int _additionalAvoidance;
        protected int _buffedAvoidance;
        protected int _additionalAccuracy;
        protected int _buffedAccuracy;
        protected float _additionalAttackSpeed;
        protected float _buffedAttackSpeed;
        protected float _additionalSpeed;
        protected float _buffedSpeed;
        protected int _additionalHp;
        protected int _buffedHp;
        protected int _additionalUp;
        protected int _buffedUp;
        protected int _additionalHpRegen;
        protected int _buffedHpRegen;
        protected int _additionalUpRegen;
        protected int _buffedUpRegen;
        protected int _additionalDamageMulti;
        protected int _additionalDefenseMulti;
        protected int _additionalHpMulti;
        protected int _additionalUpMulti;
        public virtual int AdditionalAttack { get { return _additionalAttack + _buffedDamage; } }
        public virtual int AdditionalDefense { get { return _additionalDefense + (int)_buffedDefense; } }
        public virtual float AdditionalInvokeSpeed { get { return _additionalInvokeSpeed; } set { _additionalInvokeSpeed = value; } }
        public virtual float AdditionalCoolTime { get { return _additionalCoolTime; } }
        public virtual int AdditionalCriticalChance { get { return _additionalCriticalChance + _buffedCriticalChance; } }
        public virtual int AdditionalCriticalDamage { get { return _additionalCriticalDamage + _buffedCriticalDamage; } }
        public virtual int AdditionalAvoidance { get { return _additionalAvoidance + _buffedAvoidance; } }
        public virtual int AdditionalAccuracy { get { return _additionalAccuracy + _buffedAccuracy; } }
        public virtual float AdditionalAttackSpeed { get { return _additionalAttackSpeed + _buffedAttackSpeed; } }
        public virtual float AdditionalSpeed { get { return _additionalSpeed + _buffedSpeed; } }
        public virtual int AdditionalHp { get { return _additionalHp + _buffedHp; } }
        public virtual int AdditionalUp { get { return _additionalUp + _buffedUp; } }
        public virtual int AdditionalHpRegen { get { return _additionalHpRegen + _buffedHpRegen; } }
        public virtual int AdditionalUpRegen { get { return _additionalUpRegen + _buffedUpRegen; } }
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
            get { return Stat.Up; }
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
        public Vector2Int GetFrontCellPos(int distance)
        {
            return GetFrontCellPos(PosInfo.MoveDir, distance);
        }
        public Vector2Int GetFrontCellPos()
        {
            return GetFrontCellPos(PosInfo.MoveDir);
        }
        public Vector2Int GetFrontCellPos(MoveDir dir, int distance = 0)
        {
            Vector2Int cellPos = CellPos;

            switch (dir)
            {
                case MoveDir.Up:
                    cellPos += Vector2Int.up;
                    cellPos += new Vector2Int(0, distance);
                    break;
                case MoveDir.Down:
                    cellPos += Vector2Int.down;
                    cellPos += new Vector2Int(0, -distance);
                    break;
                case MoveDir.Left:
                    cellPos += Vector2Int.left;
                    cellPos += new Vector2Int(-distance, 0);
                    break;
                case MoveDir.Right:
                    cellPos += Vector2Int.right;
                    cellPos += new Vector2Int(distance, 0);
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
            return OnDamaged(attacker, damage, true);
        }
        public virtual int OnDamaged(GameObject attacker, int damage, bool action = true)
        {
            if (Room == null)
                return 0;
            damage = Room.CalculateDamage(attacker,Id,damage,this);      
            int hp = Math.Max(Stat.Hp - damage, 0);
            ChangeHp(hp);
            if (action)
                DamageAction?.Invoke();
            if (hp <= 0)
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

            Hp = hp;
            S_ChangeHp changePacket = new S_ChangeHp();
            changePacket.ObjectId = Id;
            changePacket.Hp = Stat.Hp;
            Room.Broadcast(CellPos, changePacket);
        }
        public virtual void ChangeUp(int up)
        {
            if (Room == null)
                return;

            Up = up;
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
        public async void Effect(GameObject target, string prefab, int skillId = 0, int count = 1, float delay = 0)
         {
            if (target == null)
                return;

            for (int i = 0; i < count; i++)
            {
                S_Effect effectPacket = new S_Effect();
                effectPacket.SkillId = skillId;
                effectPacket.ObjectId = target.Id;
                effectPacket.Prefab = prefab;
                Room.Broadcast(target.CellPos, effectPacket);

                await Task.Delay((int)(delay * 1000));
            }
        }

        public void ApplyBuff(BuffInfo buff, int count = 1)
        {
            if (buff == null)
                return;
            for (int i = 0; i < count; i++)
            {
                float afterValue = 0;
                afterValue = ApplyEffect(true, buff.value, buff: buff);

                lock (_buffsLock)
                {
                    if (Buffs.ContainsKey(buff.id))
                    {
                        Buffs[buff.id] += afterValue;
                    }
                    else
                    {
                        Buffs[buff.id] = afterValue;
                    }
                }

                float tValue = 0;
                lock (_buffsLock)
                    Buffs.TryGetValue(buff.id, out tValue);

                S_Buff buffPacket = new S_Buff() 
                {
                    ObjectId = Id,
                    BuffId = buff.id,
                    Value = tValue
                };
                Room.Broadcast(CellPos, buffPacket);

                Console.WriteLine($"Buff {buff.name} applied with value {afterValue}");
                if(Room != null)
                {
                    Room.PushAfter(buff.duration * 1000, () =>
                    {
                        RemoveBuff(buff, afterValue, count);
                    });
                }
            }
        }
        public void RemoveBuff(BuffInfo buff, float value,  int count = 1)
        {
            if (Room == null)
                return;
            if (Buffs.ContainsKey(buff.id) == false)
                return;

            for (int i = 0; i < count; i++)
            {
                float tValue = 0;
                Buffs.TryGetValue(buff.id, out tValue);
                tValue -= value;
                ApplyEffect(false, value, buff: buff);

                if(tValue <= 0)
                    Buffs.Remove(buff.id);

                S_Buff buffPacket = new S_Buff()
                {
                    ObjectId = Id,
                    BuffId = buff.id,
                    Value = tValue
                };

                Room.Broadcast(CellPos, buffPacket);

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
                else value = ApplyEffect(true, value, debuff:debuff);

                lock (_debuffsLock)
                {
                    if (Debuffs.ContainsKey(debuff.id))
                    {
                        Debuffs[debuff.id] += value;
                    }
                    else
                    {
                        Debuffs[debuff.id] = value;
                    }
                }

                float tValue = 0;
                lock (_debuffsLock)
                    Debuffs.TryGetValue(debuff.id, out tValue);

                S_Buff buffPacket = new S_Buff()
                {
                    ObjectId = Id,
                    DebuffId = debuff.id,
                    Value = tValue
                };
                Console.WriteLine($"Debuff {debuff.name} applied with value {debuff.value}");
                if(Room != null)
                {
                    Room.PushAfter(debuff.duration * 1000, () =>
                    {
                        RemoveDebuff(debuff, value, count);
                    });
                }
            }
        }
        public void RemoveDebuff(DebuffInfo debuff, float value, int count = 1)
        {
            if (Room == null)
                return;
            if (Debuffs.ContainsKey(debuff.id) ==false)
                return;

            for (int i = 0; i < count; i++)
            {
                float tValue = 0;
                Debuffs.TryGetValue(debuff.id, out tValue);
                ApplyEffect(false, value, debuff: debuff);

                if (tValue <= 0)
                    Debuffs.Remove(debuff.id);

                S_Buff buffPacket = new S_Buff()
                {
                    ObjectId = Id,
                    DebuffId = debuff.id,
                    Value = tValue
                };
                Room.Broadcast(CellPos, buffPacket);
                Console.WriteLine($"Debuff {debuff.id} removed");
            }
        }
        private float ApplyEffect(bool isApplying, float value, BuffInfo buff = null, DebuffInfo debuff = null)
        {
            int multiplier = isApplying ? 1 : -1;
            float applyValue = 0;
            string buffName = buff != null ? buff.name : debuff.name;
            
            if (buffName == "부패")
            {
                buffName = "방어력";
            }
            switch (buffName)
            {
                case "공격력":
                    applyValue = isApplying ? TotalAttack * value * multiplier : value * multiplier;
                    _buffedDamage += (int)applyValue;
                    break;
                case "방어력":
                    applyValue = isApplying ? TotalDefense * value * multiplier : value * multiplier;
                    _buffedDefense += (int)applyValue;
                    break;
                case "공격 속도":
                    applyValue = isApplying ? TotalAttackSpeed * value * multiplier: value * multiplier;
                    _buffedAttackSpeed += applyValue;
                    break;
                case "치명타 확률":
                    applyValue = isApplying ? value * multiplier: value * multiplier;
                    _buffedCriticalChance += (int)applyValue;
                    break;
                case "치명타 피해":
                    applyValue = isApplying ? TotalCriticalDamage * value * multiplier: value * multiplier;
                    _buffedCriticalDamage += (int)applyValue;
                    break;
                case "회피":
                    applyValue = isApplying ? TotalAvoidance * value * multiplier : value * multiplier;
                    _buffedAvoidance += (int)applyValue;
                    break;
                case "명중":
                    applyValue = isApplying ? TotalAccuracy * value * multiplier : value * multiplier;
                    _buffedAccuracy += (int)applyValue;
                    break;
                case "이동 속도":
                    applyValue = isApplying ? TotalSpeed * value * multiplier : value * multiplier;
                    _buffedSpeed += applyValue;
                    break;
                case "체력":
                    applyValue = isApplying ? MaxHp * value * multiplier : value * multiplier;
                    _buffedHp += (int)applyValue;
                    break;
                case "회복":
                    if (isApplying)
                        OnHealed((int)value, this);
                    break;
                case "데미지 감소":
                    applyValue = isApplying ? value * multiplier : value * multiplier;
                    TotalDamageReduce += applyValue;
                    break;
                case "주시":
                    if (isApplying)
                    {
                        Action action = () =>
                        {
                            if (Room == null)
                                return;
                            Effect(this, debuff.damagePrefab);
                            Room.PushAfter((int)(debuff.damageTerms[0] * 1000), () =>
                            {
                                if (Room == null)
                                    return;
                                int damage = (int)(TotalAttack * debuff.additionalDamage);
                                OnDamaged(this, damage, false);
                            });
                        };
                        if (DamageEffects.ContainsKey(buffName) == false)
                            DamageEffects[buffName] = action;
                        DamageAction += action;
                    }
                    else
                    {
                        if (DamageEffects.ContainsKey(buffName))
                        {
                            DamageAction -= DamageEffects[buffName];
                            DamageEffects.Remove(buffName);
                        }
                    }
                    applyValue = debuff.value;
                    break;
                case "출혈":
                    if (isApplying)
                    {
                        for(int i = 0; i < debuff.duration; i++)
                        {
                            Room.PushAfter(i * 1000, () =>
                            {
                                if (Room == null)
                                    return;
                                Effect(this, debuff.damagePrefab);
                                int damage = (int)(TotalAttack * debuff.value);
                                OnDamaged(this, damage, false);
                            });
                        }
                    }
                    else
                    {
                        if (DamageEffects.ContainsKey(buffName))
                        {
                            DamageEffects.Remove(buffName);
                        }
                    }
                    applyValue = debuff.value;
                    break;
                default:
                    applyValue = value;
                    break;
            }
            SendAdditionalStat();
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
            if (room == null)
                return;
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

        public virtual void SendAdditionalStat()
        {
        }
    }
}
