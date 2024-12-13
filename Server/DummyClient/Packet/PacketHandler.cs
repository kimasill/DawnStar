using DummyClient.Session;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using static System.Net.Mime.MediaTypeNames;


class PacketHandler
{
    // Step 4
    public static void S_EnterGameHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame enterGamePacket = packet as S_EnterGame;
    }
    public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
    {
        S_LeaveGame leaveGameHandler = packet as S_LeaveGame;
    }
    public static void S_SpawnHandler(PacketSession session, IMessage packet)
    {
        S_Spawn spawnPacket = packet as S_Spawn;

        foreach (ObjectInfo obj in spawnPacket.Objects)
        {
        }
    }
    public static void S_DespawnHandler(PacketSession session, IMessage packet)
    {
        S_Despawn despawnPacket = packet as S_Despawn;
        foreach (int obj in despawnPacket.ObjectId)
        {
        }
    }
    public static void S_MoveHandler(PacketSession session, IMessage packet)
    {
    }

    public static void S_SkillHandler(PacketSession session, IMessage packet)
    {
    }

    public static void S_ChangeHpHandler(PacketSession session, IMessage packet)
    {
        S_ChangeHp changePacket = packet as S_ChangeHp;

    }
    public static void S_DieHandler(PacketSession session, IMessage packet)
    {
        S_Die diePacket = packet as S_Die;
    }

    // Step 1
    public static void S_ConnectedHandler(PacketSession session, IMessage packet)
    {
        C_Login loginPacket = new C_Login();
        ServerSession serverSession = (ServerSession)session;
        loginPacket.UniqueId = $"DummyClient_{serverSession.DummyId.ToString("0000")}";
        serverSession.Send(loginPacket);
    }

    // Step 2
    //로그인 결과 + 캐릭터 목록
    public static void S_LoginHandler(PacketSession session, IMessage packet)
    {
        S_Login loginPacket = (S_Login)packet;
        ServerSession serverSession = (ServerSession)session;

        //TODO: 로비 UI 에서 캐릭터 선택하도록 만들기
        if (loginPacket.Players == null || loginPacket.Players.Count == 0)
        {
            C_CreatePlayer createPlayerPacket = new C_CreatePlayer();
            createPlayerPacket.Name = $"Player_{serverSession.DummyId.ToString("0000")}";
            serverSession.Send(createPlayerPacket);
        }
        else
        {
            //일단 첫번째로 로그인
            LobbyPlayerInfo info = loginPacket.Players[0];
            C_EnterGame enterGamePacket = new C_EnterGame();
            enterGamePacket.Name = info.Name;
            serverSession.Send(enterGamePacket);
        }
    }

    // Step 3
    public static void S_CreatePlayerHandler(PacketSession session, IMessage packet)
    {
        S_CreatePlayer createOkPacket = (S_CreatePlayer)packet;
        ServerSession serverSession = (ServerSession)session;
        
        if (createOkPacket.Player == null)
        {            
        }
        else
        {
            C_EnterGame enterGamePacket = new C_EnterGame();
            enterGamePacket.Name = createOkPacket.Player.Name;
            serverSession.Send(enterGamePacket);
        }
    }

    public static void S_ItemListHandler(PacketSession session, IMessage packet)
    {

        S_ItemList itemPacket = packet as S_ItemList;
    }

    public static void S_AddItemHandler(PacketSession session, IMessage packet)
    {

        S_AddItem itemList = packet as S_AddItem;
    }
    public static void S_EquipItemHandler(PacketSession session, IMessage packet)
    {
        S_EquipItem equipItemOk = packet as S_EquipItem;
    }
    public static void S_ChangeStatHandler(PacketSession session, IMessage packet)
    {

        S_ChangeStat itemList = (S_ChangeStat)packet;

    }

    public static void S_PingHandler(PacketSession session, IMessage packet)
    {
        C_Pong pongPacket = new C_Pong();
    }

    public static void S_TutorialHandler(PacketSession session, IMessage packet)
    {
        S_Tutorial tutorialPacket = packet as S_Tutorial;
    }

    public static void S_MapChangeHandler(PacketSession session, IMessage packet)
    {
        S_MapChange mapChangePacket = packet as S_MapChange;
    }

    public static void S_StartQuestHandler(PacketSession session, IMessage packet)
    {
        S_StartQuest startQuest = packet as S_StartQuest;
    }

    public static void S_QuestCompleteHandler(PacketSession session, IMessage packet)
    {
        S_QuestComplete QuestComplete = packet as S_QuestComplete;
    }

    public static void S_ShopListHandler(PacketSession session, IMessage packet)
    {
        S_ShopList shopList = packet as S_ShopList;
    }

    public static void S_BuyItemHandler(PacketSession session, IMessage packet)
    {
        S_BuyItem buyItem = packet as S_BuyItem;
    }

    public static void S_ChangePositionHandler(PacketSession session, IMessage packet)
    {
        S_ChangePosition inventory = packet as S_ChangePosition;
    }
    public static void S_DropItemHandler(PacketSession session, IMessage packet)
    {
        S_DropItem dropItem = packet as S_DropItem;
    }
    public static void S_RemoveItemHandler(PacketSession session, IMessage packet)
    {
        S_RemoveItem removeItem = packet as S_RemoveItem;
    }
    public static void S_RespawnHandler(PacketSession session, IMessage packet)
    {
        S_Respawn respawn = packet as S_Respawn;
    }
    public static void S_LoadingHandler(PacketSession session, IMessage packet)
    {
        S_Loading loading = packet as S_Loading;
    }
    public static void S_QuestListHandler(PacketSession session, IMessage packet)
    {
        S_QuestList questList = packet as S_QuestList;
    }
    public static void S_ChangeExpHandler(PacketSession session, IMessage packet)
    {
        S_ChangeExp changeExp = packet as S_ChangeExp;
    }
    public static void S_DamageHandler(PacketSession session, IMessage packet)
    {
        S_Damage damage = packet as S_Damage;
    }
    public static void S_MakeChestHandler(PacketSession session, IMessage packet)
    {
        S_MakeChest chest = packet as S_MakeChest;
    }

    public static void S_SkillCoolHandler(PacketSession session, IMessage packet)
    {
        S_SkillCool skillCool = packet as S_SkillCool;
    }

    public static void S_PartyInviteHandler(PacketSession session, IMessage packet)
    {
        S_PartyInvite partyInvite = packet as S_PartyInvite;
    }

    public static void S_InteractionHandler(PacketSession session, IMessage packet)
    {
        S_Interaction interaction = packet as S_Interaction;
    }
    public static void S_ChangeAdditionalStatHandler(PacketSession session, IMessage packet)
    {
        S_ChangeAdditionalStat additionalStat = packet as S_ChangeAdditionalStat;
    }

    public static void S_EffectHandler(PacketSession session, IMessage packet)
    {
        S_Effect effect = packet as S_Effect;
    }

    public static void S_BossKillHandler(PacketSession session, IMessage packet)
    {
        S_BossKill bossKill = packet as S_BossKill;
    }

    public static void S_EnhanceHandler(PacketSession session, IMessage packet)
    {
        S_Enhance enhance = packet as S_Enhance;
    }
}
