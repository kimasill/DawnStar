using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DawnTown : BaseScene
{
    UI_GameScene _sceneUi;
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.DawnTown;

        Managers.Map.LoadMap(1);

        Screen.SetResolution(640, 480, false);
        
        _sceneUi = Managers.UI.ShowSceneUI<UI_GameScene>();
    }

    public override void Clear()
    {
        
    }
}
