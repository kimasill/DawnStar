using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Notification : UI_Base
{
    [SerializeField] private Transform _itemNotiPanel;
    [SerializeField] private Transform _expNotiPanel;
    [SerializeField] private Transform _levelNotiPanel;
    [SerializeField] private Transform _bossKillNotiPanel;
    [SerializeField] private Transform _basicNotiPanel;
    [SerializeField] private UI_QuestNoti _questNoti;
    [SerializeField] private int _maxNotis = 8;
    [SerializeField] private int _notiHeight = 50;


    private Queue<UI_ItemNoti> _activeNotis = new Queue<UI_ItemNoti>();
    private Queue<GameObject> _activeExpNotis = new Queue<GameObject>();
    private Queue<IEnumerator> _coroutineQueue = new Queue<IEnumerator>();
    private bool _isCoroutineRunning = false;
    Coroutine _basicNotiCoroutine;
    public override void Init()
    {
        if (_itemNotiPanel == null) { _itemNotiPanel = transform.Find("ItemNotiPanel"); }
        _levelNotiPanel.gameObject.SetActive(false);
        _bossKillNotiPanel.gameObject.SetActive(false);
        _basicNotiPanel.gameObject.SetActive(false);
    }

    public void ShowItemNoti(Item item)
    {
        if (_activeNotis.Count >= _maxNotis)
        {
            var oldestNoti = _activeNotis.Dequeue();
            StartCoroutine(FadeOutAndRemove(oldestNoti.gameObject));
        }

        Managers.Data.ItemDict.TryGetValue(item.TemplateId, out Data.ItemData itemData);
        Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);

        if (_activeNotis.Count > 0)
        {
            // 기존 알림을 미리 슬라이드 업
            StartCoroutine(SlideUpNotis(() =>
            {
                // 새로운 알림 생성
                CreateNewItemNoti(icon, itemData.name, item.Count);
            }));
        }
        else
        {
            // 기존 알림이 없으면 바로 새로운 알림 생성
            CreateNewItemNoti(icon, itemData.name, item.Count);
        }
    }
    public void ShowBossKillNoti(string bossName)
    {
        _bossKillNotiPanel.gameObject.SetActive(true);
        TMP_Text notiText = _bossKillNotiPanel.GetComponentInChildren<TMP_Text>();
        notiText.text = $"{bossName} 처치!";
        StartCoroutine(HideNoti(_bossKillNotiPanel.gameObject));
    }
    public void ShowExpNoti(int exp)
    {
        if (_activeExpNotis.Count >= _maxNotis)
        {
            var oldestNoti = _activeExpNotis.Dequeue();
            StartCoroutine(FadeOutAndRemove(oldestNoti));
        }
        CreateNewExpNoti(exp.ToString());
        if(_activeExpNotis.Count > 0)
        {
            StartCoroutine(HideNoti(_activeExpNotis.Dequeue()));
        }
    }
    public void ShowLevelNoti()
    {
        _levelNotiPanel.gameObject.SetActive(true);
        StartCoroutine(HideNoti(_levelNotiPanel.gameObject));
    }
    public void ShowQuestStartNoti(string questName)
    {
        if (_questNoti != null)
        {
            string text = $"{questName}";
            EnqueueCoroutine(ShowAndHideQuestNoti(text));
        }
    }
    public void ShowQuestCompleteNoti(string questName)
    {
        if (_questNoti != null)
        {
            string text = $"완료:{questName} ";
            EnqueueCoroutine(ShowAndHideQuestNoti(text));
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
            IEnumerator coroutine = _coroutineQueue.Dequeue();
            yield return StartCoroutine(coroutine);
        }
        _isCoroutineRunning = false;
    }
    private IEnumerator ShowAndHideQuestNoti(string text)
    {
        _questNoti.SetText(text);
        yield return StartCoroutine(ShowNoti(_questNoti.gameObject)); // 알림을 표시하는 시간
        yield return new WaitForSeconds(2f); // 알림이 표시되는 시간
        yield return StartCoroutine(HideNoti(_questNoti.gameObject)); // 알림을 숨기는 시간
    }
    public void ShowBasicNoti(string script)
    {
        // 기존 코루틴이 실행 중이면 중지
        if (_basicNotiCoroutine != null)
        {
            StopCoroutine(_basicNotiCoroutine);
        }
        _basicNotiPanel.gameObject.SetActive(true);
        TMP_Text notiText = _basicNotiPanel.GetComponentInChildren<TMP_Text>();
        notiText.text = $"{script}";

        _basicNotiCoroutine = StartCoroutine(ShowAndHideBasicNoti(notiText));
    }
    private void CreateNewExpNoti(string exp)
    {
        var expNoti = Managers.Resource.Instantiate("UI/Popup/UI_ExpNoti", _expNotiPanel);
        expNoti.GetComponent<TMP_Text>().text = $"+{exp} Exp";
        expNoti.GetComponent<RectTransform>().anchoredPosition = new Vector2(
            Random.Range(0, _expNotiPanel.GetComponent<RectTransform>().rect.width),
            Random.Range(0, _expNotiPanel.GetComponent<RectTransform>().rect.height)
        );
        _activeExpNotis.Enqueue(expNoti);
    }
    private void CreateNewItemNoti(Sprite icon, string itemName, int itemCount)
    {
        var newItemNoti = Managers.Resource.Instantiate("UI/Popup/UI_ItemNoti", _itemNotiPanel);
        UI_ItemNoti ui = newItemNoti.GetComponent<UI_ItemNoti>();
        ui.ShowItemNoti(icon, itemName, itemCount);
        _activeNotis.Enqueue(ui);
    }

    private IEnumerator SlideUpNotis(System.Action onComplete)
    {
        float slideDuration = 0.5f;
        float elapsedTime = 0f;

        List<Vector3> startPositions = new List<Vector3>();
        List<Vector3> endPositions = new List<Vector3>();

        foreach (var noti in _activeNotis)
        {
            Vector3 startPos = noti.transform.localPosition;
            Vector3 endPos = startPos + Vector3.up * _notiHeight;
            startPositions.Add(startPos);
            endPositions.Add(endPos);
        }

        while (elapsedTime < slideDuration)
        {
            elapsedTime += Time.deltaTime;
            for (int i = 0; i < _activeNotis.Count; i++)
            {
                if (i >= startPositions.Count || i >= endPositions.Count)
                    continue;
                var noti = _activeNotis.ToArray()[i];
                noti.transform.localPosition = Vector3.Lerp(startPositions[i], endPositions[i], elapsedTime / slideDuration);
            }
            yield return null;
        }

        for (int i = 0; i < _activeNotis.Count; i++)
        {
            if (i >= endPositions.Count)
                continue;
            var noti = _activeNotis.ToArray()[i];
            noti.transform.localPosition = endPositions[i];
        }

        onComplete?.Invoke();
    }
    private IEnumerator ShowNoti(GameObject noti)
    {
        noti.SetActive(true);
        CanvasGroup canvasGroup = noti.GetComponent<CanvasGroup>();
        float fadeDuration = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            yield return null;
        }
    }
    private IEnumerator HideNoti(GameObject noti)
    {
        CanvasGroup canvasGroup = noti.GetComponent<CanvasGroup>();
        yield return new WaitForSeconds(2f);
        float fadeDuration = 1f; // 서서히 사라지는 시간
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            yield return null;
        }
        noti.SetActive(false);
    }
    private IEnumerator ShowAndHideBasicNoti(TMP_Text notiText)
    {
        yield return StartCoroutine(ShowTextNoti(notiText));
        yield return new WaitForSeconds(2f); // 알림이 표시되는 시간
        yield return StartCoroutine(HideTextNoti(notiText));
        _basicNotiPanel.gameObject.SetActive(false);
        _basicNotiCoroutine = null; // 코루틴 완료 후 null로 설정
    }
    private IEnumerator ShowTextNoti(TMP_Text text)
    {
        float fadeDuration = 1f;
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            text.alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            yield return null;
        }
    }
    private IEnumerator HideTextNoti(TMP_Text text)
    {
        float fadeDuration = 1f;
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            text.alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            yield return null;
        }
    }
    private IEnumerator FadeOutAndRemove(GameObject noti)
    {
        CanvasGroup canvasGroup = noti.GetComponent<CanvasGroup>();
        float fadeDuration = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            yield return null;
        }

        Destroy(noti.gameObject);
    }
}