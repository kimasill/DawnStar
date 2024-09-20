using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class Inventory
    {
        public Dictionary<int, Item> Items { get; } = new Dictionary<int, Item>();

        public void Add(Item item)
        {
            Items.Add(item.ItemDbId, item);
        }

        public void Remove(int itemId)
        {
            Items.Remove(itemId);
        }

        public Item Get(int itemId)
        {
            Item item = null;
            Items.TryGetValue(itemId, out item);
            return item;
        }

        public Item Find(Func<Item, bool> condition)
        {
            foreach(var item in Items.Values)
            {
                if (condition.Invoke(item))
                    return item;
            }
            return null;
        }

        public int? GetEmptySlot()
        {
            for (int slot = 0; slot<20; slot++)
            {
                Item item = Items.Values.FirstOrDefault(i=> i.Slot == slot);
                if (item == null)
                    return slot;
            }
            return null;
        }
    }
}
