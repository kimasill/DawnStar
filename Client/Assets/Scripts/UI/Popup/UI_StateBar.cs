using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Item;

public class UI_StateBar : UI_Base
{
    MyPlayerController _myPlayer;
    int _nextExp = 0;
    int _prevExp = 0;
    enum Texts
    {
        LevelText,
        ExpValue_Text,
        HpValue_Text,
        UpValue_Text
    }
    bool _init = false;
    public bool Set = false;
    UI_ExpBar _expBar;
    UI_HpBar _hpBar;
    UI_UpBar _upBar;
    RectTransform _hpBarRect;
    RectTransform _upBarRect;
    Coroutine _hpBarCoroutine;
    Coroutine _upBarCoroutine;
    Coroutine _expBarCoroutine;
    TMP_Text _upText;
    TMP_Text _hpText;
    TMP_Text _expText;
    public override void Init()
    {
        Bind<TMP_Text>(typeof(Texts));
        _expBar = GetComponentInChildren<UI_ExpBar>();
        _hpBar = GetComponentInChildren<UI_HpBar>();
        _upBar = GetComponentInChildren<UI_UpBar>();
        _hpBarRect = _hpBar.GetComponent<RectTransform>();
        _upBarRect = _upBar.GetComponent<RectTransform>();
        _upText = GetTextMeshPro((int)Texts.UpValue_Text);
        _hpText = GetTextMeshPro((int)Texts.HpValue_Text);
        _expText = GetTextMeshPro((int)Texts.ExpValue_Text);
        _upText.text = "0/0";
        _upText.gameObject.SetActive(false);        
        _hpText.text = "0/0";
        _hpText.gameObject.SetActive(false);
        _expText.gameObject.SetActive(false);
        _hpBar.gameObject.BindEvent((PointerEventData data) => OnPointerEnter(data, _hpText), Define.UIEvent.MouseOver);
        _hpBar.gameObject.BindEvent((PointerEventData data) => OnPointerExit(data, _hpText), Define.UIEvent.MouseOut);
        _upBar.gameObject.BindEvent((PointerEventData data) => OnPointerEnter(data, _upText), Define.UIEvent.MouseOver);
        _upBar.gameObject.BindEvent((PointerEventData data) => OnPointerExit(data, _upText), Define.UIEvent.MouseOut);
        _expBar.gameObject.BindEvent((PointerEventData data) => OnPointerEnter(data, _expText), Define.UIEvent.MouseOver);
        _expBar.gameObject.BindEvent((PointerEventData data) => OnPointerExit(data, _expText), Define.UIEvent.MouseOut);
        _init = true;
    }

    public void SetInfo()
    {
        _myPlayer = Managers.Object.MyPlayer;
        Managers.Data.StatDict.TryGetValue(_myPlayer.Stat.Level + 1, out StatData nextStat);
        if (nextStat != null)
        {
            _nextExp = nextStat.TotalExp;
        }

        Managers.Data.StatDict.TryGetValue(_myPlayer.Stat.Level, out StatData prevStat);
        if (prevStat != null)
        {
            _prevExp = prevStat.TotalExp;
        }
        RefreshUI();
    }
    public void UpdateExpBar()
    {
        if (_expBar == null)
            return;
        float targetRatio = 0.0f;
        if (_myPlayer.Stat.TotalExp > 0)
        {
            int nextExp = _nextExp - _prevExp;
            targetRatio = (float)(_myPlayer.Stat.TotalExp - _prevExp) / nextExp;
        }
        if (_expBarCoroutine != null)
        {
            StopCoroutine(_expBarCoroutine);
        }
        _expBarCoroutine = StartCoroutine(LerpExpBar(targetRatio));
    }
    public void UpdateHpBar()
    {
        if (_hpBar == null)
            return;
        if (_myPlayer == null)
        {
            _myPlayer = Managers.Object.MyPlayer;
        }
        float targetRatio = 0.0f;
        if (_myPlayer.Stat.MaxHp > 0)
        {
            targetRatio = ((float)_myPlayer.Stat.Hp / _myPlayer.Stat.MaxHp);
        }
        _hpBar.InitializeFrame(_myPlayer.Stat.MaxHp);
        if (_hpBarCoroutine != null)
        {
            StopCoroutine(_hpBarCoroutine);
        }
        _hpBarCoroutine = StartCoroutine(LerpHpBar(targetRatio));
    }
    public void UpdateUpBar()
    {
        if (_upBar == null) return;
        if (_myPlayer == null)
        {
            _myPlayer = Managers.Object.MyPlayer;
        }
        float targetRatio = 0.0f;
        if (_myPlayer.Stat.MaxUp > 0)
        {
            targetRatio = ((float)_myPlayer.Stat.Up / _myPlayer.Stat.MaxUp);
        }
        _upBar.InitializeFrame(_myPlayer.Stat.MaxUp);
        if (_upBarCoroutine != null)
        {
            StopCoroutine(_upBarCoroutine);
        }
        _upBarCoroutine = StartCoroutine(LerpUpBar(targetRatio));
    }
    private IEnumerator LerpHpBar(float targetRatio)
    {
        float startRatio = _hpBar.CurrentRatio;
        float elapsedTime = 0f;
        float duration = 0.5f; // Lerp duration

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newRatio = Mathf.Lerp(startRatio, targetRatio, elapsedTime / duration);
            _hpBar.SetHpBar(newRatio);
            _hpText.text = $"{Mathf.RoundToInt(newRatio * _myPlayer.TotalHp)},{_myPlayer.TotalHp}";
            yield return null;
        }

        _hpBar.SetHpBar(targetRatio);
        _hpText.text = $"{_myPlayer.Hp},{_myPlayer.TotalHp}";
    }
    private IEnumerator LerpUpBar(float targetRatio)
    {
        float startRatio = _upBar.CurrentRatio;
        float elapsedTime = 0f;
        float duration = 0.5f; // Lerp duration
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newRatio = Mathf.Lerp(startRatio, targetRatio, elapsedTime / duration);
            _upBar.SetUpBar(newRatio);
            _upText.text = $"{Mathf.RoundToInt(newRatio * _myPlayer.TotalUp)}/{_myPlayer.TotalUp}";
            yield return null;
        }
        _upBar.SetUpBar(targetRatio);
        _upText.text = $"{_myPlayer.Up}/{_myPlayer.TotalUp}";
    }
    private IEnumerator LerpExpBar(float targetRatio)
    {
        float startRatio = _expBar.CurrentRatio;
        float elapsedTime = 0f;
        float duration = 0.5f; // Lerp duration
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newRatio = Mathf.Lerp(startRatio, targetRatio, elapsedTime / duration);
            _expBar.SetUIExpBar(newRatio);
            _expText.text = $"Exp:{Mathf.RoundToInt(newRatio * (_nextExp - _prevExp))}/{_nextExp - _prevExp}";
            yield return null;
        }
        _expBar.SetUIExpBar(targetRatio);
        _expText.text = $"Exp:{_myPlayer.Stat.TotalExp - _prevExp}/{_nextExp - _prevExp}";
    }
    public void RefreshUI()
    {
        if (_init == false)
            return;
        UpdateExpBar();
        UpdateHpBar();
        UpdateUpBar();
        GetTextMeshPro((int)Texts.LevelText).text = $"Lv. {_myPlayer.Stat.Level}";
    }
    private void OnPointerEnter(PointerEventData eventData, TMP_Text text)
    {
        text.gameObject.SetActive(true);
    }

    private void OnPointerExit(PointerEventData eventData, TMP_Text text)
    {
        text.gameObject.SetActive(false);
    }
}
