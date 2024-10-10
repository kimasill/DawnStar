using Data;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;


class PacketHandler
{
	public static void S_EnterGameHandler(PacketSession session, IMessage packet)
	{
		S_EnterGame enterGamePacket = packet as S_EnterGame; 
        
        if (enterGamePacket.Player.MapInfo != null)
        {
            Managers.Scene.LoadScene(enterGamePacket.Player.MapInfo.Scene);
            SceneManager.sceneLoaded += (scene, mode) => OnSceneLoaded(enterGamePacket.Player);                  
        }
	}

    public static void OnFirstSceneLoaded(ObjectInfo objectInfo)
    {
        Managers.Object.Add(objectInfo, myPlayer: true);
        C_StartQuest startQuestPacket = new C_StartQuest();
        startQuestPacket.TemplateId = 0;
        Managers.Network.Send(startQuestPacket);
        SceneManager.sceneLoaded -= (s, m) => OnFirstSceneLoaded(objectInfo);
    }

    public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
    {
        S_LeaveGame leaveGameHandler = packet as S_LeaveGame;
        ServerSession serverSession = session as ServerSession;
        Managers.Object.Clear();
    }
    public static void S_SpawnHandler(PacketSession session, IMessage packet)
    {
        S_Spawn spawnPacket = packet as S_Spawn;        
        Debug.Log("S_SpawnHandler");
        
        foreach (ObjectInfo obj in spawnPacket.Objects)
        {
             Managers.Object.Add(obj, myPlayer: false);
        }
    }
    public static void S_DespawnHandler(PacketSession session, IMessage packet)
    {
        S_Despawn despawnPacket = packet as S_Despawn;
        foreach (int obj in despawnPacket.ObjectId)
        {
            Managers.Object.Remove(obj);
        }
    }

    public static void S_LoadingHandler(PacketSession session, IMessage packet)
    {
        if (Managers.UI.IsLoading())
        {
            Debug.Log("Loading is already in progress or completed.");
            return;
        }

        S_Loading loadingPacket = packet as S_Loading;        
        if(loadingPacket.Loading == false)
        {
            Managers.UI.HideLoadingUI();
        }
        else
        {
            Managers.UI.ShowLoadingUI();
        }
    }

    public static void S_MoveHandler(PacketSession session, IMessage packet)
    {
        S_Move movePacket = packet as S_Move;
        ServerSession serverSession = session as ServerSession;

        //다른 player 이동
        GameObject go = Managers.Object.FindById(movePacket.ObjectId);
        if(go == null)
        {
            return;
        }

        if(Managers.Object.MyPlayer.Id == movePacket.ObjectId)
        {
            return;
        }
        BaseController bc = go.GetComponent<BaseController>();
        if(bc == null)
        {
            return;
        }

        bc.PosInfo = movePacket.Position;
    }

    public static void S_SkillHandler(PacketSession session, IMessage packet)
    {
        S_Skill skillPacket = packet as S_Skill;       

        GameObject go = Managers.Object.FindById(skillPacket.ObjectId);
        if (go == null)
        {
            return;
        }

        CreatureController cc = go.GetComponent<CreatureController>();
        if (cc == null)
        {
            return;
        }        
        cc.UseSkill(skillPacket.Info.SkillId);
    }

    public static void S_ChangeHpHandler(PacketSession session, IMessage packet)
    {
        S_ChangeHp changePacket = packet as S_ChangeHp;

        GameObject go = Managers.Object.FindById(changePacket.ObjectId);
        if (go == null)
        {
            return;
        }

        CreatureController cc = go.GetComponent<CreatureController>();
        if (cc == null)
        {
            return;
        }
        if(cc.Hp > changePacket.Hp)
        {
            cc.OnDamaged();
        }
        else if (cc.Hp < changePacket.Hp)
        {
            //TODO 회복
        }
        cc.Hp = changePacket.Hp;
    }
    public static void S_DieHandler(PacketSession session, IMessage packet)
    {
        S_Die diePacket = packet as S_Die;

        GameObject go = Managers.Object.FindById(diePacket.ObjectId);
        if (go == null)
        {
            return;
        }

        CreatureController cc = go.GetComponent<CreatureController>();
        if (cc == null)
        {
            return;
        }
        cc.Hp = 0;
        cc.OnDead();
    }

