using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;

class PacketManager
{
	#region Singleton
	static PacketManager _instance = new PacketManager();
	public static PacketManager Instance { get { return _instance; } }
	#endregion

	PacketManager()
	{
		Register();
	}

	Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>>();
	Dictionary<ushort, Action<PacketSession, IMessage>> _handler = new Dictionary<ushort, Action<PacketSession, IMessage>>();
		
	public Action<PacketSession, IMessage, ushort> CustomHandler { get; set; }
	public void Register()
	{		
		_onRecv.Add((ushort)MsgId.SEnterGame, MakePacket<S_EnterGame>);
		_handler.Add((ushort)MsgId.SEnterGame, PacketHandler.S_EnterGameHandler);		
		_onRecv.Add((ushort)MsgId.SLeaveGame, MakePacket<S_LeaveGame>);
		_handler.Add((ushort)MsgId.SLeaveGame, PacketHandler.S_LeaveGameHandler);		
		_onRecv.Add((ushort)MsgId.SSpawn, MakePacket<S_Spawn>);
		_handler.Add((ushort)MsgId.SSpawn, PacketHandler.S_SpawnHandler);		
		_onRecv.Add((ushort)MsgId.SDespawn, MakePacket<S_Despawn>);
		_handler.Add((ushort)MsgId.SDespawn, PacketHandler.S_DespawnHandler);		
		_onRecv.Add((ushort)MsgId.SMove, MakePacket<S_Move>);
		_handler.Add((ushort)MsgId.SMove, PacketHandler.S_MoveHandler);		
		_onRecv.Add((ushort)MsgId.SSkill, MakePacket<S_Skill>);
		_handler.Add((ushort)MsgId.SSkill, PacketHandler.S_SkillHandler);		
		_onRecv.Add((ushort)MsgId.SChangeHp, MakePacket<S_ChangeHp>);
		_handler.Add((ushort)MsgId.SChangeHp, PacketHandler.S_ChangeHpHandler);		
		_onRecv.Add((ushort)MsgId.SDie, MakePacket<S_Die>);
		_handler.Add((ushort)MsgId.SDie, PacketHandler.S_DieHandler);		
		_onRecv.Add((ushort)MsgId.SConnected, MakePacket<S_Connected>);
		_handler.Add((ushort)MsgId.SConnected, PacketHandler.S_ConnectedHandler);		
		_onRecv.Add((ushort)MsgId.SLogin, MakePacket<S_Login>);
		_handler.Add((ushort)MsgId.SLogin, PacketHandler.S_LoginHandler);		
		_onRecv.Add((ushort)MsgId.SCreatePlayer, MakePacket<S_CreatePlayer>);
		_handler.Add((ushort)MsgId.SCreatePlayer, PacketHandler.S_CreatePlayerHandler);		
		_onRecv.Add((ushort)MsgId.SItemList, MakePacket<S_ItemList>);
		_handler.Add((ushort)MsgId.SItemList, PacketHandler.S_ItemListHandler);		
		_onRecv.Add((ushort)MsgId.SAddItem, MakePacket<S_AddItem>);
		_handler.Add((ushort)MsgId.SAddItem, PacketHandler.S_AddItemHandler);		
		_onRecv.Add((ushort)MsgId.SEquipItem, MakePacket<S_EquipItem>);
		_handler.Add((ushort)MsgId.SEquipItem, PacketHandler.S_EquipItemHandler);		
		_onRecv.Add((ushort)MsgId.SChangeStat, MakePacket<S_ChangeStat>);
		_handler.Add((ushort)MsgId.SChangeStat, PacketHandler.S_ChangeStatHandler);		
		_onRecv.Add((ushort)MsgId.SPing, MakePacket<S_Ping>);
		_handler.Add((ushort)MsgId.SPing, PacketHandler.S_PingHandler);		
		_onRecv.Add((ushort)MsgId.STutorial, MakePacket<S_Tutorial>);
		_handler.Add((ushort)MsgId.STutorial, PacketHandler.S_TutorialHandler);		
		_onRecv.Add((ushort)MsgId.SMapChange, MakePacket<S_MapChange>);
		_handler.Add((ushort)MsgId.SMapChange, PacketHandler.S_MapChangeHandler);		
		_onRecv.Add((ushort)MsgId.SStartQuest, MakePacket<S_StartQuest>);
		_handler.Add((ushort)MsgId.SStartQuest, PacketHandler.S_StartQuestHandler);		
		_onRecv.Add((ushort)MsgId.SQuestComplete, MakePacket<S_QuestComplete>);
		_handler.Add((ushort)MsgId.SQuestComplete, PacketHandler.S_QuestCompleteHandler);		
		_onRecv.Add((ushort)MsgId.SShopList, MakePacket<S_ShopList>);
		_handler.Add((ushort)MsgId.SShopList, PacketHandler.S_ShopListHandler);		
		_onRecv.Add((ushort)MsgId.SBuyItem, MakePacket<S_BuyItem>);
		_handler.Add((ushort)MsgId.SBuyItem, PacketHandler.S_BuyItemHandler);		
		_onRecv.Add((ushort)MsgId.SChangePosition, MakePacket<S_ChangePosition>);
		_handler.Add((ushort)MsgId.SChangePosition, PacketHandler.S_ChangePositionHandler);		
		_onRecv.Add((ushort)MsgId.SDropItem, MakePacket<S_DropItem>);
		_handler.Add((ushort)MsgId.SDropItem, PacketHandler.S_DropItemHandler);		
		_onRecv.Add((ushort)MsgId.SRemoveItem, MakePacket<S_RemoveItem>);
		_handler.Add((ushort)MsgId.SRemoveItem, PacketHandler.S_RemoveItemHandler);		
		_onRecv.Add((ushort)MsgId.SRespawn, MakePacket<S_Respawn>);
		_handler.Add((ushort)MsgId.SRespawn, PacketHandler.S_RespawnHandler);		
		_onRecv.Add((ushort)MsgId.SLoading, MakePacket<S_Loading>);
		_handler.Add((ushort)MsgId.SLoading, PacketHandler.S_LoadingHandler);		
		_onRecv.Add((ushort)MsgId.SQuestList, MakePacket<S_QuestList>);
		_handler.Add((ushort)MsgId.SQuestList, PacketHandler.S_QuestListHandler);		
		_onRecv.Add((ushort)MsgId.SChangeExp, MakePacket<S_ChangeExp>);
		_handler.Add((ushort)MsgId.SChangeExp, PacketHandler.S_ChangeExpHandler);		
		_onRecv.Add((ushort)MsgId.SDamage, MakePacket<S_Damage>);
		_handler.Add((ushort)MsgId.SDamage, PacketHandler.S_DamageHandler);		
		_onRecv.Add((ushort)MsgId.SMakeChest, MakePacket<S_MakeChest>);
		_handler.Add((ushort)MsgId.SMakeChest, PacketHandler.S_MakeChestHandler);		
		_onRecv.Add((ushort)MsgId.SSkillCool, MakePacket<S_SkillCool>);
		_handler.Add((ushort)MsgId.SSkillCool, PacketHandler.S_SkillCoolHandler);		
		_onRecv.Add((ushort)MsgId.SPartyInvite, MakePacket<S_PartyInvite>);
		_handler.Add((ushort)MsgId.SPartyInvite, PacketHandler.S_PartyInviteHandler);		
		_onRecv.Add((ushort)MsgId.SInteraction, MakePacket<S_Interaction>);
		_handler.Add((ushort)MsgId.SInteraction, PacketHandler.S_InteractionHandler);		
		_onRecv.Add((ushort)MsgId.SChangeAdditionalStat, MakePacket<S_ChangeAdditionalStat>);
		_handler.Add((ushort)MsgId.SChangeAdditionalStat, PacketHandler.S_ChangeAdditionalStatHandler);
	}

	public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
	{
		ushort count = 0;

		ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
		count += 2;
		ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
		count += 2;

		Action<PacketSession, ArraySegment<byte>, ushort> action = null;
		if (_onRecv.TryGetValue(id, out action))
			action.Invoke(session, buffer, id);
	}

	void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer, ushort id) where T : IMessage, new()
	{
		T pkt = new T();
		pkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);

        if (CustomHandler != null)
        {
            CustomHandler.Invoke(session, pkt, id);
        }
		else
		{
		Action<PacketSession, IMessage> action = null;
		if (_handler.TryGetValue(id, out action))
			action.Invoke(session, pkt);
		}
	}

	public Action<PacketSession, IMessage> GetPacketHandler(ushort id)
	{
		Action<PacketSession, IMessage> action = null;
		if (_handler.TryGetValue(id, out action))
			return action;
		return null;
	}
}