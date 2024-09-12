using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UI_Shop : UI_Base
{
    public List<UI_Shop_Item> Items { get; } = new List<UI_Shop_Item>();
    public Text shopTitle;
    public Text shopDescription;

    public override void Init()
    {
        Items.Clear();

        GameObject grid = transform.Find("ItemGrid").gameObject;
        foreach (Transform child in grid.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < 20; i++)
        {
            GameObject go = Managers.Resource.Instantiate("UI/Scene/UI_Shop_Item", grid.transform);
            UI_Shop_Item item = go.GetOrAddComponent<UI_Shop_Item>();
            Items.Add(item);
        }
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (Items.Count == 0)
        {
            return;
        }
        List<Item> items = Managers.Shop.Items.Values.ToList();
        items.Sort((left, right) => { return left.Slot - right.Slot; });

        foreach (Item item in items)
        {
            if (item.Slot < 0 || item.Slot >= 20)
                continue;
            Items[item.Slot].SetItem(item);
        }
    }

    public void OpenShop(string title, string description)
    {
        shopTitle.text = title;
        shopDescription.text = description;
        gameObject.SetActive(true);
    }

    public void CloseShop()
    {
        gameObject.SetActive(false);
    }
}