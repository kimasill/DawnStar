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
    UI_GameScene _gameScene = null;
    public override void Init()
    {
        Items.Clear();

        ScrollRect = GetComponentInChildren<ScrollRect>();
        foreach (Transform child in grid.transform)
        {
            Destroy(child.gameObject);
        }
        _gameScene = Managers.UI.SceneUI as UI_GameScene;
        for (int i = 0; i < 100; i++)
        {
            GameObject go = Managers.Resource.Instantiate("UI/Scene/UI_Inventory_Item", grid.transform);
            UI_Inventory_Item item = go.GetOrAddComponent<UI_Inventory_Item>();
            item.GameWindow = _gameScene.GameWindow;
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
            if (item.Slot < 0 || item.Slot >= 100 )
                continue;
            Items[item.Slot].SetItem(item);
        }
        foreach (UI_Inventory_Item item in Items)
        {
            if (Managers.Inventory.Get(item.ItemDbId) == null)
            {
                item.SetItem(null);
            }
        }
    }
}
