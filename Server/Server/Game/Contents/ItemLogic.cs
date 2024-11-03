using Server.Data;
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
            int sum = 0;
            List<ItemRewardData> itemsToAdd = new List<ItemRewardData>();

            foreach (ItemRewardData rewardData in rewards)
            {
                sum += rewardData.probability;
                if (rand <= sum)
                {
                    itemsToAdd.Add(rewardData);
                }
            }

            rewards.AddRange(itemsToAdd);
            return rewards;
        }

        public static int GetRewardCount(ItemRewardData rewardData)
        {
            int min = rewardData.minCount;
            int max = rewardData.maxCount;
            return new Random().Next(min, max + 1);
        }
    }
}
