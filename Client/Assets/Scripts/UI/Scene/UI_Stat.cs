using Data;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Item;

public class UI_Stat : UI_Base
{
    enum Texts
    {
        NameText,
        AttackValueText,
        DefenseValueText,
        HPValueText,
        AttackSpeedValueText,
        CriticalChanceValueText,
        CriticalDamageValueText,
        AvoidValueText,
        AccuracyValueText,
        UPValueText,
        UPRegenValueText,
        HPRegenValueText,
        MoveSpeedValueText,
        RageValueText,
        ReasonValueText,
        UnchartedValueText,
        TruthValueText,
        MaxPotionValueText,
        StatPointText,
        StatPointText_Warning,
        Card_DescriptionText,
        BaseStatText,
        SpecialStatText,        
    }
    enum Images
    {
        Slot_Helmet,
        //Slot_Cloth,
        Slot_Armor,
        Slot_Weapon,
        Slot_Shield,
        Slot_Boots,
        Slot_Back,
        Slot_Earing,
        Slot_Ring,
        Slot_Ring2,
        Slot_Necklace,
        Card_Uncharted,
        Card_Rage,        
        Card_Reason,
        Card_Truth,
        Card_Panel,
        Card_DescriptionPanel,
        BaseStatPanel,
        SpecialStatPanel,        
    }

    enum Objects
    {
        InfoButton,
        StatPointButton,
    }
    bool _init = false;
    bool _isCardPanelActive = false;
    bool _isBaseStatPanelActive = true;
    bool _isSpecialStatPanelActive = false;
    public List<GameObject> CardObjects = new List<GameObject>();
    int _statPoint = 0;
    public override void Init()
    {
        if(_init)
            return;
        _init = true;

        Bind<TMP_Text>(typeof(Texts)); // Change TMPro to TextMeshProUGUI
        Bind<Image>(typeof(Images));
        Bind<GameObject>(typeof(Objects));
        BindEvent(GetObject((int)Objects.InfoButton), (PointerEventData data) => { OnClickInfoButton();});
        BindEvent(GetObject((int)Objects.StatPointButton), (PointerEventData data) => { OnClickStatPointButton(); });
        CardObjects.Add(GetImage((int)Images.Card_Rage).gameObject);
        CardObjects.Add(GetImage((int)Images.Card_Reason).gameObject);
        CardObjects.Add(GetImage((int)Images.Card_Uncharted).gameObject);
        CardObjects.Add(GetImage((int)Images.Card_Truth).gameObject);
        
        for (int i = 0; i < CardObjects.Count; i++)
        {
            int index = i+1;
            GameObject card = CardObjects[i];
            
            BindEvent(card, (PointerEventData data) => { OnClickCard(index); });
            BindEvent(card, (PointerEventData data) => { OnMouseOverCard(index); }, Define.UIEvent.MouseOver);
            BindEvent(card, (PointerEventData data) => { OnMouseExitCard(); }, Define.UIEvent.MouseOut);    
        }
        ShowImage((int)Images.Card_Panel, false);
        ShowImage((int)Images.Card_DescriptionPanel, false);

        GetImage((int)Images.BaseStatPanel).gameObject.SetActive(true);
        GetTextMeshPro((int)Texts.BaseStatText).gameObject.SetActive(true);
        GetImage((int)Images.SpecialStatPanel).gameObject.SetActive(false);
        GetTextMeshPro((int)Texts.SpecialStatText).gameObject.SetActive(false);

        RefreshUI();
    }

