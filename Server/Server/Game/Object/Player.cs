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
        public VIsionCube Vision { get; private set; }
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
        int _maxPotion = 3;
        public int MaxPotion
        {
            get 
            { 
                if(Stat.MaxPotion >= _maxPotion)
                {
                    return Stat.MaxPotion;
                }
                else
                {
                    return _maxPotion;
                }
            }
            set
            {
                if (value >= _maxPotion)
                {
                    Stat.MaxPotion = value;
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
            get { return _additionalAttack + EquipDamage; }
            set
            {
                _additionalAttack = value;
                SendAdditionalStat();
            }
        }

        public override int AdditionalDefense
        {
            get { return _additionalDefense + EquipDefense; }
            set
            {
                _additionalDefense = value;
                SendAdditionalStat();
            }
        }

        public override float AdditionalInvokeSpeed
        {
            get { return _additionalInvokeSpeed + EquipInvokeSpeed; }
            set
            {
                _additionalInvokeSpeed = value;
                SendAdditionalStat();
            }
        }

        public override float AdditionalCoolTime
        {
            get { return _additionalCoolTime + EquipCoolTime; }
            set
            {
                _additionalCoolTime = value;
                SendAdditionalStat();
            }
        }

        public override int AdditionalCriticalChance
        {
            get { return _additionalCriticalChance + EquipCriticalChance; }
            set
            {
                _additionalCriticalChance = value;
                SendAdditionalStat();
            }
        }

        public override int AdditionalCriticalDamage
        {
            get { return _additionalCriticalDamage + EquipCriticalDamage; }
            set
            {
                _additionalCriticalDamage = value;
                SendAdditionalStat();
            }
        }

        public override int AdditionalAvoidance
        {
            get { return _additionalAvoidance + EquipAvoidance; }
            set
            {
                _additionalAvoidance = value;
                SendAdditionalStat();
            }
        }

        public override int AdditionalAccuracy
        {
            get { return _additionalAccuracy + EquipAccuracy; }
            set
            {
                _additionalAccuracy = value;
                SendAdditionalStat();
            }
        }

        public override float AdditionalAttackSpeed
        {
            get { return _additionalAttackSpeed + EquipAttackSpeed; }
            set
            {
                _additionalAttackSpeed = value;
                SendAdditionalStat();
            }
        }

        public override float AdditionalSpeed
        {
            get { return _additionalSpeed + EquipSpeed; }
            set
            {
                _additionalSpeed = value;
                SendAdditionalStat();
            }
        }

        public override int AdditionalHp
        {
            get { return _additionalHp + EquipHp; }
            set
            {
                _additionalHp = value;
                SendAdditionalStat();
            }
        }

        public override int AdditionalUp
        {
            get { return _additionalUp + EquipUp; }
            set
            {
                _additionalUp = value;
                SendAdditionalStat();
            }
        }
        public int WeaponRange { get; private set; }
        public int EquipDamage { get; private set; }
        public int EquipDefense { get; private set; }
        public int EquipAvoidance { get; private set; }
        public int EquipAccuracy { get; private set; }
        public int EquipCriticalChance { get; private set; }
        public int EquipCriticalDamage { get; private set; }
        public float EquipAttackSpeed { get; private set; }
        public float EquipSpeed { get; private set; }
        public float EquipInvokeSpeed { get; private set; }
        public float EquipCoolTime { get; private set; }
        public int EquipHp { get; private set; }
        public int EquipUp { get; private set; }
        public float EquipUpRegen { get; private set; }
        #endregion

        public Player()
        {
            ObjectType = GameObjectType.Player;
            Vision = new VIsionCube(this);
            IsDead = false;
        }

        public override int OnDamaged(GameObject target, int damage)
        {
            if (IsDead) { return 0; }
            return base.OnDamaged(target, damage);
            //TODO : 피해를 입었을 때 처리 -> 플레이어 스탯에 따라 딜레이 시간 변경
        }
        public override void ChangeHp(int hp)
        {
            if (Room == null)
                return;

            Stat.Hp = hp;
            DbTransaction.HpNoti(this, hp, Room);
        }
        public void ChangeUp(int up)
        {
            if (Room == null)
                return;

            Stat.Up = up;
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

            GameRoom room = Room;//Room이 null이 될 수 있으므로 미리 저장  

            Room.PushAfter(1000, () =>
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
        public void SendAdditionalStat()
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
                }
            };
            Session.Send(additionalStat);
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
            Room.Push(Room.HandleStatChange, this);
        }

        public void OnLeaveGame(bool save)
        {            
            //문제 : 플레이어가 게임을 나가면, 플레이어의 정보를 저장해야 한다.
            // 코드흐름 막아버린다. 데이터 베이스 접근하는 부분이 Core한 부분에 있으면 안됨.
            //해결 : 비동기 처리를 한다. 비동기 처리를 하면, 코드흐름이 막히지 않는다.
            // 다른 쓰레드 하나를 만들어서, 데이터베이스에 저장하는 작업을 한다.
            //TODO : 플레이어의 정보를 데이터베이스에 저장한다.
            //위치정보, 레벨, 경험치, 아이템 정보, 퀘스트 정보 등등
            if(Quest.CurrentQuest != null && Quest.CurrentQuest.Progress<50)
                Room.Push(Room.HandleUpdateQuest,this, Quest.CurrentQuest.TemplateId, 0);
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
                DbTransaction.Instance.Push(DbTransaction.SavePlayerMap, this, mapDb);
            }
            if(Session.ServerState == PlayerServerState.ServerStateSingle)
            {
                Room.Push(Room.ResetRoom);
            }
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
                // 장비를 착용하려는데 이미 착용중인 아이템이 있다면
                if (item.ItemType == ItemType.Weapon)
                {
                    unequipItem = Inven.Find(
                        i => i.Equipped && i.ItemType == ItemType.Weapon);
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

                    // Ring 타입의 아이템은 최대 2개까지 장착 가능
                    if (jewelryType == JewelryType.Ring)
                    {
                        List<Item> equippedRings = Inven.Items.Values
                            .Where(i => i.Equipped && i.ItemType == ItemType.Jewelry && ((Jewelry)i).JewelryType == JewelryType.Ring)
                            .ToList();

                        if (equippedRings.Count >= 2)
                        {
                            // 이미 2개의 반지가 장착되어 있으면 가장 오래된 반지 해제
                            unequipItem = equippedRings.OrderBy(i => i.ItemDbId).First();
                        }
                    }
                    else
                    {
                        // Ring 이외의 장신구는 기존 로직과 동일하게 처리
                        unequipItem = Inven.Find(
                            i => i.Equipped && i.ItemType == ItemType.Jewelry
                            && ((Jewelry)i).JewelryType == jewelryType);
                    }
                }
                if (unequipItem != null)
                {
                    // 해당 아이템을 해제
                    // 메모리 선적용
                    unequipItem.Equipped = false;

                    // DB에 적용
                    DbTransaction.EquipItemNoti(this, unequipItem);

                    // 클라이언트에게 알림
                    S_EquipItem equipOkItem = new S_EquipItem();
                    equipOkItem.ItemDbId = unequipItem.ItemDbId;
                    equipOkItem.Equipped = unequipItem.Equipped;
                    Session.Send(equipOkItem);
                }
            }

            // 메모리 선적용
            item.Equipped = equipPacket.Equipped;

            // DB에 적용
            DbTransaction.EquipItemNoti(this, item);

            // 클라이언트에게 알림
            S_EquipItem equipNoti = new S_EquipItem();
            equipNoti.ItemDbId = equipPacket.ItemDbId;
            equipNoti.Equipped = equipPacket.Equipped;
            Session.Send(equipNoti);

            RefreshAdditionalStat();
            SendAdditionalStat();
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


            foreach (Item item in Inven.Items.Values)
            {
                if (item.Equipped == false)
                    continue;

                MapField<string, string> options = item.Info.Options;
                if (options == null)
                    continue;
                foreach (var option in options)
                {
                    if (Enum.TryParse(option.Key, out ItemOptionType optionType))
                    {
                        switch (optionType)
                        {
                            case ItemOptionType.Damage:
                                EquipDamage += int.Parse(option.Value);
                                break;
                            case ItemOptionType.Defense:
                                EquipDefense += int.Parse(option.Value);
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
                                EquipAttackSpeed += int.Parse(option.Value);
                                break;
                            case ItemOptionType.Speed:
                                EquipSpeed += int.Parse(option.Value);
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
                            case ItemOptionType.Up:
                                EquipUp += int.Parse(option.Value);
                                break;
                            case ItemOptionType.UpRegen:
                                EquipUpRegen += int.Parse(option.Value);
                                break;
                        }
                    }
                }

                switch (item.ItemType)
                {
                    case ItemType.Weapon:
                        EquipDamage += ((Weapon)item).Damage;
                        WeaponRange = ((Weapon)item).Range;
                        EquipAttackSpeed += ((Weapon)item).AttackSpeed;
                        break;
                    case ItemType.Armor:
                        EquipDefense += ((Armor)item).Defense;
                        break;
                }

            }
        }
    }
}