    public static void S_RespawnHandler(PacketSession session, IMessage packet)
    {
        S_Respawn respawnPacket = packet as S_Respawn;

        // 부활 UI 생성
        UI_Respawn respawnUI = Managers.UI.ShowPopupUI<UI_Respawn>();
        respawnUI.SetRespawnPacket(respawnPacket);

        // 부활 UI가 활성화 되어 있는 동안 다른 조작 차단
        Managers.Object.MyPlayer.gameObject.SetActive(false);
    }
    public static void S_ConnectedHandler(PacketSession session, IMessage packet)
    {
        Debug.Log("S_ConnectedHandler");
        C_Login loginPacket = new C_Login();

        string path = Application.dataPath;
        loginPacket.UniqueId = path.GetHashCode().ToString();

        Managers.Network.Send(loginPacket);
    }

    //로그인 결과 + 캐릭터 목록
    public static void S_LoginHandler(PacketSession session, IMessage packet)
    {
        S_Login loginPacket = (S_Login)packet;
        Debug.Log($"S_LoginHandler {loginPacket.LoginOk}");

        //TODO: 로비 UI 에서 캐릭터 선택하도록 만들기
        if(loginPacket.Players == null || loginPacket.Players.Count == 0)
        {
            C_CreatePlayer createPlayerPacket = new C_CreatePlayer();
            createPlayerPacket.Name = $"Player_{Random.Range(0 , 1000).ToString("0000")}";
            Managers.Network.Send(createPlayerPacket);
        }
        else
        {
            //일단 첫번째로 로그인
            LobbyPlayerInfo info = loginPacket.Players[0];
            C_EnterGame enterGamePacket = new C_EnterGame();
            enterGamePacket.Name = info.Name;
            Managers.Network.Send(enterGamePacket);
        }
    }

    public static void S_TutorialHandler(PacketSession session, IMessage packet)
    {
        S_Tutorial tutorialPacket = (S_Tutorial)packet;
        Debug.Log($"Tutorial : {tutorialPacket.TutorialInfo}");
        //Managers.Object.Add(enterGamePacket.Player, myPlayer: true);
    }

    public static void S_MapChangeHandler(PacketSession session, IMessage packet)
    {
        S_MapChange mapChangePacket = (S_MapChange)packet;
        Debug.Log($"S_MapChangeHandler: Changing to map {mapChangePacket.MapId}");
        
        Managers.Object.Remove(Managers.Object.MyPlayer.Id);
        Managers.Data.MapDict.TryGetValue(mapChangePacket.MapId, out MapData map);
        Managers.Scene.LoadScene(map.name);
        SceneManager.sceneLoaded += (scene, mode) => OnSceneLoaded(mapChangePacket.ObjectInfo);
    }

    public static void OnSceneLoaded(ObjectInfo objectInfo)
    {
        if (objectInfo == null)
        {
            Debug.LogError("ObjectInfo is null in OnSceneLoaded");
            return;
        }

        // 플레이어를 새로운 맵에 다시 생성
        Managers.Object.Add(objectInfo, myPlayer: true);

        C_StartQuest startQuestPacket = new C_StartQuest();
        startQuestPacket.TemplateId = 0;
        Managers.Network.Send(startQuestPacket);

        // 플레이어 정보 업데이트
        if (Managers.Object.MyPlayer != null)
        {
            Managers.Object.MyPlayer.PosInfo = objectInfo.Position;
            Managers.Object.MyPlayer.Stat.MergeFrom(objectInfo.StatInfo);
        }

        // 이벤트 핸들러 제거
        SceneManager.sceneLoaded -= (s, m) => OnSceneLoaded(objectInfo);
    }

