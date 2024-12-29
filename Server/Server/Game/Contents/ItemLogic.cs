using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using Server.Data;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class ItemLogic
    {
        public static List<ItemRewardData> GetRandomReward(List<ItemRewardData> rewards)
        {
            int rand = new Random().Next(0, 101);
            
            List<ItemRewardData> itemsToAdd = new List<ItemRewardData>();

            foreach (ItemRewardData rewardData in rewards)
            {
                if (rand <= rewardData.probability)
                {
                    itemsToAdd.Add(rewardData);
                }
            }

            return itemsToAdd;
        }

        public static int GetRewardCount(ItemRewardData rewardData)
        {
            int min = rewardData.minCount;
            int max = rewardData.maxCount;
            return new Random().Next(min, max + 1);
        }

        public static ItemDb EnhanceItem(Player player, Item item, EnhanceData enhanceData)
        {
            Random rand = new Random();
            double chance = rand.NextDouble();
            if (chance <= enhanceData.percent)
            {
                ItemDb itemDb = new ItemDb();
                ItemData itemData = null;
                DataManager.ItemDict.TryGetValue(item.TemplateId, out itemData);

                item.Rank += 1;

                if (item.ItemType == ItemType.Weapon)
                {
                    WeaponData weaponData = (WeaponData)itemData;
                    itemDb.Damage = (int)(weaponData.damage + (weaponData.damage*0.5)*item.Rank + (weaponData.damage* enhanceData.value));
                }
                else if(item.ItemType == ItemType.Armor)
                {
                    ArmorData armorData = (ArmorData)itemData;
                    itemDb.Defense = (int)(armorData.defense + (armorData.defense * 0.5)*item.Rank + (armorData.defense * enhanceData.value));
                }

                foreach(KeyValuePair<string, string> option in item.Options)
                {
                    string value = option.Value;
                    if (option.Key == ItemOptionType.CriticalChance.ToString())
                    {
                        value = (float.Parse(option.Value) * (enhanceData.value * 2)).ToString();
                    }
                    else if (option.Key == ItemOptionType.CriticalDamage.ToString())
                    {
                        value = (int.Parse(option.Value) * enhanceData.value).ToString();
                    }
                    else if (option.Key == ItemOptionType.Avoid.ToString())
                    {
                        value = (int.Parse(option.Value) * enhanceData.value).ToString();
                    }
                    else if (option.Key == ItemOptionType.Accuracy.ToString())
                    {
                        value = (int.Parse(option.Value) * enhanceData.value).ToString();
                    }
                    else if (option.Key == ItemOptionType.Hp.ToString())
                    {
                        value = (int.Parse(option.Value) * (enhanceData.value*2)).ToString();
                    }
                    else if (option.Key == ItemOptionType.Up.ToString())
                    {
                        value = (int.Parse(option.Value) * enhanceData.value).ToString();
                    }
                    else if (option.Key == ItemOptionType.UpRegen.ToString())
                    {
                        value = (int.Parse(option.Value) * enhanceData.value).ToString();
                    }
                    itemDb.Options.Add(option.Key, value);
                }

                itemDb.ItemDbId = item.ItemDbId;
                itemDb.TemplateId = item.TemplateId;
                itemDb.Count = item.Count;
                itemDb.Slot = item.Slot;
                itemDb.Equipped = item.Equipped;
                itemDb.Enhance = item.Rank;
                //itemDb.Grade = item.Grade,
                itemDb.OwnerDbId = player.PlayerDbId;
                
                return itemDb;
            }
            return null;
        }

        public static Dictionary<string,string> Enchant(Player player, Item item, EnchantData enchantData)
        {
            if(item.Enchant >= 4){return null;}
            if(item.ItemType != enchantData.itemType) { return null; }
            Dictionary<string, string> options = new Dictionary<string, string>();
            Random rand = new Random();
            ItemDb itemDb = new ItemDb();
            double probability = 0;
            foreach (var optionData in enchantData.optionData)
            {
                probability += optionData.probability;
                if (rand.NextDouble() <= probability)
                {
                    string optionValue = rand.Next(optionData.minValue, optionData.maxValue).ToString();
                    options.Add(optionData.option.ToString(), optionValue);
                    break;
                }
            }
            return options;
        }
    }
}
