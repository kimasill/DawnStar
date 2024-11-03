using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Shop_Item : UI_Base, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    Transform _itemRoot = null;

    [SerializeField]
    Image _icon = null;

    [SerializeField]
    Image _frame = null;

    [SerializeField]
    private UI_ItemDescription _itemDescription = null;

    private bool _isDescription = false;
    public int ItemDbId { get; private set; }
    public int TemplateId { get; private set; }
    public int Count { get; private set; }
    public int Price { get; private set; }
    public bool Equipped { get; private set; }
    public int ShopId { get; set; }

    enum Texts
    {
        ItemCount,
        GoldText,
        ItemNameText
    }

    public override void Init()
    {
        Binding();
    }

    private void Binding()
    {
        _itemRoot.gameObject.BindEvent((e) =>
        {
            UnityEngine.Debug.Log("아이템 구매");
            
            // 구매 패킷 넘겨주기
            C_BuyItem buyItemPacket = new C_BuyItem();
            buyItemPacket.TemplateId = TemplateId;            
            buyItemPacket.Count = 1;
            buyItemPacket.ShopId = ShopId;
            Managers.Network.Send(buyItemPacket);
        });

        Bind<TMP_Text>(typeof(Texts));

        _itemRoot.gameObject.BindEvent(OnPointerEnter, Define.UIEvent.MouseOver);
        _itemRoot.gameObject.BindEvent(OnPointerExit, Define.UIEvent.MouseOut);
        // 마우스 오버 이벤트
        _itemRoot.gameObject.BindEvent((e) =>
        {
            _frame.gameObject.SetActive(true);
        }, Define.UIEvent.MouseOver);

        // 마우스 아웃 이벤트
        _itemRoot.gameObject.BindEvent((e) =>
        {
            _frame.gameObject.SetActive(false);
        }, Define.UIEvent.MouseOut);
        
    }
    public void SetItem(Item item)
    {
        if (item == null)
        {
            ItemDbId = 0;
            TemplateId = 0;
            Count = 0;
            Equipped = false;
            Price = 0;

            gameObject.SetActive(false);
            _icon.gameObject.SetActive(false);
            _frame.gameObject.SetActive(false);
            return;
        }

        // 아이템 정보 저장 : 슬롯에 세팅 시 
        ItemDbId = item.ItemDbId;
        TemplateId = item.TemplateId;
        Count = item.Count;
        Equipped = item.Equipped;
        Price = item.Price;

        Data.ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(TemplateId, out itemData);

        Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);
        _icon.sprite = icon;

        _icon.gameObject.SetActive(true);
        GetTextMeshPro((int)Texts.ItemNameText).text = itemData.name;
        GetTextMeshPro((int)Texts.GoldText).text = itemData.price.ToString();
        GetTextMeshPro((int)Texts.ItemCount).text = item.Count.ToString();
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isDescription)
            return;
        Item item = Managers.Shop.Shops[ShopId].Get(TemplateId);
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