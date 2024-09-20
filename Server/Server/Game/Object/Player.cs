using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
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
        public QuestInfo Quest { get; set; } = new QuestInfo();
        public int Exp {
            get { return Stat.TotalExp; }
            set {
                if (value >= 0)
                {
                    Stat.TotalExp = value;
                }                
                
                StatData stat = DataManager.StatDict.GetValueOrDefault(Level);
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
            get { return Stat.Gold; }
            set
            {
                if (value >= 0)
                {
                    Stat.Gold = value;
                }
            }
        }
        public int WeaponDamage { get; private set; }
        public int ArmorDef { get; private set; }

        public override int TotalAttack { get { return Stat.Attack + WeaponDamage; } }
        public override int TotalDefense { get { return ArmorDef; } }


        public Player()
        {
            ObjectType = GameObjectType.Player;
            Vision = new VIsionCube(this);
        }

        public override void OnDamaged(GameObject target, int damage)
        {            
            base.OnDamaged(target, damage);
        }

        public override void Ondead(GameObject attacker)
        {
            base.Ondead(attacker);
        }

        public void OnlevelUp()
        {
            StatData stat = DataManager.StatDict.GetValueOrDefault(Level);
            Stat.MaxHp = stat.MaxHp;
            Hp = stat.MaxHp;
            Stat.Attack = stat.Attack;
            Stat.Speed = stat.Speed;            
            Room.HandleStatChange(this);
        }

        public void OnLeaveGame()
        {
            
            //문제 : 플레이어가 게임을 나가면, 플레이어의 정보를 저장해야 한다.
            // 코드흐름 막아버린다. 데이터 베이스 접근하는 부분이 Core한 부분에 있으면 안됨.
            //해결 : 비동기 처리를 한다. 비동기 처리를 하면, 코드흐름이 막히지 않는다.
            // 다른 쓰레드 하나를 만들어서, 데이터베이스에 저장하는 작업을 한다.
            //TODO : 플레이어의 정보를 데이터베이스에 저장한다.
            //위치정보, 레벨, 경험치, 아이템 정보, 퀘스트 정보 등등
            DbTransaction.SavePlayerStatus_All(this, Room);
            DbTransaction.SavePlayerMap(this, MapInfo);
        }

        public void HandleEquipItem(C_EquipItem equipPacket)
        {
            Item item = Inven.Get(equipPacket.ItemDbId);
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

        public void HandleQuestComplete(int id)
        {
            // 퀘스트 완료 처리
            QuestInfo quest = Quest; // 현재 퀘스트 정보 가져오기
            if (quest == null || quest.TemplateId != id)
                return;

            // 퀘스트 완료 상태로 변경
            quest.Completed = true;

            // DB에 퀘스트 완료 상태 저장
            DbTransaction.SaveCompleteQuest(this, quest);
            //DbTransaction.SavePlayerMap(this, MapInfo);
        }

        public void HandleStartQuest(int id)
        {
            QuestData questData = DataManager.QuestDict.GetValueOrDefault(id);
            if (questData == null)
            {
                // 퀘스트 데이터가 없는 경우 처리
                return;
            }
                    // 퀘스트 시작 처리
            QuestInfo quest = new QuestInfo
            {                
                TemplateId = questData.id,
                Progress = 0,
                Completed = false,
                QuestType = questData.questType,
            };

            // DB에 퀘스트 정보 저장
            // DB에 퀘스트 시작 상태 저장
            DbTransaction.SaveStartQuest(this, quest);

            // 클라이언트에게 퀘스트 시작 정보 전송
            S_StartQuest startQuestPacket = new S_StartQuest
            {
                Quest = quest
            };
            Session.Send(startQuestPacket);
        }



        public void RefreshAdditionalStat()
        {
            WeaponDamage = 0;
            ArmorDef = 0;

            foreach(Item item in Inven.Items.Values)
            {
                if(item.Equipped == false)
                    continue;

                switch (item.ItemType)
                { 
                    case ItemType.Weapon:
                        WeaponDamage += ((Weapon)item).Damage;
                        break;
                    case ItemType.Armor:
                        ArmorDef += ((Armor)item).Defense;
                        break;
                }

            }
        }
    }
}
