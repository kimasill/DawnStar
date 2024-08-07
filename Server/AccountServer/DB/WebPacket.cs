namespace AccountServer.DB
{
    //client -> server
    public class CreateAccountPacketReq
    {
        public string AccountName { get; set; }
        public string Password { get; set; }
    }

    //server -> client
    public class CreateAccountPacketRes
    {
        public bool CreateSuccess { get; set; }
    }

    public class  ServerInfo
    {
        public string Name { get; set; }
        public string IP { get; set; }
        public int CrowdedLevel { get; set; }
    }

    public class LoginAccountPacketReq
    {
        public string AccountName { get; set; }
        public string Password { get; set; }
    }

    public class LoginAccountPacketRes
    {
        public bool LoginSuccess { get; set; }
        public List<ServerInfo> ServerList { get; set; } = new List<ServerInfo>();
    }
}
