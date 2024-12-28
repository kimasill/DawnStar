using Data;
using Google.Protobuf.Collections;
using Google.Protobuf.Protocol;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;


public class Item
{
    public ItemInfo Info { get; } = new ItemInfo();
    public event Action<Item> OnEquipped;
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
        set {           
                Info.Equipped = value;
            OnEquipped?.Invoke(this);
        }
    }
    public int Price
    {
        get { return Info.Price; }
        set { Info.Price = value; }
    }
    public int Rank
    {
        get { return Info.Rank; }
        set { Info.Rank = value; }
    }
    public Grade Grade
    {
        get { return Info.Grade; }
        set { Info.Grade = value; }
    }

    public MapField<string, string> Options
    {
        get { return Info.Options; }
        set
        {
            Info.Options.Clear();
            foreach (var option in value)
            {
                Info.Options.Add(option.Key, option.Value);
            }
        }
    }
    public List<TargetInteract> TargetInteract { get; private set; }
        
    public ItemType ItemType { get; private set; }
    public bool Stackable { get; protected set; }

    public Item(ItemType itemType)
    {
        ItemType = itemType;
    }

    public static Item MakeItem(ItemInfo itemInfo)
    {
        Item item = null;
        ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(itemInfo.TemplateId, out itemData);

        if (itemData == null)
            return null;

        switch (itemData.itemType)
        {
            case ItemType.Weapon:
                item = new Weapon(itemInfo.TemplateId, itemInfo);
                break;
            case ItemType.Armor:
                item = new Armor(itemInfo.TemplateId, itemInfo);
                break;
            case ItemType.Jewelry:
                item = new Jewelry(itemInfo.TemplateId);
                break;
            case ItemType.Consumable:
                item = new Consumable(itemInfo.TemplateId);
                break;
            case ItemType.Material:
                item = new Material(itemInfo.TemplateId);
                break;
            case ItemType.Goods:
                item = new Goods(itemInfo.TemplateId);
                break;
        }

        if (item != null)
        {
            item.ItemDbId = itemInfo.ItemDbId;
            item.Count = itemInfo.Count;
            item.Slot = itemInfo.Slot;
            item.Equipped = itemInfo.Equipped;
            item.Price = itemInfo.Price;
            item.Rank = itemInfo.Rank;
            item.Grade = itemInfo.Grade;
            item.Options = itemInfo.Options;
            if (itemData.interaction != null) {
                List<TargetInteract> targetInteracts = new List<TargetInteract>();
                foreach (var interact in itemData.interaction)
                {
                    targetInteracts.Add(new TargetInteract(interact));
                }
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
        public Weapon(int templateId, ItemInfo itemInfo) : base(ItemType.Weapon)
        {
            Init(templateId, itemInfo);
        }

        void Init(int templateId, ItemInfo itemInfo)
        {
            ItemData itemData = null;
            Managers.Data.ItemDict.TryGetValue(templateId, out itemData);
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

            if(itemInfo.Rank > 0)
            {
                EnhanceData enhanceData = null;
                Managers.Data.EnhanceDict.TryGetValue(itemInfo.Rank, out enhanceData);
                if (enhanceData == null)
                    return;
                Damage = (int)(data.damage + (data.damage * 0.5)*itemInfo.Rank + (data.damage * enhanceData.value));
            }
        }
    }

    public class Armor : Item
    {
        public ArmorType ArmorType { get; private set; }
        public int Defense { get; private set; }
        public Armor(int templateId, ItemInfo itemInfo) : base(ItemType.Armor)
        {
            Init(templateId, itemInfo);
        }

        void Init(int templateId, ItemInfo itemInfo)
        {
            ItemData itemData = null;
            Managers.Data.ItemDict.TryGetValue(templateId, out itemData);
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

            if (itemInfo.Rank > 0)
            {
                EnhanceData enhanceData = null;
                Managers.Data.EnhanceDict.TryGetValue(itemInfo.Rank, out enhanceData);
                if (enhanceData == null)
                    return;
                Defense = (int)(data.defense + (data.defense * 0.5)*itemInfo.Rank + (data.defense * enhanceData.value));
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
            Managers.Data.ItemDict.TryGetValue(templateId, out itemData);
            if (itemData.itemType != ItemType.Jewelry)
            {
                return;
            }

            JewelryData data = (JewelryData)itemData;
            {
                TemplateId = data.id;
                Count = 1;
                JewelryType = data.jewelryType;
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
            Managers.Data.ItemDict.TryGetValue(templateId, out itemData);
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
            Managers.Data.ItemDict.TryGetValue(templateId, out itemData);
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
                Stackable = true; // 재료 아이템은 기본적으로 스택 가능
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
            Managers.Data.ItemDict.TryGetValue(templateId, out itemData);
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
public struct TargetInteract
{
    public GameObjectType objectType;
    public string detail;
    public TargetInteract(string code)
    {
        string[] strings = code.Split('_');
        GameObjectType type = (GameObjectType)Enum.Parse(typeof(GameObjectType), strings[0]);
        objectType = type;
        detail = strings[1];
    }

    public GameObjectType type  { get{ return objectType;} }     
}
