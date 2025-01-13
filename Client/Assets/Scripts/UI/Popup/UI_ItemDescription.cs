using Data;
using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using System;
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

    [SerializeField]
    private GameObject _skillPanel = null;

    private SkillData _skillData;

    private RectTransform _statPanelRectTransform;
    private RectTransform _itemPanelRectTransform;
    private RectTransform _itemDescriptionRectTransform;
    enum Images
    {
        ItemPopup_Image,
        ItemDescriptionImage
    }

    enum Texts
    {
        ItemPopup_Name,
        ItemDescriptionText,
        ItemSkillName,
        ItemSkillDescription,
        ItemPopup_Add,
        ItemPopup_Grade,
        PricePanel_Text,
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
        _skillPanel.gameObject.SetActive(false);
        _statPanelRectTransform = _grid.gameObject.transform as RectTransform;
        _itemPanelRectTransform = _itemPopup.GetComponent<RectTransform>();
        _itemDescriptionRectTransform = GetImage((int)Images.ItemDescriptionImage).gameObject.transform as RectTransform;
    }

    public void OpenUI(PointerEventData eventData)
    {
        UpdatePopupPosition(eventData);
        _itemPopup.SetActive(true);
    }

    public void CloseUI(PointerEventData eventData)
    {
        ClosePopupUI();
    }

    public void SetItem(Item item)
    {
        ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(item.TemplateId, out itemData);

        if (itemData == null)
        {
            Debug.Log("ItemData is null");
            return;
        }

        GetImage((int)Images.ItemPopup_Image).sprite = Managers.Resource.Load<Sprite>(itemData.iconPath);
        GetTextMeshPro((int)Texts.ItemPopup_Grade).text = Content.ConvertGrade(item.Grade);
        GetTextMeshPro((int)Texts.ItemPopup_Grade).color = Content.GetGradeColor(item.Grade);
        GetTextMeshPro((int)Texts.ItemPopup_Name).text = itemData.name;
        GetTextMeshPro((int)Texts.ItemPopup_Name).color = Content.GetEnhanceColor(item.Rank);
        GetTextMeshPro((int)Texts.ItemDescriptionText).text = itemData.description;
        GetTextMeshPro((int)Texts.ItemPopup_Add).text = item.Rank == 0 ? "" : $"(+{item.Rank.ToString()})";
        GetTextMeshPro((int)Texts.PricePanel_Text).text = (itemData.price/2).ToString();
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

        foreach (var option in item.Options)
        {            
            if (option.Key == "Skill")
            {
                _skillPanel.SetActive(true);

                SkillData skilldata = null;
                Managers.Data.SkillDict.TryGetValue(int.Parse(option.Value), out skilldata);
                _skillData = skilldata;
                if (_skillData != null)
                {
                    GetTextMeshPro((int)Texts.ItemSkillName).text = _skillData.name;
                    GetTextMeshPro((int)Texts.ItemSkillDescription).text = _skillData.description;
                    continue;
                }
            }
            string key = Content.ConvertSpecialOptions(option.Key);
            string value = Content.ConvertSpecialOptionsValue(option.Key, option.Value);
            AddStat($"{key}: {value}");
        }
        // StatPanel과 ItemPanel의 크기 조정
        LayoutRebuilder.ForceRebuildLayoutImmediate(_statPanelRectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_itemPanelRectTransform);
    }
    private void SetConsumableItem(Item.Consumable item)
    {
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

        RectTransform parentRect = _itemPopup.transform.parent as RectTransform;
        Vector2 minPosition = parentRect.rect.min - _itemPanelRectTransform.rect.min;
        Vector2 maxPosition = parentRect.rect.max - _itemPanelRectTransform.rect.max;


        localPoint.x = Mathf.Clamp(localPoint.x, minPosition.x, maxPosition.x - _itemDescriptionRectTransform.rect.width);
        localPoint.y = Mathf.Clamp(localPoint.y, minPosition.y, maxPosition.y);

        _itemPopup.GetComponent<RectTransform>().localPosition = localPoint;
    }
}