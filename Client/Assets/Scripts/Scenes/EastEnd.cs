using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class EastEnd : BaseScene
{
    protected UI_GameScene _sceneUi;
    protected UI_Description _description;
    CameraController _cameraController;
    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.EastEnd;
        Managers.Map.LoadMap(5); // DawnTownDead 맵 로드
        Screen.SetResolution(640, 480, false);
        _sceneUi = Managers.UI.ShowSceneUI<UI_GameScene>();
        _sceneUi.SetActive(_sceneUi.GameWindow, true);
        _sceneUi.GetMap("005");
        _description = Managers.UI.ShowPopupUI<UI_Description>();
        _description.gameObject.SetActive(false);


        // 추가 초기화 로직
        InitializeNPCs();
        InitializeShop();
    }

    public override void Clear()
    {
        // 씬 클리어 시 필요한 작업을 여기에 작성
    }

    public override void ShowDescriptionUI(List<string> description)
    {
        if (_description == null)
        {
            _description = Managers.UI.ShowPopupUI<UI_Description>();
        }
        _description.ShowDescription(description);
    }

    protected void InitializeNPCs()
    {
        // NPC 초기화 로직
    }

    protected void InitializeShop()
    {
        // 상점 초기화 로직
    }

    private void HandleCompleteQuest(int questId)
    {
        Quest quest = Managers.Quest.GetQuest(questId);
        if (quest != null && !quest.IsCompleted)
        {
            Managers.Quest.EndQuest(questId);
            Debug.Log($"Quest {questId} has been completed.");
        }
    }

    public override void StartInteractionQuest(int questId)
    {
        if (_cameraController == null)
        {
            _cameraController= Camera.main.GetComponent<CameraController>();
        }
        if (questId == 9)
        {
            Managers.Object.MyPlayer.gameObject.SetActive(false);
            Vector2Int cameraPos = Managers.Map.GetCameraPosition(1);
            _cameraController.SetTarget(null);
            _cameraController.MoveToPosition(new Vector3(cameraPos.x, cameraPos.y, -10));
            // 3초 후에 카메라를 원래 위치로 되돌리고 타겟을 플레이어로 설정
            StartCoroutine(_cameraController.ResetCameraAndTarget(3.0f));
            Managers.Quest.ShowQuestScript(9, 1);
        }
    }

    public override void CheckOnSceneLoadedQuest()
    {
        if(Managers.Quest.IsQuestInProgress(7))
        {
            Managers.Quest.EndQuest(7);
        }
    }

    public override void StartBattleQuest(Quest quest)
    {
    }
}
