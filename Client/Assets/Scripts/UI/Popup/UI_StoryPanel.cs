using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_StoryPanel : UI_Popup
{
    enum Texts
    {
        UI_Conversation_Text,
    }

    enum Images
    {
        UI_Conversation_Image,
    }

    private TMP_Text _storyText;    
    private CanvasGroup _canvas;
    private Queue<string> scriptQueue = new Queue<string>();    
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private bool isEndOfScript = false;
    private Action onCloseAction;
    private Image _conversationImage;

    public override void Init()
    {
        _canvas = GetComponent<CanvasGroup>();
        _canvas.alpha = 0;
        Bind<TMP_Text>(typeof(Texts));
        Bind<Image>(typeof(Images));
        _storyText = GetTextMeshPro((int)Texts.UI_Conversation_Text);
        _conversationImage = GetImage((int)Images.UI_Conversation_Image);
        gameObject.BindEvent(OnPanelClick);
    }

    public void SetStoryTexts(List<string> scripts)
    {
        foreach (var script in scripts)
        {
            scriptQueue.Enqueue(script);
        }
        if (!isTyping)
        {
            ShowNextScript();
        }
    }

    private void SetConversationImage(string imageName)
    {
        // 이미지 로드 로직 (예: Resources 폴더에서 로드)
        Sprite image = Resources.Load<Sprite>($"Textures/Images/{imageName}");
        if (image != null)
        {
            Debug.Log($"Image '{imageName}' loaded successfully");
            _conversationImage.sprite = image;
        }
        else
        {
            Debug.LogWarning($"Image '{imageName}' not found in 'Textures/Image/'");
        }
    }

    public void ShowScriptAndProfile(List<NPCScript> scripts, string name)
    {
        gameObject.SetActive(true);
        StartCoroutine(FadeIn());
        foreach (var script in scripts)
        {
            if (script.type == "UI_Shop")
            {
                onCloseAction = () =>
                {
                    UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
                    if (gameSceneUI != null && gameSceneUI.ShopUI != null)
                    {
                        gameSceneUI.ShopUI.OpenShop(script.name, ""); // 타이틀을 script.name으로 설정
                    }
                };
            }
            SetConversationImage(name);
            SetStoryTexts(script.script);            
        }
    }
    public void ShowStoryPanel(Script scripts, bool IsQuestEnd = false)
    {
        if (IsQuestEnd)
        {
            onCloseAction = () =>
            {
                Managers.Quest.EndQuest();
            };
        }
        gameObject.SetActive(true);
        StartCoroutine(FadeIn());
        SetConversationImage(scripts.name);
        SetStoryTexts(scripts.script);
    }
    public void ShowStoryPanel(ScriptData scriptData, int id,  bool isQuestEnd = false)
    {
        Script scripts = scriptData.scripts[id-1];
        ShowStoryPanel(scripts, isQuestEnd);
    }

    private void ShowNextScript()
    {        
        if (scriptQueue.Count > 0)
        {
            string nextScript = scriptQueue.Dequeue();
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            typingCoroutine = StartCoroutine(TypeText(nextScript));
        }
        else
        {
            isEndOfScript = true;
        }
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        _storyText.text = "";
        foreach (char letter in text.ToCharArray())
        {
            _storyText.text += letter;
            yield return new WaitForSeconds(0.05f); // 한 글자씩 출력되는 속도 조절
        }
        isTyping = false;
        ShowNextScript();
    }

    private IEnumerator FadeIn()
    {
        float alpha = 0;
        while (alpha < 1)
        {
            alpha += Time.deltaTime;
            _canvas.alpha = alpha;
            yield return null;
        }
    }

    private IEnumerator FadeOut()
    {
        float alpha = 1;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime;
            _canvas.alpha = alpha;
            yield return null;
        }
        gameObject.SetActive(false);
        onCloseAction?.Invoke();
        onCloseAction = null;
    }

    

    private void OnPanelClick(PointerEventData evt)
    {
        if (isTyping)
        {
            // 현재 타이핑 중인 텍스트를 모두 출력
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                _storyText.text = scriptQueue.Peek();
                isTyping = false;
            }
        }
        else if (isEndOfScript)
        {
            // 스크립트가 끝났을 때 패널을 닫음
            StartCoroutine(FadeOut());
        }
        else
        {
            // 다음 스크립트로 넘어감
            ShowNextScript();
        }
    }
}