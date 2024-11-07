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
    UI_SkillSlot_Icon _helmetSkill;
    public Dictionary<int, UI_SkillSlot_Icon> SkillSlots = new Dictionary<int, UI_SkillSlot_Icon>();

    public override void Init()
    {
        _weaponSkill = transform.GetChild(0).GetComponent<UI_SkillSlot_Icon>();
        _helmetSkill = transform.GetChild(1).GetComponent<UI_SkillSlot_Icon>();
        _ringSkill = transform.GetChild(2).GetComponent<UI_SkillSlot_Icon>();
        SkillSlots.Add(0, _weaponSkill);
        SkillSlots.Add(1, _helmetSkill);
        SkillSlots.Add(2, _ringSkill);
        foreach (var skillSlot in SkillSlots)
        {
            if(skillSlot.Value.IsInit == false)
                skillSlot.Value.Init();
        }
        RefreshUI();
    }

    public void RefreshUI()
    {
        for(int i = 0; i<SkillSlots.Count; i++)
        {
            if(SkillSlots[i].SkillData == null)
            {
                SkillSlots[i].ClearSlot();
            }
            else
            {
                SkillSlots[i].gameObject.SetActive(true);
            }
        }
        if (Managers.Object.MyPlayer)
        {
            foreach (var item in Managers.Inventory.Items)
            {
                if (item.Value.Options.ContainsKey("Skill") && item.Value.Equipped)
                {
                    if (item.Value is Item.Weapon)
                    {
                        _weaponSkill.KeyText.text = Managers.Object.MyPlayer.SkillKeys[0].ToString();
                        if(_weaponSkill.KeyText.text == "None")
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
                            _ringSkill.KeyText.text = Managers.Object.MyPlayer.SkillKeys[2].ToString();
                        if (_ringSkill.KeyText.text == "None")
                            _ringSkill.KeyText.text = "F";
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
                if(((Item.Armor)item).ArmorType == ArmorType.Helmet)
                    _helmetSkill.SetSkill(skillData);
            }
            else if (item is Item.Jewelry)
            {
                if(((Item.Jewelry)item).JewelryType == JewelryType.Ring)
                    _ringSkill.SetSkill(skillData);
            }
        }
        else
        {
            if (item is Item.Weapon)
            {
                _weaponSkill.ClearSlot();
            }
            else if (item is Item.Armor)
            {
                if (((Item.Armor)item).ArmorType == ArmorType.Helmet)
                    _helmetSkill.ClearSlot();
            }
            else if (item is Item.Jewelry)
            {
                if (((Item.Jewelry)item).JewelryType == JewelryType.Ring)
                    _ringSkill.ClearSlot();
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