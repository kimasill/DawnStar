using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Data
{
    [Serializable]
    public class ServerConfig 
    {
        public string dataPath;
    }

    public class ConfigManager
    {
        public static ServerConfig Config { get; private set; }

        //실행 파일이랑 같은위치에 있는 설정파일 
        public static void LoadConfig()
        {
            string text = File.ReadAllText("Config.json");
            Config = Newtonsoft.Json.JsonConvert.DeserializeObject<ServerConfig>(text);
        }
    }
}
