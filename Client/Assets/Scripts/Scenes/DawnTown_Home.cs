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
        CheckInventoryAndHandleQuest03();
    }

    private void CheckInventoryAndHandleQuest03()
    {
        bool hasItem1001 = Managers.Inventory.Items.Any(item => item.Value.TemplateId == 1001);
        bool hasItem1002 = Managers.Inventory.Items.Any(item => item.Value.TemplateId == 1002);

        if (hasItem1001 && hasItem1002)
        {   
            foreach (var item in Managers.Inventory.Items)
            {
                if(item.Value.ItemDbId == 1001 || item.Value.ItemDbId == 1002)
                {
                    C_RemoveItem removeItemPacket = new C_RemoveItem(){ ItemDbId = item.Value.ItemDbId };
                    Managers.Network.Send(removeItemPacket);
                    Managers.Inventory.Remove(item.Key);
                }
            }

            // 진행 중인 퀘스트 완료 처리
            Managers.Quest.EndQuest();
        }
        else
        {
            // 퀘스트 완료 처리하지 않고 대사 출력
            ShowQuestScript(2);
        }
    }

    private void ShowQuestScript(int id)
    {
        int currentQuestId = Managers.Quest.GetCurrentQuestId();
        ScriptData questScriptData = null; 
        Managers.Data.ScriptDict.TryGetValue(currentQuestId, out questScriptData);
        
        if (questScriptData != null && questScriptData.scripts.Count > 1)
        {            
            UI_GameWindow gameWindow = _sceneUi.GameWindow;
            gameWindow.StoryPanel.ShowStoryPanel(questScriptData, id);
        }
    }

    public override void StartInteractionQuest(Quest quest)
    {
        if(quest.Id == 4)
        {
            ShowQuestScript(1);
        }
    }
    public override void CheckInteractionQuest(Quest quest)
    {
        int id = Managers.Quest.GetCurrentQuestId();
        if (quest.Id == id)
        {
            Managers.Quest.EndQuest();
        }
    }

    public override void ShowStoryScene(ScriptData scriptData)
    {
        UI_GameScene gameUI = Managers.UI.SceneUI as UI_GameScene;
        UI_StoryScene storyScene = gameUI.StoryScene;
        storyScene.gameObject.SetActive(true);
        storyScene.LoadStoryData(scriptData);
        storyScene.ShowStory();
    }

    public void QuestCheck()
    {

    }

    public override void Clear()
    {
        // 필요에 따라 Clear 메서드를 구현
    }
}