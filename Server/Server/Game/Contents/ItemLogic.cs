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
    }
}
