using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{  
    #region Skill
    [Serializable]
    public class Skill
    {
        public int id;
        public string name;
        public float coolTime;
        public int damage;
        public SkillType skillType;
        public ShapeInfo shape;
        public ProjectileInfo projectile;
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
        public string location;
        public float posX;
        public float posY;
    }

    public class SpawnData
    {
        public int id;
        public int objectId;
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
}