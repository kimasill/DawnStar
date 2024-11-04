using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_QuickSlot : UI_Base
{

    public List<UI_QuickSlotItem> QuickSlotItems { get; private set; } = new List<UI_QuickSlotItem>();

    public override void Init()
    {
        for (int i = 0; i < 4; i++)
        {
            UI_QuickSlotItem quickSlotItem = transform.GetChild(i).GetComponent<UI_QuickSlotItem>();
            quickSlotItem.Index = i;            
            QuickSlotItems.Add(quickSlotItem);
            quickSlotItem.Init();
        }
        RefreshUI();
    }

    public void RefreshUI()
    {
        foreach (var quickSlotItem in QuickSlotItems)
        {
            if(quickSlotItem.Item != null && Managers.Inventory.Get(quickSlotItem.Item.ItemDbId) == null)
            {
                quickSlotItem.ClearItem();
            }
            if (quickSlotItem.Item != null && quickSlotItem.gameObject.activeSelf == false)
            {                
                quickSlotItem.gameObject.SetActive(true);                
            }
            else if(quickSlotItem.Item == null && quickSlotItem.gameObject.activeSelf == true)
            {                
                quickSlotItem.gameObject.SetActive(false);
            }
        }
    }

    public void RegisterItem(Item.Consumable item)
    {
        foreach (var quickSlotItem in QuickSlotItems)
        {
            if (quickSlotItem.Item == null)
            {
                quickSlotItem.SetItem(item);
                break;
            }
        }
        RefreshUI();
    }

    public void UnregisterItem(UI_QuickSlotItem quickSlotItem)
    {
        quickSlotItem.ClearItem();
        RefreshUI();
    }

    public void UseItem(int index)
    {
        if (index < 0 || index >= QuickSlotItems.Count)
            return;

        QuickSlotItems[index].UseItem();
    }
}