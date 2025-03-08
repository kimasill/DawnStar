using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Select : UI_Popup
{

    [SerializeField] private GameObject _selectPanel;
    private List<string> _elems = new List<string>();
    private List<Action> _actions = new List<Action>();

    enum Images
    {
        FrontPanel,
        ExitButton
    }

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        GetImage((int)Images.ExitButton).gameObject.BindEvent(OnCloseButtonClick);
    }

    public void OpenUI(PointerEventData evt, List<string> elems, List<Action> actions)
    {    
        _elems = elems;
        _actions = actions;
        UpdatePopupPosition(_selectPanel.transform, evt);
        RefreshUI();
    }

    private void RefreshUI()
    {
        GameObject container = GetImage((int)Images.FrontPanel).gameObject;
        foreach (Transform child in container.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < _elems.Count; i++)
        {
            AddSelectElem(_elems[i], _actions[i], container);
        }
    }

    private void AddSelectElem(string text, Action action, GameObject container)
    {
        GameObject elem = Managers.Resource.Instantiate("UI/UI_SelectElement", container.transform);
        UI_SelectElement selectElem = elem.GetOrAddComponent<UI_SelectElement>();
        selectElem.SetInfo(text, action, this);
    }
    private void OnCloseButtonClick(PointerEventData evt)
    {
        ClosePopupUI();
    }

    public void CloseUI()
    {
        ClosePopupUI();
    }
}