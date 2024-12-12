using Google.Protobuf.Collections;
using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class Item
    {
        public ItemInfo Info { get; } = new ItemInfo();

        public int ItemDbId
        {
            get { return Info.ItemDbId; }
            set { Info.ItemDbId = value; }
        }

        public int TemplateId
        {
            get { return Info.TemplateId; }
            set { Info.TemplateId = value; }
        }

        public int Count
        {
            get { return Info.Count; }
            set { Info.Count = value; }
        }

        public int Slot
        {
            get { return Info.Slot; }
            set { Info.Slot = value; }
        }

        public bool Equipped
        {
            get { return Info.Equipped; }
            set { Info.Equipped = value; }
        }
        public MapField<string, string> Options
        {
            get { return Info.Options; }
        }
        public ItemType ItemType { get; private set; }
        public bool Stackable { get; protected set; }

        public int Rank { 
            get { return Info.Rank; }
            set { Info.Rank = value; }
        }
        public Grade Grade { get; protected set; }


        public Item(ItemType itemType)
        {
            ItemType = itemType;
        }

        public static Item MakeItem(ItemDb itemDb)
        {
            Item item = null;
            ItemData itemData = null;
            DataManager.ItemDict.TryGetValue(itemDb.TemplateId, out itemData);

            if (itemData == null)
                return null;

            switch (itemData.itemType)
            {
                case ItemType.Weapon:
                    item = new Weapon(itemDb);
                    break;
                case ItemType.Armor:
                    item = new Armor(itemDb);
                    break;
                case ItemType.Jewelry:
                    item = new Jewelry(itemDb.TemplateId);
                    break;
                case ItemType.Consumable:
                    item = new Consumable(itemDb.TemplateId);
                    break;
                case ItemType.Material:
                    item = new Material(itemDb.TemplateId);
                    break;
                case ItemType.Goods:
                    item = new Goods(itemDb.TemplateId);
                    break;
            }

            if (item != null)
            {
                item.ItemDbId = itemDb.ItemDbId;
                item.Count = itemDb.Count;     
                item.Slot = itemDb.Slot;
                item.Equipped = itemDb.Equipped;
                item.Rank = itemDb.Enhance;
                MapField<string, string> options = new MapField<string, string>();
                if (itemDb.Options.Count > 0)
                {
                    item.Options.Clear();
                    foreach (var option in itemDb.Options)
                    {
                        options.Add(option.Key, option.Value);
                    }
                    item.Options.Add(options);
                }                
            }
            return item;
        }
        public class Weapon : Item
        {
            public WeaponType WeaponType { get; private set; }
            public int Damage { get; private set; }
            public int Range { get; private set; }            
            public float AttackSpeed { get; private set; }
            public Weapon(ItemDb itemDb) : base(ItemType.Weapon)
            {
                Init(itemDb);
            }

            void Init(ItemDb itemDb)
            {
                ItemData itemData = null;
                DataManager.ItemDict.TryGetValue(itemDb.TemplateId, out itemData);
                if (itemData.itemType != ItemType.Weapon)
                {
                    return;
                }

                WeaponData data = (WeaponData)itemData;
                {
                    TemplateId = data.id;
                    Count = 1;
                    WeaponType = data.weaponType;
                    Damage = data.damage;
                    Range = data.range;
                    AttackSpeed = data.attackSpeed;
                    Stackable = false;
                }

                if (itemDb.Enhance > 0)
                {
                    Damage = itemDb.Damage;
                }
            }
        }

        public class Armor : Item
        {
            public ArmorType ArmorType { get; private set; }
            public int Defense { get; private set; }
            public Armor(ItemDb itemDb) : base(ItemType.Armor)
            {
                Init(itemDb);
            }

            void Init(ItemDb itemDb)
            {
                ItemData itemData = null;
                DataManager.ItemDict.TryGetValue(itemDb.TemplateId, out itemData);
                if (itemData.itemType != ItemType.Armor)
                {
                    return;
                }

                ArmorData data = (ArmorData)itemData;
                {
                    TemplateId = data.id;
                    Count = 1;
                    ArmorType = data.armorType;
                    Defense = data.defense;
                    Stackable = false;
                }

                if (itemDb.Enhance > 0)
                {
                    Defense = itemDb.Defense;
                }
            }
        }
        public class Jewelry : Item
        {
            public JewelryType JewelryType { get; private set; }
            public Jewelry(int templateId) : base(ItemType.Jewelry)
            {
                Init(templateId);
            }

            void Init(int templateId)
            {
                ItemData itemData = null;
                DataManager.ItemDict.TryGetValue(templateId, out itemData);
                if (itemData.itemType != ItemType.Jewelry)
                {
                    return;
                }

                JewelryData data = (JewelryData)itemData;
                {
                    TemplateId = data.id;
                    Count = 1;
                    JewelryType = data.jewelType;
                    Stackable = false;
                }
            }
        }
        public class Consumable : Item
        {
            public ConsumableType ConsumableType { get; private set; }
            public int MaxCount { get; private set; }
            public Consumable(int templateId) : base(ItemType.Consumable)
            {
                Init(templateId);
            }

            void Init(int templateId)
            {
                ItemData itemData = null;
                DataManager.ItemDict.TryGetValue(templateId, out itemData);
                if (itemData.itemType != ItemType.Consumable)
                {
                    return;
                }

                ConsumableData data = (ConsumableData)itemData;
                {
                    TemplateId = data.id;
                    Count = 1;
                    MaxCount = data.maxCount;
                    ConsumableType = data.consumableType;
                    Stackable = (data.maxCount > 1);
                }
            }
        }

        public class Material : Item
        {
            public MaterialType MaterialType { get; private set; }
            public int MaxCount { get; private set; }
            public Material(int templateId) : base(ItemType.Material)
            {
                Init(templateId);
            }

            void Init(int templateId)
            {
                ItemData itemData = null;
                DataManager.ItemDict.TryGetValue(templateId, out itemData);
                if (itemData.itemType != ItemType.Material)
                {
                    return;
                }

                MaterialData data = (MaterialData)itemData;
                {
                    TemplateId = data.id;
                    Count = 1;
                    MaxCount = data.maxCount;
                    MaterialType = data.materialType;
                    Stackable = (data.maxCount > 1);
                }
            }
        }

        public class Goods : Item
        {
            public GoodsType GoodsType { get; private set; }
            public int MaxCount { get; private set; }
            public Goods(int templateId) : base(ItemType.Goods)
            {
                Init(templateId);
            }

            void Init(int templateId)
            {
                ItemData itemData = null;
                DataManager.ItemDict.TryGetValue(templateId, out itemData);
                if (itemData.itemType != ItemType.Goods)
                {
                    return;
                }

                GoodsData data = (GoodsData)itemData;
                {
                    TemplateId = data.id;
                    Count = 1;
                    MaxCount = data.maxCount;
                    GoodsType = data.goodsType;
                    Stackable = (data.maxCount > 1);
                }
            }
        }
    }
}
