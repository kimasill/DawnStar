using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Data
{
    public interface ILoader<Key, Value>
    {
        Dictionary<Key, Value> MakeDict();
    }

    public class DataManager
    {
        public static Dictionary<int, Data.StatData> StatDict { get; private set; } = new Dictionary<int, Data.StatData>();
        public static Dictionary<int, Data.SkillData> SkillDict { get; private set; } = new Dictionary<int, Data.SkillData>();
        public static Dictionary<int, Data.ItemData> ItemDict { get; private set; } = new Dictionary<int, Data.ItemData>();
        public static Dictionary<int, Data.MonsterData> MonsterDict { get; private set; } = new Dictionary<int, Data.MonsterData>();
        public static Dictionary<int, Data.MapData> MapDict { get; private set; } = new Dictionary<int, Data.MapData>();
        public static Dictionary<int, Data.QuestData> QuestDict { get; private set; } = new Dictionary<int, Data.QuestData>();
        public static Dictionary<int, Data.ShopData> ShopDict { get; private set; } = new Dictionary<int, Data.ShopData>();
        public static Dictionary<int, Data.AcquireData> AcquireDict { get; private set; } = new Dictionary<int, Data.AcquireData>();
        public static Dictionary<int, Data.RealizationData> RealizationData { get; private set; } = new Dictionary<int, Data.RealizationData>();

        public static void LoadData()
        {
            StatDict = LoadJson<Data.StatLoader, int, Data.StatData>("StatData").MakeDict();
            SkillDict = LoadJson<Data.SkillLoader, int, Data.SkillData>("SkillData").MakeDict();
            ItemDict = LoadJson<Data.ItemLoader, int, Data.ItemData>("ItemData").MakeDict();
            MonsterDict = LoadJson<Data.MonsterLoader, int, Data.MonsterData>("MonsterData").MakeDict();
            MapDict = LoadJson<Data.MapLoader, int, Data.MapData>("MapData").MakeDict();
            QuestDict = LoadJson<Data.QuestLoader, int, Data.QuestData>("QuestData").MakeDict();
            ShopDict = LoadJson<Data.ShopLoader, int, Data.ShopData>("ShopData").MakeDict();
            AcquireDict = LoadJson<Data.AcquireLoader, int, Data.AcquireData>("AcquireData").MakeDict();
            RealizationData = LoadJson<Data.RealizationLoader, int, Data.RealizationData>("RealizationData").MakeDict();
        }

        static Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
        {
            string text = File.ReadAllText($"{ConfigManager.Config.dataPath}/{path}.json");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Loader>(text);
        }
    }
}
