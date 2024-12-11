using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Item;

namespace Data
{
    #region Stat 
    [Serializable]
    public class StatData
    {
        public int Level;
        public int MaxHp;
        public int Attack;
        public int Defense;
        public float Speed;
        public int TotalExp;
        public int StatPoint;
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
    public class SkillData
    {
        public int id;
        public string name;
        public float coolTime;
        public int damage;
        public int count;
        public float term;
        public int range;
        public SkillType skillType;
        public SkillLogicType skillLogicType;
        public ShapeInfo shape;
        public ProjectileInfo projectile;
        public SpotInfo spot;
        public BuffInfo buff;
        public List<BuffInfo> buffList;
        public DebuffInfo debuff;
        public List<DebuffInfo> debuffList;
        public int unchartedPoint;
        public string description;
        public string prefab;
        public List<string> prefabs;
        public string icon;
        public bool isObject;
        public bool fix;
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
        public bool isHoming;
        public string prefab;
    }
    public class SpotInfo
    {
        public string name;
        public float range;
        public float delay;
        public bool isHoming;
        public int maxCount;
        public int minCount;
    }
    public class BuffInfo
    {
        public int id;
        public string name;
        public int duration;
        public float value;
        public bool isPercent;
    }

    public class DebuffInfo
    {
        public int id;
        public string name;
        public int duration;
        public float value;
        public bool isPercent;
    }

    [Serializable]
    public class SkillLoader : ILoader<int, SkillData>
    {
        public List<SkillData> skills = new List<SkillData>();

        public Dictionary<int, SkillData> MakeDict()
        {
            Dictionary<int, SkillData> dict = new Dictionary<int, SkillData>();
            foreach (SkillData skill in skills)
                dict.Add(skill.id, skill);
            return dict;
        }
    }
    #endregion
    #region Buff
    [Serializable]
    public class BuffData
    {
        public int id;
        public string name;
        public string icon;
    }

    [Serializable]
    public class BuffLoader : ILoader<int, BuffData>
    {
        public List<BuffData> buffs = new List<BuffData>();

        public Dictionary<int, BuffData> MakeDict()
        {
            Dictionary<int, BuffData> dict = new Dictionary<int, BuffData>();
            foreach (BuffData buff in buffs)
                dict.Add(buff.id, buff);
            return dict;
        }
    }

    #endregion
    #region Debuff

    [Serializable]
    public class DebuffData
    {
        public int id;
        public string name;
        public string icon;
    }

    [Serializable]

    public class DebuffLoader : ILoader<int, DebuffData>
    {
        public List<DebuffData> debuffs = new List<DebuffData>();

        public Dictionary<int, DebuffData> MakeDict()
        {
            Dictionary<int, DebuffData> dict = new Dictionary<int, DebuffData>();
            foreach (DebuffData debuff in debuffs)
                dict.Add(debuff.id, debuff);
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
        public string prefabPath;
        public string description;
        public Dictionary<string, string> options = new Dictionary<string, string>();
        public List<CostData> pieces;
    }

    public class WeaponData : ItemData
    {
        public WeaponType weaponType;
        public int damage;
        public int range;
        public float attackSpeed;
    }

    public class ArmorData : ItemData
    {
        public ArmorType armorType;
        public int defense;
    }

    public class JewelryData : ItemData
    {
        public JewelryType jewelType;
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
        public List<JewelryData> jewelry = new List<JewelryData>();
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
            foreach(ItemData item in jewelry)
            {
                item.itemType = ItemType.Jewelry;
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
    public class RewardData
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
        public MonsterType type;
        public MonsterGrade grade;
        public StatInfo stat;
        public List<RewardData> rewards;
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
        public int mapId;
        public int destination;        
        public float posX;
        public float posY;
    }

    public class SpawnData
    {
        public int id;
        public int monsterId;
        public int count;
    }

    public class DungeonData
    {        
        public string name;
        public int level;
        public int maxPlayer;
        public List<int> monsters;
        public List<int> rewards;
    }

    [Serializable]
    public class MapData
    {
        public int id;
        public string name;
        public MapType type;
        public List<PortalData> portals;
        public List<SpawnData> spawns;
        public DungeonData dungeon;
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
    #region Script

    [Serializable]
    public class Script
    {
        public int id;
        public string name;
        public List<string> script;
        public string image;
    }

    [Serializable]
    public class ScriptData
    {
        public int id;
        public string title;
        public List<Script> scripts;
    }

    [Serializable]
    public class ScriptLoader : ILoader<int, ScriptData>
    {
        public List<ScriptData> scriptdatas = new List<ScriptData>();

        public Dictionary<int, ScriptData> MakeDict()
        {
            Dictionary<int, ScriptData> dict = new Dictionary<int, ScriptData>();
            foreach (ScriptData scriptData in scriptdatas)
            {
                dict.Add(scriptData.id, scriptData);
            }
            return dict;
        }
    }
    #endregion
    #region NPC
    [Serializable]
    public class NPCScript
    {
        public int id;
        public string name;
        public string type;
        public List<string> script;
    }

