
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace PacketGenerator
{
	class Program
	{
		static string clientRegister;
		static string serverRegister;
		static string msgIdCacheEntries;

		static void Main(string[] args)
		{
			string file = "../../../Common/protoc-28.0-rc-1-win64/bin/Protocol.proto";
			if (args.Length >= 1)
				file = args[0];

			bool startParsing = false;
			foreach (string line in File.ReadAllLines(file))
			{
				if (!startParsing && line.Contains("enum MsgId"))
				{
					startParsing = true;
					continue;
				}

				if (!startParsing)
					continue;

				if (line.Contains("}"))
					break;

				string trimmed = line.Trim();
				if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("//"))
					continue;

				Match match = Regex.Match(trimmed, @"^([A-Z_][A-Z_0-9]*)\s*=\s*(\d+)");
				if (!match.Success)
					continue;

				string name = match.Groups[1].Value;

				if (name.StartsWith("S_"))
				{
					string msgName = ConvertToMsgName(name);
					string packetName = ConvertToPacketName(name);
					clientRegister += string.Format(PacketFormat.managerRegisterFormat, msgName, packetName);
					msgIdCacheEntries += string.Format(PacketFormat.msgIdCacheEntryFormat, packetName, msgName);
				}
				else if (name.StartsWith("C_"))
				{
					string msgName = ConvertToMsgName(name);
					string packetName = ConvertToPacketName(name);
					serverRegister += string.Format(PacketFormat.managerRegisterFormat, msgName, packetName);
					msgIdCacheEntries += string.Format(PacketFormat.msgIdCacheEntryFormat, packetName, msgName);
				}
			}

			string clientManagerText = string.Format(PacketFormat.managerFormat, clientRegister);
			File.WriteAllText("ClientPacketManager.cs", clientManagerText);
			string serverManagerText = string.Format(PacketFormat.managerFormat, serverRegister);
			File.WriteAllText("ServerPacketManager.cs", serverManagerText);

			string msgIdCacheText = string.Format(PacketFormat.msgIdCacheFormat, msgIdCacheEntries);
			File.WriteAllText("MsgIdCache.cs", msgIdCacheText);

			Console.WriteLine("Code generation complete.");
			Console.WriteLine($"  ClientPacketManager.cs");
			Console.WriteLine($"  ServerPacketManager.cs");
			Console.WriteLine($"  MsgIdCache.cs");
		}

		static string ConvertToMsgName(string enumName)
		{
			string[] words = enumName.Split('_');
			string msgName = "";
			foreach (string word in words)
				msgName += FirstCharToUpper(word);
			return msgName;
		}

		static string ConvertToPacketName(string enumName)
		{
			string[] words = enumName.Split('_');
			string msgName = ConvertToMsgName(enumName);
			return $"{words[0]}_{msgName.Substring(1)}";
		}

		public static string FirstCharToUpper(string input)
		{
			if (string.IsNullOrEmpty(input))
				return "";
			return input[0].ToString().ToUpper() + input.Substring(1).ToLower();
		}
	}
}
