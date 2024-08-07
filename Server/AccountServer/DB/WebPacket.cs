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
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public int BusyScore { get; set; }
    }

    public class LoginAccountPacketReq
    {
        public string AccountName { get; set; }
        public string Password { get; set; }
    }

    public class LoginAccountPacketRes
    {
        public bool LoginSuccess { get; set; }
        public int AccountId { get; set; }
        public int Token { get; set;}
        public List<ServerInfo> ServerList { get; set; } = new List<ServerInfo>();
    }
}