    private void OnClickCard(int index)
    {
        if (index < 1 || index >= CardObjects.Count + 1)
        {
            Debug.LogError("Invalid card index");
            return;
        }
        MyPlayerController player = Managers.Object.MyPlayer;
        if (player == null)
            return;
        C_SelectStat selectStat = new C_SelectStat();
        selectStat.TemplateId = index;
        Managers.Network.Send(selectStat);
        StartCoroutine(ChangeAndResetCardSize(CardObjects[index-1]));
        _statPoint--;
        if (_statPoint <= 0)
        {
            OnClickStatPointButton();
        }
    }
    private void OnClickStatPointButton()
    {
        MyPlayerController player = Managers.Object.MyPlayer;
        if (player == null)
            return;
        
        if (_statPoint > 0 && _isCardPanelActive == false)
        {
            ShowImage((int)Images.Card_Panel, true);
            ShowImage((int)Images.Card_DescriptionPanel, true);
            _isCardPanelActive = true;
        }
        else if(_isCardPanelActive == true)
        {
            ShowImage((int)Images.Card_Panel, false);
            ShowImage((int)Images.Card_DescriptionPanel, false);
            _isCardPanelActive = false;
        }
        else if(_isCardPanelActive == false && _statPoint <= 0)
        {
            GameObject warningText = GetTextMeshPro((int)Texts.StatPointText_Warning).gameObject;
            StartCoroutine(CoNotificationText(warningText));
        }
    }
    private void OnClickInfoButton()
    {
        if(_isBaseStatPanelActive == false)
        {
            ShowImage((int)Images.SpecialStatPanel, false);
            GetTextMeshPro((int)Texts.SpecialStatText).gameObject.SetActive(false);

            ShowImage((int)Images.BaseStatPanel, true);
            GetTextMeshPro((int)Texts.BaseStatText).gameObject.SetActive(true);
            _isBaseStatPanelActive = true;
            _isSpecialStatPanelActive = false;
        }
        else
        {
            ShowImage((int)Images.BaseStatPanel, false);
            GetTextMeshPro((int)Texts.BaseStatText).gameObject.SetActive(false);
            
            ShowImage((int)Images.SpecialStatPanel, true);
            GetTextMeshPro((int)Texts.SpecialStatText).gameObject.SetActive(true);
            _isBaseStatPanelActive = false;
            _isSpecialStatPanelActive = true;
        }
    }
    private void OnMouseOverCard(int index)
    {
        if (index < 1 || index >= CardObjects.Count+1)
        {
            Debug.LogError("Invalid card index");
            return;
        }
        Managers.Data.RealizationDict.TryGetValue(index, out RealizationData realization);
        if (realization == null)
            return;

        string description = "";
        description += realization.script[0] + "\n";
        for (int i = 0; i < realization.specialStatDatas.Count; i++)
        {
            description += ReplacePlaceholders(realization.script[i+1], realization.specialStatDatas[i]) + "\n";
        }

        Get<TMP_Text>((int)Texts.Card_DescriptionText).text = description;
        ShowImage((int)Images.Card_DescriptionPanel, true);
    }
    private void OnMouseExitCard()
    {
        ShowImage((int)Images.Card_DescriptionPanel, false);
    }
    private string ReplacePlaceholders(string line, SpecialStatData data)
    {
        MyPlayerController player = Managers.Object.MyPlayer;
        if (player == null)
            return line;
        line = line.Replace("_point_", data.point.ToString());
        line = line.Replace("_stat_", data.name.ToString());
        line = line.Replace("_value_", data.value.ToString());
        return line;
    }
    public void RefreshUI()
    {
        if (_init == false)
        {
            Init();
            return;
        }
        Get<TMP_Text>((int)Texts.StatPointText_Warning).gameObject.SetActive(false);
        foreach (Item item in Managers.Inventory.Items.Values)
        {
            if (item.Equipped == false)
                continue;
            ItemData itemData = null;
            Managers.Data.ItemDict.TryGetValue(item.TemplateId, out itemData);
            Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);

            if (item.ItemType == ItemType.Weapon)
            {
                Get<Image>((int)Images.Slot_Weapon).enabled = true;
                Get<Image>((int)Images.Slot_Weapon).sprite = icon;
            }
            else if (item.ItemType == ItemType.Armor)
            {
                Armor armor = (Armor)item;
                switch (armor.ArmorType)
                {
                    case ArmorType.Helmet:
                        Get<Image>((int)Images.Slot_Helmet).enabled = true;
                        Get<Image>((int)Images.Slot_Helmet).sprite = icon;
                        break;
                    case ArmorType.Armor:
                        Get<Image>((int)Images.Slot_Armor).enabled = true;
                        Get<Image>((int)Images.Slot_Armor).sprite = icon;
                        break;
                    case ArmorType.Boots:
                        Get<Image>((int)Images.Slot_Boots).enabled = true;
                        Get<Image>((int)Images.Slot_Boots).sprite = icon;
                        break;
                    case ArmorType.Back:
                        Get<Image>((int)Images.Slot_Back).enabled = true;
                        Get<Image>((int)Images.Slot_Back).sprite = icon;
                        break;
                }
            }
            else if (item.ItemType == ItemType.Jewelry)
            {
                Jewelry jewelry = (Jewelry)item;
                switch (jewelry.JewelryType)
                {
                    case JewelryType.Earring:
                        Get<Image>((int)Images.Slot_Earing).enabled = true;
                        Get<Image>((int)Images.Slot_Earing).sprite = icon;
                        break;
                    case JewelryType.Ring:
                        if (Get<Image>((int)Images.Slot_Ring).enabled == false)
                        {
                            Get<Image>((int)Images.Slot_Ring).enabled = true;
                            Get<Image>((int)Images.Slot_Ring).sprite = icon;
                        }
                        else
                        {
                            Get<Image>((int)Images.Slot_Ring2).enabled = true;
                            Get<Image>((int)Images.Slot_Ring2).sprite = icon;
                        }
                        break;
                    case JewelryType.Necklace:
                        Get<Image>((int)Images.Slot_Necklace).enabled = true;
                        Get<Image>((int)Images.Slot_Necklace).sprite = icon;
                        break;
                }
            }
                //Text ¼³Á¤
            MyPlayerController player = Managers.Object.MyPlayer;
            player.RefreshAdditionalStat();

            Get<TMP_Text>((int)Texts.NameText).text = player.name;
            Get<TMP_Text>((int)Texts.AttackValueText).text = $"{player.Stat.Attack} + ({player.AdditionalAttack})";
            Get<TMP_Text>((int)Texts.DefenseValueText).text = $"{player.Stat.Defense} + ({player.AdditionalDefense})";
            Get<TMP_Text>((int)Texts.HPValueText).text = $"{player.Stat.MaxHp}  + ({player.AdditionalHp})";
            Get<TMP_Text>((int)Texts.AttackSpeedValueText).text = $"{player.Stat.AttackSpeed} + ({player.AdditionalAttackSpeed})/s";
            Get<TMP_Text>((int)Texts.CriticalChanceValueText).text = $"{player.Stat.CriticalChance} + ({player.AdditionalCriticalChance})%";
            Get<TMP_Text>((int)Texts.CriticalDamageValueText).text = $"{player.Stat.CriticalDamage} + ({player.AdditionalCriticalDamage})%";
            Get<TMP_Text>((int)Texts.AvoidValueText).text = $"{player.Stat.Avoid} + ({player.AdditionalAvoidance})%";
            Get<TMP_Text>((int)Texts.AccuracyValueText).text = $"{player.Stat.Accuracy} + ({player.AdditionalAccuracy})%";
            Get<TMP_Text>((int)Texts.UPValueText).text = $"{player.Stat.Up} + ({player.AdditionalUp})";
            Get<TMP_Text>((int)Texts.UPRegenValueText).text = $"{player.Stat.UpRegen}/s";
            Get<TMP_Text>((int)Texts.HPRegenValueText).text = $"{player.Stat.HpRegen}/s";
            Get<TMP_Text>((int)Texts.MoveSpeedValueText).text = $"{player.Stat.Speed} + ({player.AdditionalSpeed})";
            Get<TMP_Text>((int)Texts.RageValueText).text = $"{player.Stat.Realizations[0]}";
            Get<TMP_Text>((int)Texts.ReasonValueText).text = $"{player.Stat.Realizations[1]}";
            Get<TMP_Text>((int)Texts.UnchartedValueText).text = $"{player.Stat.Realizations[2]}";
            Get<TMP_Text>((int)Texts.TruthValueText).text = $"{player.Stat.Realizations[3]}";
            Get<TMP_Text>((int)Texts.MaxPotionValueText).text = $"{player.Stat.MaxPotion}";
            Get<TMP_Text>((int)Texts.StatPointText).text = $"{player.Stat.StatPoint}";
            Get<TMP_Text>((int)Texts.StatPointText_Warning).gameObject.SetActive(false);

            if (_isSpecialStatPanelActive == true)
                OnClickInfoButton();

            if (player.Stat.StatPoint > 0 && _isCardPanelActive == false)
            {
                _statPoint = player.Stat.StatPoint;
                ShowImage((int)Images.Card_Panel, true);
                _isCardPanelActive = true;
            }
        }
    }
    private IEnumerator ChangeAndResetCardSize(GameObject obj)
    {
        yield return StartCoroutine(ChangeCardSize(obj));
        if (_statPoint > 0)
        {
            yield return StartCoroutine(ResetCardSize(obj));
        }
    }
    private IEnumerator ChangeCardSize(GameObject obj)
    {
        Vector3 originalScale = obj.transform.localScale;
        Vector3 targetScale = originalScale * 0.9f;
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            obj.transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.transform.localScale = targetScale;
    }
    private IEnumerator ResetCardSize(GameObject obj)
    {
        Vector3 targetScale = Vector3.one;
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            obj.transform.localScale = Vector3.Lerp(obj.transform.localScale, targetScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.transform.localScale = targetScale;
    }
    private IEnumerator CoNotificationText(GameObject obj)
    {
        obj.SetActive(true);
        TMP_Text text = obj.GetComponent<TMP_Text>();
        if (text == null)
        {
            yield break;
        }

        float fadeInDuration = 0.5f;
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            Color color = text.color;
            color.a = Mathf.Lerp(0, 1, elapsed / fadeInDuration);
            text.color = color;
            elapsed += Time.deltaTime;
            yield return null;
        }
        Color finalColor = text.color;
        finalColor.a = 1;
        text.color = finalColor;

        yield return new WaitForSeconds(2.0f);

        float fadeOutDuration = 0.5f;
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            Color color = text.color;
            color.a = Mathf.Lerp(1, 0, elapsed / fadeOutDuration);
            text.color = color;
            elapsed += Time.deltaTime;
            yield return null;
        }
        finalColor.a = 0;
        text.color = finalColor;
        obj.SetActive(false);
    }
    public void ShowImage(int idx, bool activate)
    {
        GetImage(idx).gameObject.SetActive(activate);
        GetImage(idx).raycastTarget = activate;
    }
}
