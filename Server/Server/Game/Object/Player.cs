using Google.Protobuf.Collections;
using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Server.Data;
using Server.DB;
using Server.Game.Room;
using Server.Migrations;
using Server.Utils;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static Server.Game.Item;
using DbTransaction = Server.DB.DbTransaction;

namespace Server.Game
{
    public class Player: GameObject
    {
        #region Properties
        public int PlayerDbId { get; set; }
        public ClientSession Session { get; set; }
        public InterestManagement Vision { get; private set; }
        public Inventory Inven { get; private set; } = new Inventory();
        
        public QuestInventory Quest { get; set; } = new QuestInventory();
        private Dictionary<int, long> _skillCooldowns = new Dictionary<int, long>();
        
        public int Exp {
            get { return Stat.TotalExp; }
            set {
                if (value >= 0)
                {
                    Stat.TotalExp = value;
                }

                StatData stat = null;
                DataManager.StatDict.TryGetValue(Level + 1, out stat);
                if (stat != null && Stat.TotalExp >= stat.TotalExp)
                    Level += 1;
            } 
        }
        public override int Level {
            get { return base.Level; } 
            set { 
                if(value <= Level)
                {
                    return;
                }
                base.Level = value;
                OnlevelUp();
            }  
        }

        public int Gold { 
            get 
            {
                return Inven.GetInvenProperty(10001);
            }
            set
            {
                if (value >= 0)
                {
                    Inven.SetInvenProperty(value,10001,this);
                }
            }
        }
        public int MaxPotion
        {
            get { return Stat.MaxPotion; }
            set
            {
                if (value >= 0)
                {
                    Stat.MaxPotion = value;
                }
            }
        }
        public int PotionPerformance
        {
            get { return Stat.PotionPerformance; }
            set
            {
                if (value >= 0)
                {
                    Stat.PotionPerformance = value;
                }
            }
        }
        public int StatPoint
        {
            get { return Stat.StatPoint; }
            set
            {
                if (value >= 0)
                {
                    Stat.StatPoint = value;
                }
            }
        }

        public override int AdditionalAttack
        {
            get 
            {
                int multiplication = (int)((Stat.Attack + EquipDamage) * (AdditionalDamageMulti / 100));
                return _additionalAttack + EquipDamage + _buffedDamage + multiplication; 
            }
        }
        float AdditionalDamageMulti { get{ return _additionalDamageMulti + EquipDamageMulti; }}
        public override int AdditionalDefense
        {
            get
            {
                int multiplication = (int)((Stat.Defense + EquipDefense) * (AdditionalDefenseMulti / 100));
                return _additionalDefense + EquipDefense + (int)_buffedDefense + multiplication;
            }
        }

        float AdditionalDefenseMulti { get { return _additionalDefenseMulti + EquipDefenseMulti; } }
        public override int AdditionalCriticalChance
        {
            get
            {                
                return _additionalCriticalChance + EquipCriticalChance + _buffedCriticalChance;
            }
        }

        public override int AdditionalCriticalDamage
        {
            get
            {
                return _additionalCriticalDamage + EquipCriticalDamage + _buffedCriticalDamage;
            }
        }

        public override int AdditionalAvoidance
        {
            get
            {
                return _additionalAvoidance + EquipAvoidance + _buffedAvoidance;
            }
        }

        public override int AdditionalAccuracy
        {
            get
            {
                return _additionalAccuracy + EquipAccuracy + _buffedAccuracy;
            }
        }

        public override float AdditionalAttackSpeed
        {
            get
            {
                return _additionalAttackSpeed + EquipAttackSpeed + _buffedAttackSpeed;
            }
        }

        public override float AdditionalSpeed
        {
            get
            {
                return _additionalSpeed + EquipSpeed + _buffedSpeed;
            }
        }
        float AdditionalHpMulti { get { return _additionalHpMulti + EquipHpMulti; } }
        public override int AdditionalHp
        {
            get
            {
                int multiplication = (int)((Stat.Hp + EquipHp) * (AdditionalHpMulti / 100));
                return _additionalHp + EquipHp + _buffedHp + multiplication;
            }
        }
        float AdditionalUpMulti { get { return _additionalUpMulti + EquipUpMulti; } }
        public override int AdditionalUp
        {
            get
            {
                int multiplication = (int)((Stat.Up + EquipUp) * (AdditionalUpMulti / 100));
                return _additionalUp + EquipUp + _buffedUp + multiplication;
            }
        }

        public override int AdditionalHpRegen
        {
            get
            {
                return _additionalHpRegen + EquipHpRegen + _buffedHpRegen;
            }
        }

