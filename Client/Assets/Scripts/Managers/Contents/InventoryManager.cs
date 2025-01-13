using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public Dictionary<int, Item> Items { get; } = new Dictionary<int, Item>();
    public event Action<Item> ItemAdded;
    public void Add(Item item)
    {
        Item pItem = Get(item.ItemDbId);
        if (pItem != null)
            pItem.Count += item.Count;
        else Items.Add(item.ItemDbId, item);        
    }
    public void AddOrUpdate(Item item)
    {
        if (Items.ContainsKey(item.ItemDbId))
        {
            Items[item.ItemDbId] = item;
        }
        else
        {
            Items.Add(item.ItemDbId, item);
        }
    }

    public void Remove(int itemId)
    {
        Items.Remove(itemId);
    }

    public void RemoveOrUpdate(ItemInfo item)
    {
        Item tItem = Get(item.ItemDbId);
        if (item.Count>0)
        {
            tItem.Count = item.Count;
        }
        else if(item.Count == 0)
        {
            Remove(item.ItemDbId);
        }
    }

    public Item Get(int itemId)
    {
        Item item = null;
        Items.TryGetValue(itemId, out item);
        return item;
    }

    public Item GetItemById(int templateId)
    {
        foreach(Item item in Items.Values)
        {
            if (item.TemplateId == templateId)
                return item;
        }
        return null;
    }


    public Item Find(Func<Item, bool> condition)
    {
        foreach (var item in Items.Values)
        {
            if (condition.Invoke(item))
                return item;
        }
        return null;
    }

    public void RefreshEquipment(EquipmentController equipmentController)
    {
        equipmentController.Refresh(Items);
    }

    public void Clear()
    {
        Items.Clear();
    }
}
