using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.DataStructures.Trainers;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using static PokemonRandomizer.Backend.Randomization.WildEncounterRandomizer;
using static PokemonRandomizer.Settings;
using static PokemonRandomizer.UI.Models.BattleFrontierDataModel;

namespace PokemonRandomizer.Backend.Randomization
{
    public class BattleFrontierRandomizer
    {
        private const int frontierBrainPokemonLevel = 100;
        private readonly IDataTranslator dataT;
        private readonly PkmnRandomizer pokeRand;
        private readonly MovesetGenerator movesetGenerator;
        private readonly EvolutionUtils evoUtils;
        private readonly Random rand;
        private readonly List<Item> heldItemList;
        public BattleFrontierRandomizer(Random rand, IDataTranslator dataT, PkmnRandomizer pokeRand, MovesetGenerator movesetGenerator, EvolutionUtils evoUtils, List<Item> heldItemList)
        {
            this.rand = rand;
            this.dataT = dataT;
            this.pokeRand = pokeRand;
            this.movesetGenerator = movesetGenerator;
            this.evoUtils = evoUtils;
            this.heldItemList = heldItemList;
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
                    FinishPokemonRandomization(pokemon, level, specialMoveSettings);
                }
                else if (dataT.GetBaseStats(pokemon.species).IsVariant) // If pokemon is variant
                {
                    // Generate equivalent level
                    int level = GetPokemonLevel(pokemon, strategy);
                    // Remap variant moves and Nature
                    FinishPokemonRandomization(pokemon, level, specialMoveSettings);
                }
            }
        }

        private void FinishPokemonRandomization(FrontierTrainerPokemon pokemon, int level, SpecialMoveSettings specialMoveSettings)
        {
            // Generate moves and nature
            var stats = dataT.GetBaseStats(pokemon.species);
            pokemon.Nature = TrainerRandomizer.GetRandomNature(rand, stats);
            pokemon.moves = movesetGenerator.SmartMoveSet(stats, level, specialMoveSettings, GetHeldItem(pokemon));
            // Post-process nature and EVs
            TrainerRandomizer.PostProcessNature(pokemon, rand, dataT);
            PostProcessFrontierTrainerEVs(pokemon);
        }

        private Item GetHeldItem(FrontierTrainerPokemon pokemon)
        {
            if(pokemon.HeldItemIndex < 0 || pokemon.HeldItemIndex >= heldItemList.Count) 
            { 
                return Item.None;
            }
            return heldItemList[pokemon.HeldItemIndex];
        }

        private void SetHeldItem(FrontierTrainerPokemon pokemon, Item item) 
        {
            int index = heldItemList.IndexOf(item);
            if(index == -1)
            {
                return;
            }
            pokemon.HeldItemIndex = index;
        }

        private void PostProcessFrontierTrainerEVs(FrontierTrainerPokemon pokemon)
        {
            if (pokemon.species == Pokemon.DITTO)
            {
                pokemon.EVs = IHasFrontierTrainerEvs.EvFlags.HP | IHasFrontierTrainerEvs.EvFlags.Speed;
            }
            if (pokemon.species == Pokemon.SHEDINJA)
            {
                pokemon.EVs = IHasFrontierTrainerEvs.EvFlags.Speed;
                if (MovesetUtils.HasPhysicalMove(pokemon.moves, dataT))
                {
                    pokemon.EVs |= IHasFrontierTrainerEvs.EvFlags.Atk;
                }
                if (MovesetUtils.HasSpecialMove(pokemon.moves, dataT))
                {
                    pokemon.EVs |= IHasFrontierTrainerEvs.EvFlags.SpAtk;
                }
            }
            else
            {
                pokemon.EVs = CorrectBadFrontierTrainerEvs(pokemon.moves, pokemon.EVs, pokemon.Nature);
            }
        }

        private IHasFrontierTrainerEvs.EvFlags CorrectBadFrontierTrainerEvs(IReadOnlyList<Move> moves, IHasFrontierTrainerEvs.EvFlags current, Nature nature)
        {
            if (current.HasFlag(IHasFrontierTrainerEvs.EvFlags.SpAtk) && !MovesetUtils.HasSpecialMove(moves, dataT))
            {
                current ^= IHasFrontierTrainerEvs.EvFlags.SpAtk;
                if (!current.HasFlag(IHasFrontierTrainerEvs.EvFlags.Atk) && MovesetUtils.HasPhysicalMove(moves, dataT))
                {
                    current |= IHasFrontierTrainerEvs.EvFlags.Atk;
                }
                else
                {
                    current = ReplaceFrontierTrainerEvs(current, nature);
                }
            }
            if (current.HasFlag(IHasFrontierTrainerEvs.EvFlags.Atk) && !MovesetUtils.HasPhysicalMove(moves, dataT))
            {
                current ^= IHasFrontierTrainerEvs.EvFlags.Atk;
                if (!current.HasFlag(IHasFrontierTrainerEvs.EvFlags.SpAtk) && MovesetUtils.HasSpecialMove(moves, dataT))
                {
                    current |= IHasFrontierTrainerEvs.EvFlags.SpAtk;
                }
                else
                {
                    current = ReplaceFrontierTrainerEvs(current, nature);
                }
            }
            return current;
        }

        private IHasFrontierTrainerEvs.EvFlags ReplaceFrontierTrainerEvs(IHasFrontierTrainerEvs.EvFlags current, Nature nature)
        {

            if (!current.HasFlag(IHasFrontierTrainerEvs.EvFlags.Speed) && !nature.ReducesStat(PokemonBaseStats.spdStatIndex))
            {
                current |= IHasFrontierTrainerEvs.EvFlags.Speed;
            }
            else if (!current.HasFlag(IHasFrontierTrainerEvs.EvFlags.HP))
            {
                current |= IHasFrontierTrainerEvs.EvFlags.HP;
            }
            else if (!current.HasFlag(IHasFrontierTrainerEvs.EvFlags.Def) && !current.HasFlag(IHasFrontierTrainerEvs.EvFlags.SpDef))
            {
                current |= rand.RandomBool() ? IHasFrontierTrainerEvs.EvFlags.Def : IHasFrontierTrainerEvs.EvFlags.SpDef;
            }
            else if (!current.HasFlag(IHasFrontierTrainerEvs.EvFlags.Def))
            {
                current |= IHasFrontierTrainerEvs.EvFlags.Def;
            }
            else if (!current.HasFlag(IHasFrontierTrainerEvs.EvFlags.SpDef))
            {
                current |= IHasFrontierTrainerEvs.EvFlags.SpDef;
            }
            return current;
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
                    pokemon.species = pokeRand.RandomPokemon(pokemonSet, pokemon.species, pokemonSettings, frontierBrainPokemonLevel);
                    // Generate moveset and Nature
                    FinishFrontTierBrainPokemonRandomization(pokemon, frontierBrainPokemonLevel, specialMoveSettings);
                }
                else if (dataT.GetBaseStats(pokemon.species).IsVariant) // If pokemon is variant
                {
                    // Remap variant moves and Nature
                    FinishFrontTierBrainPokemonRandomization(pokemon, frontierBrainPokemonLevel, specialMoveSettings);
                }
            }
        }

        private void FinishFrontTierBrainPokemonRandomization(FrontierBrainTrainerPokemon pokemon, int level, SpecialMoveSettings specialMoveSettings)
        {
            var stats = dataT.GetBaseStats(pokemon.species);
            pokemon.Nature = TrainerRandomizer.GetRandomNature(rand, stats);
            pokemon.moves = movesetGenerator.SmartMoveSet(stats, level, specialMoveSettings, pokemon.heldItem);
            TrainerRandomizer.PostProcessNature(pokemon, rand, dataT);
            TrainerRandomizer.PostProcessEvs(pokemon, rand, dataT);
        }
    }
}
