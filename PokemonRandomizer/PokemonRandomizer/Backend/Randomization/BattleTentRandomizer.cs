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
        private readonly IDataTranslator dataT;
        private readonly PkmnRandomizer pokeRand;
        private readonly ItemRandomizer itemRand;
        private readonly List<Action> delayedRandomizationCalls;
        private readonly MovesetGenerator movesetGenerator;
        private readonly Random rand;
        public BattleTentRandomizer(Random rand, IDataTranslator dataT, PkmnRandomizer pokeRand, ItemRandomizer itemRand, List<Action> delayedRandomizationCalls, MovesetGenerator movesetGenerator)
        {
            this.rand = rand;
            this.dataT = dataT;
            this.pokeRand = pokeRand;
            this.itemRand = itemRand;
            this.delayedRandomizationCalls = delayedRandomizationCalls;
            this.movesetGenerator = movesetGenerator;
        }
        public void RandomizeBattleTent(BattleTent battleTent, Settings settings, IEnumerable<Pokemon> pokemonSet, IList<ItemData> allItems)
        {
            RandomizePokemon(battleTent, settings, pokemonSet);
            RandomizeRewards(battleTent, settings, allItems);
        }

        private void RandomizePokemon(BattleTent battleTent, Settings settings, IEnumerable<Pokemon> pokemonSet)
        {
            if (settings.PokemonRandChance <= 0)
            {
                // Todo: remap variant movesets
                return;
            }

            var pokemonSettings = new PokemonSettings()
            {
                BanLegendaries = settings.BanLegendaries,
            };
            foreach (var pokemon in battleTent.Pokemon)
            {
                if (rand.RollSuccess(settings.PokemonRandChance))
                {
                    // TODO: config pokemon settings for power scaling
                    pokemon.species = pokeRand.RandomPokemon(pokemonSet, pokemon.species, pokemonSettings);


                    // Generate equivalent level
                    int level = settings.PokemonRandStrategy switch
                    {
                        FrontierPokemonRandStrategy.PowerScaled => 50, // TODO: actual power scaling
                        FrontierPokemonRandStrategy.AllStrongest => 100,
                        FrontierPokemonRandStrategy.FixedLevel => 30,
                        _ => 50
                    };

                    // TODO: special move support
                    pokemon.moves = movesetGenerator.SmartMoveSet(dataT.GetBaseStats(pokemon.species), level);
                }
                else // If pokemon is variant
                {
                    // Remap variant moves
                }
            }
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
