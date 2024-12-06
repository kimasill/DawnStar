using Data;
using Google.Protobuf.Protocol;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class UI_Enhance : UI_Base
{
    public GameObject enhancePanel;
    public Button enhanceButton;        
    private Item _selectedItem;
    private UI_Enhance_Item _enhanceItem;
    private Image _enhanceCostPanel;
    private List<UI_Display_Item> Items = new List<UI_Display_Item>();
    enum Texts
    {
        Enhance_Item_Name,
        EnhanceResultText,
        Enhance_Item_Rank,
    }

    enum Buttons
    {
        EnhanceButton,
        EnhanceExitButton,
    }

    enum Images
    {
        Enhance_Item_Icon,
        EnhanceCostPanel,
    }

    bool _isInit = false;
    public override void Init()
    {
        if (_isInit)
            return;
        _isInit = true;

        Bind<TMP_Text>(typeof(Texts)); // Change TMPro to TextMeshProUGUI
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));

        BindEvent(GetButton((int)Buttons.EnhanceButton).gameObject, (PointerEventData data) => { OnClickEnhanceButton(); });
        BindEvent(GetButton((int)Buttons.EnhanceExitButton).gameObject, (PointerEventData data) => { OnClickExitButton(); });

        _enhanceCostPanel = GetImage((int)Images.EnhanceCostPanel);

    }
    public void RefreshUI()
    {
        if (_isInit == false)
        {
            Init();
            return;
        }

        if (_enhanceItem == null)
        {
            return;
        }
        Dictionary<int, EnhanceData> enhanceDataDict = Managers.Data.EnhanceDict;
        EnhanceData enhanceData = null;
        foreach (KeyValuePair<int, EnhanceData> data in enhanceDataDict)
        {
            if (data.Value.itemType == _selectedItem.ItemType && data.Value.rank == _selectedItem.Rank)
            {
                enhanceData = data.Value;
            }
        }
        foreach (var cost in enhanceData.costData)
        {
            if (Managers.Data.ItemDict.TryGetValue(cost.templateId, out Data.ItemData itemData))
            {
                UI_Display_Item itemIcon = Managers.Resource.Instantiate("UI/Scene/UI_Display_Item", _enhanceCostPanel.gameObject.transform).GetComponent<UI_Display_Item>();
                Items.Add(itemIcon);
                ItemInfo itemInfo = new ItemInfo()
                {
                    ItemDbId = 0,
                    TemplateId = cost.templateId,
                    Count = cost.count,
                    Equipped = false
                };
                itemInfo.Options.AddRange(itemData.options);

                Item item = Item.MakeItem(itemInfo);
                itemIcon.SetItem(item);
            }
        }
    }
    public void SelectItem(Item item)
    {
        _selectedItem = item;
        ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(item.TemplateId, out itemData);

        _enhanceItem.SetItem(item);
        if (itemData != null)
        {
            GetTextMeshPro((int)Texts.Enhance_Item_Name).text = itemData.name;
            GetTextMeshPro((int)Texts.Enhance_Item_Rank).text = item.Grade.ToString();
        }
    }
    private void OnEnhanceButtonClicked()
    {
        if (_selectedItem == null)
        {
            GetTextMeshPro((int)Texts.EnhanceResultText).text = "No item selected.";
            return;
        }

        bool success = EnhanceItem(_selectedItem);
        GetTextMeshPro((int)Texts.EnhanceResultText).text = success ? "Enhancement successful!" : "Enhancement failed.";
    }

    private bool EnhanceItem(Item item)
    {
        // 강화 로직 구현 (성공 확률 등)
        float successRate = 0.5f; // 50% 성공 확률
        bool isSuccess = Random.value < successRate;

        if (isSuccess)
        {
        }
        return isSuccess;
    }

    private void OnClickEnhanceButton()
    {
        EnhanceItem(_selectedItem);
    }

    private void OnClickExitButton()
    {
        enhancePanel.SetActive(false);
    }
}