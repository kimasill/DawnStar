using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_BossHpBar : UI_Base
{
    [SerializeField]
    public UI_HpBar HPBar = null;

    public TMP_Text Name;

    enum Texts
    {
        BossNameText,
    }
    public override void Init()
    {
        Bind<TMP_Text>(typeof(Texts));
        Name = GetTextMeshPro((int)Texts.BossNameText);
        HPBar = GetComponentInChildren<UI_HpBar>();
    }
}
