using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ItemDescription : UI_Popup
{
    public List<UI_Item_Stat> Items { get; } = new List<UI_Item_Stat>();
    [SerializeField]
    public GameObject _grid = null;

    [SerializeField]
    private GameObject _itemPopup = null;

    private RectTransform _statPanelRectTransform;
    private RectTransform _itemPanelRectTransform;
    enum Images
    {
        ItemPopup_Image
    }

    enum Texts
    {
        ItemPopup_Name,
        ItemDescriptionText
    }

    public override void Init()
    {
        Items.Clear();
        foreach (Transform child in _grid.transform)
        {
            Destroy(child.gameObject);
        }
        GameObject go = Managers.Resource.Instantiate("UI/Popup/UI_Item_Stat", _grid.transform);
        UI_Item_Stat stat = go.GetOrAddComponent<UI_Item_Stat>();
        Items.Add(stat);

        Bind<Image>(typeof(Images));
        Bind<TMP_Text>(typeof(Texts));

        _statPanelRectTransform = _grid.gameObject.transform as RectTransform;
        _itemPanelRectTransform = _itemPopup.GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UpdatePopupPosition(eventData);
        _itemPopup.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ClosePopupUI();
    }

    public void SetItem(Item item)
    {
        ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(item.TemplateId, out itemData);
        if (itemData != null)
        {
            GetImage((int)Images.ItemPopup_Image).sprite = Managers.Resource.Load<Sprite>(itemData.iconPath);
            GetTextMeshPro((int)Texts.ItemPopup_Name).text = itemData.name;
            //_description.text = item.Info.Description;
        }

        switch (item.ItemType)
        {
            case ItemType.Consumable:
                SetConsumableItem(item as Item.Consumable);
                break;
            case ItemType.Weapon:
                SetWeaponItem(item as Item.Weapon);
                break;
            case ItemType.Armor:
                SetArmorItem(item as Item.Armor);
                break;
            case ItemType.Goods:
                SetGoodsItem(item as Item.Goods);
                break;
        }

        // StatPanel과 ItemPanel의 크기 조정
        LayoutRebuilder.ForceRebuildLayoutImmediate(_statPanelRectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_itemPanelRectTransform);
    }

    private void SetConsumableItem(Item.Consumable item)
    {
        AddStat($"수량: {item.MaxCount}");
    }

    private void SetWeaponItem(Item.Weapon item)
    {
        AddStat($"공격력: {item.Damage}");
    }

    private void SetArmorItem(Item.Armor item)
    {
        AddStat($"방어력: {item.Defense}");
    }

    private void SetGoodsItem(Item.Goods item)
    {
        AddStat($"수량: {item.MaxCount}");
    }

    private void AddStat(string statText)
    {
        GameObject statObject = Managers.Resource.Instantiate("UI/Popup/UI_Item_Stat", _grid.transform);
        UI_Item_Stat stat = statObject.GetOrAddComponent<UI_Item_Stat>();
        stat.Name.text = statText;
    }

    private void UpdatePopupPosition(PointerEventData eventData)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _itemPopup.transform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint);

        // 마우스 위치의 오른쪽에 팝업의 좌상단이 위치하도록 조정
        localPoint.x += _itemPanelRectTransform.rect.width / 2 + 25;
        localPoint.y -= _itemPanelRectTransform.rect.height / 2;

        _itemPopup.GetComponent<RectTransform>().localPosition = localPoint;
    }

    public override void ClosePopupUI()
    {
        _itemPopup.SetActive(false);
    }
}