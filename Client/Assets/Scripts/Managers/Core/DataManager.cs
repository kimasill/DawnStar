using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILoader<Key, Value>
{
    Dictionary<Key, Value> MakeDict();
}

public class DataManager
{
    public Dictionary<int, Data.Skill> SkillDict { get; private set; } = new Dictionary<int, Data.Skill>();
    public Dictionary<int, Data.ItemData> ItemDict { get; private set; } = new Dictionary<int, Data.ItemData>();
    public Dictionary<int, Data.MonsterData> MonsterDict { get; private set; } = new Dictionary<int, Data.MonsterData>();
    public Dictionary<int, Data.MapData> MapDict { get; private set; } = new Dictionary<int, Data.MapData>();
    public Dictionary<int, Data.ScriptData> ScriptDict { get; private set; } = new Dictionary<int, Data.ScriptData>();
    public Dictionary<string, Data.NPCData> NPCDict { get; private set; } = new Dictionary<string, Data.NPCData>();
    public Dictionary<int, Data.QuestData> QuestDict { get; private set; } = new Dictionary<int, Data.QuestData>();

    public void Init()
    {
        SkillDict = LoadJson<Data.SkillData, int, Data.Skill>("SkillData").MakeDict();
        ItemDict = LoadJson<Data.ItemLoader, int, Data.ItemData>("ItemData").MakeDict();
        MonsterDict = LoadJson<Data.MonsterLoader, int, Data.MonsterData>("MonsterData").MakeDict();
        MapDict = LoadJson<Data.MapLoader, int, Data.MapData>("MapData").MakeDict();
        ScriptDict = LoadJson<Data.ScriptLoader, int, Data.ScriptData>("QuestScriptData").MakeDict();
        QuestDict = LoadJson<Data.QuestLoader, int, Data.QuestData>("QuestData").MakeDict();
        NPCDict = LoadJson<Data.NPCLoader, string, Data.NPCData>("NPCData").MakeDict();
    }

    Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        TextAsset textAsset = Managers.Resource.Load<TextAsset>($"Data/{path}");
        if (textAsset == null)
        {
            Debug.LogWarning($"Failed to load data file at path: Data/{path}");
            return default(Loader);
        }
        return Newtonsoft.Json.JsonConvert.DeserializeObject<Loader>(textAsset.text);
    }
}