using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DawnTownDead : DawnTown
{
    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.DawnTownDead;

        Managers.Map.LoadMap(4); // DawnTownDead 맵 로드

        Screen.SetResolution(640, 480, false);
        _sceneUi = Managers.UI.ShowSceneUI<UI_GameScene>();
        _sceneUi.SetActive(_sceneUi.GameWindow, true);
        StartQuest07();
    }

    public override void Clear()
    {
        // 씬 클리어 시 필요한 작업을 여기에 작성
    }

    private void StartQuest07()
    {
        // 퀘스트 07 시작
        if(Managers.Quest.GetQuest(6).IsCompleted == true)
        {
            C_StartQuest quest = new C_StartQuest() { TemplateId = 7 };
            Managers.Network.Send(quest);
        }
    }

    public override void CheckInteractionQuest(Quest quest)
    {
        if(quest.Id == 7)
        {
            Managers.Quest.ShowQuestScript(1);
        }
    }

    public void OnPlayerExit()
    {
        // 플레이어가 DawnTownDead 씬을 나갈 때 호출되는 메서드
        Debug.Log("Player is exiting DawnTownDead scene.");
        // 다음 씬으로 이동하는 로직 추가
        //Managers.Scene.LoadScene(Define.Scene.NextScene); // Define.Scene.NextScene는 다음 씬의 Enum 값
    }
}