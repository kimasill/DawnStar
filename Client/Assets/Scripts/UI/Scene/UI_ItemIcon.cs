using Google.Protobuf.Protocol;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class UI_ItemIcon : UI_Base, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    protected Image _icon = null;

    [SerializeField]
    protected UI_ItemDescription _itemDescription = null;
    protected bool _isDescription = false;
    public Item Item = null;
    protected bool _init = false;
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

    public virtual void SetItem(Item item, bool countDisplay = true)
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
    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (Item == null)
            return;

        gameObject.GetOrAddComponent<CanvasGroup>().blocksRaycasts = false;
        _originalPosition = transform.position;
        _originalSiblingIndex = transform.GetSiblingIndex();
        _originalParent = transform.parent;
        UI_GameScene gameScene = Managers.UI.SceneUI as UI_GameScene;
        transform.SetParent(gameScene.transform);
        transform.SetAsLastSibling();
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        gameObject.GetOrAddComponent<CanvasGroup>().blocksRaycasts = true;
        GameObject target = eventData.pointerEnter;
        bool success = false;
        UI_ItemIcon item = target.GetComponentInParent<UI_ItemIcon>();

        if (target != null && item != null)
        {
            // 상호작용 로직
            success = InteractWith(item);
        }
        // 원위치
        transform.position = _originalPosition;
        transform.SetParent(_originalParent);
        transform.SetSiblingIndex(_originalSiblingIndex);
    }

    public virtual bool InteractWith(UI_ItemIcon target)
    {
        if (target == null || Item == null || Item.TargetInteract == null) return false;

        foreach (var targetInteract in Item.TargetInteract)
        {
            if (targetInteract.objectType == GameObjectType.Item)
            {
                if(target is UI_Inventory_Item == false)
                {
                    return false;
                }
                if (Enum.TryParse(targetInteract.detail, out ItemType itemType))
                {
                    if (target.Item.ItemType == itemType)
                    {
                        target.Interact(Item);
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public virtual void Interact(Item item) 
    {
    }
}