using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DawnTown_Store : DawnTown
{
    UI_Shop _shop;
    protected override void Init()
    {
        SceneType = Define.Scene.DawnTownStore;

        Managers.Map.LoadMap(2);

        Screen.SetResolution(640, 480, false);
        Camera.main.orthographicSize = ZoomLevel;
        _sceneUi = Managers.UI.ShowSceneUI<UI_GameScene>();
        _sceneUi.SetActive(_sceneUi.GameWindow, true);
        // 추가적인 초기화 작업이 필요하면 여기에 작성        
        InitializeNPCs();
        RequestShop(2);
    }
    public override void CheckOnSceneLoadedQuest()
    {
        if (Managers.Scene.IsSceneLoaded)
        {
            return;
        }
        StartQuest(3);
    }
    private void StartQuest(int questId)
    {
        if (Managers.Quest.IsQuestInProgress(questId))
        {
            // 퀘스트 스크립트 데이터를 가져옴
            ScriptData questScriptData = null;
            Managers.Data.ScriptDict.TryGetValue(questId, out questScriptData);

            if (questScriptData != null && questScriptData.scripts.Count > 0)
            {
                // UI_StoryPanel을 통해 스크립트 출력
                UI_GameWindow gameWindow = _sceneUi.GameWindow;
                gameWindow.StoryPanel.ShowStoryPanel(questScriptData, 1);
            }
        }
    }
    public override void Clear()
    {
        // 필요에 따라 Clear 메서드를 구현
    }
}