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
        NameText,
        LevelText,
        ExpText,
    }
    enum Images
    {
        Profile_Image,
    }
    bool _init = false;
    public bool Set = false;
    ExpBar _expBar;
    public override void Init()
    {
        Bind<TMP_Text>(typeof(Texts));
        Bind<Image>(typeof(Images));
        _expBar = GetComponentInChildren<ExpBar>();
        _init = true;        
    }

    public void SetInfo()
    {
        _myPlayer = Managers.Object.MyPlayer;
        Managers.Data.StatDict.TryGetValue(_myPlayer.Stat.Level, out StatData nextStat);
        if (nextStat != null)
        {
            _nextExp = nextStat.TotalExp;
        }

        Managers.Data.StatDict.TryGetValue(_myPlayer.Stat.Level - 1, out StatData prevStat);
        if (prevStat != null)
        {
            _prevExp = prevStat.TotalExp;
        }
    }

    public void UpdateExpBar()
    {
        if (_expBar == null)
            return;
        float ratio = 0.0f;
        if (_myPlayer.Stat.TotalExp > 0)
        {
            int nextExp = _nextExp - _prevExp;
            ratio = ((float)(_myPlayer.Stat.TotalExp - _prevExp) / nextExp);
        }
        _expBar.SetExpBar(ratio);
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
