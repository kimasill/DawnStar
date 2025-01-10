using static UI_StoryScene;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.Progress;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Data;
using Google.Protobuf.Protocol;
using Unity.VisualScripting;

public class UI_ItemProduction : UI_Base
{
    bool _isInit = false;
    public ScrollRect ScrollRect { get; private set; }
    public List<UI_Display_Item> Items { get; set; } = new List<UI_Display_Item>();
    public List<UI_Display_Item> CostItems { get; set; } = new List<UI_Display_Item>();
    private UI_Enhance _ui_Enhance;
    private Item _selectedItem;
    private RectTransform _productContentRect;

    enum Images
    {
        ItemProduction_CostPanel,
    }
    enum Buttons
    {
        ItemProduction_Button,
    }

    public override void Init()
    {
        if (_isInit)
            return;
        _isInit = true;
        Items.Clear();

        ScrollRect = GetComponentInChildren<ScrollRect>();
        _productContentRect = ScrollRect.content.GetComponent<RectTransform>();
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
        GetButton((int)Buttons.ItemProduction_Button).gameObject.BindEvent(OnClickProductionButton);
        BindEvent(gameObject, OnPointerEnter, Define.UIEvent.MouseOver);
        BindEvent(gameObject, OnPointerExit, Define.UIEvent.MouseOut);
        foreach (Transform child in ScrollRect.content.transform)
        {
            Destroy(child.gameObject);
        }
        UI_GameScene gameScene = Managers.UI.SceneUI as UI_GameScene;
        _ui_Enhance = gameScene.EnhanceUI;

        List<ItemData> itemDatas = new List<ItemData>();
        itemDatas = Managers.Data.ItemDict.Select(x => x.Value).Where(Data => Data.pieces != null).ToList();
        foreach (ItemData itemData in itemDatas)
        {
            ItemInfo itemInfo = new ItemInfo() 
            {
                TemplateId = itemData.id,   
            };
            itemInfo.Options.Add(itemData.options);
            Item item = Item.MakeItem(itemInfo);
         
            GameObject go = Managers.Resource.Instantiate("UI/Scene/UI_Display_Item_2", ScrollRect.content.transform);
            UI_Display_Item displayUI = go.GetOrAddComponent<UI_Display_Item>();
            displayUI.GameWindow = gameScene.GameWindow;
            Items.Add(displayUI);
            displayUI.SetItem(item);
        }
        foreach (UI_Display_Item item in Items)
        {
            BindEvent(item.gameObject, OnClickDisplayItem);
        }
        RefreshUI();
    }
    public void RefreshUI()
    {
        if (_isInit == false)
        {
            Init();
            return;
        }
        ClearCostItems();
        if (_selectedItem == null)
        {
            return;
        }
        Managers.Data.ItemDict.TryGetValue(_selectedItem.TemplateId, out ItemData itemData);
        List<CostData> costData = itemData.pieces.Select(x => new CostData()
        {
            templateId = x.templateId,
            count = x.count
        }).ToList();
        SetDisplayItem(costData, GetImage((int)Images.ItemProduction_CostPanel).gameObject.transform);
        UpdateContentSize();
    }
    public void SetDisplayItem(List<CostData> costData, Transform transform)
    {        
        foreach (var cost in costData)
        {
            if (Managers.Data.ItemDict.TryGetValue(cost.templateId, out Data.ItemData itemData))
            {
                UI_Display_Item itemIcon = Managers.Resource.Instantiate("UI/Scene/UI_Display_Item", transform).GetComponent<UI_Display_Item>();
                CostItems.Add(itemIcon);
                ItemInfo itemInfo = new ItemInfo()
                {
                    ItemDbId = 0,
                    TemplateId = cost.templateId,
                    Count = cost.count,
                    Equipped = false
                };
                itemInfo.Options.AddRange(itemData.options);

                Item item = Item.MakeItem(itemInfo);
                itemIcon.SetItem(item);
            }
        }
    }
    private void UpdateContentSize()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(_productContentRect);
    }
    public override void OnScroll(float scrollDelta)
    {
        if (ScrollRect != null)
        { 
            float scrollAmount = -scrollDelta * 0.1f; // 스크롤 속도 조절
            ScrollRect.verticalNormalizedPosition = Mathf.Clamp01(ScrollRect.verticalNormalizedPosition - scrollAmount);
        }
    }
    public void OnClickDisplayItem(PointerEventData eventData)
    {
        UI_Display_Item displayItem = eventData.pointerPress.GetComponent<UI_Display_Item>();
        if (displayItem == null)
            return;

        _selectedItem = displayItem.Item;
        RefreshUI();
    }
    public void OnClickProductionButton(PointerEventData evt)
    {
        if (_selectedItem == null)
            return;
        C_MakeItem production = new C_MakeItem()
        {
            TemplateId = _selectedItem.TemplateId,
            Count = 1
        };
        Managers.Network.Send(production);
    }
    private void ClearCostItems()
    {
        foreach (UI_Display_Item item in CostItems)
        {
            Destroy(item.gameObject);
        }
        CostItems.Clear();
    }
}