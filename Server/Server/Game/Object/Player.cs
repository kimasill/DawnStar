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
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static Server.Game.Item;

namespace Server.Game
{
    public class Player: GameObject
    {
        public int PlayerDbId { get; set; }
        public ClientSession Session { get; set; }
        public VIsionCube Vision { get; private set; }
        public Inventory Inven { get; private set; } = new Inventory();
        public QuestInventory Quest { get; set; } = new QuestInventory();
        private Dictionary<int, long> _skillCooldowns = new Dictionary<int, long>();
        public bool IsDead { get; set; }
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
        public int WeaponDamage { get; private set; }
        public int ArmorDef { get; private set; }
        public int AdditionalAvoidance { get; private set; }
        public int AdditionalAccuracy { get; private set; }
        public int AdditionalCriticalChance { get; private set; }
        public int AdditionalCriticalDamage { get; private set; }
        public float AdditionalAttackSpeed { get; private set; }

        public override int TotalAttack { get { return Stat.Attack + WeaponDamage; } }
        public override int TotalDefense { get { return ArmorDef; } }        
        public virtual int TotalCriticalChance { get {return Stat.CriticalChance + AdditionalCriticalChance ; } }
        public virtual int TotalCriticalDamage { get { return Stat.CriticalDamage + AdditionalCriticalDamage; } }
        public override int TotalAvoidance { get { return Stat.Avoid + AdditionalAvoidance; } }
        public override int TotalAccuracy { get { return Stat.Accuracy + AdditionalAccuracy; } }
        public override float TotalAttackSpeed { get { return Stat.AttackSpeed + AdditionalAttackSpeed; } }
        public Player()
        {
            ObjectType = GameObjectType.Player;
            Vision = new VIsionCube(this);
            IsDead = false;
        }

        public override int OnDamaged(GameObject target, int damage)
        {
            if (IsDead)
                return 0;
            return base.OnDamaged(target, damage);
            //TODO : 피해를 입었을 때 처리 -> 플레이어 스탯에 따라 딜레이 시간 변경
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
        public bool HandleSkillCool(SkillData skillData)
        {
            if (_skillCooldowns.TryGetValue(skillData.id, out long cooldownEnd))
            {
                if (cooldownEnd > Environment.TickCount64)
                {
                    // 쿨타임이 끝나지 않았음
                    return false;
                }
            }

            if (skillData.id<3)
            {
                _skillCooldowns[skillData.id] = (long)(Environment.TickCount64 + 1000 /TotalAttackSpeed);
            }
            else
            {
                _skillCooldowns[skillData.id] = (long)(Environment.TickCount64 + skillData.coolTime);
            }
            return true;
        }
        public void SendRespawnPacket()
        {
            S_Respawn respawnPacket = new S_Respawn();            
            Session.Send(respawnPacket);
        }

        public void OnlevelUp()
        {
            StatData stat = DataManager.StatDict.GetValueOrDefault(Level);
            Stat.MaxHp = stat.MaxHp;
            Hp = stat.MaxHp;
            Stat.Attack = stat.Attack;
            Stat.Speed = stat.Speed;           
            DbTransaction.SavePlayerStatus_All(this, Room);
            Room.Push(Room.HandleStatChange, this);
        }

        public void OnLeaveGame()
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
            DbTransaction.SavePlayerMap(this, MapInfo);
            if (Session.ServerState == PlayerServerState.ServerStateSingle)
            {
                Room.Push(Room.ResetSingleRoom,this);
            }
        }

        public void HandleEquipItem(C_EquipItem equipPacket)
        {
            Item item = Inven.Get(equipPacket.ItemDbId);
            if(item.ItemType == ItemType.Goods || item.ItemType == ItemType.Material)
            {
                return;
            }
            if (item == null)
                return;

            if (equipPacket.Equipped)
            {
                Item unequipItem = null;
                //장비를 착용하려는데 이미 착용중인 아이템이 있다면
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
                if (unequipItem != null)
                {
                    //해당 아이템을 해제
                    //메모리 선적용
                    unequipItem.Equipped = false;

                    //DB에 적용
                    DbTransaction.EquipItemNoti(this, unequipItem);

                    //클라이언트에게 알림
                    S_EquipItem equipOkItem = new S_EquipItem();
                    equipOkItem.ItemDbId = unequipItem.ItemDbId;
                    equipOkItem.Equipped = unequipItem.Equipped;
                    Session.Send(equipOkItem);
                }
            }
            {
                //메모리 선적용
                item.Equipped = equipPacket.Equipped;

                //DB에 적용
                DbTransaction.EquipItemNoti(this, item);

                //클라이언트에게 알림
                S_EquipItem equipNoti = new S_EquipItem();
                equipNoti.ItemDbId = equipPacket.ItemDbId;
                equipNoti.Equipped = equipPacket.Equipped;
                Session.Send(equipNoti);
            }

            RefreshAdditionalStat();
        }

        

        public void RefreshAdditionalStat()
        {
            WeaponDamage = 0;
            ArmorDef = 0;
            AdditionalAvoidance = 0;
            AdditionalAccuracy = 0;
            AdditionalCriticalChance = 0;
            AdditionalCriticalDamage = 0;
            AdditionalAttackSpeed = 0;

            foreach (Item item in Inven.Items.Values)
            {
                if (item.Equipped == false)
                    continue;

                MapField<string, string> options = item.Info.Options;
                if (options == null)
                    continue;
                foreach (var option in options)
                {
                    switch (option.Key)
                    {
                        case "Avoid":
                            AdditionalAvoidance += int.Parse(option.Value);
                            break;
                        case "Acc":
                            AdditionalAccuracy += int.Parse(option.Value);
                            break;
                        case "CriticalChance":
                            AdditionalCriticalChance += int.Parse(option.Value);
                            break;
                        case "CriticalDamage":
                            AdditionalCriticalDamage += int.Parse(option.Value);
                            break;
                    }
                }

                switch (item.ItemType)
                {
                    case ItemType.Weapon:
                        WeaponDamage += ((Weapon)item).Damage;
                        AdditionalAttackSpeed += ((Weapon)item).AttackSpeed;
                        break;
                    case ItemType.Armor:
                        ArmorDef += ((Armor)item).Defense;
                        break;
                }

            }
        }
    }
}
