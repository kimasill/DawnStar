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
        SceneType = Define.Scene.DawnTown_Store;

        Managers.Map.LoadMap(2);

        Screen.SetResolution(640, 480, false);
        _sceneUi = Managers.UI.ShowSceneUI<UI_GameScene>();
        _sceneUi.SetActive(_sceneUi.GameWindow, true);
        // 추가적인 초기화 작업이 필요하면 여기에 작성        
        InitializeNPCs();
        RequestShop();
        CheckQuestAndShowScript(3); //Quest id
    }

    private void CheckQuestAndShowScript(int questId)
    {
        // 퀘스트 진행 중인지 확인
        if (Managers.Quest.IsQuestInProgress(questId))
        {
            // 퀘스트 스크립트 데이터를 가져옴
            List<string> scripts = Managers.Quest.GetQuestScripts(questId, 1); // 스크립트 리스트 ID 1번 추출
            if (scripts != null && scripts.Count > 0)
            {
                // UI_StoryPanel을 통해 스크립트 출력
                UI_GameWindow gameWindow = _sceneUi.GameWindow;
                gameWindow.StoryPanel.ShowStoryPanel(scripts);
            }
        }
    }
    public override void Clear()
    {
        // 필요에 따라 Clear 메서드를 구현
    }
}