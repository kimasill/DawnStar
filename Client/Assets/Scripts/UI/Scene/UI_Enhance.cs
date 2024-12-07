using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tiled2Unity;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class UI_Enhance : UI_Base
{
    public GameObject _enhanceItemPanel;
    public Button enhanceButton;        
    private Item _selectedItem;
    private UI_Enhance_Item _enhanceItem;
    private Image _enhanceCostPanel;
    private List<UI_Display_Item> Items = new List<UI_Display_Item>();
    enum Texts
    {
        EnhanceShopTitle_Text,
        Enhance_Item_Name,
        EnhanceResultNoti_Text,
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
        EnhanceResultNoti,
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
        GetImage((int)Images.EnhanceResultNoti).gameObject.SetActive(false);
        _enhanceCostPanel = GetImage((int)Images.EnhanceCostPanel);
        _enhanceItem = _enhanceItemPanel.GetOrAddComponent<UI_Enhance_Item>();

        RefreshUI();
    }
    public void RefreshUI()
    {
        if (_isInit == false)
        {
            Init();
            return;
        }
        foreach(var item in Items)
        {
            Destroy(item.gameObject);
        }
        Items.Clear();
        if(_selectedItem == null)
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
    public void SetItem(Item item)
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
        RefreshUI();
    }
    private void EnhanceItem(Item item)
    {
        if (item == null)
            return;
        C_Enhance enhance = new C_Enhance() 
        {
            ItemDbId = item.ItemDbId,
            TemplateId = item.TemplateId
        };
        Managers.Network.Send(enhance);
    }
    public void EnhanceResult(S_Enhance enhance)
    {
        if (enhance == null)
            return;
        StartCoroutine(CoEnhance(enhance.Success, enhance.ItemInfo));
    }
    private IEnumerator CoEnhance(bool success, ItemInfo itemInfo)
    {
        GameObject effect = Managers.Resource.Instantiate("UI/Scene/EnhanceEffect", transform);
        Animator anim = effect.GetComponent<Animator>();
        anim.Play("START");

        yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f);

        Managers.Resource.Destroy(effect);
        anim.StopPlayback(); // Stop the animation playback

        GetImage((int)Images.EnhanceResultNoti).gameObject.SetActive(true);
        FadeInAll(GetImage((int)Images.EnhanceResultNoti).gameObject, 0.5f);
        if (success)
        {
            GetTextMeshPro((int)Texts.EnhanceResultNoti_Text).text = "강화 성공";
        }
        else
        {
            GetTextMeshPro((int)Texts.EnhanceResultNoti_Text).text = "강화 실패";
        }
        yield return new WaitForSeconds(1.0f);
        FadeOutAll(GetImage((int)Images.EnhanceResultNoti).gameObject, 0.5f);
        GetImage((int)Images.EnhanceResultNoti).gameObject.SetActive(false);


        if (success)
        {
            Item item = Item.MakeItem(itemInfo);
            SetItem(item);
        }
        RefreshUI();
    }
    private void OnClickEnhanceButton()
    {
        EnhanceItem(_selectedItem);
    }

    private void OnClickExitButton()
    {
        CloseUI();
    }
    public void OpenUI(string title, string description)
    {
        gameObject.SetActive(true);
        GetText((int)Texts.EnhanceShopTitle_Text).text = title;
    }
    public void CloseUI()
    {
        _selectedItem = null;
        _enhanceItem.SetItem(null);
        GetTextMeshPro((int)Texts.Enhance_Item_Name).text = "";
        GetTextMeshPro((int)Texts.Enhance_Item_Rank).text = "";
        gameObject.SetActive(false);
    }
}