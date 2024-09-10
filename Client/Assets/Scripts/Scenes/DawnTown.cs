using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DawnTown : BaseScene
{
    private UI_GameScene _sceneUi;
    private UI_Description _description;

    public override void Clear()
    {
        throw new System.NotImplementedException();
    }

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.DawnTown;

        Managers.Map.LoadMap(1);

        Screen.SetResolution(640, 480, false);

        _sceneUi = Managers.UI.ShowSceneUI<UI_GameScene>();
        _sceneUi.GetMap("001");
        _description = Managers.UI.ShowPopupUI<UI_Description>();
        _description.gameObject.SetActive(false);
    }

    public override void ShowDescriptionUI(List<string> description)
    {
        _description.gameObject.SetActive(true);      
        _description.ShowDescription(description);
    }
}
