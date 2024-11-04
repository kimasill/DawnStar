using Data;
using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
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
    enum Images
    {
        ItemPopup_Image
    }

    enum Texts
    {
        ItemPopup_Name,
        ItemDescriptionText,
        ItemSkillName,
        ItemSkillDescription
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
            GetTextMeshPro((int)Texts.ItemDescriptionText).text = itemData.description;
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

        Dictionary<string, string> options = itemData.options;
        foreach (var option in options)
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
            string key = ConvertSpecialOptions(option.Key);
            AddStat($"{key}: {option.Value}");
        }
        // StatPanel과 ItemPanel의 크기 조정
        LayoutRebuilder.ForceRebuildLayoutImmediate(_statPanelRectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_itemPanelRectTransform);
    }
    public string ConvertSpecialOptions(string option)
    {
        switch(option) {
            case "Critical":
                option = "치명타 확률";
                break;
            case "CriticalDamage":
                option = "치명타 피해";
                break;
            case "AttackSpeed":
                option = "공격 속도";
                break;
            case "MoveSpeed":
                option = "이동 속도";
                break;
            case "HPRegen":
                option = "체력 회복";
                break;
            case "Heal":
                option = "회복량";
                break;
            case "UPRegen":
                option = "미지력 회복";
                break;
            case "Skill":
                option = "특수기술";
                break;
            case "SkillDamage":
                option = "특수기술 피해";
                break;
            case "SkillUP":
                option = "특수기술 미지력 소모량";
                break;
            case "SkillDescription":
                option = "특수기술 설명";
                break;
            default:
                break;
        }
        return option;
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

        _itemPopup.GetComponent<RectTransform>().localPosition = localPoint;
    }
}