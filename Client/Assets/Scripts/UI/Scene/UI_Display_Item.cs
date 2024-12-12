using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//이미지 찾고 교체
public class UI_Display_Item : UI_ItemIcon
{

    [SerializeField]
    TMP_Text _itemCount = null;

    public bool Condition = false;
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
            Count = 0;
            _icon.gameObject.SetActive(false);
            return;
        }
        Item = item;
        //아이템 정보 저장 : 슬롯에 세팅 시 

        ItemDbId = item.ItemDbId;
        TemplateId = item.TemplateId;
        Count = item.Count;

        if(_itemCount != null)
        {
            Item invenItem = Managers.Inventory.GetItemById(TemplateId);
            int invenItemCount = 0;
            if (invenItem != null)
                invenItemCount = invenItem.Count;

            _itemCount.text = $"{invenItemCount}/{Count}";
            if(invenItemCount>=Count)
                Condition = true;
        }

        Data.ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(TemplateId, out itemData);

        Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);
        _icon.sprite = icon;
        _icon.gameObject.SetActive(true);
    }
}
