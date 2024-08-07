using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginScene : BaseScene
{
    UI_LoginScene _sceneUi;
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Login;
        Managers.Web.BaseUrl = "http://localhost:5273/api";

        Screen.SetResolution(640, 480, false);
        
        _sceneUi = Managers.UI.ShowSceneUI<UI_LoginScene>();
    }

    public override void Clear()
    {
        
    }
}
