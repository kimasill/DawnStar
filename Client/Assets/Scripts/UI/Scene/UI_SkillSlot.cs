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
        RefreshUI();
    }

    public void RefreshUI()
    {
        for(int i = 0; i<SkillSlots.Count; i++)
        {
            SkillSlots[i].KeyText.text = Managers.Object.MyPlayer.SkillKeys[i].ToString();
            if(SkillSlots[i].SkillData == null)
            {
                SkillSlots[i].ClearSlot();
            }
            else
            {
                SkillSlots[i].gameObject.SetActive(true);
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
}