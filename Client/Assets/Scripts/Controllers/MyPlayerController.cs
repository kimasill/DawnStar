using Data;
using Google.Protobuf.Collections;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Define;
using static Item;

public class MyPlayerController : PlayerController
{
    bool _moveKeyPressed = false;
    
    private NPCController _NPCController;
    private CameraController _cameraController;
    private ChestController _chestController;
    private InteractionController _interactionController;
    private DoorController _doorController;
    private int _nearByPortalId = -1;
    private GameObject _headUpIcon;
    public CameraController CameraController { get; private set; }

    public KeyCode[] SkillKeys { get; private set;} = new KeyCode[4];

    public int Gold
    {
        get { return Stat.Gold; }
        set { Stat.Gold = value; }
    }

    public override int Exp
    {
        get { return Stat.TotalExp; }
        set 
        { 
            Stat.TotalExp = value;
            GameScene.GameWindow.UpdateStateInfo();
        }
    }
    
    protected override void UpdateHpBar()
    {
        GameScene.GameWindow.UpdateHpUI();
    }
    public override StatInfo Stat
    {
        get { return base.Stat; }
        set
        {
            base.Stat = value;            
            GameScene.GameWindow.UpdateStateInfo();
            UpdateHpBar();
        }
    }
    protected override void Init()
    {
        base.Init();

        RefreshExpBar();
        RefreshAdditionalStat();
        _cameraController = Camera.main.GetComponent<CameraController>();
        if (_cameraController != null)
        {
            _cameraController.SetTarget(transform);
        }
        SkillKeys[0] = KeyCode.E;
        SkillKeys[1] = KeyCode.R;
        SkillKeys[2] = KeyCode.F;
        SkillKeys[3] = KeyCode.T;
    }

    protected override void UpdateController() 
    {
        GetUIKeyInput();
        switch (State)
        {
            case CreatureState.Idle:
                GetDirInput();
                break;
            case CreatureState.Moving:
                GetDirInput();
                break;
        }
        
        base.UpdateController();
    }

