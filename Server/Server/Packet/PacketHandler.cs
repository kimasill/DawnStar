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

        if (player == null)
            return;
        room.Push(room.HandleMapChanged, player, mapChangePacket.PortalId);
    }

    public static void C_EnterDungeonHandler(PacketSession session, IMessage packet)
    {
        ClientSession clientSession = session as ClientSession;
        C_EnterDungeon enterDungeonPacket = packet as C_EnterDungeon;
        Player player = clientSession.MyPlayer;
        GameRoom room = player.Room;
        if (room == null)
            return;
        if (enterDungeonPacket.AdmitType == AdmitType.None)
        {
            room.Push(room.HandleEnterDungeon, player, enterDungeonPacket.MapId);
        }
        else if(enterDungeonPacket.AdmitType == AdmitType.Matching || enterDungeonPacket.AdmitType == AdmitType.Cancel)
        {
            room.Push(room.HandleMatching, player, enterDungeonPacket.MapId, enterDungeonPacket.AdmitType);
        }
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
        room.Push(room.HandleRequestShop, player, requestShopPacket.ShopId);
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
    public static void C_SellItemHandler(PacketSession session, IMessage packet)
    {
        C_SellItem sellItemPacket = packet as C_SellItem;
        ClientSession clientSession = session as ClientSession;
        Player player = clientSession.MyPlayer;
        if (player == null)
            return;
        GameRoom room = player.Room;
        if (room == null)
            return;
        room.Push(room.HandleSellItem, player, sellItemPacket);
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

    public static void C_OpenChestHandler(PacketSession session, IMessage packet)
    {
        C_OpenChest openChestPacket = packet as C_OpenChest;
        ClientSession clientSession = session as ClientSession;

        if (clientSession == null || clientSession.MyPlayer == null)
            return;

        GameRoom room = clientSession.MyPlayer.Room;
        if (room == null)
            return;

        room.HandleOpenChest(clientSession.MyPlayer, openChestPacket);
    }

    public static void C_UseItemHandler(PacketSession session, IMessage packet)
    {
        C_UseItem useItemPacket = packet as C_UseItem;
        ClientSession clientSession = session as ClientSession;

        if (clientSession == null || clientSession.MyPlayer == null)
            return;

        GameRoom room = clientSession.MyPlayer.Room;
        if (room == null)
            return;

        room.HandleUseItem(clientSession.MyPlayer, useItemPacket);
    }

    public static void C_SelectStatHandler(PacketSession session, IMessage packet)
    {
        C_SelectStat selectStatPacket = packet as C_SelectStat;
        ClientSession clientSession = session as ClientSession;

        if (clientSession == null || clientSession.MyPlayer == null)
            return;

        GameRoom room = clientSession.MyPlayer.Room;
        if (room == null)
            return;

        room.HandleSelectStat(clientSession.MyPlayer, selectStatPacket.TemplateId);
    }

    public static void C_InteractionHandler(PacketSession session, IMessage packet)
    {
        C_Interaction interactionPacket = packet as C_Interaction;
        ClientSession clientSession = session as ClientSession;

        if (clientSession == null || clientSession.MyPlayer == null)
            return;

        GameRoom room = clientSession.MyPlayer.Room;
        if (room == null)
            return;

        if(interactionPacket.InteractionType == InteractionType.None)
            return;
        else if (interactionPacket.InteractionType == InteractionType.Door)
            room.HandleDoorInteraction(clientSession.MyPlayer, interactionPacket.ObjectId);        
        else if (interactionPacket.InteractionType == InteractionType.Trigger)
            room.HandleTriggerInteraction(clientSession.MyPlayer, interactionPacket.ObjectId);
        else if (interactionPacket.InteractionType == InteractionType.ItemTable)
            room.HandleItemTableInteraction(clientSession.MyPlayer, interactionPacket.ObjectId);
        else if (interactionPacket.InteractionType == InteractionType.Quest)
            room.HandleQuestSignInteraction(clientSession.MyPlayer, interactionPacket.ObjectId);
    }

    public static void C_RequestStatHandler(PacketSession session, IMessage packet)
    {
        C_RequestStat requestStatPacket = packet as C_RequestStat;
        ClientSession clientSession = session as ClientSession;

        clientSession.MyPlayer.RefreshAdditionalStat();
        clientSession.MyPlayer.SendAdditionalStat();
    }

    public static void C_EnhanceHandler(PacketSession session, IMessage packet)
    {
        C_Enhance enhancePacket = packet as C_Enhance;
        ClientSession clientSession = session as ClientSession;

        if (clientSession == null || clientSession.MyPlayer == null)
            return;

        GameRoom room = clientSession.MyPlayer.Room;
        if (room == null)
            return;

        room.HandleEnhanceItem(clientSession.MyPlayer, enhancePacket);
    }

    public static void C_EnchantHandler(PacketSession session, IMessage packet)
    {
        C_Enchant enchantPacket = packet as C_Enchant;
        ClientSession clientSession = session as ClientSession;

        if (clientSession == null || clientSession.MyPlayer == null)
            return;

        GameRoom room = clientSession.MyPlayer.Room;
        if (room == null)
            return;

        room.HandleEnchantItem(clientSession.MyPlayer, enchantPacket);
    }
    public static void C_MakeItemHandler(PacketSession session, IMessage packet)
    {
        C_MakeItem makeItemPacket = packet as C_MakeItem;
        ClientSession clientSession = session as ClientSession;

        if (clientSession == null || clientSession.MyPlayer == null)
            return;

        GameRoom room = clientSession.MyPlayer.Room;
        if (room == null)
            return;

        room.HandleMakeItem(clientSession.MyPlayer, makeItemPacket);
    }

    public static void C_SortItemHandler(PacketSession session, IMessage packet)
    {
        C_SortItem sortItemPacket = packet as C_SortItem;
        ClientSession clientSession = session as ClientSession;

        if (clientSession == null || clientSession.MyPlayer == null)
            return;

        GameRoom room = clientSession.MyPlayer.Room;
        if (room == null)
            return;

        clientSession.MyPlayer.Inven.SortInven(clientSession.MyPlayer);
    }

    public static void C_ChatHandler(PacketSession session, IMessage packet)
    {
        C_Chat chatPacket = packet as C_Chat;
        ClientSession clientSession = session as ClientSession;
        if (clientSession == null || clientSession.MyPlayer == null)
            return;
        GameRoom room = clientSession.MyPlayer.Room;
        if (room == null)
            return;
        room.HandleChat(clientSession.MyPlayer, chatPacket.Message);
    }
}
