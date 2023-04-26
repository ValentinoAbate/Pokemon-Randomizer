using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using System;
using System.Collections.Generic;
using static PokemonRandomizer.Settings;

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
        public BattleTentRandomizer(IDataTranslator dataT, PkmnRandomizer pokeRand, ItemRandomizer itemRand, List<Action> delayedRandomizationCalls, MovesetGenerator movesetGenerator)
        {
            this.dataT = dataT;
            this.pokeRand = pokeRand;
            this.itemRand = itemRand;
            this.delayedRandomizationCalls = delayedRandomizationCalls;
            this.movesetGenerator = movesetGenerator;
        }
        public void RandomizeBattleTent(BattleTent battleTent, IEnumerable<Pokemon> pokemonSet, PokemonSettings pokemonSettings, IList<ItemData> allItems)
        {
            RandomizePokemon(battleTent, pokemonSet, pokemonSettings);
            // Randomize Rewards
            RandomizeRewards(battleTent, allItems);
        }

        private void RandomizePokemon(BattleTent battleTent, IEnumerable<Pokemon> pokemonSet, PokemonSettings pokemonSettings)
        {
            foreach(var pokemon in battleTent.Pokemon)
            {
                // TODO: move logic to battle frontier randomizer and reuse here
                // TODO: evolution-stage / power weighting
                pokemon.species = pokeRand.RandomPokemon(pokemonSet, pokemon.species, pokemonSettings);

                // TODO: frontier pokemon moveset generation
                pokemon.moves = movesetGenerator.SmartMoveSet(dataT.GetBaseStats(pokemon.species), 100);
            }
        }

        private void RandomizeRewards(BattleTent battleTent, IEnumerable<ItemData> allItems)
        {
            // TODO: expand rewards (if desired) - just pad the reward list with Item.None
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
