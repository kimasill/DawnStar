using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_QuestNoti : UI_Base
{
    enum Texts
    {
        QuestTitle
    }

    TMP_Text _questNotiText;
    Queue<IEnumerator> _coroutineQueue = new Queue<IEnumerator>();
    bool _isCoroutineRunning = false;

    public override void Init()
    {
        Bind<TMP_Text>(typeof(Texts));
        _questNotiText = GetTextMeshPro((int)Texts.QuestTitle);
        gameObject.SetActive(false);
    }
    public void SetText(string text)
    {
        if (_questNotiText != null)
        {
            _questNotiText.text = text;
        }
    }
}