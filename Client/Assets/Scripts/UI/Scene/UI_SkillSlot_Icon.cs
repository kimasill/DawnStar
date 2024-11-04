using Data;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_SkillSlot_Icon : UI_Base
{
    [SerializeField] private Image _skillIcon;
    public TMP_Text KeyText;

    enum Texts
    {
        SkillSlot_KeyText
    }
    public SkillData SkillData { get; private set; }

    public void SetSkill(SkillData skillData)
    {
        Sprite skillIcon = Managers.Resource.Load<Sprite>($"{skillData.icon}");
        _skillIcon.sprite = skillIcon;
        gameObject.SetActive(true);
    }

    public void ClearSlot()
    {
        _skillIcon.sprite = null;
        gameObject.SetActive(false);
    }

    public override void Init()
    {
        Bind<TMP_Text>(typeof(Texts));
        KeyText = GetTextMeshPro((int)Texts.SkillSlot_KeyText);
    }
}