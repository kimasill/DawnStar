using Data;
using Google.Protobuf.Collections;
using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;
using static Item;

public class MyPlayerController : PlayerController
{
    bool _moveKeyPressed = false;    
    public int WeaponDamage { get; private set; }
    public int ArmorDef { get; private set; }
    private Queue<NPCController> _nearbyNPCs;
    private CameraController _cameraController;
    private Queue<ChestController> _chestControllers;
    public CameraController CameraController { get; private set; }

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
            UI_GameScene gameScene = Managers.UI.SceneUI as UI_GameScene;
            gameScene.GameWindow.UpdateStateInfo();
        }
    }
    protected override void UpdateHpBar()
    {
        UI_GameScene gameScene = Managers.UI.SceneUI as UI_GameScene;
        gameScene.GameWindow.UpdateHpUI();
    }
    public override StatInfo Stat
    {
        get { return base.Stat; }
        set
        {
            base.Stat = value;
            UI_GameScene gameScene = Managers.UI.SceneUI as UI_GameScene;
            gameScene.GameWindow.UpdateStateInfo();
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
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            UI_Inventory invenUI = gameSceneUI.InvenUI;

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
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            UI_Stat statUI = gameSceneUI.StatUI;

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
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            UI_Map mapUI = gameSceneUI.MapUI;

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
            if(_chestControllers.Count > 0)
            {
                _chestControllers.Dequeue().OpenChest();
            }
            else if(_nearbyNPCs.Count > 0)
                _nearbyNPCs.Dequeue().StartInteraction();
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            UI_Quest questUI = gameSceneUI.QuestUI;

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

        if (Input.mouseScrollDelta.y != 0)
        {
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;            
            if(gameSceneUI.MapUI == null)
            {
                return;
            }
            UI_Map mapUI = gameSceneUI.MapUI;
            if (mapUI.gameObject.activeSelf)
            {
                mapUI.ZoomMap(Input.mouseScrollDelta.y); // 스크롤에 따라 지도 크기 조절
            }
            // 인벤토리가 활성화된 경우 스크롤뷰를 내립니다.
            UI_Inventory invenUI = gameSceneUI.InvenUI;
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
    private bool _isAttacking = false;
    protected override void UpdateIdle()
    {
        if (_isAttacking)
            return;
        if (State == CreatureState.Stiff || State == CreatureState.Dead)
            return;
        // 이동 상태로 갈지 확인
        if (_moveKeyPressed)
        {
            State = CreatureState.Moving;
            return;
        }

        // 스킬 상태로 갈지 확인
        if (Input.GetKey(KeyCode.Space))
        {
            _isAttacking = true;
            Debug.Log("기본공격");
            //TODO: 무기따라 ID선택
            C_Skill skill = new C_Skill() { Info = new SkillInfo()};
            skill.Info.SkillId = 1;
            Managers.Network.Send(skill); 
            
            _coInputCooltime = StartCoroutine(CoInputCooltime(0.1f));
        }
    }
    Coroutine _coInputCooltime;
    
    IEnumerator CoInputCooltime(float time)
    {
        yield return new WaitForSeconds(time);
        _isAttacking = false;
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
                CheckQuest();                
            }
        }
        CheckUpdatedFlag();
    }

    private void CheckIfPlayerAtPortal(Vector3Int playerCellPosition)
    {
        string portal = Managers.Map.IsPlayerAtPortal(playerCellPosition);
        if (portal != null)
        {
            int id = Managers.Map.GetPortalId(portal);
            C_MapChange mapPacket = new C_MapChange { MapId = id };
            Managers.Network.Send(mapPacket);
        }
    }

    private void CheckIfPlayerAtItem()
    {
        Vector3Int playerCellPosition = CellPos;
        GameObject itemObject = Managers.Object.FindItem(playerCellPosition);
        if (itemObject != null && itemObject.GetComponent<ItemController>() != null)
        {
            ItemController itemController = itemObject.GetComponent<ItemController>();
            itemController.MoveToPlayer(transform);
            // 아이템 루팅 로직 추가
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

        Dictionary<int, GameObject> npcs = currentScene.GetNPCs();
        foreach (var npc in npcs.Values)
        {
            Vector3Int npcCellPos = Managers.Map.CurrentGrid.WorldToCell(npc.transform.position);
            float distance = Vector3Int.Distance(CellPos, npcCellPos);
            NPCController npcController = npc.GetComponent<NPCController>();
            if (distance <= 5.0f)
            {
                npcController.ActivateNotification();
                _nearbyNPCs.Enqueue(npcController);
            }
            else
            {
                foreach (var nearbyNPC in _nearbyNPCs)
                {
                    nearbyNPC.DeactivateNotification();
                    _nearbyNPCs.Clear();
                }                
            }
        }
    }

    private void DetectNearbyChests()
    {
        //플레이어 주변 3x3 영역에 상자가 있는지 확인
        Vector3Int playerCellPos = CellPos;
        ChestController cc = null;
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3Int checkPos = playerCellPos + new Vector3Int(x, y, 0);
                cc = Managers.Map.GetChest((Vector2Int)checkPos);
                if (cc != null)
                {   
                    _chestControllers.Enqueue(cc);                    
                }
            }
        }
        if (cc == null)
        {
            foreach (var chest in _chestControllers)
            {
                chest.DeactivateNotification();
            }
            _chestControllers.Clear();
        }
        else
        {
            _chestControllers.Peek().ActivateNotification();
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
        WeaponDamage = 0;
        ArmorDef = 0;
        AttackSpeed = 0;
        Console.WriteLine($"AttackSpeed:{AttackSpeed}");
        foreach (Item item in Managers.Inventory.Items.Values)
        {
            if (item.Equipped == false)
                continue;
            MapField<string, string> options = item.Info.Options;
            if (options == null)
                continue;
            foreach (var option in options)
            {
                switch (option.Key)
                {  
                    case "WeaponDamage":
                        WeaponDamage += int.Parse(option.Value);
                        break;
                    case "ArmorDef":
                        ArmorDef += int.Parse(option.Value);
                        break;
                    case "AttackSpeed":
                        AttackSpeed += int.Parse(option.Value);
                        break;
                }
            }
            switch (item.ItemType)
            {
                case ItemType.Weapon:
                    WeaponDamage += ((Weapon)item).Damage;
                    AttackSpeed += ((Weapon)item).AttackSpeed;
                    break;
                case ItemType.Armor:
                    ArmorDef += ((Armor)item).Defense;
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
