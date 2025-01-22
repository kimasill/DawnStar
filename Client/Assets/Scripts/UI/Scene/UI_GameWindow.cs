using Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_GameWindow : UI_Base
{
    public UI_StateBar StateUI { get; set; }
    public UI_StoryPanel StoryPanel { get; set; }
    public UI_QuickSlot QuickSlot { get; set; }
    public UI_SkillSlot SkillSlot { get; set; }
    public UI_BuffPanel BuffPanel { get; set; }
    public UI_Chat Chat { get; set; }
    public UI_BossHpBar BossHpBar { get; set; }
    [SerializeField] public GameObject ChatButton;
    public override void Init()
    {
        StateUI = GetComponentInChildren<UI_StateBar>();
        StoryPanel = GetComponentInChildren<UI_StoryPanel>();
        QuickSlot = GetComponentInChildren<UI_QuickSlot>();
        SkillSlot = GetComponentInChildren<UI_SkillSlot>();
        BuffPanel = GetComponentInChildren<UI_BuffPanel>();
        Chat = GetComponentInChildren<UI_Chat>();
        BossHpBar = GetComponentInChildren<UI_BossHpBar>();
        Chat.CloseAction = () => ChatButton.SetActive(true); 

        ChatButton.BindEvent(OnClickChatButton);
        StoryPanel.gameObject.SetActive(false);
        Chat.gameObject.SetActive(false);
        BossHpBar.gameObject.SetActive(false);
    }
    public void ShowScript(List<string> strings)
    {
        if(StoryPanel != null)
            StoryPanel.ShowOnlyScript(strings);
    }
    public void ShowStoryPanel(Dictionary<int, Script> scripts, bool questEnd)
    {
        if (StoryPanel != null)
        {

            foreach (var script in scripts.Values)
            {
                //TODO : 여러개의 스토리를 보여주기 위한 처리. 지금은 조건을 따지지않고 순서대로 다보여줌
                StoryPanel.ShowStoryPanel(script, questEnd);
            }
            StoryPanel.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("StoryPanel을 찾을 수 없습니다.");
        }
    }

    public void ShowStoryPanel(NPCData npcData)
    {

        if (StoryPanel != null)
        {
            StoryPanel.ShowScriptAndProfile(npcData);
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
    public void UpdateHpUI()
    {
        if (StateUI != null)
        {
            StateUI.UpdateHpBar();
        }
        else
        {
            Debug.LogWarning("StateUI를 찾을 수 없습니다.");
        }
    }
    public void UpdateUpUI()
    {
        if(StateUI != null)
        {
            StateUI.UpdateUpBar();
        }
        else
        {
            Debug.LogWarning("StateUI를 찾을 수 없습니다.");
        }
    }

    public void OnClickChatButton(PointerEventData eventData)
    {
        if (Chat != null)
        {
            Chat.OpenUI(eventData);
        }
        ChatButton.SetActive(false);
    }
}
