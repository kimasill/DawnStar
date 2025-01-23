using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_HpBar : UI_Base
{
    Vector3 _originalSize = Vector3.zero;
    [SerializeField]
    RectTransform _hpBar = null;
    [SerializeField]
    RectTransform _hpGroup = null;
    [SerializeField]
    TMP_Text _hpText= null;
    enum Texts
    {
        HpValue_Text,
    }

    public float CurrentRatio { get; private set; } = 1;
    private Coroutine _hpBarCoroutine;
    public void SetHpBar(float ratio)
    {
        CurrentRatio = ratio;
        ratio = Mathf.Clamp(ratio, 0, 1);
        _hpBar.localScale = new Vector3(ratio, 1, 1);
        _hpBar.anchoredPosition = new Vector2(-_hpBar.rect.width * (1 - ratio) / 2, 0);
    }

    public void InitializeFrame(float maxHp, bool frameControl)
    {
        if (_originalSize == Vector3.zero)
            _originalSize = transform.localScale;

        // HP 비율에 따라 HpGroup 오브젝트의 크기를 조정
        if(frameControl && maxHp > 1000)
        {
            float xScale = _originalSize.x * maxHp / 1000;
            _hpGroup.localScale = new Vector3(xScale, 1, 1);

            _hpGroup.anchoredPosition = new Vector2(_hpGroup.rect.size.x, _hpGroup.anchoredPosition.y);
        }
    }
    public void UpdateHpBar(float targetRatio, int currentHp, int maxHp, bool frameControl = false)
    {
        InitializeFrame(maxHp, frameControl);
        if (_hpBarCoroutine != null)
        {
            StopCoroutine(_hpBarCoroutine);
        }
        _hpBarCoroutine = StartCoroutine(LerpHpBar(targetRatio, currentHp, maxHp));
    }

    private IEnumerator LerpHpBar(float targetRatio, int currentHp, int maxHp)
    {
        float startRatio = CurrentRatio;
        float elapsedTime = 0f;
        float duration = 0.5f; // Lerp duration

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newRatio = Mathf.Lerp(startRatio, targetRatio, elapsedTime / duration);
            SetHpBar(newRatio);
            _hpText.text = $"{Mathf.RoundToInt(newRatio * maxHp)}/{maxHp}";
            yield return null;
        }

        SetHpBar(targetRatio);
        _hpText.text = $"{currentHp}/{maxHp}";
    }
    public override void Init()
    {
        _hpGroup = GetComponent<RectTransform>();
        Bind<TMP_Text>(typeof(Texts));
        _hpText = GetTextMeshPro((int)Texts.HpValue_Text);
        _hpText.gameObject.SetActive(false);
        
        _hpGroup.gameObject.BindEvent((PointerEventData data) => OnPointerEnter(data), Define.UIEvent.MouseOver);
        _hpGroup.gameObject.BindEvent((PointerEventData data) => OnPointerExit(data), Define.UIEvent.MouseOut);
    }
    public override void OnPointerEnter(PointerEventData eventData)
    {
        _hpText.gameObject.SetActive(true);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        _hpText.gameObject.SetActive(false);
    }
}
