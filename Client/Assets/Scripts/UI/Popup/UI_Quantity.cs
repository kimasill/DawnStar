using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class UI_Quantity : UI_Popup
{
    private TMP_InputField _countInputField;
    [SerializeField] private GameObject _quantityPanel;
    [SerializeField] private Button NumUpButton;
    [SerializeField] private Button NumDownButton;
    [SerializeField] private Button EnterButton;
    public Action<int> Check;
    private int _count = 1;

    enum InputFields
    {
        CountInputField,
    }
    enum Images
    {
        NumUpButton,
        NumDownButton,
        EnterButton,
        ExitButton,
    }

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<TMP_InputField>(typeof(InputFields));

        GetImage((int)Images.NumUpButton).gameObject.BindEvent(OnNumUpButtonClicked);
        GetImage((int)Images.NumDownButton).gameObject.BindEvent(OnNumDownButtonClicked);
        GetImage((int)Images.EnterButton).gameObject.BindEvent(OnEnterButtonClicked);
        GetImage((int)Images.ExitButton).gameObject.BindEvent(OnCloseButtonClick);
        _countInputField = Get<TMP_InputField>((int)InputFields.CountInputField);
        _countInputField.onEndEdit.AddListener(OnCountInputFieldChanged);
    }
    public void OpenUI(PointerEventData eventData)
    {
        UpdatePopupPosition(_quantityPanel.transform, eventData);
    }

    public void CloseUI()
    {
        ClosePopupUI();
    }

    private void OnNumUpButtonClicked(PointerEventData evt)
    {
        _count++;
        UpdateCountInputField();
    }

    private void OnNumDownButtonClicked(PointerEventData evt)
    {
        if (_count > 1)
        {
            _count--;
            UpdateCountInputField();
        }
    }
    private void OnCountInputFieldChanged(string input)
    {
        if (int.TryParse(input, out int newCount) && newCount > 0)
        {
            _count = newCount;
        }
        else
        {
            _count = 1;
        }
        UpdateCountInputField();
    }
    private void OnEnterButtonClicked(PointerEventData evt)
    {
        Check.Invoke(_count);
        ClosePopupUI();
    }

    private void UpdateCountInputField()
    {
        _countInputField.text = _count.ToString();
    }

    private void OnCloseButtonClick(PointerEventData evt)
    {
        CloseUI();
    }
}