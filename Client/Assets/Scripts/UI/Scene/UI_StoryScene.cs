using Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_StoryScene : UI_Base
{
    public Image SceneChangeImage;
    public Image StoryImage;
    public TMP_Text StoryScriptPanel;
    private List<Sprite> storyImages = new List<Sprite>();
    private List<string> storyScripts = new List<string>();
    private int currentImageIndex = 0;
    private int currentScriptIndex = 0;
    private float imageDisplayTime = 5.0f; // 이미지가 자동으로 넘어가는 시간
    private Coroutine imageCoroutine;
    private Coroutine typingCoroutine;

    public enum Images
    {
        StoryImage,
    }
    public enum Texts
    {
        StoryScriptPanel,
    }

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<TMP_Text>(typeof(Texts));
    }

    public void ShowStory()
    {
        // 초기화 작업
        SceneChangeImage.gameObject.SetActive(true);
        StartCoroutine(FadeInOut(SceneChangeImage, () =>
        {
            ShowNextImage();
        }));
    }

    public void LoadStoryData(ScriptData scriptData)
    {
        foreach (var script in scriptData.scripts)
        {
            storyScripts.AddRange(script.script);
            if (!string.IsNullOrEmpty(script.image))
            {
                Sprite image = Managers.Resource.Load<Sprite>(script.image);
                if (image != null)
                {
                    storyImages.Add(image);
                }
            }
        }
    }

    private IEnumerator FadeInOut(Image image, System.Action onComplete)
    {
        // 페이드 인
        for (float t = 0; t < 1; t += Time.deltaTime)
        {
            image.color = new Color(0, 0, 0, t);
            yield return null;
        }

        // 페이드 아웃
        for (float t = 1; t > 0; t -= Time.deltaTime)
        {
            image.color = new Color(0, 0, 0, t);
            yield return null;
        }

        image.gameObject.SetActive(false);
        onComplete?.Invoke();
    }

    private void ShowNextImage()
    {
        if (currentImageIndex < storyImages.Count)
        {
            StoryImage.sprite = storyImages[currentImageIndex];
            currentImageIndex++;
            currentScriptIndex = 0;

            if (storyScripts != null && storyScripts.Count > 0)
            {
                ShowNextScript();
            }
            else
            {
                imageCoroutine = StartCoroutine(WaitAndShowNextImage());
            }
        }
        else
        {
            // 스토리 종료 처리
            EndStory();
        }
    }

    private IEnumerator WaitAndShowNextImage()
    {
        yield return new WaitForSeconds(imageDisplayTime);
        ShowNextImage();
    }

    private void ShowNextScript()
    {
        if (currentScriptIndex < storyScripts.Count)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            typingCoroutine = StartCoroutine(TypeText(storyScripts[currentScriptIndex]));
            currentScriptIndex++;
        }
        else
        {
            ShowNextImage();
        }
    }

    private IEnumerator TypeText(string text)
    {
        StoryScriptPanel.text = "";
        foreach (char letter in text.ToCharArray())
        {
            StoryScriptPanel.text += letter;
            yield return new WaitForSeconds(0.05f); // 한 글자씩 출력되는 속도 조절
        }
    }

    public void OnScriptPanelClick()
    {
        if (currentScriptIndex < storyScripts.Count)
        {
            ShowNextScript();
        }
        else
        {
            ShowNextImage();
        }
    }

    private void EndStory()
    {
        // 스토리 종료 처리
        gameObject.SetActive(false);
    }
}