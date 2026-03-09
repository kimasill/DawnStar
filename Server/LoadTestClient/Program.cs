using LoadTestClient.Session;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;

namespace LoadTestClient
{
    
    class Program
    {
        static int LoadTestClientCount { get;} = 500;
        static void Main(string[] args)
        {
            Thread.Sleep(3000);
            // DNS (Domain Name System)
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[1];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            Connector connector = new Connector();

            connector.Connect(endPoint,
                () => { return ConnectionRegistry.Instance.Generate(); },
                Program.LoadTestClientCount);
                
            while(true)
            {
                Thread.Sleep(10000);
            }
        }
    }
}