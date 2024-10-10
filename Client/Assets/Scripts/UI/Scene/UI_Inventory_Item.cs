using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//이미지 찾고 교체
public class UI_Inventory_Item : UI_Base, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    Image _icon = null;

    [SerializeField]
    Image _frame = null;

    [SerializeField]
    TMP_Text _itemCount = null;

    [SerializeField]
    private UI_ItemDescription _itemDescription = null;

    private bool _isDescription = false;

    public int ItemDbId { get; private set; }
    public int TemplateId { get; private set; }
    public int Count { get; private set; }
    public bool Equipped { get; private set; }

    public override void Init()
    {

        //클릭했을때 패킷
        _icon.gameObject.BindEvent((e) =>
        {
            Debug.Log("아이템 사용");

            Data.ItemData itemData = null;
            Managers.Data.ItemDict.TryGetValue(TemplateId, out itemData);
            
            if(itemData == null)
                return;
            //TODO : 패킷 넘겨주기
            if (itemData.itemType == ItemType.Consumable || itemData.itemType == ItemType.Goods)
                return;

            C_EquipItem equipItemPacket = new C_EquipItem();
            equipItemPacket.ItemDbId = ItemDbId;
            equipItemPacket.Equipped = !Equipped;

            Managers.Network.Send(equipItemPacket);
        });

        _icon.gameObject.BindEvent(OnPointerEnter, Define.UIEvent.MouseOver);
        _icon.gameObject.BindEvent(OnPointerExit, Define.UIEvent.MouseOut);
    }

    public void SetItem(Item item)
    {
        if (item == null)
        {
            ItemDbId = 0;
            TemplateId = 0;
            Count = 0;
            Equipped = false;

            _itemCount.gameObject.SetActive(false);
            _icon.gameObject.SetActive(false);
            _frame.gameObject.SetActive(false);
        }

        //아이템 정보 저장 : 슬롯에 세팅 시 
        ItemDbId = item.ItemDbId;
        TemplateId = item.TemplateId;
        if(item.Count > 0)
        {
            Count = item.Count;
            _itemCount.text = Count.ToString();
        }
            
        Equipped = item.Equipped;

        Data.ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(TemplateId, out itemData);

        Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);
        _icon.sprite = icon;       

        _itemCount.gameObject.SetActive(true);
        _icon.gameObject.SetActive(true);
        _frame.gameObject.SetActive(Equipped);
    }

    public void OnPointerEnter(PointerEventData eventData)
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

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_isDescription)
            return;
        _isDescription = false;
        _itemDescription.OnPointerExit(eventData);
    }
}
