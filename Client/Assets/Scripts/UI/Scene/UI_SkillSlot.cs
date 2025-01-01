using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SkillSlot : UI_Base
{
    UI_SkillSlot_Icon _weaponSkill;
    UI_SkillSlot_Icon _ringSkill;
    UI_SkillSlot_Icon _ringSkill2;
    UI_SkillSlot_Icon _helmetSkill;
    UI_SkillSlot_Icon _necklaceSkill;
    public Dictionary<int, UI_SkillSlot_Icon> SkillSlots = new Dictionary<int, UI_SkillSlot_Icon>();
    bool _isInit = false;
    public override void Init()
    {
        _weaponSkill = CreateSkillSlotIcon();
        _helmetSkill = CreateSkillSlotIcon();
        _ringSkill = CreateSkillSlotIcon();
        _ringSkill2 = CreateSkillSlotIcon();
        _necklaceSkill = CreateSkillSlotIcon();
        SkillSlots.Add(0, _weaponSkill);
        SkillSlots.Add(1, _helmetSkill);
        SkillSlots.Add(2, _ringSkill);
        SkillSlots.Add(3, _ringSkill2);
        SkillSlots.Add(4, _necklaceSkill);
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
            _isInit = true;
        }
        for (int i = 0; i < SkillSlots.Count; i++)
        {
            SkillSlots[i].ClearSlot();
        }
        if (Managers.Object.MyPlayer)
        {
            int ringCount = 0;
            foreach (var item in Managers.Inventory.Items)
            {
                if (item.Value.Options.ContainsKey("Skill") && item.Value.Equipped)
                {
                    if (item.Value is Item.Weapon)
                    {
                        _weaponSkill.KeyText.text = Managers.Object.MyPlayer.SkillKeys[0].ToString();
                        if (_weaponSkill.KeyText.text == "None")
                            _weaponSkill.KeyText.text = "E";
                    }
                    else if (item.Value is Item.Armor)
                    {
                        if (((Item.Armor)item.Value).ArmorType == ArmorType.Helmet)
                            _helmetSkill.KeyText.text = Managers.Object.MyPlayer.SkillKeys[1].ToString();
                        if (_helmetSkill.KeyText.text == "None")
                            _helmetSkill.KeyText.text = "R";
                    }
                    else if (item.Value is Item.Jewelry)
                    {
                        if (((Item.Jewelry)item.Value).JewelryType == JewelryType.Ring)
                        {
                            if (ringCount == 0)
                            {
                                _ringSkill.KeyText.text = Managers.Object.MyPlayer.SkillKeys[2].ToString();
                                if (_ringSkill.KeyText.text == "None")
                                    _ringSkill.KeyText.text = "F";
                                ringCount++;
                            }
                            else if (ringCount == 1)
                            {
                                _ringSkill2.KeyText.text = Managers.Object.MyPlayer.SkillKeys[4].ToString();
                                if (_ringSkill2.KeyText.text == "None")
                                    _ringSkill2.KeyText.text = "V";
                                ringCount++;
                            }
                        }
                        else if (((Item.Jewelry)item.Value).JewelryType == JewelryType.Necklace)
                        {
                            _necklaceSkill.KeyText.text = Managers.Object.MyPlayer.SkillKeys[3].ToString();
                            if (_necklaceSkill.KeyText.text == "None")
                                _necklaceSkill.KeyText.text = "T";
                        }
                    }

                    SetSkill(item.Value);
                }
            }
        }
    }

    public void SetSkill(Item item)
    {
        if (item.Options.ContainsKey("Skill"))
        {
            int skillId = int.Parse(item.Options["Skill"]);
            SkillData skillData = null;
            Managers.Data.SkillDict.TryGetValue(skillId, out skillData);

            if (item is Item.Weapon)
            {
                _weaponSkill.SetSkill(skillData);
            }
            else if (item is Item.Armor)
            {
                if (((Item.Armor)item).ArmorType == ArmorType.Helmet)
                    _helmetSkill.SetSkill(skillData);
            }
            else if (item is Item.Jewelry)
            {
                if (((Item.Jewelry)item).JewelryType == JewelryType.Ring)
                {
                    if (_ringSkill.SkillData == null)
                        _ringSkill.SetSkill(skillData);
                    else
                        _ringSkill2.SetSkill(skillData);
                }
                else if (((Item.Jewelry)item).JewelryType == JewelryType.Necklace)
                {
                    _necklaceSkill.SetSkill(skillData);
                }
            }
        }
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