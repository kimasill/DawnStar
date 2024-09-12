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
    }
    
    public void ShowShop()
    {
        _sceneUi.SetActive(_shop, true);
    }

    public override void Clear()
    {
        // 필요에 따라 Clear 메서드를 구현
    }
}