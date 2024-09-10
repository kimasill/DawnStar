using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_GameWindow : UI_Base
{           
    UI_StateBar StateUI { get; set; }
    UI_StoryPanel StoryPanel { get; set; }

    public override void Init()
    {        
        StateUI = GetComponentInChildren<UI_StateBar>();        
        StoryPanel = GetComponentInChildren<UI_StoryPanel>();

        StateUI.gameObject.SetActive(false);
        StoryPanel.gameObject.SetActive(false);
    }

    public void ShowStoryPanel(List<string> storyText)
    {
        if (StoryPanel != null)
        {
            StoryPanel.ShowStoryPanel(storyText);
            StoryPanel.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("StoryPanel을 찾을 수 없습니다.");
        }
    }

    public void HideStoryPanel()
    {
        if (StoryPanel != null)
        {
            StoryPanel.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("StoryPanel을 찾을 수 없습니다.");
        }
    }
}
