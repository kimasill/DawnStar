using Data;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_QuickSlotItem : UI_Base, IPointerClickHandler
{
    public Item.Consumable Item { get; private set; }
    public int Index { get; set; }
    bool _init = false;
    enum Images
    {
        QuickItemImage,
    }

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        transform.gameObject.BindEvent(OnPointerClick, Define.UIEvent.Click);
        if (_init == false)
        {
            _init = true;
        }
    }
    public void SetItem(Item.Consumable item)
    {
     
        if(_init == false)
        {
            return;
        }

        if (item == null) {
            ClearItem();
            return;
        }
        Item = item;
        Managers.Data.ItemDict.TryGetValue(item.TemplateId, out ItemData itemData);
        Sprite iconImage = Managers.Resource.Load<Sprite>(itemData.iconPath);
        Image icon = GetImage((int)Images.QuickItemImage);
        icon.sprite = iconImage;
        icon.enabled = true;
    }

    public void ClearItem()
    {
        Item = null;
        GetImage((int)Images.QuickItemImage).enabled = false;
    }

    public void UseItem()
    {
        if (Item == null)
            return;

        C_UseItem useItemPacket = new C_UseItem();
        useItemPacket.ItemDbId = Item.ItemDbId;
        useItemPacket.TemplateId = Item.TemplateId;
        Managers.Network.Send(useItemPacket);
        // Implement item usage logic here
        Debug.Log($"Using item: {Item}"); // Assuming Item has a Name property    
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Item != null)
        {
            UI_QuickSlot quickSlot = GetComponentInParent<UI_QuickSlot>();
            quickSlot.UnregisterItem(this);
        }
    }

}