    public static void S_ChangePositionHandler(PacketSession session, IMessage packet)
    {
        S_ChangePosition changePosPacket = (S_ChangePosition)packet;
        Managers.Object.MyPlayer.PosInfo = changePosPacket.Position;
        Managers.Object.MyPlayer.SyncPos();
    }

    public static void S_CreatePlayerHandler(PacketSession session, IMessage packet)
    {
        S_CreatePlayer createOkPacket = (S_CreatePlayer)packet;
        if (createOkPacket.Player == null)
        {
            C_CreatePlayer createPlayerPacket = new C_CreatePlayer();
            createPlayerPacket.Name = $"Player_{Random.Range(0, 1000).ToString("0000")}";
            Managers.Network.Send(createPlayerPacket);
        }
        else
        {
            C_EnterGame enterGamePacket = new C_EnterGame();
            enterGamePacket.Name = createOkPacket.Player.Name;
            Managers.Network.Send(enterGamePacket);
        }
    }

    public static void S_ItemListHandler(PacketSession session, IMessage packet)
    {
        S_ItemList itemPacket = packet as S_ItemList;
        Managers.Inventory.Clear();

        //메모리에 아이템 정보 저장
        foreach (ItemInfo itemInfo in itemPacket.Items)
        {
            Item item = Item.MakeItem(itemInfo);
            Managers.Inventory.Add(item);
        }
        if (Managers.Object.MyPlayer != null)
        {
            Managers.Object.MyPlayer.RefreshAdditionalStat();
            EquipmentController equipment = Managers.Object.MyPlayer.Equipment;
            Managers.Inventory.RefreshEquipment(equipment);
        }
    }

    public static void S_AddItemHandler(PacketSession session, IMessage packet)
    {

        S_AddItem itemList = packet as S_AddItem;
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        //메모리에 아이템 정보 저장
        foreach (ItemInfo itemInfo in itemList.Items)
        {
            Item item = Item.MakeItem(itemInfo);
            Managers.Inventory.Add(item);
            gameSceneUI.NotificationUI.ShowItemNoti(item);
        }

        Debug.Log("S_AddItem");

        
        //아이템 획득시 자동 갱신
        gameSceneUI.InvenUI.RefreshUI();
        gameSceneUI.StatUI.RefreshUI();

        if (Managers.Object.MyPlayer != null)
        {
            Managers.Object.MyPlayer.RefreshAdditionalStat();
        }   
    }

    public static void S_RemoveItemHandler(PacketSession session, IMessage packet)
    {
        S_RemoveItem removeItem = packet as S_RemoveItem;
        //메모리에서 아이템 정보 삭제 or 수정
        foreach (ItemInfo item in removeItem.Items)
        {            
            Managers.Inventory.RemoveOrUpdate(item);
        }

        Debug.Log("S_RemoveItem");

        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        gameSceneUI.InvenUI.RefreshUI();
        gameSceneUI.StatUI.RefreshUI();
        if (Managers.Object.MyPlayer != null)
        {
            Managers.Object.MyPlayer.RefreshAdditionalStat();
        }
    }

    public static void S_EquipItemHandler(PacketSession session, IMessage packet)
    {
        S_EquipItem equipItemOk = packet as S_EquipItem;

        //메모리에 아이템 정보 적용
        Item item  = Managers.Inventory.Get(equipItemOk.ItemDbId);
        if(item == null)
        {
            return;
        }
        item.Equipped = equipItemOk.Equipped;        
        Debug.Log("S_EquipItemHandler");

        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        gameSceneUI.InvenUI.RefreshUI();
        gameSceneUI.StatUI.RefreshUI();
        if (Managers.Object.MyPlayer != null)
        {
            Managers.Object.MyPlayer.Equipment.SetItemInSlot(item);
            Managers.Object.MyPlayer.RefreshAdditionalStat();
        }            
    }

