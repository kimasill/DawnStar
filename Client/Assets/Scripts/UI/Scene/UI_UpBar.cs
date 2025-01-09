using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_UpBar : UI_Base
{
    Vector3 _originalSize = Vector3.zero;
    [SerializeField]
    RectTransform _upBar = null;
    [SerializeField]
    RectTransform _upGroup = null;
    [SerializeField]
    TMP_Text _upText = null;

    public float CurrentRatio { get; private set; } = 1;
    public void SetUpBar(float ratio)
    {
        CurrentRatio = ratio;
        ratio = Mathf.Clamp(ratio, 0, 1);
        _upBar.localScale = new Vector3(ratio, 1, 1);
        _upBar.anchoredPosition = new Vector2(-_upBar.rect.width * (1 - ratio) / 2, 0);
    }

    public void InitializeFrame(float maxUp)
    {
        if (_originalSize == Vector3.zero)
            _originalSize = transform.localScale;

        // HP 비율에 따라 HpGroup 오브젝트의 크기를 조정
        if (maxUp > 500)
        {
            float xScale = _originalSize.x * maxUp / 500;
            _upGroup.localScale = new Vector3(xScale, 1, 1);

            _upGroup.anchoredPosition = new Vector2(_upGroup.rect.size.x, _upGroup.anchoredPosition.y);
        }
    }

    public override void Init()
    {
        _upGroup = GetComponent<RectTransform>();
    }
}
