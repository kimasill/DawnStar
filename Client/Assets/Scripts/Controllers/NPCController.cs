using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Define;

public class NPCController : CreatureController
{
    private GameObject _headUpIcon;
    private TextMeshPro _headUpText;
    public int _id;
    public string Name { get; private set; }
    public int QuestId { get; private set; }
    public NPCType Type { get; private set; }
    public int TemplateId { 
        get { return _id; }
        set { _id = value; } 
    }

    protected override void Init()
    {
    }

    public void SetNPC(string npcName)
    {        
        if (Managers.Data.NPCDict.TryGetValue(npcName, out NPCData npcData))
        {
            TemplateId = npcData.id;
            Name = npcData.name;
            Type = npcData.npcType;
        }
    }

    public void ActivateNotification()
    {
        _headUpIcon = Managers.Resource.Instantiate("UI/HeadUpIcon", transform);
        if (_headUpIcon == null)
            return;        
        Sprite iconSprite = null;
            // NPC 타입에 따라 아이콘 설정
        switch (Type)
        {
            case NPCType.Talk:
                iconSprite = Managers.Resource.Load<Sprite>("Textures/Images/QuestIcons/Icon_Talk");
                break;
            case NPCType.Shop:
                iconSprite = Managers.Resource.Load<Sprite>("Textures/Images/QuestIcons/Icon_Shop");
                break;
            case NPCType.Quest:
                iconSprite = Managers.Resource.Load<Sprite>("Textures/Images/QuestIcons/Icon_Quest");
                break;
            case NPCType.Smith:
                iconSprite = Managers.Resource.Load<Sprite>("Textures/Images/QuestIcons/Icon_Smith");
                break;
            default:
                iconSprite = Managers.Resource.Load<Sprite>("Textures/Images/QuestIcons/Icon_Frame");
                break;
        }        
        _headUpIcon.GetComponent<SpriteRenderer>().sprite = iconSprite;
        _headUpIcon.transform.localPosition = new Vector3(0, 1f, 0);
        _headUpIcon.SetActive(true);
        _headUpText = _headUpIcon.GetComponentInChildren<TextMeshPro>();
        StartCoroutine(BlinkText(_headUpText));
    }

    public void DeactivateNotification()
    {
        StopCoroutine(BlinkText(_headUpText));
        _headUpIcon?.SetActive(false);        
    }

    private IEnumerator BlinkText(TextMeshPro text)
    {        
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