        public override int AdditionalUpRegen
        {
            get
            {
                return _additionalUpRegen + EquipUpRegen + _buffedUpRegen;
            }
        }

        public int WeaponRange { get; private set; }
        public int EquipDamage { get; private set; }
        public int EquipDamageMulti { get; private set; }
        public int EquipDefense { get; private set; }
        public int EquipDefenseMulti { get; private set; }
        public int EquipAvoidance { get; private set; }
        public int EquipAccuracy { get; private set; }
        public int EquipCriticalChance { get; private set; }
        public int EquipCriticalDamage { get; private set; }
        public float EquipAttackSpeed { get; private set; }
        public float EquipSpeed { get; private set; }
        public float EquipInvokeSpeed { get; private set; }
        public float EquipCoolTime { get; private set; }
        public int EquipHp { get; private set; }
        public int EquipHpMulti { get; private set; }
        public int EquipUp { get; private set; }
        public int EquipUpMulti { get; private set; }
        public int EquipUpRegen { get; private set; }
        public int EquipHpRegen { get; private set; }
        #endregion

        public Player()
        {
            ObjectType = GameObjectType.Player;
            Vision = new InterestManagement(this);
            IsDead = false;
        }
        public override void Update()
        {
            base.Update();
        }
        public override int OnDamaged(GameObject target, int damage)
        {
            if (IsDead) { return 0; }
            return base.OnDamaged(target, damage);
            //TODO : ?쇳빐瑜??낆뿀????泥섎━ -> ?뚮젅?댁뼱 ?ㅽ꺈???곕씪 ?쒕젅???쒓컙 蹂寃?
        }
        public override void ChangeHp(int hp)
        {
            if (Room == null)
                return;
            DbTransaction.HpNoti(this, hp, Room);
        }
        public override void ChangeUp(int up)
        {
            if (Room == null)
                return;
            DbTransaction.UpNoti(this, up, Room);
        }
        public override void Ondead(GameObject attacker)
        {
            if (Room == null)
                return;
            if (IsDead)
                return;

            IsDead = true;

            S_Die diePacket = new S_Die();
            diePacket.ObjectId = Id;
            diePacket.AttackerId = attacker.Id;
            Room.Broadcast(CellPos, diePacket);

            GameRoom room = Room;//Room??null???????덉쑝誘濡?誘몃━ ??? 

            Room.EnqueueAfter(1000, () =>
            {
                if (Room != null)
                {
                    SendRespawnPacket();
                }
            });
        }
        public void SendRespawnPacket()
        {
            S_Respawn respawnPacket = new S_Respawn();            
            Session.Send(respawnPacket);
        }
        

        public void OnlevelUp()
        {
            StatData stat = DataManager.StatDict.GetValueOrDefault(Level);            
            Stat.MaxHp += stat.MaxHp;
            Hp = Stat.MaxHp;
            Stat.Attack += stat.Attack;
            Stat.Speed += stat.Speed;
             Stat.StatPoint += stat.StatPoint;
            DbTransaction.SavePlayerStatus_All(this, Room);
            Room.Enqueue(Room.HandleStatChange, this);
        }

        public void OnLeaveGame(bool save)
        {            
            //臾몄젣 : ?뚮젅?댁뼱媛 寃뚯엫???섍?硫? ?뚮젅?댁뼱???뺣낫瑜???ν빐???쒕떎.
            // 肄붾뱶?먮쫫 留됱븘踰꾨┛?? ?곗씠??踰좎씠???묎렐?섎뒗 遺遺꾩씠 Core??遺遺꾩뿉 ?덉쑝硫??덈맖.
            //?닿껐 : 鍮꾨룞湲?泥섎━瑜??쒕떎. 鍮꾨룞湲?泥섎━瑜??섎㈃, 肄붾뱶?먮쫫??留됲엳吏 ?딅뒗??
            // ?ㅻⅨ ?곕젅???섎굹瑜?留뚮뱾?댁꽌, ?곗씠?곕쿋?댁뒪????ν븯???묒뾽???쒕떎.
            //TODO : ?뚮젅?댁뼱???뺣낫瑜??곗씠?곕쿋?댁뒪????ν븳??
            //?꾩튂?뺣낫, ?덈꺼, 寃쏀뿕移? ?꾩씠???뺣낫, ?섏뒪???뺣낫 ?깅벑
            if(Quest.CurrentQuest != null && Quest.CurrentQuest.Progress<50)
                Room.Enqueue(Room.HandleUpdateQuest,this, Quest.CurrentQuest.TemplateId, 0);
            DbTransaction.SavePlayerStatus_All(this, Room);
            if (save)
            {
                MapDb mapDb = new MapDb()
                {
                    PlayerDbId = PlayerDbId,
                    MapDbId = MapInfo.MapDbId,
                    TemplateId = MapInfo.TemplateId,
                    Scene = MapInfo.Scene,
                    MapName = MapInfo.MapName
                };
                DbTransaction.Instance.Enqueue(DbTransaction.SavePlayerMap, this, mapDb);
            }
            if(Session.ServerState == PlayerServerState.ServerStateSingle)
            {
                Room.Enqueue(Room.ResetRoom);
            }
            GameLogic.Instance.UpdateRoom(Room);
        }

