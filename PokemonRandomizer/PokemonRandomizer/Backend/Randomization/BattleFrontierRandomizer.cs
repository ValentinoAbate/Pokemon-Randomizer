using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.DataStructures.Trainers;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using static PokemonRandomizer.Settings;
using static PokemonRandomizer.UI.Models.BattleFrontierDataModel;

namespace PokemonRandomizer.Backend.Randomization
{
    public class BattleFrontierRandomizer
    {
        private readonly IDataTranslator dataT;
        private readonly PkmnRandomizer pokeRand;
        private readonly MovesetGenerator movesetGenerator;
        private readonly EvolutionUtils evoUtils;
        private readonly Random rand;
        public BattleFrontierRandomizer(Random rand, IDataTranslator dataT, PkmnRandomizer pokeRand, MovesetGenerator movesetGenerator, EvolutionUtils evoUtils)
        {
            this.rand = rand;
            this.dataT = dataT;
            this.pokeRand = pokeRand;
            this.movesetGenerator = movesetGenerator;
            this.evoUtils = evoUtils;
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
                    // Generate moveset and Nature
                    var stats = dataT.GetBaseStats(pokemon.species);
                    pokemon.Nature = TrainerRandomizer.GetRandomNature(rand, stats);
                    pokemon.moves = movesetGenerator.SmartMoveSet(stats, level, specialMoveSettings);
                    pokemon.Nature = CorrectBadNature(pokemon.moves, pokemon.Nature);
                }
                else if (dataT.GetBaseStats(pokemon.species).IsVariant) // If pokemon is variant
                {
                    // Generate equivalent level
                    int level = GetPokemonLevel(pokemon, strategy);
                    // Remap variant moves and Nature
                    var stats = dataT.GetBaseStats(pokemon.species);
                    pokemon.Nature = TrainerRandomizer.GetRandomNature(rand, stats);
                    pokemon.moves = movesetGenerator.SmartMoveSet(stats, level, specialMoveSettings);
                    pokemon.Nature = CorrectBadNature(pokemon.moves, pokemon.Nature);
                }
            }
        }

        private int GetPokemonLevel(FrontierTrainerPokemon pokemon, FrontierPokemonRandStrategy strategy)
        {
            return strategy switch
            {
                FrontierPokemonRandStrategy.PowerScaled => DynanamicLevel(pokemon),
                FrontierPokemonRandStrategy.Level100 => 100,
                FrontierPokemonRandStrategy.Level50 => 50,
                FrontierPokemonRandStrategy.Level30 => 30,
                _ => 50
            };
        }

        private int DynanamicLevel(FrontierTrainerPokemon pokemon)
        {
            int minLevel = 1;
            int maxLevel = 100;
            var stats = dataT.GetBaseStats(pokemon.species);
            foreach (var evolution in stats.evolvesTo)
            {
                if (!evolution.IsRealEvolution)
                    continue;
                int evoLevel = evoUtils.EquivalentLevelReq(evolution, stats);
                if(evoLevel < maxLevel)
                {
                    maxLevel = evoLevel;
                }
            }
            if (!stats.IsBasicOrEvolvesFromBaby)
            {
                foreach(var evolution in stats.evolvesFrom)
                {
                    if (!evolution.IsRealEvolution)
                        continue;
                    int evoLevel = evoUtils.EquivalentLevelReq(evolution, stats);
                    if (evoLevel > minLevel)
                    {
                        minLevel = evoLevel;
                    }
                }
            }
            var learnsetLookup = stats.OriginalLearnset.GetMinimumLearnLevelLookup();
            foreach(var move in pokemon.moves)
            {
                if (!learnsetLookup.ContainsKey(move))
                    continue;
                int learnLevel = learnsetLookup[move];
                if(learnLevel > minLevel)
                {
                    minLevel = learnLevel;
                }
            }
            return minLevel >= maxLevel ? minLevel : rand.RandomInt(minLevel, maxLevel + 1);
        }

        private void RandomizeFrontierBrainPokemon(RomData data, Settings settings, IEnumerable<Pokemon> pokemonSet)
        {
            var pokemonSettings = new PokemonSettings()
            {
                BanLegendaries = settings.FrontierBrainBanLegendaries,
                ForceHighestLegalEvolution = true,
                RestrictIllegalEvolutions = false,
            };
            var specialMoveSettings = settings.FrontierBrainSpecialMoveSettings;
            foreach (var pokemon in data.BattleFrontierBrainPokemon)
            {
                if (rand.RollSuccess(settings.FrontierBrainPokemonRandChance))
                {
                    pokemon.species = pokeRand.RandomPokemon(pokemonSet, pokemon.species, pokemonSettings);
                    // Generate moveset and Nature
                    var stats = dataT.GetBaseStats(pokemon.species);
                    pokemon.Nature = TrainerRandomizer.GetRandomNature(rand, stats);
                    pokemon.moves = movesetGenerator.SmartMoveSet(stats, 100, specialMoveSettings);
                    pokemon.Nature = CorrectBadNature(pokemon.moves, pokemon.Nature);
                }
                else if (dataT.GetBaseStats(pokemon.species).IsVariant) // If pokemon is variant
                {
                    // Remap variant moves and Nature
                    var stats = dataT.GetBaseStats(pokemon.species);
                    pokemon.Nature = TrainerRandomizer.GetRandomNature(rand, stats);
                    pokemon.moves = movesetGenerator.SmartMoveSet(stats, 100, specialMoveSettings);
                    pokemon.Nature = CorrectBadNature(pokemon.moves, pokemon.Nature);
                }
            }
        }

        private Nature CorrectBadNature(IReadOnlyList<Move> moves, Nature currentNature)
        {
            int currPositiveStat = currentNature.PositiveStatIndex();
            int currNegativeStat = currentNature.NegativeStatIndex();
            int newPositiveStat = -1;
            int newNegativeStat = -1;

            if (currPositiveStat == PokemonBaseStats.spAtkStatIndex)
            {
                // if +special nature and no special moves, change to +atk
                if (!moves.Any(m => dataT.GetMoveData(m).MoveCategory == MoveData.Type.Special))
                {
                    newPositiveStat = PokemonBaseStats.atkStatIndex;
                }
            }
            else if(currPositiveStat == PokemonBaseStats.atkStatIndex)
            {
                // if +atk and no physical moves, change to +special
                if (!moves.Any(m => dataT.GetMoveData(m).MoveCategory == MoveData.Type.Physical))
                {
                    newPositiveStat = PokemonBaseStats.spAtkStatIndex;
                }
            }
            if (currNegativeStat != PokemonBaseStats.atkStatIndex)
            {
                // if not -atk nature and no physical moves, change to -atk
                if (!moves.Any(m => dataT.GetMoveData(m).MoveCategory == MoveData.Type.Physical))
                {
                    newNegativeStat = PokemonBaseStats.atkStatIndex;
                }
            }
            if (currNegativeStat != PokemonBaseStats.spAtkStatIndex)
            {
                // if not -special nature and no special moves, change to -special
                if (!moves.Any(m => dataT.GetMoveData(m).MoveCategory == MoveData.Type.Special))
                {
                    newNegativeStat = PokemonBaseStats.spAtkStatIndex;
                }
            }

            // if -special and no physical moves, change to -atk
            // if -atk and no special moves, change to -special
            if (newPositiveStat == -1)
            {
                if (newNegativeStat == -1)
                    return currentNature;
                return NatureUtils.GetNature(currPositiveStat, newNegativeStat);
            }
            else if(newNegativeStat == -1)
            {
                return NatureUtils.GetNature(newPositiveStat, currNegativeStat);
            }
            else
            {
                return NatureUtils.GetNature(newPositiveStat, newNegativeStat);
            }
        }
    }
}
