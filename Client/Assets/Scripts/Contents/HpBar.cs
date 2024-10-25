using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpBar : MonoBehaviour
{
    Vector3 _originalSize = Vector3.zero;
    [SerializeField]
    Transform _hpBar = null;
    [SerializeField]
    Transform _hpGroup = null;
    public void SetHpBar(float ratio)
    {
        ratio = Mathf.Clamp(ratio, 0, 1);
        _hpBar.localScale = new Vector3(ratio, 1, 1);
    }

    public void InitializeFrame(float ratio)
    {
        if (_originalSize == Vector3.zero)
        {
            _originalSize = transform.localScale;
        }

        float xScale = _originalSize.x * ratio/1000;
        _hpGroup.localScale = new Vector3(xScale, 1, 1);
    }
}
