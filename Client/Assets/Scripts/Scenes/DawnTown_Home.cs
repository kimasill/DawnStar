using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DawnTown_Home : DawnTown
{
    protected override void Init()
    {
        SceneType = Define.Scene.DawnTownHome;

        Managers.Map.LoadMap(3);

        Screen.SetResolution(640, 480, false);
        _sceneUi = Managers.UI.ShowSceneUI<UI_GameScene>();
        _sceneUi.SetActive(_sceneUi.GameWindow, true);

        // 추가적인 초기화 작업이 필요하면 여기에 작성        
        InitializeNPCs();

        // 인벤토리에서 아이템 확인 및 퀘스트 처리
        if (Managers.Quest.IsQuestInProgress(3)) 
        {
            CheckInventoryAndHandleQuest03();
        }    
        if(Managers.Quest.GetQuest(6) == null)
        {
            // 퀘스트 6을 완료하고 재접한 경우
            DoorChangeAfterQuest06();
        }
    }

    private void CheckInventoryAndHandleQuest03()
    {
        bool hasItem1001 = Managers.Inventory.Items.Any(item => item.Value.TemplateId == 1001);
        bool hasItem1002 = Managers.Inventory.Items.Any(item => item.Value.TemplateId == 1002);

        var removeList = new List<int>();
        if (hasItem1001 || hasItem1002)
        {   
            foreach (var item in Managers.Inventory.Items)
            {
                if(item.Value.TemplateId == 1001 || item.Value.TemplateId == 1002)
                {
                    C_RemoveItem removeItemPacket = new C_RemoveItem(){ ItemDbId = item.Value.ItemDbId };
                    Managers.Network.Send(removeItemPacket);
                    removeList.Add(item.Key);                    
                }
            }
            foreach (var key in removeList)
            {
                Managers.Inventory.Remove(key);
            }
            // 진행 중인 퀘스트 완료 처리
            Managers.Quest.EndQuest();
        }
        else
        {
            // 퀘스트 완료 처리하지 않고 대사 출력
            Managers.Quest.ShowQuestScript(3, 2);
        }
    }

    public override void StartInteractionQuest(int questId)
    {
        if(questId == 4)
        {
            Managers.Quest.ShowQuestScript(questId);
        }
        else if(questId == 6)
        {
            Managers.Quest.ShowQuestScript(questId);
            SpawnItemBelowPlayer(3);
        }
    }
    public override void CheckInteractionQuest(int questId)
    {
        int id = Managers.Quest.GetCurrentQuestId();
        if (id == questId)
        {
            Managers.Quest.EndQuest();
        }        
    }

    private void SpawnItemBelowPlayer(int itemId)
    {
        Vector3Int playerCellPos = Managers.Object.MyPlayer.CellPos;
        Vector3Int spawnCellPos = new Vector3Int(playerCellPos.x, playerCellPos.y - 1);
        PositionInfo positionInfo = new PositionInfo();
        positionInfo.PosX = spawnCellPos.x;
        positionInfo.PosY = spawnCellPos.y;
        ItemController ic = null;
        if (Managers.Data.ItemDict.TryGetValue(itemId, out ItemData itemData))
        {
            int objectId = 0;
            Managers.Object.GenerateId(GameObjectType.Item, out objectId);
            ObjectInfo itemInfo = new ObjectInfo
            {
                Name = itemData.name,
                ObjectId = objectId,
                Position = positionInfo,                
            };

            Managers.Object.Add(itemInfo);
            ic = Managers.Object.FindById(objectId).GetComponent<ItemController>();
            ic.Sprite = Managers.Resource.Load<Sprite>(itemData.iconPath);
            ic.Count = 1;
            ic.ItemData = itemData;
        }
        Managers.Inventory.ItemAdded += OnInventoryItemAdded;
        DoorChangeAfterQuest06();
    }

    private void DoorChangeAfterQuest06()
    {
        Managers.Map.RemovePortalByName("DawnTown");
        GameObject[] doorObjects = GameObject.FindGameObjectsWithTag("Door");
        if (doorObjects != null && doorObjects.Length > 0)
        {
            foreach (var doorObject in doorObjects)
            {
                doorObject.name = "DawnTownDead";
            }
        }
        Managers.Map.FindDoors();
    }

    private void OnInventoryItemAdded(Item item)
    {
        if(item.TemplateId == 3)
        {
            Managers.Inventory.ItemAdded -= OnInventoryItemAdded;
            item.OnEquipped += HandleAddedItem03;
        }
    }

    private void HandleAddedItem03(Item item)
    {
        if(item.TemplateId == 3)
        {
            Managers.Quest.EndQuest();
        }
    }

    public override void Clear()
    {
        // 필요에 따라 Clear 메서드를 구현
    }
}