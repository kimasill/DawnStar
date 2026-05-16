using Data;
using Google.Protobuf.Protocol;
using System.Collections.Generic;
using UnityEngine;

public class UI_SkillSlot : UI_Base
{
    private const string SkillOptionKey = "Skill";
    private const int InvalidSkillSlotIndex = -1;
    private const int WeaponSkillSlotIndex = 0;
    private const int HelmetSkillSlotIndex = 1;
    private const int FirstRingSkillSlotIndex = 2;
    private const int SecondRingSkillSlotIndex = 3;
    private const int NecklaceSkillSlotIndex = 4;

    private static readonly KeyCode[] DefaultSkillKeys =
    {
        KeyCode.E,
        KeyCode.R,
        KeyCode.F,
        KeyCode.T,
        KeyCode.V,
    };

    UI_SkillSlot_Icon _weaponSkill;
    UI_SkillSlot_Icon _ringSkill;
    UI_SkillSlot_Icon _ringSkill2;
    UI_SkillSlot_Icon _helmetSkill;
    UI_SkillSlot_Icon _necklaceSkill;
    public Dictionary<int, UI_SkillSlot_Icon> SkillSlots = new Dictionary<int, UI_SkillSlot_Icon>();
    bool _isInit = false;
    public override void Init()
    {
        if (_isInit)
            return;

        _weaponSkill = CreateSkillSlotIcon();
        _helmetSkill = CreateSkillSlotIcon();
        _ringSkill = CreateSkillSlotIcon();
        _ringSkill2 = CreateSkillSlotIcon();
        _necklaceSkill = CreateSkillSlotIcon();
        SkillSlots.Add(WeaponSkillSlotIndex, _weaponSkill);
        SkillSlots.Add(HelmetSkillSlotIndex, _helmetSkill);
        SkillSlots.Add(FirstRingSkillSlotIndex, _ringSkill);
        SkillSlots.Add(SecondRingSkillSlotIndex, _ringSkill2);
        SkillSlots.Add(NecklaceSkillSlotIndex, _necklaceSkill);
        foreach (var skillSlot in SkillSlots)
        {
            if (skillSlot.Value.IsInit == false)
                skillSlot.Value.Init();
        }
        _isInit = true;
        RefreshUI();
    }
    private UI_SkillSlot_Icon CreateSkillSlotIcon()
    {
        GameObject go = Managers.Resource.Instantiate("UI/Scene/UI_SkillSlot_Icon", transform);
        UI_SkillSlot_Icon slot = go.GetComponent<UI_SkillSlot_Icon>();
        return slot;
    }
    public void RefreshUI()
    {
        if(_isInit == false)
        {
            Init();
            return;
        }

        ClearSkillSlots();

        MyPlayerController myPlayer = Managers.Object.MyPlayer;
        if (myPlayer == null)
            return;

        int ringCount = 0;
        foreach (var itemPair in Managers.Inventory.Items)
        {
            Item item = itemPair.Value;
            if (IsEquippedSkillItem(item) == false)
                continue;

            if (TryGetSkillData(item, out SkillData skillData) == false)
                continue;

            int skillSlotIndex = GetSkillSlotIndex(item, ref ringCount);
            if (skillSlotIndex == InvalidSkillSlotIndex)
                continue;

            if (SetSkill(skillSlotIndex, skillData))
                SetSkillKeyText(skillSlotIndex, myPlayer);
        }
    }

    public void SetSkill(Item item)
    {
        if (TryGetSkillData(item, out SkillData skillData) == false)
            return;

        int skillSlotIndex = GetSkillSlotIndex(item);
        SetSkill(skillSlotIndex, skillData);
    }

    private bool SetSkill(int skillSlotIndex, SkillData skillData)
    {
        if (skillData == null)
            return false;

        if (TryGetSkillSlot(skillSlotIndex, out UI_SkillSlot_Icon skillSlot) == false)
            return false;

        skillSlot.SetSkill(skillData);
        return true;
    }

    private void ClearSkillSlots()
    {
        foreach (var skillSlot in SkillSlots.Values)
        {
            skillSlot.ClearSlot();
        }
    }

    private bool IsEquippedSkillItem(Item item)
    {
        return item != null && item.Equipped && item.Options.ContainsKey(SkillOptionKey);
    }

