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
            double rand = 0f; 

            List<ItemRewardData> itemsToAdd = new List<ItemRewardData>();

            foreach (ItemRewardData rewardData in rewards)
            {
                rand = new Random().NextDouble();
                if (rand * 100 <= rewardData.probability)
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
                itemDb.Options = new Dictionary<string, string>();
                ItemData itemData = null;
                DataManager.ItemDict.TryGetValue(item.TemplateId, out itemData);

                item.Rank += 1;

                if (item.ItemType == ItemType.Weapon)
                {
                    WeaponData weaponData = (WeaponData)itemData;
                    itemDb.Damage = (int)(weaponData.damage + (weaponData.damage * 0.5) * item.Rank + (weaponData.damage * enhanceData.value));
                    itemDb.Options = item.Options.ToDictionary(entry => entry.Key, entry => entry.Value);
                }
                else if (item.ItemType == ItemType.Armor)
                {
                    ArmorData armorData = (ArmorData)itemData;
                    itemDb.Defense = (int)(armorData.defense + (armorData.defense * 0.5) * item.Rank + (armorData.defense * enhanceData.value));
                    itemDb.Options = item.Options.ToDictionary(entry => entry.Key, entry => entry.Value);
                }
                else if (item.ItemType == ItemType.Jewelry)
                {
                    Dictionary<string, string> options = new Dictionary<string, string>();
                    foreach (KeyValuePair<string, string> option in item.Options)
                    {
                        string value = option.Value;
                        if (option.Key == ItemOptionType.CriticalChance.ToString())
                        {
                            value = ((int)(int.Parse(option.Value) * (1 + enhanceData.value * 2))).ToString();
                        }
                        else if (option.Key == ItemOptionType.CriticalDamage.ToString())
                        {
                            value = ((int)(int.Parse(option.Value) * (1 + enhanceData.value))).ToString();
                        }
                        else if (option.Key == ItemOptionType.Avoid.ToString())
                        {
                            value = ((int)(int.Parse(option.Value) * (1 + enhanceData.value))).ToString();
                        }
                        else if (option.Key == ItemOptionType.Accuracy.ToString())
                        {
                            value = ((int)(int.Parse(option.Value) * (1 + enhanceData.value))).ToString();
                        }
                        else if (option.Key == ItemOptionType.Hp.ToString())
                        {
                            value = ((int)(int.Parse(option.Value) * (1 + enhanceData.value * 2))).ToString();
                        }
                        else if (option.Key == ItemOptionType.Up.ToString())
                        {
                            value = ((int)(int.Parse(option.Value) * (1 + enhanceData.value))).ToString();
                        }
                        else if (option.Key == ItemOptionType.UpRegen.ToString())
                        {
                            value = ((int)(int.Parse(option.Value) * (1 + enhanceData.value))).ToString();
                        }
                        options.Add(option.Key, value);
                    }
                    itemDb.Options = options;
                }

                itemDb.ItemDbId = item.ItemDbId;
                itemDb.TemplateId = item.TemplateId;
                itemDb.Count = item.Count;
                itemDb.Slot = item.Slot;
                itemDb.Equipped = item.Equipped;
                itemDb.Enhance = item.Rank;
                itemDb.Grade = item.Grade.ToString();
                itemDb.OwnerDbId = player.PlayerDbId;

                return itemDb;
            }
            return null;
        }

        public static Dictionary<string, string> Enchant(Player player, Item item, EnchantData enchantData)
        {
            if (item.Enchant >= (1+(int)item.Grade)) 
            {
                S_SystemNotice systemNotice = new S_SystemNotice();
                systemNotice.Message = "남은 마법부여 횟수가 없습니다.";
                player.Session.Send(systemNotice);
                return null; 
            }
            if (item.ItemType != enchantData.itemType) { return null; }
            Dictionary<string, string> options = new Dictionary<string, string>();
            Random rand = new Random();
            ItemDb itemDb = new ItemDb();
            double probability = 0;
            foreach (var optionData in enchantData.optionData)
            {
                probability += optionData.probability;
                if (rand.NextDouble() <= probability)
                {
                    float gradeBonous = 1 + ((int)item.Grade / 2.0f);
                    int randomValue = rand.Next(optionData.minValue, optionData.maxValue + 1);
                    string optionValue = ((int)(randomValue * gradeBonous)).ToString();
                    options.Add(optionData.option.ToString(), optionValue);
                    break;
                }
            }
            return options;
        }
    }
}
