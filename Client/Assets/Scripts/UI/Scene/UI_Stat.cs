using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Item;

public class UI_Stat : UI_Base
{
    enum Texts
    {
        NameText,
        AttackValueText,
        DefenceValueText
    }
    enum Images
    {
        Slot_Helmet,
        Slot_Armor,
        Slot_Weapon,
        Slot_Shield,
        Slot_Boots
    }
    bool _init = false;
    public override void Init()
    {
        Bind<Text>(typeof(Texts));
        Bind<Image>(typeof(Images));
        _init = true;
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (_init == false)
            return;

        //먼저 items 다 가린다
        Get<Image>((int)Images.Slot_Helmet).enabled = false;
        Get<Image>((int)Images.Slot_Armor).enabled = false;
        Get<Image>((int)Images.Slot_Weapon).enabled = false;
        Get<Image>((int)Images.Slot_Shield).enabled = false;
        Get<Image>((int)Images.Slot_Boots).enabled = false;

        foreach(Item item in Managers.Inventory.Items.Values)
        {
            if(item.Equipped == false)
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
                }
            }
            //Text 설정
            MyPlayerController player = Managers.Object.MyPlayer;
            player.RefreshAdditionalStat();

            Get<Text>((int)Texts.NameText).text = player.name;
            int totalDamage = player.Stat.Attack + player.WeaponDamage;
            Get<Text>((int)Texts.AttackValueText).text = $"{totalDamage}(+{player.WeaponDamage})";
            Get<Text>((int)Texts.DefenceValueText).text = $"{player.ArmorDef}";
        }
    }
}
