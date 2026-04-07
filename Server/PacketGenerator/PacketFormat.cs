using System;
using System.Collections.Generic;
using System.Text;

namespace PacketGenerator
{
	class PacketFormat
	{
		// {0} 패킷 등록
		public static string managerFormat =
@"using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;

class PacketManager
{{
	#region Singleton
	static PacketManager _instance = new PacketManager();
	public static PacketManager Instance {{ get {{ return _instance; }} }}
	#endregion

	PacketManager()
	{{
		Register();
	}}

	Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>>();
	Dictionary<ushort, Action<PacketSession, IMessage>> _handler = new Dictionary<ushort, Action<PacketSession, IMessage>>();
		
	public Action<PacketSession, IMessage, ushort> CustomHandler {{ get; set; }}
	public void Register()
	{{{0}
	}}

	public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
	{{
		ushort count = 0;

		ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
		count += 2;
		ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
		count += 2;

		Action<PacketSession, ArraySegment<byte>, ushort> action = null;
		if (_onRecv.TryGetValue(id, out action))
			action.Invoke(session, buffer, id);
	}}

	void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer, ushort id) where T : IMessage, new()
	{{
		T pkt = new T();
		pkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);

		if (CustomHandler != null)
		{{
			CustomHandler.Invoke(session, pkt, id);
		}}
		else
		{{
			Action<PacketSession, IMessage> action = null;
			if (_handler.TryGetValue(id, out action))
				action.Invoke(session, pkt);
		}}
	}}

	public Action<PacketSession, IMessage> GetPacketHandler(ushort id)
	{{
		Action<PacketSession, IMessage> action = null;
		if (_handler.TryGetValue(id, out action))
			return action;
		return null;
	}}
}}";

		// {0} MsgId
		// {1} 패킷 이름
		public static string managerRegisterFormat =
@"		
		_onRecv.Add((ushort)MsgId.{0}, MakePacket<{1}>);
		_handler.Add((ushort)MsgId.{0}, PacketHandler.{1}Handler);";

		// {0} 캐시 엔트리 목록
		public static string msgIdCacheFormat =
@"using Google.Protobuf;
using Google.Protobuf.Protocol;
using System.Collections.Generic;

// Auto-generated: MsgId lookup cache to avoid runtime Enum.Parse
public static class MsgIdCache
{{
	static readonly Dictionary<string, MsgId> _nameToId = new Dictionary<string, MsgId>()
	{{{0}
	}};

	public static MsgId GetMsgId(IMessage packet)
	{{
		string name = packet.Descriptor.Name;
		if (_nameToId.TryGetValue(name, out MsgId id))
			return id;

		throw new System.ArgumentException($""Unknown packet type: {{name}}"");
	}}
}}";

		// {0} 패킷 이름 (ex: S_EnterGame)
		// {1} MsgId 이름 (ex: SEnterGame)
		public static string msgIdCacheEntryFormat =
@"
		{{ ""{0}"", MsgId.{1} }},";

	}
}