    private bool TryGetSkillData(Item item, out SkillData skillData)
    {
        skillData = null;

        if (item == null)
            return false;

        if (item.Options.TryGetValue(SkillOptionKey, out string skillIdText) == false)
            return false;

        if (int.TryParse(skillIdText, out int skillId) == false)
            return false;

        return Managers.Data.SkillDict.TryGetValue(skillId, out skillData);
    }

    private int GetSkillSlotIndex(Item item)
    {
        switch (item)
        {
            case Item.Weapon _:
                return WeaponSkillSlotIndex;
            case Item.Armor armor:
                return armor.ArmorType == ArmorType.Helmet ? HelmetSkillSlotIndex : InvalidSkillSlotIndex;
            case Item.Jewelry jewelry:
                return GetJewelrySkillSlotIndex(jewelry);
            default:
                return InvalidSkillSlotIndex;
        }
    }

    private int GetSkillSlotIndex(Item item, ref int ringCount)
    {
        switch (item)
        {
            case Item.Weapon _:
                return WeaponSkillSlotIndex;
            case Item.Armor armor:
                return armor.ArmorType == ArmorType.Helmet ? HelmetSkillSlotIndex : InvalidSkillSlotIndex;
            case Item.Jewelry jewelry:
                return GetJewelrySkillSlotIndex(jewelry, ref ringCount);
            default:
                return InvalidSkillSlotIndex;
        }
    }

    private int GetJewelrySkillSlotIndex(Item.Jewelry jewelry)
    {
        switch (jewelry.JewelryType)
        {
            case JewelryType.Ring:
                if (TryGetSkillSlot(FirstRingSkillSlotIndex, out UI_SkillSlot_Icon firstRingSlot) == false)
                    return InvalidSkillSlotIndex;

                return firstRingSlot.SkillData == null ? FirstRingSkillSlotIndex : SecondRingSkillSlotIndex;
            case JewelryType.Necklace:
                return NecklaceSkillSlotIndex;
            default:
                return InvalidSkillSlotIndex;
        }
    }

    private int GetJewelrySkillSlotIndex(Item.Jewelry jewelry, ref int ringCount)
    {
        switch (jewelry.JewelryType)
        {
            case JewelryType.Ring:
                return GetRingSkillSlotIndex(ref ringCount);
            case JewelryType.Necklace:
                return NecklaceSkillSlotIndex;
            default:
                return InvalidSkillSlotIndex;
        }
    }

    private int GetRingSkillSlotIndex(ref int ringCount)
    {
        if (ringCount == 0)
        {
            ringCount++;
            return FirstRingSkillSlotIndex;
        }

        if (ringCount == 1)
        {
            ringCount++;
            return SecondRingSkillSlotIndex;
        }

        return InvalidSkillSlotIndex;
    }

    private bool TryGetSkillSlot(int skillSlotIndex, out UI_SkillSlot_Icon skillSlot)
    {
        if (skillSlotIndex == InvalidSkillSlotIndex)
        {
            skillSlot = null;
            return false;
        }

        return SkillSlots.TryGetValue(skillSlotIndex, out skillSlot);
    }

    private void SetSkillKeyText(int skillSlotIndex, MyPlayerController myPlayer)
    {
        if (TryGetSkillSlot(skillSlotIndex, out UI_SkillSlot_Icon skillSlot) == false)
            return;

        skillSlot.KeyText.text = GetSkillKeyText(skillSlotIndex, myPlayer);
    }

    private string GetSkillKeyText(int skillSlotIndex, MyPlayerController myPlayer)
    {
        if (myPlayer.SkillKeys != null &&
            skillSlotIndex >= 0 &&
            skillSlotIndex < myPlayer.SkillKeys.Length &&
            myPlayer.SkillKeys[skillSlotIndex] != KeyCode.None)
        {
            return myPlayer.SkillKeys[skillSlotIndex].ToString();
        }

        if (skillSlotIndex < 0 || skillSlotIndex >= DefaultSkillKeys.Length)
            return KeyCode.None.ToString();

        return DefaultSkillKeys[skillSlotIndex].ToString();
    }

    public SkillData GetSkill(int index)
    {
        return SkillSlots[index].SkillData;
    }

    public void StartCooldown(int skillId, float cooldownTime)
    {
        foreach (var skillSlot in SkillSlots)
        {
            if (skillSlot.Value.SkillData != null && skillSlot.Value.SkillData.id == skillId)
            {
                skillSlot.Value.StartCooldown(cooldownTime);
            }
        }
    }
}
