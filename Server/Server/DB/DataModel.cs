using Google.Protobuf.Protocol;
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
        public float Speed { get; set; }
        public int Exp { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }
        public int Gold { get; set; }
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
}