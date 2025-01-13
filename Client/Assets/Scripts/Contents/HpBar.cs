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
    [SerializeField]
    SpriteRenderer _hpBarRenderer = null;
    private int _currentHp = 0;
    private int _maxHp = 0;
    private int _colorChangeThreshold = 2000;
    private int _colorChangeCount = 0;

    public void SetHpBar(float ratio)
    {
        _currentHp = (int)(_maxHp * ratio);

        ratio = Mathf.Clamp(ratio, 0, 1);
        _hpBar.localScale = new Vector3(ratio, 1, 1);

        UpdateHpBarColor();
    }

    public void InitializeFrame(float maxHp)
    {
        if (_originalSize == Vector3.zero)
        {
            _originalSize = transform.localScale;
        }
        _hpBarRenderer = _hpBar.GetComponentInChildren<SpriteRenderer>();
        _maxHp = (int)maxHp;
        float xScale = Mathf.Min(_originalSize.x * _maxHp / 1000, _originalSize.x * 2);
        _hpGroup.localScale = new Vector3(xScale, 1, 1);
    }

    private void UpdateHpBarColor()
    {
        int colorIndex = _currentHp / _colorChangeThreshold;
        Color newColor = Content.GetColorByIndex(colorIndex);
        _hpBarRenderer.color = newColor;

        if ((_currentHp % _colorChangeThreshold == 0 && _currentHp != 0)|| _colorChangeCount <(_maxHp/_colorChangeThreshold - colorIndex))
        {
            _hpBar.localScale = new Vector3(1, 1, 1);
            _colorChangeCount++;
        }
        else if (_currentHp % _colorChangeThreshold != 0 && _currentHp > _colorChangeThreshold)
        {
            int remainingHp = _currentHp % _colorChangeThreshold;
            float ratio = (float)remainingHp / _colorChangeThreshold;
            if (_colorChangeCount == 0)
            {
                ratio = (float)remainingHp / (_maxHp % _colorChangeThreshold);
            }
            
            _hpBar.localScale = new Vector3(ratio, 1, 1);
        }
    }
}