    void GetUIKeyInput()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {            
            UI_Inventory invenUI = GameScene.InvenUI;

            if (invenUI.gameObject.activeSelf)
            {
                invenUI.gameObject.SetActive(false);
                Managers.UI.CloseAllPopupUI();
            }
            else
            {
                invenUI.gameObject.SetActive(true);
                invenUI.RefreshUI();
            }
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {            
            UI_Stat statUI = GameScene.StatUI;

            if (statUI.gameObject.activeSelf)
            {
                statUI.gameObject.SetActive(false);
            }
            else
            {
                statUI.gameObject.SetActive(true);
                statUI.RefreshUI();
            }
        }
        else if (Input.GetKeyDown(KeyCode.M))
        {            
            UI_Map mapUI = GameScene.MapUI;

            if (mapUI.gameObject.activeSelf)
            {
                mapUI.gameObject.SetActive(false);
                mapUI.OnCloseMap();
            }
            else
            {                
                mapUI.gameObject.SetActive(true);
                mapUI.OnOpenMap();            
            }
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            if (_nearByPortalId != -1)
            {
                UI_GameScene gameScene = Managers.UI.SceneUI as UI_GameScene;
                gameScene.MatchingUI.RefreshUI(_nearByPortalId);
                if (gameScene.MatchingUI.gameObject.activeSelf)
                {
                    gameScene.MatchingUI.gameObject.SetActive(false);
                    if(_doorController!=null)
                        _doorController.CloseDoor();
                }
                else
                {
                    gameScene.MatchingUI.gameObject.SetActive(true);
                    if (_doorController != null)
                        _doorController.OpenDoor();
                }
            }
            else if(_chestController != null)
            {
                _chestController.OpenChest();
            }
            else if(_NPCController != null)
                _NPCController.StartInteraction();
            else if (_interactionController != null)
                _interactionController.StartInteraction();
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {            
            UI_Quest questUI = GameScene.QuestUI;

            if (questUI.gameObject.activeSelf)
            {
                questUI.gameObject.SetActive(false);
            }
            else
            {
                questUI.gameObject.SetActive(true);
                questUI.RefreshUI();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))
             GameScene.GameWindow.QuickSlot.UseItem(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            GameScene.GameWindow.QuickSlot.UseItem(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            GameScene.GameWindow.QuickSlot.UseItem(2);
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            GameScene.GameWindow.QuickSlot.UseItem(3);

        if (Input.mouseScrollDelta.y != 0)
        {
            if (GameScene.MapUI == null)
            {
                return;
            }
            UI_Map mapUI = GameScene.MapUI;
            if (mapUI.gameObject.activeSelf)
            {
                mapUI.ZoomMap(Input.mouseScrollDelta.y); // 스크롤에 따라 지도 크기 조절
            }
            // 인벤토리가 활성화된 경우 스크롤뷰를 내립니다.
            UI_Inventory invenUI = GameScene.InvenUI;
            if (invenUI.gameObject.activeSelf && invenUI.ScrollRect != null)
            {
                float scrollDelta = Input.mouseScrollDelta.y * 0.1f; // 스크롤 속도 조절
                invenUI.ScrollRect.verticalNormalizedPosition = Mathf.Clamp01(invenUI.ScrollRect.verticalNormalizedPosition - scrollDelta);
            }
        }
    }
    void GetDirInput()
    {
        _moveKeyPressed = true;
        if (Input.GetKey(KeyCode.W))
        {
            Dir = MoveDir.Up;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            Dir = MoveDir.Down;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            Dir = MoveDir.Left;
            LookDir = LookDir.LookLeft;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            Dir = MoveDir.Right;
            LookDir = LookDir.LookRight;
        }
        else
        {
            _moveKeyPressed = false;
        }
    }
    

    protected override void UpdateSkill()
    {
        if (State == CreatureState.Stiff || State == CreatureState.Dead)
            return;
        for(int i = 0; i < SkillKeys.Length; i ++)
        {
            if (Input.GetKeyDown(SkillKeys[i]))
            {
                SkillData skill = GameScene.GameWindow.SkillSlot.GetSkill(i);
                if (skill == null)
                    return;
                C_Skill skillPacket = new C_Skill() { Info = new SkillInfo() };
                skillPacket.Info.SkillId = skill.id;
                Managers.Network.Send(skillPacket);
            }
        }
    }
    protected override void UpdateIdle()
    {
        if (State == CreatureState.Stiff || State == CreatureState.Dead)
            return;
        _updated = true;
        CheckUpdatedFlag();
        // 이동 상태로 갈지 확인
        if (_moveKeyPressed)
        {
            State = CreatureState.Moving;
            return;
        }

        for (int i = 0; i < SkillKeys.Length; i++)
        {
            if (Input.GetKeyDown(SkillKeys[i]))
            {
                SkillData skill = GameScene.GameWindow.SkillSlot.GetSkill(i);
                if (skill == null)
                    return;
                C_Skill skillPacket = new C_Skill() { Info = new SkillInfo() };
                skillPacket.Info.SkillId = skill.id;
                Managers.Network.Send(skillPacket);
            }
        }

        if (_isAttacking)
            return;

        // 스킬 상태로 갈지 확인
        if (Input.GetKey(KeyCode.Space))
        {
            Debug.Log("기본공격");
            //TODO: 무기따라 ID선택
            C_Skill skill = new C_Skill() { Info = new SkillInfo()};
            skill.Info.SkillId = 1;
            Managers.Network.Send(skill);            
        }
    }
    public IEnumerator AttackDelay()
    {
        yield return new WaitForSeconds(1000*(Stat.AttackSpeed+AdditionalAttackSpeed));
    }
    protected override void MoveToNextPos()
    {
        if (_moveKeyPressed == false)
        {
            State = CreatureState.Idle;
            CheckUpdatedFlag();
            return;
        }

        Vector3Int destPos = CellPos;

        switch (Dir)
        {
            case MoveDir.Up:
                destPos += Vector3Int.up;
                break;
            case MoveDir.Down:
                destPos += Vector3Int.down;
                break;
            case MoveDir.Left:
                destPos += Vector3Int.left;
                break;
            case MoveDir.Right:
                destPos += Vector3Int.right;
                break;
        }

        if (Managers.Map.CanGo(destPos))
        {
            if (Managers.Object.FindCreature(destPos) == null)
            {
                CellPos = destPos;
                CheckIfPlayerAtPortal(destPos);
                CheckIfPlayerAtItem();
                DetectNearbyNPCs();
                DetectNearbyChests();
                DetectNearbyInteractions();
                CheckQuest();                
            }
        }
        CheckUpdatedFlag();
    }

    private void CheckIfPlayerAtPortal(Vector3Int playerCellPosition)
    {
        GameObject portalObj = Managers.Map.IsPlayerAtPortal(playerCellPosition);
        if (portalObj == null)
            return;
        string tempId = portalObj.name.Split("_")[1];
        int portalId = 0;
        if (tempId != null) {
            portalId = int.Parse(tempId);
        }
         
        if (portalId != 0)
        {
            int mapId = Managers.Map.CurrentMapId;
            Managers.Data.MapDict.TryGetValue(mapId, out MapData mapData);
            if (mapData == null)
                return;
            PortalData portalData = null;
            foreach (var portal in mapData.portals)
            {
                if (portal.id == portalId)
                {
                    portalData = portal;
                    break;
                }
            }
            Managers.Data.MapDict.TryGetValue(portalData.mapId, out MapData nextMapData);
            if (nextMapData == null)
                return;
            _nearByPortalId = portalData.mapId;
            DoorController dc = portalObj.GetComponentInChildren<DoorController>();
            if (dc != null)
            {
                _doorController = dc;
            }
            if (nextMapData.type == MapType.Dungeon)
            {
                if(_headUpIcon == null)
                {
                    _headUpIcon = Managers.Resource.Instantiate("UI/HeadUpIcon", transform);
                    _headUpIcon.GetComponent<SpriteRenderer>().sprite = Managers.Resource.Load<Sprite>("Textures/Images/QuestIcons/Icon_Dungeon");
                    _headUpIcon.GetComponentInChildren<TMP_Text>().text = "던전 입장 : G";
                }
            }
            else
            {
                C_MapChange mapPacket = new C_MapChange { PortalId = portalId };
                Managers.Network.Send(mapPacket);
                if(_doorController!=null)
                    _doorController.OpenDoor();
            }            
        }
        else
        {
            if (_nearByPortalId != -1)
            {
                Managers.Resource.Destroy(_headUpIcon);
                _headUpIcon = null;
                _nearByPortalId = -1;
                _doorController = null;
            }            
        }
    }

    private void CheckIfPlayerAtItem()
    {
        Vector3Int playerCellPosition = CellPos;
        List<GameObject> itemObjects = Managers.Object.FindItem(playerCellPosition);
        if (itemObjects != null && itemObjects.Count>=1)
        {
            foreach (GameObject itemObject in itemObjects)
            {
                ItemController itemController = itemObject.GetComponent<ItemController>();
                itemController.MoveToPlayer(transform);
            }
        }
    }

    private void CheckQuest()
    {
        GameObject go = Managers.Map.IsPlayerAtQuest(CellPos);
        if (go == null)
            return;
        string questIdString = go.name.Replace("quest_", "");
        int questId;
        if (int.TryParse(questIdString, out questId))
        {
            Managers.Scene.CurrentScene.CheckInteractionQuest(questId);
        }        
    }

    private void DetectNearbyNPCs()
    {
        BaseScene currentScene = Managers.Scene.CurrentScene as BaseScene;
        if (currentScene == null)
            return;
        bool npcFound = false;
        Dictionary<int, GameObject> npcs = currentScene.GetNPCs();
        foreach (var npc in npcs.Values)
        {
            Vector3Int npcCellPos = Managers.Map.CurrentGrid.WorldToCell(npc.transform.position);
            float distance = Vector3Int.Distance(CellPos, npcCellPos);
            if (distance <= 3.0f)
            {
                NPCController npcController = npc.GetComponent<NPCController>();
                if(npcController != _NPCController)
                {
                    npcController.ActivateNotification();
                    if (_NPCController != null)
                    {
                        _NPCController.DeactivateNotification();
                    }
                    _NPCController = npcController;
                }                
                npcFound = true;
                break;
            }
        }

        if (!npcFound && _NPCController != null)
        {
            _NPCController.DeactivateNotification();
            _NPCController = null;
        }
    }

    private void DetectNearbyChests()
    {
        //플레이어 주변 3x3 영역에 상자가 있는지 확인
        Vector3Int playerCellPos = CellPos;
        ChestController cc = null;
        bool chestFound = false;
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3Int checkPos = playerCellPos + new Vector3Int(x, y, 0);
                cc = Managers.Map.GetChest((Vector2Int)checkPos);
                if (cc != null)
                {
                    if (_chestController != cc)
                    {
                        cc.ActivateNotification();
                        if (_chestController != null)
                        {                           
                            _chestController.DeactivateNotification();
                        }
                        _chestController = cc;
                    }
                    chestFound = true;
                    Debug.Log($"Chest Detected: {checkPos}");
                    break;
                }
            }
            if (chestFound)
                break;
        }

        if (!chestFound && _chestController != null)
        {
            _chestController.DeactivateNotification();
            _chestController = null;
        }
    }
    private void DetectNearbyInteractions()
    {
        Vector3Int playerCellPos = CellPos;
        InteractionController ic = null;
        bool interactionFound = false;
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3Int checkPos = playerCellPos + new Vector3Int(x, y, 0);
                ic = Managers.Map.GetInteraction((Vector2Int)checkPos);
                if (ic != null)
                {
                    if (_interactionController != ic)
                    {
                        ic.ActivateNotification();
                        if (_interactionController != null)
                        {
                            _interactionController.DeactivateNotification();
                        }
                        _interactionController = ic;
                    }
                    interactionFound = true;
                    Debug.Log($"Interaction Detected: {checkPos}");
                    break;
                }
            }
            if (interactionFound)
                break;
        }

