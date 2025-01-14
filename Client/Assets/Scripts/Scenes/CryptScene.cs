using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class CryptScene : BaseScene
{
    protected UI_GameScene _sceneUi;
    protected UI_Description _description;
    CameraController _cameraController;
    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.Crypt;
        Managers.Map.LoadMap(7); // DawnTownDead 맵 로드
        Screen.SetResolution(640, 480, false);
        Camera.main.orthographicSize = ZoomLevel;

        _cameraController = Camera.main.GetComponent<CameraController>();
        _cameraController.SetPixelPerfect();
        _sceneUi = Managers.UI.ShowSceneUI<UI_GameScene>();
        _sceneUi.SetActive(_sceneUi.GameWindow, true);
        _description = Managers.UI.ShowPopupUI<UI_Description>();
        _description.gameObject.SetActive(false);
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
            _cameraController = Camera.main.GetComponent<CameraController>();
        }
    }

    public override void CheckOnSceneLoadedQuest()
    {
    }

    public override void StartBattleQuest(Quest quest)
    {
    }
}
