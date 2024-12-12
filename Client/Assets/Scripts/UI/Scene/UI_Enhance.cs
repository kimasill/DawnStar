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
using static Item;
using static UnityEditor.Progress;

public class UI_Enhance : UI_Base
{
    public UI_ItemProduction ItemProduction;
    public GameObject _enhanceItemPanel;
    public Transform _enhanceItemInfoPanel;
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
        ItemProductionButton,
    }
    enum Images
    {
        EnhanceCostPanel,
        EnhanceResultNoti,
        UI_ItemProduction,
        Enhance_ItemInfoPanel
    }

    bool _isInit = false;
    public bool IsProduction = false;
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
        BindEvent(GetButton((int)Buttons.ItemProductionButton).gameObject, (PointerEventData data) => { OnClickProductionUI(); });
        BindEvent(_enhanceItemPanel, (PointerEventData data) => 
        { 
            if (_selectedItem != null)
            {
                SetItem(null);
                RefreshUI();
            }
                
        });

        BindEvent(gameObject, OnPointerEnter, Define.UIEvent.MouseOver);
        BindEvent(gameObject, OnPointerExit, Define.UIEvent.MouseOut);

        GetImage((int)Images.EnhanceResultNoti).gameObject.SetActive(false);
        ItemProduction = GetImage((int)Images.UI_ItemProduction).GetComponent<UI_ItemProduction>();
        ItemProduction.gameObject.SetActive(false);
        _enhanceCostPanel = GetImage((int)Images.EnhanceCostPanel);
        _enhanceItem = _enhanceItemPanel.GetOrAddComponent<UI_Enhance_Item>();
        _enhanceItemInfoPanel = GetImage((int)Images.Enhance_ItemInfoPanel).transform;

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
            _enhanceItem.SetItem(null);
            return;
        }
        Dictionary<int, EnhanceData> enhanceDataDict = Managers.Data.EnhanceDict;
        EnhanceData enhanceData = null;
        foreach (KeyValuePair<int, EnhanceData> data in enhanceDataDict)
        {
            if (data.Value.itemType == _selectedItem.ItemType && data.Value.rank == _selectedItem.Rank+1)
            {
                enhanceData = data.Value;
            }
        }
        List<CostData> costData = enhanceData.costData;
        SetDisplayItem(costData, _enhanceCostPanel.gameObject.transform);

        DisplayEnhancedStats();
    }
    public void SetDisplayItem(List<CostData> costData, Transform transform)
    {
        foreach (var cost in costData)
        {
            if (Managers.Data.ItemDict.TryGetValue(cost.templateId, out Data.ItemData itemData))
            {
                UI_Display_Item itemIcon = Managers.Resource.Instantiate("UI/Scene/UI_Display_Item", transform).GetComponent<UI_Display_Item>();
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
    private void DisplayEnhancedStats()
    {
        foreach (Transform child in _enhanceItemInfoPanel)
        {
            Destroy(child.gameObject);
        }
        if (_selectedItem == null)
            return;
        ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(_selectedItem.TemplateId, out itemData);
        
        if (itemData == null)
            return;

        if(itemData.itemType == ItemType.Weapon)
        {
            Weapon weapon = _selectedItem as Weapon;
            WeaponData weaponData = itemData as WeaponData;
            AddStat($"공격력 + {weapon.Damage - weaponData.damage}");
        }
        else if (itemData.itemType == ItemType.Armor)
        {
            Armor armor = _selectedItem as Armor;
            ArmorData armorData = itemData as ArmorData;
            AddStat($"방어력 + {armor.Defense - armorData.defense}");
        }

        foreach (var option in itemData.options)
        {
            string baseKey = Content.ConvertSpecialOptions(option.Key);
            int baseValue = int.Parse(option.Value);
            int enhanceValue = 0;
            if (_selectedItem.Options.ContainsKey(option.Key))
            {
                enhanceValue = int.Parse(_selectedItem.Options[option.Key]);
            }

            AddStat($"{baseKey} + {enhanceValue - baseValue}");
        }
    }
    private void AddStat(string statText)
    {
        GameObject statObject = Managers.Resource.Instantiate("UI/Popup/UI_Item_Stat", _enhanceItemInfoPanel.transform);
        UI_Item_Stat stat = statObject.GetOrAddComponent<UI_Item_Stat>();
        stat.Name.text = statText;
    }
    public void SetItem(Item item)
    {
        if(item != null)
        {
            UI_GameScene uI_GameScene = Managers.UI.SceneUI as UI_GameScene;
            if (item.Equipped == true)
            {
                uI_GameScene.NotificationUI.ShowBasicNoti("장착중인 아이템은 강화할 수 없습니다.");
                return;
            }
            else if (item.ItemType != ItemType.Weapon && item.ItemType != ItemType.Armor && item.ItemType != ItemType.Jewelry)
            {                
                uI_GameScene.NotificationUI.ShowBasicNoti("장비 아이템만 강화할 수 있습니다.");
                return;
            }
        }

        _selectedItem = item;
        _enhanceItem.SetItem(item);
        if (item == null)
        {
            GetTextMeshPro((int)Texts.Enhance_Item_Name).text = "이름";
            GetTextMeshPro((int)Texts.Enhance_Item_Rank).text = "단계";
            return;
        }
        else
        {
            ItemData itemData = null;
            Managers.Data.ItemDict.TryGetValue(item.TemplateId, out itemData);
            GetTextMeshPro((int)Texts.Enhance_Item_Name).text = itemData.name;
            GetTextMeshPro((int)Texts.Enhance_Item_Rank).text = $"+{item.Rank.ToString()}" ;
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
        GameObject effect = Managers.Resource.Instantiate("Effect/EnhanceEffect", transform);
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

            //강화 성공시 아이템 정보 갱신
            Managers.Inventory.UpdateItemValue(item);

            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            gameSceneUI.InvenUI.RefreshUI();
            gameSceneUI.StatUI.RefreshUI();
        }
        else if (success == false)
        {
            SetItem(_selectedItem);
        }
    }
    private void OnClickEnhanceButton()
    {
        if (Items.All(x => x.Condition == true))
            EnhanceItem(_selectedItem);
        else
        {
            UI_GameScene uI_GameScene = Managers.UI.SceneUI as UI_GameScene;
            uI_GameScene.NotificationUI.ShowBasicNoti("강화조건이 충족되지 않았습니다.");
        }
    }

    private void OnClickExitButton()
    {
        CloseUI();
    }

    private void OnClickProductionUI()
    {
        if (IsProduction == false)
        {
            OpenProducitonUI();
        }
        else if (IsProduction == true)
        {
            CloseProductionUI();
        }
    }
    public void OpenProducitonUI()
    {
        ItemProduction.gameObject.SetActive(true);
        IsProduction = true;
    }
    public void CloseProductionUI()
    {
        ItemProduction.gameObject.SetActive(false);
        IsProduction = false;
    }
    public void OpenUI(string title, string description)
    {
        gameObject.SetActive(true);
        GetTextMeshPro((int)Texts.EnhanceShopTitle_Text).text = title;
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