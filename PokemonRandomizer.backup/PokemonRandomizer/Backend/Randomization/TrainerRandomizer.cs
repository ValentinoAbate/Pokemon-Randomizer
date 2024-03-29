﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.Backend.Randomization
{
    using DataStructures;
    using EnumTypes;
    using static Settings;
    public class TrainerRandomizer
    {
        private readonly PkmnRandomizer pokeRand;
        private readonly Random rand;
        private readonly EvolutionUtils evoUtils;
        private readonly IDataTranslator dataT;

        public TrainerRandomizer(Random rand, PkmnRandomizer pokeRand, EvolutionUtils evoUtils, IDataTranslator dataT)
        {
            this.pokeRand = pokeRand;
            this.rand = rand;
            this.evoUtils = evoUtils;
            this.dataT = dataT;
        }

        #region Trainer Randomization

        private void RandomizeTrainerPokemon(Trainer trainer, IEnumerable<Pokemon> pokemonSet, PokemonSettings settings)
        {
            // Class based?
            // Local environment based
            // Get type sample
            var partyTypeOccurence = PokemonMetrics.TypeOccurence(trainer.pokemon.Select(p => dataT.GetBaseStats(p.species)));
            foreach (var pokemon in trainer.pokemon)
            {
                // Chose pokemon
                var metrics = CreatePokemonMetrics(pokemonSet, pokemon.species, partyTypeOccurence, settings.Data);
                pokemon.species = pokeRand.RandomPokemon(pokemonSet, pokemon.species, metrics, settings, pokemon.level);

                // Reset special moves if necessary
                if (pokemon.HasSpecialMoves)
                {
                    //pokemon.moves = MovesetGenerator.DefaultMoveset(data.PokemonLookup[pokemon.species], pokemon.level);
                    pokemon.moves = MovesetGenerator.SmartMoveSet(rand, dataT.GetBaseStats(pokemon.species), pokemon.level, dataT);
                }
            }
        }

        private IEnumerable<Metric<Pokemon>> CreatePokemonMetrics(IEnumerable<Pokemon> all, Pokemon pokemon, WeightedSet<PokemonType> partyTypeOccurence, IReadOnlyList<MetricData> data)
        {
            var metrics = pokeRand.CreateBasicMetrics(all, pokemon, data, out List<MetricData> specialData);
            foreach (var d in specialData)
            {
                WeightedSet<Pokemon> input = d.DataSource switch
                {
                    PokemonMetric.typeTrainerParty => pokeRand.TypeSimilarityGroup(all, partyTypeOccurence, d.Sharpness),
                    _ => null,
                };
                if (input != null)
                {
                    metrics.Add(new Metric<Pokemon>(input, d.Filter, d.Priority));
                }
            }
            return metrics;
        }

        /// <summary> Radnomize the given trainer encounter </summary>
        public void Randomize(Trainer trainer, IEnumerable<Pokemon> pokemonSet, TrainerSettings settings, bool safe = true)
        {
            // Set data type
            // Apply level scaling
            ApplyLevelScaling(trainer, settings);
            // Set item stock (if applicable)
            // Set Battle Type
            if (rand.RollSuccess(settings.BattleTypeRandChance))
            {
                trainer.isDoubleBattle = rand.RollSuccess(settings.DoubleBattleChance);
            }
            // Fix any unsafe values if safe is set to true
            if (safe)
            {
                // Set 1-pokemon battle to solo if appropriate
                trainer.EnsureSafeBattleType();
            }
            // Set AI flags
            ApplyAISettings(trainer, settings);
            // Set pokemon
            if (rand.RollSuccess(settings.PokemonRandChance))
            {
                RandomizeTrainerPokemon(trainer, pokemonSet, settings.PokemonSettings);
            }
        }

        /// <summary>
        /// Randomize A sequence of battles from the same trainer.
        /// Battles is assumed to be in chronological order, and that the first battle has been appropriately randomized.
        /// Use unsafe randomization for randomizing the first battle.
        /// </summary>
        public void RandomizeReoccurring(Trainer firstBattle, List<Trainer> battles, IEnumerable<Pokemon> pokemonSet, TrainerSettings settings)
        {
            var pkmnSettings = settings.PokemonSettings;

            // Battle Type
            if (settings.BattleTypeStrategy == TrainerSettings.BattleTypePcgStrategy.None)
            {
                foreach (var battle in battles)
                {
                    if (rand.RollSuccess(settings.BattleTypeRandChance))
                    {
                        battle.isDoubleBattle = rand.RollSuccess(settings.DoubleBattleChance);
                    }
                }
            }
            else if (settings.BattleTypeStrategy == TrainerSettings.BattleTypePcgStrategy.KeepSameType)
            {
                foreach (var battle in battles)
                {
                    battle.isDoubleBattle = firstBattle.isDoubleBattle;
                }
            }
            // Ensure Battle Type Safety
            foreach (var battle in battles)
            {
                battle.EnsureSafeBattleType();
            }
            firstBattle.EnsureSafeBattleType();

            // Apply AI settings
            foreach (var battle in battles)
            {
                ApplyAISettings(battle, settings);
            }
            // Apply level scaling
            foreach (var battle in battles)
            {
                ApplyLevelScaling(battle, settings);
            }
            // Pokemon
            if (settings.PokemonStrategy == TrainerSettings.PokemonPcgStrategy.None)
            {
                foreach (var battle in battles)
                {
                    RandomizeTrainerPokemon(battle, pokemonSet, pkmnSettings);
                }
            }
            else if (settings.PokemonStrategy == TrainerSettings.PokemonPcgStrategy.KeepAce)
            {
                var lastBattle = firstBattle;
                foreach (var battle in battles)
                {
                    RandomizeTrainerPokemon(battle, pokemonSet, pkmnSettings);
                    // Migrate Ace pokemon from the last battle
                    var currAce = battle.pokemon[battle.pokemon.Length - 1];
                    var lastAce = lastBattle.pokemon[lastBattle.pokemon.Length - 1];
                    currAce.species = evoUtils.MaxEvolution(lastAce.species, currAce.level, pkmnSettings.RestrictIllegalEvolutions);
                    if (currAce.HasSpecialMoves)
                    {
                        currAce.moves = MovesetGenerator.SmartMoveSet(rand, dataT.GetBaseStats(currAce.species), currAce.level, dataT);
                    }
                    lastBattle = battle;
                };
            }
            else if (settings.PokemonStrategy == TrainerSettings.PokemonPcgStrategy.KeepParty)
            {
                var lastBattle = firstBattle;
                foreach (var battle in battles)
                {
                    RandomizeTrainerPokemon(battle, pokemonSet, pkmnSettings);
                    int lastBattleSize = lastBattle.pokemon.Length;
                    int battleSize = battle.pokemon.Length;
                    // Migrate pokemon from the last battle
                    for (int i = 0; i < lastBattleSize && i < battleSize; ++i)
                    {
                        var currPokemon = battle.pokemon[battleSize - (i + 1)];
                        var lastPokemon = lastBattle.pokemon[lastBattleSize - (i + 1)];
                        currPokemon.species = evoUtils.MaxEvolution(lastPokemon.species, currPokemon.level, pkmnSettings.RestrictIllegalEvolutions);
                        if (currPokemon.HasSpecialMoves)
                        {
                            currPokemon.moves = MovesetGenerator.SmartMoveSet(rand, dataT.GetBaseStats(currPokemon.species), currPokemon.level, dataT);
                        }
                    }
                    lastBattle = battle;
                }
            }
        }

        private void ApplyLevelScaling(Trainer trainer, TrainerSettings settings)
        {
            // Apply Difficulty Settings
            if (settings.LevelMultiplier > 0)
            {
                foreach (var pokemon in trainer.pokemon)
                {
                    pokemon.level = (int)Math.Max(0, Math.Min(100, pokemon.level * settings.LevelMultiplier));
                }
            }
            if(settings.PokemonIVModifier != 0)
            {
                foreach (var pokemon in trainer.pokemon)
                {
                    pokemon.IVLevel = Math.Max(byte.MinValue, Math.Min(byte.MaxValue, pokemon.IVLevel + settings.PokemonIVModifier));
                }
            }

        }

        private void ApplyAISettings(Trainer trainer, TrainerSettings settings)
        {
            if (settings.UseSmartAI)
            {
                // Move failure check (basic)
                trainer.AIFlags.Set(0, true);
                // Go for KO
                trainer.AIFlags.Set(1, true);
                // Move failure check (advanced)
                trainer.AIFlags.Set(2, true);
                // If double battle, set double battle strats
                if (trainer.isDoubleBattle)
                {
                    trainer.AIFlags.Set(7, true);
                }
                // HP Awareness
                trainer.AIFlags.Set(8, true);
            }
        }

        #endregion
    }
}