        if (!interactionFound && _interactionController != null)
        {
            _interactionController.DeactivateNotification();
            _interactionController = null;
        }
    }
    //Dirty Flag Check
    protected override void CheckUpdatedFlag()
    {
        if (_updated)
        {
            C_Move movePacket = new C_Move();
            movePacket.Position = PosInfo;
            Managers.Network.Send(movePacket);
            _updated = false;
        }
    }
    public void RefreshAdditionalStat()
    {
        EquipDamage = 0;
        EquipDefense = 0;
        EquipAvoidance = 0;
        EquipAccuracy = 0;
        EquipCriticalChance = 0;
        EquipCriticalDamage = 0;
        EquipAttackSpeed = 0;
        EquipSpeed = 0;
        EquipInvokeSpeed = 0;
        EquipCoolTime = 0;
        EquipHp = 0;
        EquipUp = 0;
        foreach (Item item in Managers.Inventory.Items.Values)
        {
            if (item.Equipped == false)
                continue;
            MapField<string, string> options = item.Info.Options;
            if (options == null)
                continue;
            foreach (var option in options)
            {
                if (Enum.TryParse(option.Key, out OptionType optionType))
                {
                    switch (optionType)
                    {
                        case OptionType.Avoid:
                            EquipAvoidance += int.Parse(option.Value);
                            break;
                        case OptionType.Accuracy:
                            EquipAccuracy += int.Parse(option.Value);
                            break;
                        case OptionType.Ciriticalchance:
                            EquipCriticalChance += int.Parse(option.Value);
                            break;
                        case OptionType.Criticaldamage:
                            EquipCriticalDamage += int.Parse(option.Value);
                            break;
                        case OptionType.Attackspeed:
                            EquipAttackSpeed += int.Parse(option.Value);
                            break;
                        case OptionType.Speed:
                            EquipSpeed += int.Parse(option.Value);
                            break;
                        case OptionType.Invokespeed:
                            EquipInvokeSpeed += int.Parse(option.Value);
                            break;
                        case OptionType.Cooltime:
                            EquipCoolTime += int.Parse(option.Value);
                            break;
                        case OptionType.Hp:
                            EquipHp += int.Parse(option.Value);
                            break;
                        case OptionType.Up:
                            EquipUp += int.Parse(option.Value);
                            break;
                    }
                }
            }
            switch (item.ItemType)
            {
                case ItemType.Weapon:
                    EquipDamage += ((Weapon)item).Damage;
                    EquipDefense = ((Weapon)item).Range;
                    EquipAttackSpeed += ((Weapon)item).AttackSpeed;
                    break;
                case ItemType.Armor:
                    EquipDefense += ((Armor)item).Defense;
                    break;
            }

        }
    }
    public void RefreshExpBar()
    {
        UI_GameScene gameScene = Managers.UI.SceneUI as UI_GameScene;
        gameScene.GameWindow.UpdateStateInfo();
    }
    protected override void AddHpBar()
    {
        UpdateHpBar();
    }
}
