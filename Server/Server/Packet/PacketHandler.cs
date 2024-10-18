using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using Server.DB;
using Server.Game;
using Server.Game.Room;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class PacketHandler
{
	//(RedZone)
	public static void C_MoveHandler(PacketSession session, IMessage packet)
	{
		//클라이언트의 이동 패킷을 받았을 때
		C_Move movePacket = packet as C_Move;
		ClientSession clientSession = session as ClientSession;

        //Console.WriteLine($"C_Move ({movePacket.Position.PosX}, {movePacket.Position.PosY}"); ;

		Player player = clientSession.MyPlayer;
		if (player == null)
			return;

		GameRoom room = player.Room;
		if(room == null)
			return;

		room.Push(room.HandleMove, player, movePacket);
		//검증
		//좌표이동
		
    }

	public static void C_SkillHandler(PacketSession session, IMessage packet) {
        
		C_Skill skillPacket = packet as C_Skill;
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;

        room.Push(room.HandleSkill, player, skillPacket);
    }

    public static void C_LoginHandler(PacketSession session, IMessage packet)
    {

        C_Login loginPacket = packet as C_Login;
        ClientSession clientSession = session as ClientSession;
        clientSession.HandleLogin(loginPacket);
    }

    public static void C_EnterGameHandler(PacketSession session, IMessage packet)
    {
        C_EnterGame enterGamePacket = (C_EnterGame)packet;
        ClientSession clientSession = session as ClientSession;
        clientSession.HandleEnterGame(enterGamePacket);
    }

    public static void C_RespawnHandler(PacketSession session, IMessage packet)
    {
        C_Respawn respawnPacket = (C_Respawn)packet;
        ClientSession clientSession = session as ClientSession;
        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;
        
        room.Push(room.HandleRespawn, player, respawnPacket.RespawnType);
    }

    public static void C_CreatePlayerHandler(PacketSession session, IMessage packet)
    {
        C_CreatePlayer createPlayerPacket = (C_CreatePlayer)packet;
        ClientSession clientSession = session as ClientSession;
        clientSession.HandleCreatePlayer(createPlayerPacket);
    }

    public static void C_EquipItemHandler(PacketSession session, IMessage packet)
    {
        C_EquipItem equipPacket = (C_EquipItem)packet;
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;

        room.Push(room.HandleEquipItem, player, equipPacket);
    }

    public static void C_QuestCompleteHandler(PacketSession session, IMessage packet)
    {
        C_QuestComplete completeQuestPacket = (C_QuestComplete)packet;
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;

        room.Push(room.HandleQuestComplete, player, completeQuestPacket.TemplateId);
    }

    public static void C_StartQuestHandler(PacketSession session, IMessage packet)
    {
        C_StartQuest startQuestPacket = (C_StartQuest)packet;
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;

        room.Push(room.HandleStartQuest, player, startQuestPacket.TemplateId);
    }

    public static void C_PongHandler(PacketSession session, IMessage packet)
    {
        ClientSession clientSession = session as ClientSession;
        clientSession.HandlePong();
    }

    public static void C_MapChangeHandler(PacketSession session, IMessage packet)
    {
        ClientSession clientSession = session as ClientSession;
        C_MapChange mapChangePacket = packet as C_MapChange;
        Player player = clientSession.MyPlayer;
        GameRoom room = player.Room;
        if (room == null)
            return;
        //GameLogic.Instance.Push(() =>
        //{
        //});

        if (player == null)
            return;
        room.Push(room.HandleMapChanged, player, mapChangePacket.MapId);
    }

    public static void C_ChangePositionHandler(PacketSession session, IMessage packet)
    {
        ClientSession clientSession = session as ClientSession;
        Player player = clientSession.MyPlayer;
        if (player == null)
            return;
        GameRoom room = player.Room;
        if (room == null)
            return;
        room.Push(room.HandleChangePosition, player);
    }

    public static void C_RequestShopHandler(PacketSession session, IMessage packet)
    {
        C_RequestShop requestShopPacket = packet as C_RequestShop;
        ClientSession clientSession = session as ClientSession;
        Player player = clientSession.MyPlayer;
        if (player == null)
            return;
        GameRoom room = player.Room;
        if (room == null)
            return;
        room.Push(room.HandleRequestShop, player);
    }

    public static void C_BuyItemHandler(PacketSession session, IMessage packet)
    {
        C_BuyItem buyItemPacket = packet as C_BuyItem;
        ClientSession clientSession = session as ClientSession;
        Player player = clientSession.MyPlayer;
        if (player == null)
            return;
        GameRoom room = player.Room;
        if (room == null)
            return;
        room.Push(room.HandleBuyItem, player, buyItemPacket);
    }

    public static void C_LootItemHandler(PacketSession session, IMessage packet)
    {
        C_LootItem rootItemPacket = packet as C_LootItem;
        ClientSession clientSession = session as ClientSession;
        Player player = clientSession.MyPlayer;
        if (player == null)
            return;
        GameRoom room = player.Room;
        if (room == null)
            return;
        room.Push(room.HandleLootItem, player, rootItemPacket);
    }

    public static void C_RemoveItemHandler(PacketSession session, IMessage packet)
    {
        C_RemoveItem removeItemPacket = packet as C_RemoveItem;
        ClientSession clientSession = session as ClientSession;
        Player player = clientSession.MyPlayer;
        if (player == null)
            return;
        GameRoom room = player.Room;
        if (room == null)
            return;
        room.Push(room.HandleRemoveItem, player, removeItemPacket);
    }

    public static void C_RequestMonsterHandler(PacketSession session, IMessage packet)
    {
        C_RequestMonster spawnPacket = packet as C_RequestMonster;
        ClientSession clientSession = session as ClientSession;

        if (clientSession == null || clientSession.MyPlayer == null)
            return;

        GameRoom room = clientSession.MyPlayer.Room;
        if (room == null)
            return;

        room.HandleSpawnMonster(clientSession.MyPlayer, spawnPacket.Id.ToList());
    }
}
