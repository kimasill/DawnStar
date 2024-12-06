using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_GameScene : UI_Scene
{
    public UI_Stat StatUI { get; private set; }
    public UI_Inventory InvenUI { get; private set; }
    public UI_GameWindow GameWindow { get; private set; }
    public UI_Map MapUI { get; private set; }
    public UI_Shop ShopUI { get; private set; }
    public UI_StoryScene StoryScene { get; private set; }
    public UI_Quest QuestUI { get; private set; }
    public UI_Notification NotificationUI { get; private set; }
    public UI_Matching MatchingUI { get; private set; }
    public UI_Enhance Enhance { get; private set; }

    public override void Init()
    {
        base.Init();
        GameWindow = GetComponentInChildren<UI_GameWindow>();         
        StatUI = GetComponentInChildren<UI_Stat>();
        InvenUI = GetComponentInChildren<UI_Inventory>();        
        ShopUI = GetComponentInChildren<UI_Shop>();
        StoryScene = GetComponentInChildren<UI_StoryScene>();
        QuestUI = GetComponentInChildren<UI_Quest>();
        NotificationUI = GetComponentInChildren<UI_Notification>();
        MatchingUI = GetComponentInChildren<UI_Matching>();
        Enhance = GetComponentInChildren<UI_Enhance>();

        GameWindow.gameObject.SetActive(false);
        StatUI.gameObject.SetActive(false);
        InvenUI.gameObject.SetActive(false);       
        ShopUI.gameObject.SetActive(false);
        StoryScene.gameObject.SetActive(false);
        QuestUI.gameObject.SetActive(false);
        MatchingUI.gameObject.SetActive(false);
        Enhance.gameObject.SetActive(false);
    }

    public void GetMap(string id)
    {
        Managers.Resource.Instantiate($"UI/UI_Map_{id}", transform);
        MapUI = GetComponentInChildren<UI_Map>();
        MapUI.gameObject.SetActive(false);
    }

    public void SetActive<T>(T ui, bool trigger) where T : UI_Base
    {
        ui.gameObject.SetActive(trigger);
    }
}
