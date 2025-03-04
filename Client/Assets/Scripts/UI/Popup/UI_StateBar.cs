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
    TMP_Text _expText;
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
        float targetRatio = 0.0f;
        if (_myPlayer.Stat.TotalExp > 0)
        {
            int nextExp = _nextExp - _prevExp;
            targetRatio = (float)(_myPlayer.Stat.TotalExp - _prevExp) / nextExp;
        }
        _expBar.UpdateExpBar(targetRatio, _myPlayer.Stat.TotalExp - _prevExp, _nextExp - _prevExp);
    }
    public void UpdateHpBar()
    {
        if (_hpBar == null || _hpBar.isActiveAndEnabled == false)
            return;
        if (_myPlayer == null)
        {
            _myPlayer = Managers.Object.MyPlayer;
        }
        float targetRatio = 0.0f;
        if (_myPlayer.Stat.MaxHp > 0)
        {
            targetRatio = ((float)_myPlayer.Stat.Hp / _myPlayer.TotalHp);
        }
        _hpBar.UpdateHpBar(targetRatio, _myPlayer.Stat.Hp, _myPlayer.TotalHp, frameControl:true);
    }
    public void UpdateUpBar()
    {
        if (_upBar == null|| _upBar.isActiveAndEnabled == false) 
            return;
        if (_myPlayer == null)
        {
            _myPlayer = Managers.Object.MyPlayer;
        }
        float targetRatio = 0.0f;
        if (_myPlayer.Stat.MaxUp > 0)
        {
            targetRatio = ((float)_myPlayer.Stat.Up / _myPlayer.TotalUp);
        }
        _upBar.UpdateUpBar(targetRatio, _myPlayer.Stat.Up, _myPlayer.TotalUp);
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
}
