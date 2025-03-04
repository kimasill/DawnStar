using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DawnTown : BaseScene
{
    protected UI_GameScene _sceneUi;
    protected UI_Description _description;
    public override void Clear()
    {
    }

    protected override void Init()
    {
        SceneType = Define.Scene.DawnTown;
        base.Init();
        Managers.Map.LoadMap(1);
        Camera.main.orthographicSize = ZoomLevel;
        _sceneUi = Managers.UI.ShowSceneUI<UI_GameScene>();
        _sceneUi.SetActive(_sceneUi.GameWindow, true);
        _sceneUi.GetMap("001");
        _description = Managers.UI.ShowPopupUI<UI_Description>();
        _description.gameObject.SetActive(false);
        Managers.Sound.PlayBGM();
    }

    public override void ShowDescriptionUI(List<string> description)
    {
        _description.gameObject.SetActive(true);      
        _description.ShowDescription(description);
    }
    
    protected void InitializeShop()
    {        
    }
}
