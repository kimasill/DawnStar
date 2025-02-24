using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;


public class UI_SelectElement : UI_Base, IPointerClickHandler
{
    private TMP_Text _text;
    private Action _clickAction;

    enum Texts
    {
        SelectElement_Text,
    }
    public override void Init()
    {
        Bind<TMP_Text>(typeof(Texts));
        _text = GetTextMeshPro((int)Texts.SelectElement_Text);
        gameObject.BindEvent(OnPointerClick);
    }

    public void SetInfo(string text, Action action)
    {
        _text.text = text;
        _clickAction = action;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _clickAction?.Invoke();
    }
}