    public static void S_ShopListHandler(PacketSession session, IMessage packet)
    {
        S_ShopList shopList = packet as S_ShopList;
        Managers.Shop.Clear();

        foreach (ItemInfo itemInfo in shopList.Items)
        {            
            Item item = Item.MakeItem(itemInfo);
            Managers.Shop.Add(item);
        }

        Debug.Log("S_ShopListHandler");
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        gameSceneUI.ShopUI.RefreshUI();
    }

    public static void S_ChangeStatHandler(PacketSession session, IMessage packet)
    {

        S_ChangeStat statPacket = (S_ChangeStat)packet;
        Managers.Object.MyPlayer.Stat.MergeFrom(statPacket.StatInfo);        
    }

    public static void S_PingHandler(PacketSession session, IMessage packet)
    {
        C_Pong pongPacket = new C_Pong();
        Debug.Log("S_PingHandler");
        Managers.Network.Send(pongPacket);
    }

    public static void S_StartQuestHandler(PacketSession session, IMessage packet)
    {
        S_StartQuest questPacket = (S_StartQuest)packet;
        Quest quest = Quest.MakeQuest(questPacket.Quest);
        Managers.Quest.Add(quest);
        Debug.Log($"Start Quest : {questPacket.Quest.TemplateId}");
        Managers.Quest.StartQuest(quest.TemplateId);
    }

    public static void S_QuestCompleteHandler(PacketSession session, IMessage packet)
    {
        S_QuestComplete questPacket = (S_QuestComplete)packet;
        Managers.Quest.UpdateQuest(questPacket.Quest);
        Debug.Log($"Complete Quest : {questPacket.Quest.TemplateId}");
    }

    public static void S_QuestListHandler(PacketSession session, IMessage packet)
    {
        S_QuestList questListPacket = (S_QuestList)packet;
        Managers.Quest.Clear();
        foreach (QuestInfo questInfo in questListPacket.Quests)
        {
            Quest quest = Quest.MakeQuest(questInfo);
            Managers.Quest.Add(quest);
        }
        Debug.Log("S_QuestListHandler");
    }
    public static void S_BuyItemHandler(PacketSession session, IMessage packet)
    {
        S_BuyItem buyItemPacket = packet as S_BuyItem;

        if (buyItemPacket == null || buyItemPacket.Count == 0)
            return;

        Debug.Log("S_BuyItemHandler");
        // 상점에서 아이템 제거
        Managers.Shop.RemoveItem(buyItemPacket.TemplateId);

        // 상점 UI 갱신
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        if (gameSceneUI != null)
        {
            gameSceneUI.ShopUI.RefreshUI();
        }
    }

    public static void S_DropItemHandler(PacketSession session, IMessage packet)
    {
        S_DropItem dropItemPacket = packet as S_DropItem;
        if (dropItemPacket == null)
            return;

        // 아이템 데이터 찾기
        ItemData itemData = null; 
        Managers.Data.ItemDict.TryGetValue(dropItemPacket.TemplateId, out itemData);
        if (itemData == null)
        {
            Debug.LogError($"ItemData not found for TemplateId: {dropItemPacket.TemplateId}");
            return;
        }
        Managers.Object.GenerateId(GameObjectType.Item, out int tempId);
        
        // ObjectInfo 생성
        ObjectInfo objectInfo = new ObjectInfo
        {
            ObjectId = tempId,
            Position = dropItemPacket.Position,
            Name = itemData.name,
        };
        // ObjectManager에 추가
        Managers.Object.Add(objectInfo, activate:false);

        Debug.Log($"item:{objectInfo.Name}");
        // 아이템 스프라이트 변경 및 초기화
        GameObject itemObject = Managers.Object.FindById(tempId);
        if (itemObject != null)
        {
            ItemController itemController = itemObject.GetComponent<ItemController>();            
            if (itemController != null)
            {
                itemController.ItemData = itemData;
                itemController.Count = dropItemPacket.Count;            
                itemController.HandleDropItem(dropItemPacket.Position); // 아이템이 튀어나오는 것처럼 보이게 설정
            }
        }
    }
}
