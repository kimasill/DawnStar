using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//이미지 찾고 교체
public class UI_Inventory_Item : UI_Base
{
    [SerializeField]
    Image _icon = null;

    [SerializeField]
    Image _frame = null;

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
            if (itemData.itemType == ItemType.Consumable)
                return;

            C_EquipItem equipItemPacket = new C_EquipItem();
            equipItemPacket.ItemDbId = ItemDbId;
            equipItemPacket.Equipped = !Equipped;

            Managers.Network.Send(equipItemPacket);
        });
    }

    public void SetItem(Item item)
    {
        if (item == null)
        {
            ItemDbId = 0;
            TemplateId = 0;
            Count = 0;
            Equipped = false;

            _icon.gameObject.SetActive(false);
            _frame.gameObject.SetActive(false);
        }

        //아이템 정보 저장 : 슬롯에 세팅 시 
        ItemDbId = item.ItemDbId;
        TemplateId = item.TemplateId;
        Count = item.Count;
        Equipped = item.Equipped;

        Data.ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(TemplateId, out itemData);

        Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);
        _icon.sprite = icon;        
        
        _icon.gameObject.SetActive(true);
        _frame.gameObject.SetActive(Equipped);
        
    }
}
