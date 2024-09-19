using Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_GameWindow : UI_Base
{
    UI_StateBar StateUI { get; set; }
    public UI_StoryPanel StoryPanel { get; set; }

    public override void Init()
    {
        StateUI = GetComponentInChildren<UI_StateBar>();
        StoryPanel = GetComponentInChildren<UI_StoryPanel>();

        StateUI.gameObject.SetActive(false);
        StoryPanel.gameObject.SetActive(false);
    }

    public void ShowStoryPanel(Dictionary<int, Script> storyDict)
    {
        if (StoryPanel != null)
        {

            foreach (var script in storyDict.Values)
            {
                //TODO : 여러개의 스토리를 보여주기 위한 처리. 지금은 조건을 따지지않고 순서대로 다보여줌
                StoryPanel.ShowStoryPanel(script.script);
            }
            StoryPanel.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("StoryPanel을 찾을 수 없습니다.");
        }
    }

    public void ShowStoryPanel(List<NPCScript> scriptList)
    {
        if (StoryPanel != null)
        {
            StoryPanel.ShowScript(scriptList);
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
