using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_ExpBar : UI_Base
{
    [SerializeField]
    RectTransform _expBar = null;

    public override void Init()
    {
        
    }

    public void SetUIExpBar(float ratio)
    {
        ratio = Mathf.Clamp(ratio, 0, 1);
        _expBar.localScale = new Vector3(ratio, 1, 1);
        _expBar.anchoredPosition = new Vector2(-_expBar.rect.width * (1 - ratio) / 2, 0);
    }
}
