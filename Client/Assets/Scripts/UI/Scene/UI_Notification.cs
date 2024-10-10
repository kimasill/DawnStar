using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Notification : UI_Base
{
    [SerializeField] private Transform _itemNotiPanel;
    [SerializeField] private int _maxNotis = 8;
    [SerializeField] private int _notiHeight = 50;

    private Queue<UI_ItemNoti> _activeNotis = new Queue<UI_ItemNoti>();

    public override void Init()
    {
        if (_itemNotiPanel == null) { _itemNotiPanel = transform.Find("ItemNotiPanel"); }
    }

    public void ShowItemNoti(Item item)
    {
        if (_activeNotis.Count >= _maxNotis)
        {
            var oldestNoti = _activeNotis.Dequeue();
            StartCoroutine(FadeOutAndRemove(oldestNoti));
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
                var noti = _activeNotis.ToArray()[i];
                noti.transform.localPosition = Vector3.Lerp(startPositions[i], endPositions[i], elapsedTime / slideDuration);
            }
            yield return null;
        }

        for (int i = 0; i < _activeNotis.Count; i++)
        {
            var noti = _activeNotis.ToArray()[i];
            noti.transform.localPosition = endPositions[i];
        }

        onComplete?.Invoke();
    }

    private IEnumerator FadeOutAndRemove(UI_ItemNoti noti)
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