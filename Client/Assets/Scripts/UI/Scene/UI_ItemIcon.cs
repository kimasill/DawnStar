using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ItemIcon : UI_Base, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Image _icon;

    [SerializeField]
    private UI_ItemDescription _itemDescription;
    bool _isDescription = false;
    private Item _item;
    private bool _init = false;

    public override void Init()
    {
        if (_init)
            return;
        _icon = GetComponent<Image>();
        gameObject.BindEvent(OnPointerEnter, Define.UIEvent.MouseOver);
        gameObject.BindEvent(OnPointerExit, Define.UIEvent.MouseOut);
        _init = true;
    }

    public void SetItem(Item item)
    {
        if(!_init)
            Init();
        _item = item;
        Data.ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(item.TemplateId, out itemData);

        Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);
        _icon.sprite = icon;        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isDescription)
            return;

        _isDescription = true;
        _itemDescription = Managers.UI.ShowPopupUI<UI_ItemDescription>();
        _itemDescription.SetItem(_item);
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