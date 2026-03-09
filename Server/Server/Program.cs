using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using Server.Data;
using Server.DB;
using Server.Game;
using Server.Game.Job;
using Server.Game.Room;
using Server.Utils;
using ServerCore;
using CommonDB;

namespace Server
{
    class Program
	{
		static Listener _listener = new Listener();
		static void GameLogicTask()
		{
            while (true)
            {
                GameLogic.Instance.Update();
                Thread.Sleep(0); //cpu 낭비를 막기위해
            }
        }

		static void DbTask()
		{
            while (true)
            {
                DbTransaction.Instance.ExecuteAll();
				Thread.Sleep(0); //cpu 낭비를 막기위해
            }
        }

		static void NetworkTask()
        {
            while (true)
            {
                List<ClientSession> sessions = ConnectionRegistry.Instance.GetSessions();
                foreach (ClientSession session in sessions)
                {
                    session.FlushSend();
                }
                Thread.Sleep(0); //cpu 낭비를 막기위해
            }
        }

        static void StartServerInfoTask()
        {
            var t = new System.Timers.Timer();
            t.AutoReset = true;
            t.Elapsed += new System.Timers.ElapsedEventHandler((s, e) =>
            { 
                using(CommonDbContext shared = new CommonDbContext())
                {
                    ServerDb serverDb = shared.Servers.Where(s => s.Name == Name).FirstOrDefault();
                    if(serverDb != null)
                    {
                        serverDb.IpAdress = IpAddress;
                        serverDb.Port = Port;
                        serverDb.BusyScore = ConnectionRegistry.Instance.GetBusyScore();
                        shared.SaveChangesEx();
                    }
                    else
                    {
                        serverDb = new ServerDb()
                        {
                            Name = Program.Name,
                            IpAdress = IpAddress,
                            Port = Program.Port,
                            BusyScore = ConnectionRegistry.Instance.GetBusyScore()
                        };
                        shared.Servers.Add(serverDb);
                        shared.SaveChangesEx();
                    }
                }
            } );
            t.Interval = 10 *1000;
            t.Start();
        }

        public static string Name { get; } = "에르도아";
        public static int Port { get; } = 7777;
        public static string IpAddress { get; set; }
		static void Main(string[] args)
		{
            using (CommonDbContext shared = new CommonDbContext())
            {

            }
			ConfigManager.LoadConfig();
			DataManager.LoadData();

            GameLogic.Instance.Enqueue(() =>
            {
                GameLogic.Instance.Add(5);
            });

            // DNS (Domain Name System)
            string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[1];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            IpAddress = ipAddr.ToString();

			_listener.Init(endPoint, () => { return ConnectionRegistry.Instance.Generate(); });
			Console.WriteLine("Listening...");

            StartServerInfoTask();

            //Db Task
			{
                Thread t = new Thread(DbTask);
                t.Name = "DB";
                t.Start();
			}
            //Network Task
            {
                Thread t = new Thread(NetworkTask);
                t.Name = "Network Send";
                t.Start();
            }
            //GameLogic Task : 메인 스레드에서 실행
            Thread.CurrentThread.Name = "Game Logic";
            GameLogicTask();

			//FlushRoom();
			//TaskTimer.Instance.Enqueue(FlushRoom);
		}
	}
}
