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
        SceneType = Define.Scene.DawnTown_Home;

        Managers.Map.LoadMap(3);

        Screen.SetResolution(640, 480, false);
        _sceneUi = Managers.UI.ShowSceneUI<UI_GameScene>();
        _sceneUi.SetActive(_sceneUi.GameWindow, true);

        // 추가적인 초기화 작업이 필요하면 여기에 작성        
        InitializeNPCs();

        // 인벤토리에서 아이템 확인 및 퀘스트 처리
        CheckInventoryAndHandleQuest();
    }

    private void CheckInventoryAndHandleQuest()
    {
        bool hasItem1001 = Managers.Inventory.Items.Any(item => item.Value.TemplateId == 1001);
        bool hasItem1002 = Managers.Inventory.Items.Any(item => item.Value.TemplateId == 1002);

        if (hasItem1001 && hasItem1002)
        {
            // 진행 중인 퀘스트 완료 처리
            Managers.Quest.EndQuest();
        }
        else
        {
            // 퀘스트 완료 처리하지 않고 대사 출력
            ShowQuestScript();
        }
    }

    private void ShowQuestScript()
    {
        int currentQuestId = Managers.Quest.GetCurrentQuestId();
        ScriptData questScriptData = null; 
        Managers.Data.ScriptDict.TryGetValue(currentQuestId, out questScriptData);

        if (questScriptData != null && questScriptData.scripts.Count > 1)
        {
            List<string> scripts = questScriptData.scripts[1].script;
            UI_GameWindow gameWindow = _sceneUi.GameWindow;
            gameWindow.StoryPanel.ShowStoryPanel(scripts);
        }
    }

    public void QuestCheck()
    {

    }

    public override void Clear()
    {
        // 필요에 따라 Clear 메서드를 구현
    }
}