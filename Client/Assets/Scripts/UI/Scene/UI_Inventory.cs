using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UI_Inventory : UI_Base
{
    public List<UI_Inventory_Item> Items { get; } = new List<UI_Inventory_Item>();

    [SerializeField]
    public GameObject grid = null;
    public ScrollRect ScrollRect { get; private set; }
    public override void Init()
    {
        Items.Clear();

        ScrollRect = GetComponentInChildren<ScrollRect>();
        foreach (Transform child in grid.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < 20; i++)
        {
            GameObject go = Managers.Resource.Instantiate("UI/Scene/UI_Inventory_Item", grid.transform);
            UI_Inventory_Item item = go.GetOrAddComponent<UI_Inventory_Item>();
            Items.Add(item);
        }
        RefreshUI();
    }
    //문제 : Init 타이밍 이슈. UI_GameScene에서 Stat_UI, Inven_UI를 false로 바꾸기 때문에 해당 클래스 Init이 호출되지 않음
    public void RefreshUI()
    {
        if(Items.Count == 0)
        {
            return;
        }
        List<Item> items = Managers.Inventory.Items.Values.ToList();
        items.Sort((left, right) => { return left.Slot - right.Slot; });

        foreach (Item item in items)
        {
            if (item.Slot < 0 || item.Slot >= 20 )
                continue;
            Items[item.Slot].SetItem(item);
        }
    }
}
