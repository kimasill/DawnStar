using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Data
{
    #region Stat 
    [Serializable]
    public class StatData
    {
        public int Level;
        public int MaxHp;
        public int Attack;
        public int Defence;
        public float Speed;
        public int TotalExp;        
    }

    [Serializable]
    public class StatLoader : ILoader<int, StatData>
    {
        public List<StatData> stats = new List<StatData>();

        public Dictionary<int, StatData> MakeDict()
        {
            Dictionary<int, StatData> dict = new Dictionary<int, StatData>();
            foreach (StatData stat in stats)
            {                
                dict.Add(stat.Level, stat);
            }
            return dict;
        }
    }
    #endregion

    #region Skill
    [Serializable]
    public class Skill
    {
        public int id;
        public string name;
        public float coolTime;
        public int damage;
        public SkillType skillType;
        public ProjectileInfo projectile;
        public ShapeInfo shape;
    }

    public class ShapeInfo
    {
        public ShapeType shapeType;
        public float range;
    }

    public class ProjectileInfo
    {
        public string name;        
        public float speed;
        public float range;
        public string prefab;
    }


    [Serializable]
    public class SkillData : ILoader<int, Skill>
    {
        public List<Skill> skills = new List<Skill>();

        public Dictionary<int, Skill> MakeDict()
        {
            Dictionary<int, Skill> dict = new Dictionary<int, Skill>();
            foreach (Skill skill in skills)
                dict.Add(skill.id, skill);
            return dict;
        }
    }
    #endregion

    #region Item
    [Serializable]
    public class ItemData
    {
        public int id;
        public string name;
        public int price;
        public ItemType itemType;
        public string iconPath;
    }

    public class WeaponData : ItemData
    {
        public WeaponType weaponType;
        public int damage;
    }

    public class ArmorData : ItemData
    {
        public ArmorType armorType;
        public int defence;
    }
    public class ConsumableData : ItemData
    {
        public ConsumableType consumableType;
        public int maxCount;
    }
    public class MaterialData : ItemData
    {
        public MaterialType materialType;
        public int maxCount;
    }
    public class GoodsData : ItemData
    {
        public GoodsType goodsType;
        public int maxCount;
    }

    [Serializable]
    public class ItemLoader : ILoader<int, ItemData>
    {
        public List<WeaponData> weapons = new List<WeaponData>();
        public List<ArmorData> armors = new List<ArmorData>();
        public List<ConsumableData> consumables = new List<ConsumableData>();
        public List<MaterialData> materials = new List<MaterialData>();
        public List<GoodsData> goods = new List<GoodsData>();

        public Dictionary<int, ItemData> MakeDict()
        {
            Dictionary<int, ItemData> dict = new Dictionary<int, ItemData>();
            foreach (ItemData item in weapons)
            {
                item.itemType = ItemType.Weapon;
                dict.Add(item.id, item);
            }
            foreach (ItemData item in armors)
            {
                item.itemType = ItemType.Armor;
                dict.Add(item.id, item);
            }
            foreach (ItemData item in consumables)
            {
                item.itemType = ItemType.Consumable;
                dict.Add(item.id, item);
            }
            foreach (ItemData item in materials)
            {
                item.itemType = ItemType.Material;
                dict.Add(item.id, item);
            }
            foreach (ItemData item in goods)
            {
                item.itemType = ItemType.Goods;
                dict.Add(item.id, item);
            }
            return dict;
        }
    }
    #endregion

    #region Monster
    [Serializable]
    public class ItemRewardData
    {         
        public int probability; //100분율
        public int itemId;
        public int minCount;
        public int maxCount;
    }
    [Serializable]
    public class MonsterData
    {
        public int id;
        public string name;
        public StatInfo stat;
        public List<ItemRewardData> rewards;
    }
    [Serializable]
    public class MonsterLoader : ILoader<int, MonsterData>
    {
        public List<MonsterData> monsters = new List<MonsterData>();

        public Dictionary<int, MonsterData> MakeDict()
        {
            Dictionary<int, MonsterData> dict = new Dictionary<int, MonsterData>();
            foreach (MonsterData monster in monsters)
            {                
                dict.Add(monster.id, monster);
            }
            return dict;
        }
    }
    #endregion

    #region Map

    [Serializable]
    public class PortalData
    {
        public int id;
        public string name;
        public string location;
        public float posX;
        public float posY;
    }
    public class SpawnData
    {
        public int id;
        public int objectId;
        public float posX;
        public float posY;
    }

    [Serializable]
    public class MapData
    {
        public int id;
        public string name;
        public List<PortalData> portals;
        public List<SpawnData> spawns;
    }



    [Serializable]
    public class MapLoader : ILoader<int, MapData>
    {
        public List<MapData> maps = new List<MapData>(); 

        public Dictionary<int, MapData> MakeDict()
        {
            Dictionary<int, MapData> dict = new Dictionary<int, MapData>();
            foreach (MapData item in maps)
            {
                dict.Add(item.id, item);
            }
            return dict;
        }
    }
    #endregion

    #region Quest

    [Serializable]
    public class RewardData
    {
        public RewardType type;
        public int amount;
    }

    [Serializable]
    public class QuestData
    {
        public int id;
        public string title;
        public List<RewardData> rewards;
        public int connection;        
        public string questType;        
    }

    [Serializable]
    public class QuestLoader : ILoader<int, QuestData>
    {
        public List<QuestData> quests = new List<QuestData>();

        public Dictionary<int, QuestData> MakeDict()
        {
            Dictionary<int, QuestData> dict = new Dictionary<int, QuestData>();
            foreach (QuestData item in quests)
            {
                dict.Add(item.id, item);
            }
            return dict;
        }
    }

    #endregion

    #region Shop
    [Serializable]
    public class ShopItemData
    {
        public int id;
        public int count;
        public int price;
    }

    [Serializable]
    public class ShopData
    {
        public int id;
        public int mapId;
        public string name;
        public List<ShopItemData> itemList;
    }

    [Serializable]
    public class ShopLoader : ILoader<int, ShopData>
    {
        public List<ShopData> shops = new List<ShopData>();

        public Dictionary<int, ShopData> MakeDict()
        {
            Dictionary<int, ShopData> dict = new Dictionary<int, ShopData>();
            foreach (ShopData shop in shops)
            {
                dict.Add(shop.id, shop);
            }
            return dict;
        }
    }
    #endregion
}
