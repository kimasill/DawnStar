using Data;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Linq;
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
            Managers.Scene.IsSceneLoaded = false;
            SceneManager.sceneLoaded -= (scene, mode) => OnSceneLoadedCallback(scene, mode, enterGamePacket.Player);
            SceneManager.sceneLoaded += (scene, mode) => OnSceneLoadedCallback(scene, mode, enterGamePacket.Player);

            Managers.Scene.LoadScene(enterGamePacket.Player.MapInfo.Scene);
         
        }
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
            if (despawnPacket.DespawnAnim)
            {
                Managers.Instance.StartCoroutine(Managers.Object.RemoveAfterAnimation(obj));
            }
            else
            {
                if (ObjectManager.GetObjectType(obj) == GameObjectType.Monster)
                {

                    GameObject go = Managers.Object.FindById(obj);
                    if (go != null)
                    {
                        MonsterController mc = go.GetComponent<MonsterController>();
                        mc.RemoveHpBar();
                    }    
                }
                Managers.Object.Remove(obj);
            }
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
        bc.HandleMovePos(movePacket.Position);
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
        cc.UseSkill(skillPacket);
    }
    public static void S_BuffHandler(PacketSession session, IMessage packet) 
    {
        S_Buff buffPacket = packet as S_Buff;
        GameObject go = Managers.Object.FindById(buffPacket.ObjectId);
        if (go == null)
        {
            return;
        }
        if (buffPacket.ObjectId != Managers.Object.MyPlayer.Id)
        {
            return;
        }

        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        if(buffPacket.BuffId > 0)
        {
            gameSceneUI.GameWindow.BuffPanel.AddBuff(buffPacket.BuffId, buffPacket.Value);
        }
        else if(buffPacket.DebuffId>0)
        {
            gameSceneUI.GameWindow.BuffPanel.AddDebuff(buffPacket.BuffId, buffPacket.Value);
        }
    }

    public static void S_EffectHandler(PacketSession session, IMessage packet)
    {
        S_Effect effectPacket = packet as S_Effect;
        GameObject go = Managers.Object.FindById(effectPacket.ObjectId);
        if (go == null)
        {
            return;
        }
        BaseController bc = go.GetComponent<BaseController>();
        if (bc == null)
        {
            return;
        }
        if(effectPacket.SkillId != 0)
        {
            bc.ActivateSkillEffect(effectPacket.Prefab, effectPacket.SkillId);
        }
        else bc.UseEffect(effectPacket.Prefab);
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
        else if (cc.Hp + 50 < changePacket.Hp)
        {
            cc.OnHealed();
        }
        cc.Hp = changePacket.Hp;
    }
    public static void S_ChangeUpHandler(PacketSession session, IMessage packet)
    {
        S_ChangeUp changePacket = packet as S_ChangeUp;

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
        cc.Up = changePacket.Up;
    }
    public static void S_DamageHandler(PacketSession session, IMessage packet)
    {
        S_Damage damagePacket = packet as S_Damage;
        GameObject go = Managers.Object.FindById(damagePacket.ObjectId);
        if (go == null)
        {
            return;
        }
        CreatureController cc = go.GetComponent<CreatureController>();
        if (cc == null)
        {
            return;
        }
        if (cc.Hp > damagePacket.Damage)
        {
            cc.ShowDamage(damagePacket.Damage, damagePacket.Critical);
        }
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

        // GUID를 사용하여 UniqueId 생성
        loginPacket.UniqueId = Managers.Network.Token.ToString();

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
        
        SceneManager.sceneLoaded += (scene, mode) => OnSceneLoaded(mapChangePacket.ObjectInfo);
        Managers.Scene.LoadScene(map.name);
    }
    private static void OnSceneLoadedCallback(Scene scene, LoadSceneMode mode, ObjectInfo objectInfo)
    {
        // 씬 로드가 완료되었을 때 호출되는 콜백
        // 필요한 초기화 작업 수행        
        OnSceneLoaded(objectInfo);

        // 콜백 등록 해제
        SceneManager.sceneLoaded -= (scene, mode) => OnSceneLoadedCallback(scene, mode, objectInfo);
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
        
        // 플레이어 정보 업데이트
        if (Managers.Object.MyPlayer != null)
        {
            Managers.Object.MyPlayer.PosInfo = objectInfo.Position;
            Managers.Object.MyPlayer.State = CreatureState.Idle;
            Managers.Object.MyPlayer.Stat.MergeFrom(objectInfo.StatInfo);
            Managers.Object.MyPlayer.RefreshAdditionalStat();
            Managers.Object.MyPlayer.RefreshPoints();

            Managers.Map.SetChests(objectInfo.MapInfo.ChestIds.ToList());
            Managers.Map.SetInteractions(objectInfo.MapInfo.InteractionIds.ToList());
            Managers.Scene.CurrentScene.CheckOnSceneLoadedQuest();
            C_RequestStat request = new C_RequestStat();
            Managers.Network.Send(request);
        }
        Managers.Scene.IsSceneLoaded = true;
    }

    public static void S_ChangePositionHandler(PacketSession session, IMessage packet)
    {
        S_ChangePosition changePosPacket = (S_ChangePosition)packet;
        GameObject go = Managers.Object.FindById(changePosPacket.ObjectId);
        if (go == null)
        {
            return;
        }
        CreatureController cc = go.GetComponent<CreatureController>();
        cc.PosInfo = changePosPacket.Position;
        cc.SyncPos();
        
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
    public static void S_UpdateItemListHandler(PacketSession session, IMessage packet)
    {
        S_UpdateItemList itemPacket = packet as S_UpdateItemList;

        GameObject obj = Managers.Object.FindById(itemPacket.ObjectId);
        if (obj == null)
        {
            Debug.LogError($"S_ItemListHandler: Object with ID {itemPacket.ObjectId} not found.");
            return;
        }
        BaseController bc = obj.GetComponent<BaseController>();
        if (bc == null)
        {
            return;
        }
        if (bc is PlayerController)
        {
            PlayerController pc = bc as PlayerController;
            foreach (ItemInfo itemInfo in itemPacket.Items)
            {
                Item item = Item.MakeItem(itemInfo);
                pc.Equipment.EquipItem(item);
            }
        }
    }
    public static void S_ItemListHandler(PacketSession session, IMessage packet)
    {
        S_ItemList itemPacket = packet as S_ItemList;
        if (itemPacket.Items == null)
        {
            return;
        }
        Managers.Inventory.Clear();
        //메모리에 아이템 정보 저장
        foreach (ItemInfo itemInfo in itemPacket.Items)
        {
            Item item = Item.MakeItem(itemInfo);
            Managers.Inventory.Add(item);
        }

        if(Managers.Object.MyPlayer != null)
        {
            Managers.Object.MyPlayer.RefreshAdditionalStat();
            Managers.Inventory.RefreshEquipment(Managers.Object.MyPlayer.Equipment);

            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            gameSceneUI.InvenUI.RefreshUI();
            gameSceneUI.GameWindow.SkillSlot.RefreshUI();
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
        gameSceneUI.EnhanceUI.RefreshUI();
        gameSceneUI.EnhanceUI.ItemProduction.RefreshUI();

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
        gameSceneUI.GameWindow.QuickSlot.RefreshUI();
        if (Managers.Object.MyPlayer != null)
        {
            Managers.Object.MyPlayer.RefreshAdditionalStat();
        }
    }

    public static void S_EquipItemHandler(PacketSession session, IMessage packet)
    {
        S_EquipItem equipItem = packet as S_EquipItem;
        if(Managers.Object.MyPlayer.Id == equipItem.ObjectId)
        {
            Item item = Managers.Inventory.Get(equipItem.ItemDbId);
            if (item == null)
            {
                return;
            }
            item.Equipped = equipItem.Equipped;

            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            gameSceneUI.InvenUI.RefreshUI();
            if (Managers.Object.MyPlayer != null)
            {
                if (item.ItemType == ItemType.Weapon || item.ItemType == ItemType.Armor)
                {
                    Managers.Object.MyPlayer.Equipment.EquipItem(item);
                }
                Managers.Object.MyPlayer.RefreshAdditionalStat();
                gameSceneUI.StatUI.RefreshUI();
                gameSceneUI.GameWindow.SkillSlot.RefreshUI();
            }
        }
    }

    public static void S_ShopListHandler(PacketSession session, IMessage packet)
    {
        S_ShopList shopList = packet as S_ShopList;
        Shop shop = Managers.Shop.Shops[shopList.ShopId];
        shop.ShopDbId = shopList.ShopDbId;
        shop.Clear();
        foreach (ItemInfo itemInfo in shopList.Items)
        {            
            Item item = Item.MakeItem(itemInfo);
            shop.Add(item);
        }

        Debug.Log("S_ShopListHandler");
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        gameSceneUI.ShopUI.RefreshUI(shopList.ShopId);
    }

    public static void S_ChangeStatHandler(PacketSession session, IMessage packet)
    {
        S_ChangeStat statPacket = (S_ChangeStat)packet;
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        Managers.Object.MyPlayer.Stat.MergeFrom(statPacket.StatInfo);
        if (Managers.Object.MyPlayer.Stat.Level != statPacket.StatInfo.Level)
        {
            gameSceneUI.GameWindow.StateUI.SetInfo();
            gameSceneUI.NotificationUI.ShowLevelNoti();
        }
        gameSceneUI.StatUI.RefreshUI();
    }

    public static void S_ChangeAdditionalStatHandler(PacketSession session, IMessage packet)
    {
        S_ChangeAdditionalStat statPacket = (S_ChangeAdditionalStat)packet;
        if(statPacket.StatInfo == null)
        {
            return;
        }

        MyPlayerController mc = Managers.Object.MyPlayer;
        mc.AdditionalAttack = statPacket.StatInfo.Attack;
        mc.AdditionalDefense = statPacket.StatInfo.Defense;
        mc.AdditionalSpeed = statPacket.StatInfo.Speed;
        mc.AdditionalAttackSpeed = statPacket.StatInfo.AttackSpeed;
        mc.AdditionalHp = statPacket.StatInfo.Hp;
        mc.AdditionalUp = statPacket.StatInfo.Up;
        mc.AdditionalCriticalChance = statPacket.StatInfo.CriticalChance;
        mc.AdditionalCriticalDamage = statPacket.StatInfo.CriticalDamage;
        mc.AdditionalAccuracy = statPacket.StatInfo.Accuracy;
        mc.AdditionalAvoidance = statPacket.StatInfo.Avoid;
        mc.AdditionalCoolTime = statPacket.StatInfo.CoolTime;
        mc.AdditionalInvokeSpeed = statPacket.StatInfo.InvokeSpeed;
        mc.AdditionalUpRegen = statPacket.StatInfo.UpRegen;
        mc.AdditionalHpRegen = statPacket.StatInfo.HpRegen;

        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        gameSceneUI.StatUI.RefreshUI();
    }

    public static void S_ChangeExpHandler(PacketSession session, IMessage packet)
    {        
        S_ChangeExp expPacket = (S_ChangeExp)packet;
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;

        int exp = expPacket.Exp - Managers.Object.MyPlayer.Exp;
        Managers.Object.MyPlayer.Exp = expPacket.Exp;
        gameSceneUI.NotificationUI.ShowExpNoti(exp);
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
        Managers.Quest.Quests.Clear();
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

        if (buyItemPacket.Count == 0)
        {
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            gameSceneUI.NotificationUI.ShowBasicNoti("소지금이 부족합니다.");
            return;
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
        int tempId = Managers.Object.GenerateId(GameObjectType.Item);
        
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

    public static void S_MakeChestHandler(PacketSession session, IMessage packet)
    {
        S_MakeChest makeChestPacket = packet as S_MakeChest;
        ChestController chestController = Managers.Map.GetChestById(makeChestPacket.ChestId);
        if (chestController != null)
        {
            chestController.gameObject.SetActive(true);
        }        
    }

    public static void S_SkillCoolHandler(PacketSession session, IMessage packet)
    {
        S_SkillCool skillCoolPacket = packet as S_SkillCool;
        UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene; 
        Debug.Log($"SkillCool : {skillCoolPacket.SkillId} {skillCoolPacket.CoolTime}");
        ui.GameWindow.SkillSlot.StartCooldown(skillCoolPacket.SkillId, skillCoolPacket.CoolTime);
    }

    public static void S_PartyInviteHandler(PacketSession session, IMessage packet)
    {
        S_PartyInvite partyInvitePacket = packet as S_PartyInvite;
        //UI_Popup popup = Managers.UI.ShowPopupUI<UI_Popup>();
        //popup.SetPopup(partyInvitePacket);
    }

    public static void S_InteractionHandler(PacketSession session, IMessage packet)
    {
        S_Interaction interactionPacket = packet as S_Interaction;
        bool action = false;
        if(Managers.Object.MyPlayer.Id == interactionPacket.PlayerId)
        {
            action = true;
        }
        InteractionController ic = Managers.Map.GetInteractionById(templateId:interactionPacket.ObjectId);
        ic.Interact(interactionPacket.Success, action , interactionPacket.TargetId.ToList());
    }
    public static void S_BossKillHandler(PacketSession session, IMessage packet)
    {
        S_BossKill partyPacket = packet as S_BossKill;
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        Managers.Data.MonsterDict.TryGetValue(partyPacket.TemplateId, out MonsterData monsterData);
        if(monsterData != null) {
            gameSceneUI.NotificationUI.ShowBossKillNoti(monsterData.name);
        }
    }
    public static void S_EnhanceHandler(PacketSession session, IMessage packet)
    {
        S_Enhance enhancePacket = packet as S_Enhance;
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        //메모리에 아이템 정보 저장
        

        if (Managers.Object.MyPlayer != null)
        {
            Managers.Object.MyPlayer.RefreshAdditionalStat();
        }
        gameSceneUI.EnhanceUI.EnhanceResult(enhancePacket);
    }
    public static void S_EnchantHandler(PacketSession session, IMessage packet)
    {
        S_Enchant enchantPacket = packet as S_Enchant;
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        //메모리에 아이템 정보 저장
        if (Managers.Object.MyPlayer != null)
        {
            if (enchantPacket.Success)
            {
                Item item = Item.MakeItem(enchantPacket.ItemInfo);
                Managers.Inventory.AddOrUpdate(item);

                gameSceneUI.InvenUI.RefreshUI();
                gameSceneUI.StatUI.RefreshUI();
            }

            Managers.Object.MyPlayer.RefreshAdditionalStat();
        }
    }

    public static void S_SystemNoticeHandler(PacketSession session, IMessage packet)
    {
        S_SystemNotice noticePacket = packet as S_SystemNotice;
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        gameSceneUI.NotificationUI.ShowBasicNoti(noticePacket.Message);
    }

    public static void S_ChatHandler(PacketSession session, IMessage packet)
    {
        S_Chat chatPacket = packet as S_Chat;
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        UI_GameWindow gameWindow = gameSceneUI.GameWindow;
        gameWindow.Chat.ApplyMessage(chatPacket.PlayerId, chatPacket.PlayerName, chatPacket.Message);
    }
}
