using PokemonRandomizer.Backend.DataStructures;
using System;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.Randomization
{
    public class BattleTentRandomizer
    {
        private static readonly ItemRandomizer.Settings itemRandSettings = new ItemRandomizer.Settings()
        {
            NoneToOtherChance = 1,
        };
        private readonly ItemRandomizer itemRand;
        private readonly List<Action> delayedRandomizationCalls;
        public BattleTentRandomizer(ItemRandomizer itemRand, List<Action> delayedRandomizationCalls)
        {
            this.itemRand = itemRand;
            this.delayedRandomizationCalls = delayedRandomizationCalls;
        }
        public void RandomizeBattleTent(BattleTent battleTent, IEnumerable<ItemData> allItems)
        {
            // TODO: expand rewards (if desired) - just pad the reward list with Item.None
            // Randomize Rewards
            void RandomizeRewards()
            {
                for (int i = 0; i < battleTent.Rewards.Count; ++i)
                {
                    battleTent.Rewards[i] = itemRand.RandomItem(allItems, battleTent.Rewards[i], itemRandSettings);
                }
            }
            delayedRandomizationCalls.Add(RandomizeRewards);
        }
    }
}
