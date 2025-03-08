using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_UpBar : UI_Base
{
    Vector3 _originalSize = Vector3.zero;
    [SerializeField]
    RectTransform _upBar = null;
    [SerializeField]
    RectTransform _upGroup = null;
    [SerializeField]
    TMP_Text _upText = null;

    public float CurrentRatio { get; private set; } = 1;
    private Coroutine _upBarCoroutine;

    public void SetUpBar(float ratio)
    {
        CurrentRatio = ratio;
        ratio = Mathf.Clamp(ratio, 0, 1);
        _upBar.localScale = new Vector3(ratio, 1, 1);
        _upBar.anchoredPosition = new Vector2(-_upBar.rect.width * (1 - ratio) / 2, 0);
    }

    public void InitializeFrame(float maxUp)
    {
        if (_originalSize == Vector3.zero)
            _originalSize = transform.localScale;

        // UP 비율에 따라 UpGroup 오브젝트의 크기를 조정
        if (maxUp > 500)
        {
            float xScale = _originalSize.x * maxUp / 500;
            _upGroup.localScale = new Vector3(xScale, _originalSize.y, _originalSize.z);

            _upGroup.anchoredPosition = new Vector2(_upGroup.rect.size.x / 2, _upGroup.anchoredPosition.y);
        }
    }

    public void UpdateUpBar(float targetRatio, int currentUp, int maxUp)
    {
        if (gameObject.activeSelf == false || gameObject == null)
        {
            return;
        }
        InitializeFrame(maxUp);
        if (_upBarCoroutine != null)
        {
            StopCoroutine(_upBarCoroutine);
        }
        _upBarCoroutine = StartCoroutine(LerpUpBar(targetRatio, currentUp, maxUp));
    }

    private IEnumerator LerpUpBar(float targetRatio, int currentUp, int maxUp)
    {
        float startRatio = CurrentRatio;
        float elapsedTime = 0f;
        float duration = 0.5f; // Lerp duration

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newRatio = Mathf.Lerp(startRatio, targetRatio, elapsedTime / duration);
            SetUpBar(newRatio);
            _upText.text = $"{Mathf.RoundToInt(newRatio * maxUp)}/{maxUp}";
            yield return null;
        }

        SetUpBar(targetRatio);
        _upText.text = $"{currentUp}/{maxUp}";
    }

    public override void Init()
    {
        _upGroup = GetComponent<RectTransform>();
        _upText = GetComponentInChildren<TMP_Text>();
        _upText.gameObject.SetActive(false);
        _upGroup.gameObject.BindEvent((PointerEventData data) => OnPointerEnter(data), Define.UIEvent.MouseOver);
        _upGroup.gameObject.BindEvent((PointerEventData data) => OnPointerExit(data), Define.UIEvent.MouseOut);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        _upText.gameObject.SetActive(true);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        _upText.gameObject.SetActive(false);
    }
}
