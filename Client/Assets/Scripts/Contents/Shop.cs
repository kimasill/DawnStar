
using System.Collections.Generic;
using System;

public class Shop
{
    public string name;
    public int shopId;
    public int ShopDbId { get; set; }
    public Dictionary<int, Item> Items = new Dictionary<int, Item>();
    public event Action OnItemRemoved;
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

    public void RemoveItem(int templateId)
    {
        if (Items.ContainsKey(templateId))
        {
            Items.Remove(templateId);
            OnItemRemoved?.Invoke();
        }
    }

    public void Clear()
    {
        Items.Clear();
        OnItemRemoved?.Invoke();
    }
}