using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_QuestNoti : UI_Popup
{
    enum Images
    {
        QuestNotiImage,
        QuestNotiPanel,
    }
    enum Texts
    {
        QuestNotiText
    }

    TMP_Text _questNotiText;
    Image _questNotiImage;
    GameObject _questNotiPanel;
    CanvasGroup _canvasGroup;
    Queue<IEnumerator> _coroutineQueue = new Queue<IEnumerator>();
    bool _isCoroutineRunning = false;

    public override void Init()
    {
        base.Init();
        Bind<Image>(typeof(Images));
        Bind<TMP_Text>(typeof(Texts));
        _questNotiPanel = GetImage((int)Images.QuestNotiPanel).gameObject;
        _questNotiImage = GetImage((int)Images.QuestNotiImage);
        _questNotiText = GetTextMeshPro((int)Texts.QuestNotiText);
        _canvasGroup = _questNotiPanel.GetOrAddComponent<CanvasGroup>();

        _questNotiPanel.gameObject.SetActive(false);
    }

    public void ShowQuestStart(string questName)
    {
        if (_questNotiPanel != null && _questNotiText != null)
        {
            _questNotiText.text = $"{questName}";
            _questNotiImage.gameObject.SetActive(true);

            EnqueueCoroutine(ShowAndHideQuestNoti());
        }
    }

    public void ShowQuestComplete(string questName)
    {
        if (_questNotiPanel != null && _questNotiText != null)
        {
            _questNotiText.text = $"완료: {questName}";
            _questNotiImage.gameObject.SetActive(false);
            _questNotiPanel.SetActive(true);

            EnqueueCoroutine(ShowAndHideQuestNoti());
        }
    }

    private void EnqueueCoroutine(IEnumerator coroutine)
    {
        _coroutineQueue.Enqueue(coroutine);
        if (!_isCoroutineRunning)
        {
            StartCoroutine(ProcessCoroutineQueue());
        }
    }

    private IEnumerator ProcessCoroutineQueue()
    {
        _isCoroutineRunning = true;
        while (_coroutineQueue.Count > 0)
        {
            yield return StartCoroutine(_coroutineQueue.Dequeue());
        }
        _isCoroutineRunning = false;
    }

    private IEnumerator ShowAndHideQuestNoti()
    {
        yield return StartCoroutine(ShowQuestNoti(1f)); // 알림을 표시하는 시간
        yield return new WaitForSeconds(3f); // 알림이 표시되는 시간
        yield return StartCoroutine(HideQuestNoti()); // 알림을 숨기는 시간
    }

    private IEnumerator ShowQuestNoti(float duration)
    {
        if (_questNotiPanel != null)
        {
            _questNotiPanel.SetActive(true);
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / duration);
                yield return null;
            }
        }
    }

    private IEnumerator HideQuestNoti()
    {
        if (_questNotiPanel != null)
        {
            float fadeDuration = 1f; // 서서히 사라지는 시간
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
                yield return null;
            }
            _questNotiPanel.SetActive(false);
        }
    }
}