using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UI_Inventory : UI_Base
{
    public List<UI_Inventory_Item> Items { get; } = new List<UI_Inventory_Item>();

    [SerializeField]
    public GameObject grid = null;
    public ScrollRect ScrollRect { get; private set; }
    UI_GameScene _gameScene = null;
    float _contentSizeY = 0;
    bool _isInit = false;
    public override void Init()
    {
        if (_isInit)
            return;
        _isInit = true;
        Items.Clear();

        ScrollRect = GetComponentInChildren<ScrollRect>();
        foreach (Transform child in grid.transform)
        {
            Destroy(child.gameObject);
        }
        _gameScene = Managers.UI.SceneUI as UI_GameScene;
        for (int i = 0; i < 150; i++)
        {
            GameObject go = Managers.Resource.Instantiate("UI/Scene/UI_Inventory_Item", grid.transform);
            UI_Inventory_Item item = go.GetOrAddComponent<UI_Inventory_Item>();
            item.GameWindow = _gameScene.GameWindow;
            Items.Add(item);            
        }
        // ContentSizeFitter와 VerticalLayoutGroup 추가
        var contentSizeFitter = grid.GetOrAddComponent<ContentSizeFitter>();
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        _contentSizeY = ScrollRect.content.sizeDelta.y;

        RefreshUI();

        GridLayoutGroup gridLayoutGroup = grid.GetComponent<GridLayoutGroup>();
        if (gridLayoutGroup != null)
        {
            RectTransform contentRect = ScrollRect.content;
            RectTransform scrollRectTransform = ScrollRect.GetComponent<RectTransform>();
            int constCount = (int)(scrollRectTransform.rect.width / (gridLayoutGroup.cellSize.x + gridLayoutGroup.spacing.x));
            int rowCount = Mathf.CeilToInt((float)Items.Count / constCount);
            float height = rowCount * (gridLayoutGroup.cellSize.y + gridLayoutGroup.spacing.y) - gridLayoutGroup.spacing.y;
            
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, height);
        }
    }    
    public void RefreshUI()
    {
        if (_isInit == false)
        {
            Init();
            return;
        }
            
        if(Items.Count == 0)
        {
            return;
        }
        List<Item> items = Managers.Inventory.Items.Values.ToList();
        items.Sort((left, right) => { return left.Slot - right.Slot; });

        foreach (Item item in items)
        {
            if (item.Slot < 0 || item.Slot >= 150 )
                continue;
            Items[item.Slot].SetItem(item);
        }
        foreach (UI_Inventory_Item item in Items)
        {
            if (Managers.Inventory.Get(item.ItemDbId) == null)
            {
                item.SetItem(null);
            }
        }
        UpdateContentSize();
    }
    private void UpdateContentSize()
    {
        if (ScrollRect != null && grid != null)
        {
            RectTransform contentRect = ScrollRect.content;
            RectTransform gridRect = grid.GetComponent<RectTransform>();
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, gridRect.rect.height);
        }
    }
    public void InvenScroll(float scrollDelta)
    {
        if (ScrollRect != null)
        {
            float scrollAmount = -scrollDelta * 0.1f; // 스크롤 속도 조절
            ScrollRect.verticalNormalizedPosition = Mathf.Clamp01(ScrollRect.verticalNormalizedPosition - scrollAmount);
        }
    }

    public void ScrollToTop()
    {
        if (ScrollRect != null)
        {
            ScrollRect.verticalNormalizedPosition = 1.0f;
        }
    }

    public void ScrollToBottom()
    {
        if (ScrollRect != null)
        {
            ScrollRect.verticalNormalizedPosition = 0.0f;
        }
    }
}
