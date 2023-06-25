using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;
using PokemonRandomizer.UI.Utilities;
using System;
using System.Collections.Generic;
using static PokemonRandomizer.Settings;
using static PokemonRandomizer.UI.Models.BattleFrontierDataModel;

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
        private readonly BattleFrontierRandomizer battleFrontierRand;
        public BattleTentRandomizer(BattleFrontierRandomizer battleFrontierRand, ItemRandomizer itemRand, List<Action> delayedRandomizationCalls)
        {
            this.itemRand = itemRand;
            this.delayedRandomizationCalls = delayedRandomizationCalls;
            this.battleFrontierRand = battleFrontierRand;
        }
        public void RandomizeBattleTent(BattleTent battleTent, Settings settings, IEnumerable<Pokemon> pokemonSet, IList<ItemData> allItems)
        {
            // Randomize Pokemon
            var pokemonSettings = new PokemonSettings()
            {
                BanLegendaries = settings.BanLegendaries,
            };
            battleFrontierRand.RandomizePokemon(battleTent.Pokemon, settings.PokemonRandChance,
                settings.PokemonRandStrategy, pokemonSettings, settings.SpecialMoveSettings, pokemonSet);
            // Randomize Other Properties
            RandomizeRewards(battleTent, settings, allItems);
        }

        private void RandomizeRewards(BattleTent battleTent, Settings settings, IEnumerable<ItemData> allItems)
        {            
            if (!settings.RandomizePrizes || battleTent.Rewards.Count <= 0)
                return;
            // Add delayed rand call
            void RandomizeRewards()
            {
                for (int i = 0; i < battleTent.Rewards.Count; ++i)
                {
                    battleTent.Rewards[i] = itemRand.RandomItem(allItems, battleTent.Rewards[i], itemRandSettings);
                }
            }
            delayedRandomizationCalls.Add(RandomizeRewards);
        }

        public class Settings
        {
            public double PokemonRandChance { get; set; } = 0;
            public FrontierPokemonRandStrategy PokemonRandStrategy { get; set; }
            public SpecialMoveSettings SpecialMoveSettings { get; set; }
            public bool BanLegendaries { get; set; }
            public bool RandomizePrizes { get; set; } = false;
        }
    }
}
