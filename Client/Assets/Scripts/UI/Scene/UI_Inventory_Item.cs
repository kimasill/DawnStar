using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Item;

//이미지 찾고 교체
public class UI_Inventory_Item : UI_ItemIcon
{
    [SerializeField]
    Image _frame = null;

    [SerializeField]
    TMP_Text _itemCount = null;

    UI_QuickSlot _quickSlot = null;
    public bool Equipped { get; protected set; }
    public override void Init()
    {
        gameObject.BindEvent(OnPointerEnter, Define.UIEvent.MouseOver);
        gameObject.BindEvent(OnPointerExit, Define.UIEvent.MouseOut);
        //클릭했을때 패킷
        gameObject.BindEvent((e) =>
        {
            Data.ItemData itemData = null;
            Managers.Data.ItemDict.TryGetValue(TemplateId, out itemData);
            
            if(itemData == null)
                return;
            //TODO : 패킷 넘겨주기
            if (itemData.itemType == ItemType.Goods)
                return;
            if(itemData.itemType == ItemType.Consumable)
            {
                GameWindow.QuickSlot.RegisterItem(Item as Item.Consumable);
            }
            else
            {
                C_EquipItem equipItemPacket = new C_EquipItem();
                equipItemPacket.ItemDbId = ItemDbId;
                equipItemPacket.Equipped = !Equipped;

                Managers.Network.Send(equipItemPacket);
            }
        });
        gameObject.BindEvent((e) =>
        {
            UI_GameScene gameScene = Managers.UI.SceneUI as UI_GameScene;      
            if(gameScene.EnhanceUI.enabled)
            {
                gameScene.EnhanceUI.SetItem(Item);
            }
            else if (gameScene.ShopUI.enabled)
            {
                if(Input.GetKey(KeyCode.LeftShift))
                {
                    UI_Quantity quantity = Managers.UI.ShowPopupUI<UI_Quantity>();
                    quantity.Check = (int count) =>
                    {
                        SellItem(count);
                    };
                }
                else
                {
                    SellItem(1);
                }
            }
        }, Define.UIEvent.RightClick);
        gameObject.BindEvent(OnBeginDrag, Define.UIEvent.BeginDrag);
        gameObject.BindEvent(OnDrag, Define.UIEvent.Drag);
        gameObject.BindEvent(OnEndDrag, Define.UIEvent.EndDrag);
    }

    public override void SetItem(Item item)
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
            return;
        }
        Item = item;
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

    public void SellItem(int count)
    {
        UnityEngine.Debug.Log("아이템 판매");

        // 구매 패킷 넘겨주기
        C_SellItem sellPacket = new C_SellItem();
        sellPacket.ItemDbId = Item.ItemDbId;
        sellPacket.Count = count;
        sellPacket.Slot = Item.Slot;
        Managers.Network.Send(sellPacket);
    }

    public override void Interact(Item item)
    {
        if(item.ItemType == ItemType.Consumable)
        {
            Consumable consumable = item as Consumable;
            switch (consumable.ConsumableType) 
            { 
                case ConsumableType.Scroll:
                    C_Enchant enchant = new C_Enchant()
                    {
                        TargetId = ItemDbId,
                        MaterialId = item.ItemDbId
                    };
                    Managers.Network.Send(enchant);
                    break;
            }
        }
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
        _itemDescription.OpenUI(eventData);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (!_isDescription)
            return;
        _isDescription = false;
        _itemDescription.CloseUI(eventData);
    }
    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        UI_GameScene gameScene = Managers.UI.SceneUI as UI_GameScene;
        gameScene.InvenUI.RefreshUI();
    }
}