        public void HandleEquipItem(C_EquipItem equipPacket)
        {
            Item item = Inven.Get(equipPacket.ItemDbId);
            if (item.ItemType == ItemType.Goods || item.ItemType == ItemType.Material)
            {
                return;
            }
            if (item == null)
                return;

            if (equipPacket.Equipped)
            {
                Item unequipItem = null;
                // ?λ퉬瑜?李⑹슜?섎젮?붾뜲 ?대? 李⑹슜以묒씤 ?꾩씠?쒖씠 ?덈떎硫?
                if (item.ItemType == ItemType.Weapon)
                {
                    WeaponType weaponType = ((Weapon)item).WeaponType;
                    unequipItem = Inven.Find(
                        i => i.Equipped && i.ItemType == ItemType.Weapon
                        && ((Weapon)i).WeaponType == weaponType);
                }
                else if (item.ItemType == ItemType.Armor)
                {
                    ArmorType armorType = ((Armor)item).ArmorType;
                    unequipItem = Inven.Find(
                        i => i.Equipped && i.ItemType == ItemType.Armor
                        && ((Armor)i).ArmorType == armorType);
                }
                else if (item.ItemType == ItemType.Jewelry)
                {
                    JewelryType jewelryType = ((Jewelry)item).JewelryType;

                    // Ring ??낆쓽 ?꾩씠?쒖? 理쒕? 2媛쒓퉴吏 ?μ갑 媛??
                    if (jewelryType == JewelryType.Ring)
                    {
                        List<Item> equippedRings = Inven.Items.Values
                            .Where(i => i.Equipped && i.ItemType == ItemType.Jewelry && ((Jewelry)i).JewelryType == JewelryType.Ring)
                            .ToList();

                        if (equippedRings.Count >= 2)
                        {
                            // ?대? 2媛쒖쓽 諛섏?媛 ?μ갑?섏뼱 ?덉쑝硫?媛???ㅻ옒??諛섏? ?댁젣
                            unequipItem = equippedRings.OrderBy(i => i.ItemDbId).First();
                        }
                    }
                    else
                    {
                        // Ring ?댁쇅???μ떊援щ뒗 湲곗〈 濡쒖쭅怨??숈씪?섍쾶 泥섎━
                        unequipItem = Inven.Find(
                            i => i.Equipped && i.ItemType == ItemType.Jewelry
                            && ((Jewelry)i).JewelryType == jewelryType);
                    }
                }
                if (unequipItem != null)
                {
                    // ?대떦 ?꾩씠?쒖쓣 ?댁젣
                    // 硫붾え由??좎쟻??
                    unequipItem.Equipped = false;

                    // DB???곸슜
                    DbTransaction.EquipItemNoti(this, unequipItem);

                    // ?대씪?댁뼵?몄뿉寃??뚮┝
                    S_EquipItem equipOkItem = new S_EquipItem();
                    equipOkItem.ObjectId = Id;
                    equipOkItem.ItemDbId = unequipItem.ItemDbId;
                    equipOkItem.Equipped = unequipItem.Equipped;
                    Room.Enqueue(()=> Room.Broadcast(CellPos,equipOkItem));
                }
            }

            // 硫붾え由??좎쟻??
            item.Equipped = equipPacket.Equipped;

            // DB???곸슜
            DbTransaction.EquipItemNoti(this, item);

            // ?대씪?댁뼵?몄뿉寃??뚮┝
            S_EquipItem equipNoti = new S_EquipItem();     
            equipNoti.ObjectId = Id;
            equipNoti.ItemDbId = equipPacket.ItemDbId;
            equipNoti.Equipped = equipPacket.Equipped;
            Room.Enqueue(() => Room.Broadcast(CellPos, equipNoti));

            RefreshAdditionalStat();
            SendAdditionalStat();
        }
        public override void SendAdditionalStat()
        {
            S_ChangeAdditionalStat additionalStat = new S_ChangeAdditionalStat()
            {
                StatInfo = new StatInfo()
                {
                    Attack = AdditionalAttack,
                    Defense = AdditionalDefense,
                    Avoid = AdditionalAvoidance,
                    Accuracy = AdditionalAccuracy,
                    CriticalChance = AdditionalCriticalChance,
                    CriticalDamage = AdditionalCriticalDamage,
                    AttackSpeed = AdditionalAttackSpeed,
                    Speed = AdditionalSpeed,
                    InvokeSpeed = AdditionalInvokeSpeed,
                    CoolTime = AdditionalCoolTime,
                    Hp = AdditionalHp,
                    Up = AdditionalUp,
                    UpRegen = AdditionalUpRegen,
                    HpRegen = AdditionalHpRegen
                }
            };
            Session.Send(additionalStat);
        }
        public void RefreshAdditionalStat()
        {
            EquipDamage = 0;
            EquipDefense = 0;
            EquipAvoidance = 0;
            EquipAccuracy = 0;
            EquipCriticalChance = 0;
            EquipCriticalDamage = 0;
            EquipAttackSpeed = 0;
            EquipSpeed = 0;
            EquipInvokeSpeed = 0;
            EquipCoolTime = 0;
            EquipHp = 0;
            EquipUp = 0;
            EquipUpRegen = 0;
            EquipHpRegen = 0;
            EquipDamageMulti = 0;
            EquipDefenseMulti = 0;
            EquipHpMulti = 0;
            EquipUpMulti = 0;


            foreach (Item item in Inven.Items.Values)
            {
                if (item.Equipped == false)
                    continue;

                MapField<string, string> options = item.Info.Options;
                if (options == null)
                    continue;
                foreach (var option in options)
                {
                    string key = option.Key.Split('_')[0];
                    if (Enum.TryParse(key, out ItemOptionType optionType))
                    {
                        switch (optionType)
                        {
                            case ItemOptionType.Damage:
                                EquipDamage += int.Parse(option.Value);
                                break;
                            case ItemOptionType.Attack:
                                EquipDamageMulti += int.Parse(option.Value);
                                break;
                            case ItemOptionType.Defense:
                                EquipDefense += int.Parse(option.Value);
                                break;
                            case ItemOptionType.DefenseMulti:
                                EquipDefenseMulti += int.Parse(option.Value);
                                break;
                            case ItemOptionType.Avoid:
                                EquipAvoidance += int.Parse(option.Value);
                                break;
                            case ItemOptionType.Accuracy:
                                EquipAccuracy += int.Parse(option.Value);
                                break;
                            case ItemOptionType.CriticalChance:
                                EquipCriticalChance += int.Parse(option.Value);
                                break;
                            case ItemOptionType.CriticalDamage:
                                EquipCriticalDamage += int.Parse(option.Value);
                                break;
                            case ItemOptionType.AttackSpeed:
                                EquipAttackSpeed += float.Parse(option.Value)/10.0f;
                                break;
                            case ItemOptionType.Speed:
                                EquipSpeed += float.Parse(option.Value)/10.0f;
                                break;
                            case ItemOptionType.InvokeSpeed:
                                EquipInvokeSpeed += int.Parse(option.Value);
                                break;
                            case ItemOptionType.CoolTime:
                                EquipCoolTime += int.Parse(option.Value);
                                break;
                            case ItemOptionType.Hp:
                                EquipHp += int.Parse(option.Value);
                                break;
                            case ItemOptionType.HpMulti:
                                EquipHpMulti += int.Parse(option.Value);
                                break;
                            case ItemOptionType.Up:
                                EquipUp += int.Parse(option.Value);
                                break;
                            case ItemOptionType.UpMulti:
                                EquipUpMulti += int.Parse(option.Value);
                                break;
                            case ItemOptionType.UpRegen:
                                EquipUpRegen += int.Parse(option.Value);
                                break;
                            case ItemOptionType.HpRegen:
                                EquipHpRegen += int.Parse(option.Value);
                                break;
                        }
                    }
                }

                switch (item.ItemType)
                {
                    case ItemType.Weapon:
                        EquipDamage += ((Weapon)item).Damage;
                        WeaponRange = ((Weapon)item).Range;
                        EquipAttackSpeed += ((Weapon)item).AttackSpeed/10f;
                        break;
                    case ItemType.Armor:
                        EquipDefense += ((Armor)item).Defense;
                        break;
                }

            }
        }
    }
}
