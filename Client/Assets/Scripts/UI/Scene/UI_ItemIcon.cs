using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ItemIcon : UI_Base, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    protected Image _icon = null;

    [SerializeField]
    protected UI_ItemDescription _itemDescription = null;
    protected bool _isDescription = false;
    public Item Item;
    private bool _init = false;
    public UI_GameWindow GameWindow { get; set; }
    public int ItemDbId { get; protected set; }
    public int TemplateId { get; protected set; }
    public int Count { get; protected set; }
    public override void Init()
    {
        if (_init)
            return;
        _icon = GetComponent<Image>();
        gameObject.BindEvent(OnPointerEnter, Define.UIEvent.MouseOver);
        gameObject.BindEvent(OnPointerExit, Define.UIEvent.MouseOut);
        _init = true;
    }

    public virtual void SetItem(Item item)
    {
        if(!_init)
            Init();
        Item = item;
        Data.ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(item.TemplateId, out itemData);

        Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);
        _icon.sprite = icon;        
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (_isDescription)
            return;

        _isDescription = true;
        _itemDescription = Managers.UI.ShowPopupUI<UI_ItemDescription>();
        _itemDescription.SetItem(Item);
        _itemDescription.OpenUI(eventData);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (!_isDescription)
            return;
        _isDescription = false;
        _itemDescription.CloseUI(eventData);
    }
}