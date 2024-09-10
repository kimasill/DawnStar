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
        UI_Conversation_Text
    }

    private TMP_Text _storyText;
    private CanvasGroup _canvas;
    private Queue<string> scriptQueue = new Queue<string>();
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private bool isEndOfScript = false;

    public override void Init()
    {
        Bind<TMP_Text>(typeof(Texts));
        _canvas = GetComponent<CanvasGroup>();
        _canvas.alpha = 0;

        _storyText = GetTextMeshPro((int)Texts.UI_Conversation_Text);
        gameObject.BindEvent(OnPanelClick);
    }

    public void SetStoryTexts(List<string> scripts)
    {
        scriptQueue.Clear();
        foreach (var script in scripts)
        {
            scriptQueue.Enqueue(script);
        }
        ShowNextScript();
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
        Managers.Quest.EndQuest();
    }

    public void ShowStoryPanel(List<string> scripts)
    {
        gameObject.SetActive(true);
        StartCoroutine(FadeIn());
        SetStoryTexts(scripts);
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
