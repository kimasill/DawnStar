using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Shop_Item : UI_Base
{
    [SerializeField]
    Transform _itemRoot = null;

    [SerializeField]
    Image _icon = null;

    [SerializeField]
    TMP_Text _name = null;

    [SerializeField]
    Image _frame = null;

    [SerializeField]
    TMP_Text _price = null;

    public int ItemDbId { get; private set; }
    public int TemplateId { get; private set; }
    public int Count { get; private set; }
    public int Price { get; private set; }
    public bool Equipped { get; private set; }

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
            Managers.Network.Send(buyItemPacket);
        });

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
        //ItemDbId = item.ItemDbId;
        TemplateId = item.TemplateId;
        Count = item.Count;
        Equipped = item.Equipped;
        Price = item.Price;

        Data.ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(TemplateId, out itemData);

        Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);
        _icon.sprite = icon;

        _icon.gameObject.SetActive(true);
        _name.text = itemData.name;
        _price.text = itemData.price.ToString();
    }
}