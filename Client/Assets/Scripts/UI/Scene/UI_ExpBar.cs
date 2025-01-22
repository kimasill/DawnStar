using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_ExpBar : UI_Base
{
    [SerializeField]
    RectTransform _expBar = null;
    [SerializeField]
    TMP_Text _expText = null;

    public float CurrentRatio { get; private set; } = 1;
    private Coroutine _expBarCoroutine;

    public void SetUIExpBar(float ratio)
    {
        CurrentRatio = ratio;
        ratio = Mathf.Clamp(ratio, 0, 1);
        _expBar.localScale = new Vector3(ratio, 1, 1);
        _expBar.anchoredPosition = new Vector2(-_expBar.rect.width * (1 - ratio) / 2, 0);
    }

    public void UpdateExpBar(float targetRatio, int currentExp, int maxExp)
    {
        if (_expBarCoroutine != null)
        {
            StopCoroutine(_expBarCoroutine);
        }
        _expBarCoroutine = StartCoroutine(LerpExpBar(targetRatio, currentExp, maxExp));
    }

    private IEnumerator LerpExpBar(float targetRatio, int currentExp, int maxExp)
    {
        float startRatio = CurrentRatio;
        float elapsedTime = 0f;
        float duration = 0.5f; // Lerp duration

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newRatio = Mathf.Lerp(startRatio, targetRatio, elapsedTime / duration);
            SetUIExpBar(newRatio);
            _expText.text = $"Exp:{Mathf.RoundToInt(newRatio * maxExp)}/{maxExp}";
            yield return null;
        }

        SetUIExpBar(targetRatio);
        _expText.text = $"Exp:{currentExp}/{maxExp}";
    }

    public override void Init()
    {
        _expText = GetComponentInChildren<TMP_Text>();
        _expText.gameObject.SetActive(false);
        _expBar.gameObject.BindEvent((PointerEventData data) => OnPointerEnter(data), Define.UIEvent.MouseOver);
        _expBar.gameObject.BindEvent((PointerEventData data) => OnPointerExit(data), Define.UIEvent.MouseOut);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        _expText.gameObject.SetActive(true);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        _expText.gameObject.SetActive(false);
    }
}