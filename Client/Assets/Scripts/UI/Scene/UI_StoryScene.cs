using Data;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_StoryScene : UI_Base
{
    public Image SceneChangeImage;
    public Image StoryImage;
    public Image Background;
    public Image TextPanel;
    public TMP_Text StoryScriptText;
    private List<Sprite> storyImages = new List<Sprite>();
    private List<List<string>> storyScripts = new List<List<string>>();
    private int sceneIndex = 0;
    private int currentLineIndex = 0;
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private bool isEndOfScript = false;
    private bool isFading = false; // 페이드 인/아웃 중인지 여부를 나타내는 플래그

    public enum Images
    {
        UI_TextPanel,
        UI_Background,
        SceneChangeImage,
        UI_StoryImage,
    }
    public enum Texts
    {
        UI_StoryText,
    }

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<TMP_Text>(typeof(Texts));

        StoryImage = GetImage((int)Images.UI_StoryImage);
        StoryScriptText = GetTextMeshPro((int)Texts.UI_StoryText);
        Background = GetImage((int)Images.UI_Background);
        Background.gameObject.SetActive(false);
        SceneChangeImage = GetImage((int)Images.SceneChangeImage);
        GetImage((int)Images.UI_TextPanel).gameObject.BindEvent(OnScriptPanelClick);
    }

    public void ShowStory()
    {
        Debug.Log("ShowStory");
        StartCoroutine(InitialFadeIn());
    }

    private IEnumerator InitialFadeIn()
    {
        isFading = true; // 페이드 인 시작
        float alpha = 0;
        while (alpha < 1)
        {
            alpha += Time.deltaTime / 1.0f;
            SceneChangeImage.color = new Color(SceneChangeImage.color.r, SceneChangeImage.color.g, SceneChangeImage.color.b, alpha);
            yield return null;
        }

        Background.gameObject.SetActive(true);
        Managers.Map.CurrentGrid.gameObject.SetActive(false);
        Managers.Object.MyPlayer.gameObject.SetActive(false);
        StoryImage.sprite = storyImages[sceneIndex];
        yield return new WaitForSeconds(3.0f);

        alpha = 1;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime / 1.0f;
            SceneChangeImage.color = new Color(SceneChangeImage.color.r, SceneChangeImage.color.g, SceneChangeImage.color.b, alpha);
            yield return null;
        }
        isFading = false; // 페이드 인 종료
        ShowNextScript();
    }

    public void LoadStoryData(ScriptData scriptData)
    {
        storyImages.Clear();
        storyScripts.Clear();

        foreach (var script in scriptData.scripts)
        {
            if (!string.IsNullOrEmpty(script.image))
            {
                Sprite image = Managers.Resource.Load<Sprite>(script.image);
                if (image != null)
                {
                    storyImages.Add(image);
                }
            }

            if (script.script != null && script.script.Count > 0)
            {
                storyScripts.Add(new List<string>(script.script));
            }
        }
    }

    private IEnumerator FadeInOut(Image image, System.Action onComplete, float waitTime)
    {
        isFading = true; // 페이드 인 시작
        float alpha = 0;
        while (alpha < 1)
        {
            alpha += Time.deltaTime / 1.0f;
            SceneChangeImage.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
            yield return null;
        }
        StoryScriptText.text = "";
        StoryImage.sprite = storyImages[sceneIndex];        
        yield return new WaitForSeconds(waitTime);

        alpha = 1;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime / 1.0f;
            SceneChangeImage.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
            yield return null;
        }
        isFading = false;
        onComplete?.Invoke();
    }

    private void ShowNextScene()
    {
        ShowNextImage();
        Debug.Log($"SceneIndex : {sceneIndex}");
    }

    private void ShowNextImage()
    {
        if (sceneIndex < storyImages.Count)
        {
            float waitTime = sceneIndex == 0 ? 3.0f : 1.0f;
            StartCoroutine(FadeInOut(SceneChangeImage, () =>
            {                
                ShowNextScript();
            }, waitTime));
             // 페이드 인/아웃 후에 스크립트 출력
        }
    }

    private void ShowNextScript()
    {
        if (sceneIndex < storyScripts.Count)
        {
            currentLineIndex = 0;
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            typingCoroutine = StartCoroutine(TypeText(storyScripts[sceneIndex]));
        }
        else
        {
            EndStory();
        }
    }

    private IEnumerator TypeText(List<string> textList)
    {
        isTyping = true;
        if (currentLineIndex < textList.Count)
        {
            StoryScriptText.text = "";
            foreach (char letter in textList[currentLineIndex].ToCharArray())
            {
                StoryScriptText.text += letter;
                yield return new WaitForSeconds(0.05f);
                if(letter == '\n')
                {
                    yield return new WaitForSeconds(0.5f);
                }
            }
            isTyping = false;
        }
    }

    public void OnScriptPanelClick(PointerEventData evt)
    {
        if (isFading) return; // 페이드 인/아웃 중에는 클릭 이벤트 무시

        Debug.Log(currentLineIndex);
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            StoryScriptText.text = storyScripts[sceneIndex][currentLineIndex];
            isTyping = false;
        }
        else if (!isTyping)
        {
            if (currentLineIndex == storyScripts[sceneIndex].Count - 1)
            {
                isEndOfScript = true;
            }
            else if (currentLineIndex < storyScripts[sceneIndex].Count)
            {
                currentLineIndex++;
                typingCoroutine = StartCoroutine(TypeText(storyScripts[sceneIndex]));
            }
        }

        if (isEndOfScript && !isTyping)
        {
            isEndOfScript = false;
            sceneIndex++;
            ShowNextScene();
        }
    }

    private void EndStory()
    {
        gameObject.SetActive(false);
    }
}