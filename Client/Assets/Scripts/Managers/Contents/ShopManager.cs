using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public Dictionary<int, Item> Items { get; } = new Dictionary<int, Item>();
    public event Action OnItemRemoved;
    public void RequestShop()
    {
        C_RequestShop requestShopPacket = new C_RequestShop();
        Managers.Network.Send(requestShopPacket);
    }
    public void Add(Item item)
    {
        Items.Add(item.TemplateId, item);
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

    public void RemoveItem(int itemDbId)
    {
        if (Items.ContainsKey(itemDbId))
        {
            Items.Remove(itemDbId);
            OnItemRemoved?.Invoke();
        }
    }

    public void Clear()
    {
        Items.Clear();
        OnItemRemoved?.Invoke();
    }
}