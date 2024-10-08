using Google.Protobuf.Protocol;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting.ReorderableList;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UI_Shop;
using static UnityEditor.Progress;

public class UI_Quest : UI_Base
{
    [SerializeField]
    private GameObject grid;

    enum Images
    {
        QuestTabFrame,
        QuestExitButton
    }

    enum Texts
    {
        DescriptionText
    }

    bool _init = false;
    private List<UI_Quest_Title> Quests = new List<UI_Quest_Title>();
    public override void Init()
    {
        Quests.Clear();

        foreach (Transform child in grid.transform)
        {
            Destroy(child.gameObject);
        }

        GameObject go = Managers.Resource.Instantiate("UI/Scene/UI_Quest_Title", grid.transform);
        UI_Quest_Title quest = go.GetOrAddComponent<UI_Quest_Title>();
        Quests.Add(quest);

        Bind<Image>(typeof(Images));
        Bind<TMP_Text>(typeof(Texts));
        GetImage((int)Images.QuestExitButton).gameObject.BindEvent(OnClickExitButton);
        GetImage((int)Images.QuestTabFrame).gameObject.BindEvent(OnClickTabImage);
        _init = true;
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (_init == false)
            return;

        if (Quests.Count == 0)
        {
            return;
        }

        List<Quest> questList = Managers.Quest.Quests.Values
            .OrderBy(quest => quest.TemplateId)
            .ToList();

        foreach (Quest quest in questList)
        {
            GameObject go = Managers.Resource.Instantiate("UI/Scene/UI_Quest_Title", grid.transform);
            UI_Quest_Title questTitle = go.GetOrAddComponent<UI_Quest_Title>();
            questTitle.SetQuest(quest);
            Quests.Add(questTitle);
            if (questTitle.Completed)
            {
                // Change alpha value to make it translucent
                Color titleColor = questTitle.GetComponent<Image>().color;
                titleColor.a = 0.5f;
                questTitle.GetComponent<Image>().color = titleColor;
            }
        }
    }

    public void OnQuestTitleClick(UI_Quest_Title questTitle)
    {
        if (questTitle.Clicked == false)
        {
            questTitle.Clicked = true;
            foreach (UI_Quest_Title quest in Quests)
            {
                if (quest == questTitle)
                    continue;
                if (quest.Clicked == true)
                {
                    quest.Clicked = false;
                }
            }
        }
        else
        {
            questTitle.Clicked = false;
            
        }       

        GetTextMeshPro((int)Texts.DescriptionText).text = questTitle.Description;
    }

    public void OnClickTabImage(PointerEventData evt)
    {
        RectTransform rectTransform = GetImage((int)Images.QuestTabFrame).GetComponent <RectTransform>();
        if (rectTransform.anchoredPosition.x == -15)
        {
            rectTransform.anchoredPosition += new Vector2(15, 0);
        }
        else
        {
            rectTransform.anchoredPosition += new Vector2(-15, 0);
        }

        RefreshUI();
    }

    public void OnClickExitButton(PointerEventData evt)
    {
        CloseQuest();
    }

    public void CloseQuest()
    {
        gameObject.SetActive(false);
    }
}