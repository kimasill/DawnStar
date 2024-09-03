using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Description : UI_Base
{
    public TextMeshPro descriptionText;
    public CanvasGroup canvasGroup;
    private Queue<string> scriptQueue = new Queue<string>();
    private Coroutine typingCoroutine;
    private bool isTyping = false;

    enum Buttons
    {
        Panel
    }

    public override void Init()
    {
        descriptionText = Get<TextMeshPro>(0);
        canvasGroup.alpha = 0;

        Bind<Button>(typeof(Buttons));

        // 패널 클릭 이벤트 추가
        GetButton((int)Buttons.Panel).onClick.AddListener(OnPanelClick);        
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
            string nextScript = scriptQueue.Dequeue();
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            typingCoroutine = StartCoroutine(TypeText(nextScript));
        }
        else
        {
            StartCoroutine(FadeOut());
        }
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        descriptionText.text = "";
        foreach (char letter in text.ToCharArray())
        {
            descriptionText.text += letter;
            yield return new WaitForSeconds(0.05f); // 한 글자씩 출력되는 속도 조절
        }
        isTyping = false;
        yield return new WaitForSeconds(1f); // 텍스트가 모두 출력된 후 대기 시간
        ShowNextScript();
    }

    private IEnumerator FadeIn()
    {
        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator FadeOut()
    {
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime;
            yield return null;
        }
        gameObject.SetActive(false);
    }

    public void ShowDescription(List<string> scripts)
    {
        gameObject.SetActive(true);
        StartCoroutine(FadeIn());
        SetTexts(scripts);
    }

    private void OnPanelClick()
    {
        if (isTyping)
        {
            // 현재 타이핑 중인 텍스트를 모두 출력
            StopCoroutine(typingCoroutine);
            descriptionText.text = scriptQueue.Peek();
            isTyping = false;
        }
        else
        {
            // 다음 스크립트로 넘어감
            ShowNextScript();
        }
    }
}
