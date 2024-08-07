using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateAccountPacketReq
{
    public string AccountName;
    public string Password;
}

//server -> client
public class CreateAccountPacketRes
{
    public bool CreateSuccess;
}

public class ServerInfo
{
    public string Name;
    public string IPAddress;
    public int Port;
    public int BusyScore;
}

public class LoginAccountPacketReq
{
    public string AccountName;
    public string Password;
}

public class LoginAccountPacketRes
{
    public bool LoginSuccess;
    public int AccountId;
    public int Token;
    public List<ServerInfo> ServerList { get; set; } = new List<ServerInfo>();
}

