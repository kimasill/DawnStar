using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Define;

public class NPCController : CreatureController
{
    public GameObject notificationIcon;
    public GameObject notificationText;

    public int _id;
    public int TemplateId { 
        get { return _id; }
        set { _id = value; } 
    }

    protected override void Init()
    {
        notificationIcon.SetActive(false);
    }

    public void ActivateNotification()
    {
        notificationIcon.SetActive(true);
        StartCoroutine(BlinkText());
    }

    public void DeactivateNotification()
    {
        notificationIcon.SetActive(false);
        StopCoroutine(BlinkText());
    }

    private IEnumerator BlinkText()
    {
        TextMeshPro text = notificationText.GetComponent<TextMeshPro>();
        if (text == null)
            yield break;

        while (true)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, Mathf.PingPong(Time.time, 1));
            yield return null;
        }
    }

    public void StartInteraction()
    {        
        if (Managers.Data.NPCDict.TryGetValue(name, out var npcData))
        {
            BaseScene currentScene = Managers.Scene.CurrentScene;
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            UI_GameWindow gameWindow = gameSceneUI.GameWindow; 
            gameWindow.ShowStoryPanel(npcData);
        }
    }
}