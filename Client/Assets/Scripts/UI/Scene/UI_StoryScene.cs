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
    public Image CharacterImage;
    public Image CharacterNameFrame;
    public TMP_Text CharacterNameText;
    public TMP_Text StoryScriptText;
    private List<Sprite> storyImages = new List<Sprite>();
    private List<List<string>> storyScripts = new List<List<string>>();
    private List<string> storySounds = new List<string>(); // 사운드 파일 경로 리스트
    private int sceneIndex = 0;
    private int currentLineIndex = 0;
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private bool isEndOfScript = false;

    public enum Images
    {
        UI_TextPanel,
        UI_Background,
        SceneChangeImage,
        UI_StoryImage,
        CharacterProfileImage,
        UI_CharacterNameFrame,
    }
    public enum Texts
    {
        UI_StoryText,
        CharacterNameText,
    }

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<TMP_Text>(typeof(Texts));

        StoryImage = GetImage((int)Images.UI_StoryImage);
        StoryScriptText = GetTextMeshPro((int)Texts.UI_StoryText);
        CharacterNameText = GetTextMeshPro((int)Texts.CharacterNameText);
        CharacterImage = GetImage((int)Images.CharacterProfileImage);
        CharacterNameFrame = GetImage((int)Images.UI_CharacterNameFrame);
        Background = GetImage((int)Images.UI_Background);
        SceneChangeImage = GetImage((int)Images.SceneChangeImage);

        GetImage((int)Images.UI_TextPanel).gameObject.SetActive(false);
        Background.gameObject.SetActive(false);

        GetImage((int)Images.UI_TextPanel).gameObject.BindEvent(OnScriptPanelClick);
    }

    public void ShowStory()
    {
        Debug.Log("ShowStory");
        StartCoroutine(InitialFadeIn());
    }

    private IEnumerator InitialFadeIn()
    {
        _isFading = true; // 페이드 인 시작

        yield return StartCoroutine(FadeIn(SceneChangeImage, 1.0f));

        Background.gameObject.SetActive(true);
        GetImage((int)Images.UI_TextPanel).gameObject.SetActive(true);
        CharacterNameFrame.gameObject.SetActive(false);
        CharacterImage.color = new Color(CharacterImage.color.r, CharacterImage.color.g, CharacterImage.color.b, 0);
        Managers.Map.CurrentGrid.gameObject.SetActive(false);
        Managers.Object.MyPlayer.gameObject.SetActive(false);
        StoryImage.sprite = storyImages[sceneIndex];
        yield return new WaitForSeconds(3.0f);

        yield return StartCoroutine(FadeOut(SceneChangeImage, 1.0f));

        _isFading = false; // 페이드 인 종료
        ShowNextScript();
    }

    public void LoadStoryData(ScriptData scriptData)
    {
        storyImages.Clear();
        storyScripts.Clear();
        storySounds.Clear(); // 사운드 파일 경로 리스트 초기화

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

            if (!string.IsNullOrEmpty(script.sound))
            {
                storySounds.Add(script.sound);
            }
            else
            {
                storySounds.Add(null); // 사운드가 없는 경우 null 추가
            }
        }
    }

    private IEnumerator FadeInOut(Image image, System.Action onFaded, System.Action onComplete, float waitTime)
    {
        _isFading = true; // 페이드 인/아웃 중에는 클릭 이벤트 무시
        yield return StartCoroutine(FadeIn(image, 1.0f));

        onFaded?.Invoke();

        yield return new WaitForSeconds(waitTime);

        yield return StartCoroutine(FadeOut(image, 1.0f));
        _isFading = false; // 페이드 인/아웃 종료

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
            StartCoroutine(FadeInOut
            (
                SceneChangeImage,
                () =>
                {
                    StoryScriptText.text = "";
                    CharacterNameText.text = "";
                    CharacterImage.sprite = null;
                    CharacterImage.color = new Color(CharacterImage.color.r, CharacterImage.color.g, CharacterImage.color.b, 0);
                    CharacterNameFrame.gameObject.SetActive(false);
                    StoryImage.sprite = storyImages[sceneIndex];

                    // 사운드 재생
                    if (!string.IsNullOrEmpty(storySounds[sceneIndex]))
                    {
                        Managers.Sound.Play(storySounds[sceneIndex], Define.Sound.Bgm);
                    }
                },
                ShowNextScript,
                waitTime
                )
            );
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
    }

    private string SplitSentence(List<string> textList)
    {
        string sentence = textList[currentLineIndex];
        if (sentence.Contains(":"))
        {
            string[] splitSentence = sentence.Split(':');
            return splitSentence[1];
        }
        return sentence;
    }

    private string SplitSentenceAndAssign(List<string> textList)
    {
        string sentence = textList[currentLineIndex];
        string characterName = "";
        string characterProfileImagePath = "";

        if (sentence.Contains(":"))
        {
            CharacterNameFrame.gameObject.SetActive(true);
            string[] splitSentence = sentence.Split(':');
            if (!splitSentence[0].Contains(","))
            {
                CharacterNameText.text = splitSentence[0];
                return splitSentence[1];
            }
            string characterImageName = splitSentence[0].Split(',')[1];
            characterName = splitSentence[0].Split(',')[0];
            sentence = splitSentence[1];

            CharacterNameText.text = characterName;
            if (characterImageName != "")
                characterProfileImagePath = "Textures/Images/" + characterImageName;
            Sprite characterProfileImage = Managers.Resource.Load<Sprite>(characterProfileImagePath);
            if (characterProfileImage != null)
            {
                CharacterImage.sprite = characterProfileImage;
                StartCoroutine(FadeIn(CharacterImage, 1.0f));
            }
        }
        else
        {
            if (CharacterImage.sprite != null)
            {
                StartCoroutine(FadeOut(CharacterImage, 1.0f));
            }
            CharacterNameFrame.gameObject.SetActive(false);
        }

        return sentence;
    }

    private IEnumerator TypeText(List<string> textList)
    {
        isTyping = true;
        if (currentLineIndex < textList.Count)
        {
            string sentence = SplitSentenceAndAssign(textList);
            StoryScriptText.text = "";

            foreach (char letter in sentence.ToCharArray())
            {
                StoryScriptText.text += letter;
                yield return new WaitForSeconds(0.05f);
                if (letter == '\n')
                {
                    yield return new WaitForSeconds(0.5f);
                }
            }
            isTyping = false;
        }
    }

    public void OnScriptPanelClick(PointerEventData evt)
    {
        if (_isFading) return; // 페이드 인/아웃 중에는 클릭 이벤트 무시

        Debug.Log(currentLineIndex);
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            StoryScriptText.text = SplitSentence(storyScripts[sceneIndex]);
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
            if (sceneIndex < storyScripts.Count)
            {
                ShowNextScene();
            }
            else
            {
                StartCoroutine(FadeOutAndEndStory());
            }
        }
    }

    private IEnumerator FadeOutAndEndStory()
    {
        Managers.Map.CurrentGrid.gameObject.SetActive(true);
        Managers.Object.MyPlayer.gameObject.SetActive(true);
        UI_GameScene gameUI = Managers.UI.SceneUI as UI_GameScene;
        gameUI.GameWindow.gameObject.SetActive(true);
        yield return StartCoroutine(FadeOut(SceneChangeImage, 1.0f));
        EndStory();
    }

    private void EndStory()
    {
        Clear();
        Managers.Quest.EndQuest();
        gameObject.SetActive(false);
    }

    private void Clear()
    {
        storyImages.Clear();
        storyScripts.Clear();
        storySounds.Clear(); // 사운드 파일 경로 리스트 초기화
        sceneIndex = 0;
        currentLineIndex = 0;
        isTyping = false;
    }
}