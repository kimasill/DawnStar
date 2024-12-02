using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Description : UI_Popup
{
    private TMP_Text _textField;
    private CanvasGroup _canvas;
    private Queue<string> scriptQueue = new Queue<string>();
    private Coroutine typingCoroutine;
    private string currentTypingText = "";
    private bool isTyping = false;

    enum Images
    {
        Description_Panel
    }

    enum TextMeshPro
    {
        Description_Text
    }

    public override void Init()
    {
        Bind<TMP_Text>(typeof(TextMeshPro));
        Bind<Image>(typeof(Images));

        _canvas = GetComponent<CanvasGroup>();
        _canvas.alpha = 0;

        GetImage((int)Images.Description_Panel).gameObject.BindEvent(OnPanelClick);

        _textField = GetTextMeshPro((int)TextMeshPro.Description_Text);
    }

    public void SetTexts(List<string> scripts)
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
            currentTypingText = scriptQueue.Dequeue();
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            typingCoroutine = StartCoroutine(TypeText(currentTypingText));
        }
        else
        {
            StartCoroutine(FadeOut());
        }
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        _textField.text = "";
        foreach (char letter in text.ToCharArray())
        {
            _textField.text += letter;
            yield return new WaitForSeconds(0.05f); // 한 글자씩 출력되는 속도 조절
            if (letter == '\n')
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
        isTyping = false;
        yield return new WaitForSeconds(1f); // 텍스트가 모두 출력된 후 대기 시간
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
        Managers.Quest.EndQuest();
    }

    public void ShowDescription(List<string> scripts)
    {
        gameObject.SetActive(true);
        StartCoroutine(FadeIn());
        SetTexts(scripts);
    }

    private void OnPanelClick(PointerEventData evt)
    {
        if (isTyping)
        {
            // 현재 타이핑 중인 텍스트를 모두 출력
            if (currentTypingText != null)
            {
                StopCoroutine(typingCoroutine);
                _textField.text = currentTypingText;
                isTyping = false;
            }
        }
        else
        {
            // 다음 스크립트로 넘어감
            ShowNextScript();
        }
    }
}
