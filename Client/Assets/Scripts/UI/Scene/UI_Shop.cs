using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Shop : UI_Base
{
    public List<UI_Shop_Item> Items { get; } = new List<UI_Shop_Item>();
    public TMP_Text shopTitle;
    public Text shopDescription;
    public GameObject grid;

    public bool _isInit;
    enum Buttons 
    { 
        ShopExitButton
    }


    public override void Init()
    {
        Items.Clear();

        grid = transform.Find("ItemGrid").gameObject;
        foreach (Transform child in grid.transform)
        {
            Destroy(child.gameObject);
        }
        GameObject go = Managers.Resource.Instantiate("UI/Scene/UI_Shop_Item", grid.transform);
        UI_Shop_Item item = go.GetOrAddComponent<UI_Shop_Item>();
        Items.Add(item);

        Bind<Button>(typeof(Buttons));
        GetButton((int)Buttons.ShopExitButton).gameObject.BindEvent(OnClick);

        gameObject.BindEvent(OnBeginDrag, Define.UIEvent.BeginDrag);
        gameObject.BindEvent(OnDrag, Define.UIEvent.Drag);
        gameObject.BindEvent(OnEndDrag, Define.UIEvent.EndDrag);
        //Managers.Shop.OnItemRemoved += RefreshUI;
        _isInit = true;
    }

    public void RefreshUI(int shopId)
    {
        if (Items.Count == 0 && _isInit == false)
        {
            return;
        }
        List<Item> items = Managers.Shop.Shops[shopId].Items.Values.ToList();
        List<Item> sortedItems = items
            .Where(item => item.Slot >= 0 && item.Slot < 20)
            .OrderBy(item => item.Slot)
            .ToList();

        List<Item> unslottedItems = items
            .Where(item => item.Slot < 0 || item.Slot >= 20)
            .ToList();

        sortedItems.AddRange(unslottedItems);

        for (int i = 0; i < sortedItems.Count; i++)
        {
            if (i >= Items.Count)
            {
                GameObject go = Managers.Resource.Instantiate("UI/Scene/UI_Shop_Item", grid.transform);
                UI_Shop_Item item = go.GetOrAddComponent<UI_Shop_Item>();
                Items.Add(item);
            }
            if (i >= 20)
                break;
            Items[i].SetItem(sortedItems[i]);
            Items[i].ShopId = shopId;
        }

        for (int i = sortedItems.Count; i < Items.Count; i++)
        {
            Items[i].SetItem(null);
        }
    }
    public void OnClick(PointerEventData evt)
    {
        CloseShop();
    }

    public void OpenShop(string title, string description)
    {
        gameObject.SetActive(true);
        shopTitle.text = title;    
    }

    public void CloseShop()
    {
        gameObject.SetActive(false);
    }
}