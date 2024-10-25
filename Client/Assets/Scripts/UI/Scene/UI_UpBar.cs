using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_UpBar : UI_Base
{
    Vector3 _originalSize = Vector3.zero;
    [SerializeField]
    RectTransform _upBar = null;
    [SerializeField]
    RectTransform _upGroup = null;
    public void SetUpBar(float ratio)
    {
        ratio = Mathf.Clamp(ratio, 0, 1);
        _upBar.localScale = new Vector3(ratio, 1, 1);
    }

    public void InitializeFrame(float ratio)
    {
        if (_originalSize == Vector3.zero)
            _originalSize = transform.localScale;

        // HP 비율에 따라 HpGroup 오브젝트의 크기를 조정
        float xScale = _originalSize.x * ratio / 1000;
        _upGroup.localScale = new Vector3(xScale, 1, 1);

        // HpGroup의 위치를 조정하여 x 방향으로 이동
        float xOffset = (xScale - _originalSize.x) / 2;
        _upGroup.anchoredPosition = new Vector2(_originalSize.x + xOffset, _upGroup.anchoredPosition.y);
    }

    public override void Init()
    {
        _upGroup = GetComponent<RectTransform>();
    }
}
