using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
        ExpText,
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
    public override void Init()
    {
        Bind<TMP_Text>(typeof(Texts));
        _expBar = GetComponentInChildren<UI_ExpBar>();
        _hpBar = GetComponentInChildren<UI_HpBar>();
        _upBar = GetComponentInChildren<UI_UpBar>();
        _hpBarRect = _hpBar.GetComponent<RectTransform>();
        _upBarRect = _upBar.GetComponent<RectTransform>();
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
        float ratio = 0.0f;
        if (_myPlayer.Stat.TotalExp > 0)
        {
            int nextExp = _nextExp - _prevExp;
            ratio = (float)(_myPlayer.Stat.TotalExp - _prevExp) / nextExp;
        }
        _expBar.SetUIExpBar(ratio);
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
            _hpBar.SetHpText(Mathf.RoundToInt(newRatio * _myPlayer.Stat.MaxHp), _myPlayer.Stat.MaxHp);
            yield return null;
        }

        _hpBar.SetHpBar(targetRatio);
        _hpBar.SetHpText(_myPlayer.Stat.Hp, _myPlayer.Stat.MaxHp);
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
            _upBar.SetUpText(Mathf.RoundToInt(newRatio * _myPlayer.Stat.MaxUp), _myPlayer.Stat.MaxUp);
            yield return null;
        }
        _upBar.SetUpBar(targetRatio);
        _upBar.SetUpText(_myPlayer.Stat.Up, _myPlayer.Stat.MaxUp);
    }
    public void RefreshUI()
    {
        if (_init == false)
            return;
        UpdateExpBar();
        GetTextMeshPro((int)Texts.ExpText).text = $"Exp:{_myPlayer.Stat.TotalExp - _prevExp}";
        GetTextMeshPro((int)Texts.LevelText).text = $"Lv. {_myPlayer.Stat.Level}";
    }
}
