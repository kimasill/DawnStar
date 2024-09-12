// UI_Map.cs

using System.Collections.Generic;
using UnityEngine;

public class UI_Map : UI_Base
{
    private RectTransform _mapRectTransform;
    private Vector2 _originalSize;
    private List<GameObject> _icons;

    public override void Init()
    {
        _mapRectTransform = GetComponent<RectTransform>();
        _originalSize = _mapRectTransform.sizeDelta;

        // 아이콘 GameObject 수집
        _icons = new List<GameObject>();
        foreach (Transform child in transform)
        {
            if (child.CompareTag("MapIcon")) // 아이콘에 "MapIcon" 태그를 설정
            {
                _icons.Add(child.gameObject);
            }
        }
    }

    public void ResetMapSize()
    {
        _mapRectTransform.sizeDelta = _originalSize;
        foreach (var icon in _icons)
        {
            icon.SetActive(true); // 아이콘 다시 활성화
        }
    }

    public void ZoomMap(float delta)
    {
        _mapRectTransform.sizeDelta += new Vector2(delta, delta);
        foreach (var icon in _icons)
        {
            icon.SetActive(false); // 아이콘 비활성화
        }
    }
}