using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class UI_Quantity : UI_Popup
{
    [SerializeField] private InputField CountInputField;
    [SerializeField] private Button NumUpButton;
    [SerializeField] private Button NumDownButton;
    [SerializeField] private Button EnterButton;

    private int _count = 1;

    enum Images
    {
        ItemPopup_Image,
        ItemDescriptionImage
    }

    enum Texts
    {
        ItemPopup_Name,
        ItemDescriptionText,
        ItemSkillName,
        ItemSkillDescription,
        ItemPopup_Add,
        ItemPopup_Grade
    }

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<TMP_Text>(typeof(Texts));
        
    }

    private void OnNumUpButtonClicked()
    {
        _count++;
        UpdateCountInputField();
    }

    private void OnNumDownButtonClicked()
    {
        if (_count > 1)
        {
            _count--;
            UpdateCountInputField();
        }
    }

    private void OnEnterButtonClicked()
    {
        // 승인 로직을 여기에 추가하세요.
        Debug.Log($"Entered count: {_count}");
    }

    private void UpdateCountInputField()
    {
        CountInputField.text = _count.ToString();
    }
}