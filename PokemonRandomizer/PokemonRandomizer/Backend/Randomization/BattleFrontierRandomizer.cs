using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.DataStructures.Trainers;
using PokemonRandomizer.Backend.EnumTypes;
using System;
using System.Collections.Generic;
using static PokemonRandomizer.Settings;
using static PokemonRandomizer.UI.Models.BattleFrontierDataModel;

namespace PokemonRandomizer.Backend.Randomization
{
    public class BattleFrontierRandomizer
    {
        private readonly IDataTranslator dataT;
        private readonly PkmnRandomizer pokeRand;
        private readonly MovesetGenerator movesetGenerator;
        private readonly Random rand;
        public BattleFrontierRandomizer(Random rand, IDataTranslator dataT, PkmnRandomizer pokeRand, MovesetGenerator movesetGenerator)
        {
            this.rand = rand;
            this.dataT = dataT;
            this.pokeRand = pokeRand;
            this.movesetGenerator = movesetGenerator;
        }

        public void Randomize(RomData data, Settings settings, IEnumerable<Pokemon> pokemonSet)
        {
            // Randomize normal frontier pokemon
            var pokemonSettings = new PokemonSettings()
            {
                BanLegendaries = settings.BattleFrontierBanLegendaries,
                ForceHighestLegalEvolution = true,
                RestrictIllegalEvolutions = false,
            };
            RandomizePokemon(data.BattleFrontierTrainerPokemon, settings.BattleFrontierPokemonRandChance, 
                settings.BattleFrontierPokemonRandStrategy, pokemonSettings, settings.BattleFrontierSpecialMoveSettings, pokemonSet);
            // Randomize Frontier Brain Pokemon
            RandomizeFrontierBrainPokemon(data, settings, pokemonSet);
        }

        public void RandomizePokemon(IEnumerable<FrontierTrainerPokemon> pokemonInput, double chance, FrontierPokemonRandStrategy strategy, PokemonSettings pokemonSettings, SpecialMoveSettings specialMoveSettings, IEnumerable<Pokemon> pokemonSet)
        {
            foreach (var pokemon in pokemonInput)
            {
                if (rand.RollSuccess(chance))
                {
                    // Generate equivalent level
                    int level = GetPokemonLevel(pokemon, strategy);
                    // Randomize Pokemon
                    pokemon.species = pokeRand.RandomPokemon(pokemonSet, pokemon.species, pokemonSettings, level);
                    // Generate moveset
                    pokemon.moves = movesetGenerator.SmartMoveSet(dataT.GetBaseStats(pokemon.species), level, specialMoveSettings);
                }
                else if (dataT.GetBaseStats(pokemon.species).IsVariant) // If pokemon is variant
                {
                    // Generate equivalent level
                    int level = GetPokemonLevel(pokemon, strategy);
                    // Remap variant moves
                    pokemon.moves = movesetGenerator.SmartMoveSet(dataT.GetBaseStats(pokemon.species), level, specialMoveSettings);
                }
            }
        }

        private int GetPokemonLevel(FrontierTrainerPokemon pokemon, FrontierPokemonRandStrategy strategy)
        {
            return strategy switch
            {
                FrontierPokemonRandStrategy.PowerScaled => 50, // TODO: actual power scaling
                FrontierPokemonRandStrategy.Level100 => 100,
                FrontierPokemonRandStrategy.Level50 => 50,
                FrontierPokemonRandStrategy.Level30 => 30,
                _ => 50
            };
        }

        private void RandomizeFrontierBrainPokemon(RomData data, Settings settings, IEnumerable<Pokemon> pokemonSet)
        {
            if (settings.FrontierBrainPokemonRandChance <= 0)
            {
                // Todo: remap variant movesets
                return;
            }
            var pokemonSettings = new PokemonSettings()
            {
                BanLegendaries = settings.FrontierBrainBanLegendaries,
            };
            var specialMoveSettings = settings.FrontierBrainSpecialMoveSettings;
            foreach (var pokemon in data.BattleFrontierBrainPokemon)
            {
                if (rand.RollSuccess(settings.FrontierBrainPokemonRandChance))
                {
                    // TODO: config pokemon settings for power scaling
                    pokemon.species = pokeRand.RandomPokemon(pokemonSet, pokemon.species, pokemonSettings);
                    // TODO: special move support
                    pokemon.moves = movesetGenerator.SmartMoveSet(dataT.GetBaseStats(pokemon.species), 100, specialMoveSettings);
                }
                else // If pokemon is variant
                {
                    // Remap variant moves
                }
            }
        }
    }
}
