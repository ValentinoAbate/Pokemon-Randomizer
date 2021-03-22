using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.Backend.Randomization
{
    using DataStructures;
    using EnumTypes;
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

        private void RandomizeTrainerPokemon(Trainer trainer, IEnumerable<Pokemon> pokemonSet, PkmnRandomizer.Settings settings)
        {
            // Class based?
            // Local environment based
            // Get type sample
            var typeSample = trainer.pokemon.Select((p) => p.species).ToArray();
            foreach (var pokemon in trainer.pokemon)
            {
                if (settings.WeightType == PkmnRandomizer.Settings.WeightingType.Group)
                {
                    pokemon.species = pokeRand.RandomTypeGroup(pokemonSet, pokemon.species, pokemon.level, typeSample, settings);
                }
                else
                {

                    pokemon.species = pokeRand.Random(pokemonSet, pokemon.species, pokemon.level, settings);
                }
                // Reset special moves if necessary
                if (pokemon.HasSpecialMoves)
                {
                    //pokemon.moves = MovesetGenerator.DefaultMoveset(data.PokemonLookup[pokemon.species], pokemon.level);
                    pokemon.moves = MovesetGenerator.SmartMoveSet(rand, dataT.GetBaseStats(pokemon.species), pokemon.level, dataT);
                }
            }
        }

        /// <summary> Radnomize the given trainer encounter </summary>
        public void Randomize(Trainer trainer, IEnumerable<Pokemon> pokemonSet, Settings.TrainerSettings settings, bool safe = true)
        {
            // Set data type
            // Set AI flags
            // Set item stock (if applicable)
            // Set Battle Type
            if (rand.RollSuccess(settings.BattleTypeRandChance))
                trainer.isDoubleBattle = rand.RollSuccess(settings.DoubleBattleChance);
            // Set pokemon
            if (rand.RollSuccess(settings.PokemonRandChance))
            {
                RandomizeTrainerPokemon(trainer, pokemonSet, settings.PokemonSettings);
            }
            // Fix any unsafe values if safe is set to true
            if (safe)
            {
                // Set 1-pokemon battle to solo if appropriate
                if (settings.MakeSoloPokemonBattlesSingle && trainer.pokemon.Length == 1)
                    trainer.isDoubleBattle = false;
            }
        }

        /// <summary>
        /// Randomize A sequence of battles from the same trainer.
        /// Battles is assumed to be in chronological order, and that the first battle has been appropriately randomized.
        /// Use unsafe randomization for randomizing the first battle.
        /// </summary>
        public void RandomizeReoccurring(Trainer firstBattle, List<Trainer> battles, IEnumerable<Pokemon> pokemonSet, Settings.TrainerSettings settings)
        {
            var speciesSettings = settings.PokemonSettings;

            // Battle Type
            if (settings.BattleTypeStrategy == Settings.TrainerSettings.BattleTypePcgStrategy.None)
            {
                for (int i = 1; i < battles.Count; i++)
                {
                    if (rand.RollSuccess(settings.BattleTypeRandChance))
                    {
                        battles[i].isDoubleBattle = rand.RollSuccess(settings.DoubleBattleChance);
                    }
                }
            }
            else if (settings.BattleTypeStrategy == Settings.TrainerSettings.BattleTypePcgStrategy.KeepSameType)
            {
                for (int i = 1; i < battles.Count; i++)
                {
                    battles[i].isDoubleBattle = battles[0].isDoubleBattle;
                }
            }

            // Pokemon
            if (settings.PokemonStrategy == Settings.TrainerSettings.PokemonPcgStrategy.None)
            {
                for (int i = 1; i < battles.Count; i++)
                {
                    RandomizeTrainerPokemon(battles[i], pokemonSet, speciesSettings);
                }
            }
            else if (settings.PokemonStrategy == Settings.TrainerSettings.PokemonPcgStrategy.KeepAce)
            {
                var lastBattle = firstBattle;
                foreach (var battle in battles)
                {
                    RandomizeTrainerPokemon(battle, pokemonSet, speciesSettings);
                    // Migrate Ace pokemon from the last battle
                    var currAce = battle.pokemon[battle.pokemon.Length - 1];
                    var lastAce = lastBattle.pokemon[lastBattle.pokemon.Length - 1];
                    currAce.species = evoUtils.MaxEvolution(lastAce.species, currAce.level, speciesSettings.RestrictIllegalEvolutions);
                    if (currAce.HasSpecialMoves)
                    {
                        currAce.moves = MovesetGenerator.SmartMoveSet(rand, dataT.GetBaseStats(currAce.species), currAce.level, dataT);
                    }
                    lastBattle = battle;
                };
            }
            else if (settings.PokemonStrategy == Settings.TrainerSettings.PokemonPcgStrategy.KeepParty)
            {
                var lastBattle = firstBattle;
                foreach (var battle in battles)
                {
                    RandomizeTrainerPokemon(battle, pokemonSet, speciesSettings);
                    int lastBattleSize = lastBattle.pokemon.Length;
                    int battleSize = battle.pokemon.Length;
                    // Migrate pokemon from the last battle
                    for (int i = 0; i < lastBattleSize && i < battleSize; ++i)
                    {
                        var currPokemon = battle.pokemon[battleSize - (i + 1)];
                        var lastPokemon = lastBattle.pokemon[lastBattleSize - (i + 1)];
                        currPokemon.species = evoUtils.MaxEvolution(lastPokemon.species, currPokemon.level, speciesSettings.RestrictIllegalEvolutions);
                        if (currPokemon.HasSpecialMoves)
                        {
                            currPokemon.moves = MovesetGenerator.SmartMoveSet(rand, dataT.GetBaseStats(currPokemon.species), currPokemon.level, dataT);
                        }
                    }
                    lastBattle = battle;
                }
            }

            // Fixes
            if (settings.MakeSoloPokemonBattlesSingle)
            {
                foreach (var battle in battles)
                {
                    if (battle.pokemon.Length <= 1)
                        battle.isDoubleBattle = false;
                }
                if (firstBattle.pokemon.Length <= 1)
                    firstBattle.isDoubleBattle = false;
            }
        }

        #endregion
    }
}
