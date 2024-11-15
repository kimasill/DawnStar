using Google.Protobuf.Protocol;
using Newtonsoft.Json;
using Server.Data;
using Server.Game.Room;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Server.DB
{
    [Table("Account")]
    public class AccountDb
    {
        public int AccountDbId { get; set; }
        public string AccountName { get; set; }
        public ICollection<PlayerDb> Players { get; set; }
    }

    [Table("Player")]
    public class PlayerDb
    {
        public int PlayerDbId { get; set; }
        public string PlayerName { get; set; }

        [ForeignKey("Account")]
        public int AccountDbId { get; set; }
        public AccountDb Account { get; set; }

        public ICollection<ItemDb> Items { get; set; }
        public PlayerServerState ServerState { get; set; }
        public int Level { get; set; }
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public float Speed { get; set; }
        public float AttackSpeed { get; set; }
        public int Avoid { get; set; }
        public int Accuracy { get; set; }
        public int CriticalChance { get; set; }
        public int CriticalDamage { get; set; }
        public int UnchartedPoint { get; set; }
        public int UnchartedPointRegen { get; set; }
        public int Exp { get; set; }
        public string RealizationsJson { get; set; }

        [NotMapped]
        public List<int> Realizations
        {
            get => string.IsNullOrEmpty(RealizationsJson) ? new List<int>() : JsonConvert.DeserializeObject<List<int>>(RealizationsJson);
            set => RealizationsJson = JsonConvert.SerializeObject(value);
        }
        public int PosX { get; set; }
        public int PosY { get; set; }
        public int Gold { get; set; }
        public int MaxPotion { get; set; }
        public int PotionPerformance { get; set; }
        public int StatPoint { get; set; }
        public ICollection<QuestDb> Quests { get; set; }
        public int MapDbId { get; set; }
    }

    [Table("Item")]
    public class ItemDb
    {
        public int ItemDbId { get; set; }
        public int TemplateId { get; set; }
        public int Count { get; set; }
        public int Slot { get; set; }
        public int Damage { get; set; }
        public int Defense { get; set; }
        public int Enhance { get; set; }
        public string OptionsJson { get; set; }

        [NotMapped]
        public Dictionary<string, string> Options
        {
            get => string.IsNullOrEmpty(OptionsJson) ? new Dictionary<string, string>() : JsonConvert.DeserializeObject<Dictionary<string, string>>(OptionsJson);
            set => OptionsJson = JsonConvert.SerializeObject(value);
        }
        public bool Equipped { get; set; } = false;
        [ForeignKey("Owner")]
        public int? OwnerDbId { get; set; }
        public PlayerDb Owner { get; set; }
    }

    [Table("Quest")]
    public class QuestDb
    {
        public int QuestDbId { get; set; }
        public int TemplateId { get; set; }
        public int Progress { get; set; }
        public bool Completed { get; set; } = false;
        [ForeignKey("Owner")]
        public int? OwnerDbId { get; set; }
        public PlayerDb Owner { get; set; }
    }

    [Table("Map")]
    public class MapDb
    {
        public int MapDbId { get; set; }
        public int TemplateId { get; set; }
        public string MapName { get; set; }
        public string Scene { get; set; }
        public int PlayerDbId { get; set; }
    }

    [Table("Chest")]
    public class ChestDb
    {
        public int ChestDbId { get; set; }
        public int ChestId { get; set; }
        public int TemplateId { get; set; }           
        public bool Visible { get; set; }
        public bool Opened { get; set; } 
        public int MapDbId { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }
    }

    [Table("Shop")]
    public class ShopDb
    {
        public int ShopDbId { get; set; }
        public int TemplateId { get; set; }
        public string ShopName { get; set; }
        public string Scene { get; set; }
        public int PlayerDbId { get; set; }
        public ICollection<ShopItemDb> ShopItems { get; set; }
    }

    [Table("ShopItem")]
    public class ShopItemDb
    {
        public int ShopItemDbId { get; set; }
        public int ShopDbId { get; set; }
        public int ItemId { get; set; }
        public int Price { get; set; }
        public int Count { get; set; }
        public string ItemType { get; set; }
    }
}