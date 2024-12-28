using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class UI_Quantity : UI_Popup
{
    [SerializeField] private InputField CountInputField;
    [SerializeField] private Button NumUpButton;
    [SerializeField] private Button NumDownButton;
    [SerializeField] private Button EnterButton;
    public Action<int> Check;
    private int _count = 1;

    enum Images
    {
        NumUpButton,
        NumDownButton,
        EnterButton,
        ExitButton,
    }

    enum Texts
    {
        CountInputField,
    }

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<TMP_Text>(typeof(Texts));

        GetImage((int)Images.NumUpButton).GetComponent<Button>().gameObject.BindEvent(OnNumUpButtonClicked);
        GetImage((int)Images.NumDownButton).GetComponent<Button>().gameObject.BindEvent(OnNumDownButtonClicked);
        GetImage((int)Images.EnterButton).GetComponent<Button>().gameObject.BindEvent(OnEnterButtonClicked);
        GetImage((int)Images.ExitButton).GetComponent<Button>().gameObject.BindEvent(OnCloseButtonClick);
        CountInputField = GetText((int)Texts.CountInputField).GetComponent<InputField>();
        CountInputField.onEndEdit.AddListener(OnCountInputFieldChanged);
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
        CountInputField.text = _count.ToString();
    }

    private void OnCloseButtonClick(PointerEventData evt)
    {
        ClosePopupUI();
    }
}