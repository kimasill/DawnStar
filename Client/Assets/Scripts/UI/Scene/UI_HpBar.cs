using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_HpBar : UI_Base
{
    Vector3 _originalSize = Vector3.zero;
    [SerializeField]
    RectTransform _hpBar = null;
    [SerializeField]
    RectTransform _hpGroup = null;
    public void SetHpBar(float ratio)
    {
        ratio = Mathf.Clamp(ratio, 0, 1);
        _hpBar.localScale = new Vector3(ratio, 1, 1);
        _hpBar.anchoredPosition = new Vector2(-_hpBar.rect.width * (1 - ratio) / 2, 0);
    }

    public void InitializeFrame(float maxHp)
    {
        if (_originalSize == Vector3.zero)
            _originalSize = transform.localScale;

        // HP 비율에 따라 HpGroup 오브젝트의 크기를 조정
        if(maxHp > 1000)
        {
            float xScale = _originalSize.x * maxHp / 1000;
            _hpGroup.localScale = new Vector3(xScale, 1, 1);

            _hpGroup.anchoredPosition = new Vector2(_hpGroup.rect.size.x, _hpGroup.anchoredPosition.y);
        }
    }

    public override void Init()
    {
        _hpGroup = GetComponent<RectTransform>();
    }
}
