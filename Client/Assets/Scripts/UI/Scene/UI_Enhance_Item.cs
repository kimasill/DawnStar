using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//이미지 찾고 교체
public class UI_Enhance_Item : UI_ItemIcon
{
    public override void Init()
    {
        gameObject.BindEvent(OnPointerEnter, Define.UIEvent.MouseOver);
        gameObject.BindEvent(OnPointerExit, Define.UIEvent.MouseOut);
    }
    public override void SetItem(Item item)
    {
        if (item == null)
        {
            ItemDbId = 0;
            TemplateId = 0;
            _icon.gameObject.SetActive(false);
            return;
        }
        _item = item;
        //아이템 정보 저장 : 슬롯에 세팅 시 
        ItemDbId = item.ItemDbId;
        TemplateId = item.TemplateId;

        Data.ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(TemplateId, out itemData);

        Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);
        _icon.sprite = icon;
        _icon.gameObject.SetActive(true);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (_isDescription)
            return;
        Item item = Managers.Inventory.Get(ItemDbId);
        if (item == null)
            return;
        _isDescription = true;
        _itemDescription = Managers.UI.ShowPopupUI<UI_ItemDescription>();
        _itemDescription.SetItem(item);
        _itemDescription.OnPointerEnter(eventData);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (!_isDescription)
            return;
        _isDescription = false;
        _itemDescription.OnPointerExit(eventData);
    }
}
