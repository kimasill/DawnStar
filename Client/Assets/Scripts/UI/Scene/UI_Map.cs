using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class UI_Map : UI_Base
{
    private RectTransform _mapRectTransform;
    private Vector2 _originalSize;

    public override void Init()
    {
        _mapRectTransform = GetComponent<RectTransform>();
        _originalSize = _mapRectTransform.sizeDelta;
    }

    public void ResetMapSize()
    {
        _mapRectTransform.sizeDelta = _originalSize;
    }

    public void ZoomMap(float delta)
    {
        _mapRectTransform.sizeDelta += new Vector2(delta, delta);
    }
}