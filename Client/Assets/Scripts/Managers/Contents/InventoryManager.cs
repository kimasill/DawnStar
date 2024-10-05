using Google.Protobuf.Protocol;
using System;
using System.Collections;
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
        Items.Add(item.ItemDbId, item);
        ItemAdded?.Invoke(item);
    }

    public void Remove(int itemId)
    {
        Items.Remove(itemId);
    }

    public void RemoveOrUpdate(ItemInfo item)
    {
        Item tItem = Get(item.ItemDbId);
        if (tItem.Count > item.Count)
        {
            tItem.Count -= item.Count;
        }
        else
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