    [Serializable]
    public class NPCData
    {
        public int id;
        public string name;
        public NPCType npcType;
        public int shopId;
        public List<NPCScript> scripts;
    }

    [Serializable]
    public class NPCLoader : ILoader<string, NPCData>
    {
        public List<NPCData> npcs = new List<NPCData>();

        public Dictionary<string, NPCData> MakeDict()
        {
            Dictionary<string, NPCData> dict = new Dictionary<string, NPCData>();
            foreach (NPCData npc in npcs)
            {
                dict.Add(npc.name, npc);
            }
            return dict;
        }
    }
    #endregion
    #region Quest

    [Serializable]
    public class QuestRewardData
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
        public string description;
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
    #region Acquire
    [Serializable]
    public class AcquireData
    {
        public int id;
        public string name;
        public AcquireType type;
        public Grade grade;
        public List<RewardData> rewards;
    }
    [Serializable]
    public class AcquireLoader : ILoader<int, AcquireData>
    {
        public List<AcquireData> acquires = new List<AcquireData>();

        public Dictionary<int, AcquireData> MakeDict()
        {
            Dictionary<int, AcquireData> dict = new Dictionary<int, AcquireData>();
            foreach (AcquireData acquire in acquires)
            {
                dict.Add(acquire.id, acquire);
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
    #region SpecialStat
    [Serializable]
    public class SpecialStatData
    {
        public int point;
        public string name;
        public float value;
    }
    public class RealizationData
    {
        public int id;
        public string name;
        public List<string> script;
        public List<SpecialStatData> specialStatDatas;
    }
    public class RealizationLoader : ILoader<int, RealizationData>
    {
        public List<RealizationData> realizations = new List<RealizationData>();

        public Dictionary<int, RealizationData> MakeDict()
        {
            Dictionary<int, RealizationData> dict = new Dictionary<int, RealizationData>();
            foreach (RealizationData realization in realizations)
            {
                dict.Add(realization.id, realization);
            }
            return dict;
        }
    }
    #endregion
    #region Interaction
    [Serializable]
    public class InteractionData
    {
        public int id;
        public string name;
        public bool multi;
        public InteractionType interactionType;
        public List<string> script;
        public bool cameraMove;
    }
    public class Vector2Set
    {
        public int x;
        public int y;
    }
    public class DoorData : InteractionData
    {
        public List<int> keyItems;
        public List<int> triggers;
        public List<Vector2Set> cells;
    }
    public class TriggerData : InteractionData
    {
        public List<int> keyItems;
        public List<int> targetInteraction;
    }

    public class ItemTableData : InteractionData
    {
        public List<int> itemIds;
    }
    public class QuestSignData : InteractionData
    {
        public int startId;
        public int endId;
    }
    public class CameraPointData : InteractionData
    {
        public int questId;
        public List<int> targetInteraction;
    }

    public class InteractionLoader : ILoader<int, InteractionData>
    {
        public List<DoorData> doors = new List<DoorData>();
        public List<TriggerData> triggers = new List<TriggerData>();
        public List<ItemTableData> itemTables = new List<ItemTableData>();
        public List<QuestSignData> questSigns = new List<QuestSignData>();
        public List<CameraPointData> cameraPoints = new List<CameraPointData>();
        public Dictionary<int, InteractionData> MakeDict()
        {
            Dictionary<int, InteractionData> dict = new Dictionary<int, InteractionData>();
            foreach (InteractionData interaction in doors)
            {
                interaction.interactionType = InteractionType.Door;
                dict.Add(interaction.id, interaction);
            }
            foreach (InteractionData interaction in triggers)
            {
                interaction.interactionType = InteractionType.Trigger;
                dict.Add(interaction.id, interaction);
            }
            foreach (InteractionData interaction in itemTables)
            {
                interaction.interactionType = InteractionType.ItemTable;
                dict.Add(interaction.id, interaction);
            }
            foreach (InteractionData interaction in questSigns)
            {
                interaction.interactionType = InteractionType.Quest;
                dict.Add(interaction.id, interaction);
            }
            foreach (InteractionData interaction in cameraPoints)
            {
                interaction.interactionType = InteractionType.Camera;
                dict.Add(interaction.id, interaction);
            }
            return dict;
        }
    }
    #endregion
    #region Enhance
    [Serializable]
    public class CostData
    {
        public int templateId;
        public int count;
    }
    public class EnhanceData
    {
        public int id;
        public int rank;
        public ItemType itemType;
        public List<CostData> costData;
        public float value;
        public bool isPercent;
    }
    public class EnhanceLoader : ILoader<int, EnhanceData>
    {
        public List<EnhanceData> enhances = new List<EnhanceData>();

        public Dictionary<int, EnhanceData> MakeDict()
        {
            Dictionary<int, EnhanceData> dict = new Dictionary<int, EnhanceData>();
            foreach (EnhanceData enhance in enhances)
            {
                dict.Add(enhance.id, enhance);
            }
            return dict;
        }
    }
    #endregion
}

