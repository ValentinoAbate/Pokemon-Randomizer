using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.Backend.Randomization
{
    using DataStructures;
    using EnumTypes;
    using PokemonRandomizer.Backend.DataStructures.Trainers;
    using PokemonRandomizer.Backend.Metadata;
    using PokemonRandomizer.Backend.Utilities;
    using static Settings;
    public class TrainerRandomizer
    {
        private readonly PkmnRandomizer pokeRand;
        private readonly Random rand;
        private readonly EvolutionUtils evoUtils;
        private readonly IDataTranslator dataT;
        private readonly Func<Trainer.Category, bool> shouldBanLegendaries;
        private readonly Func<Trainer.Category, bool> shouldApplyTheming;
        private readonly Func<Trainer.Category, int> numBonusPokemon;
        private readonly MovesetGenerator movesetGenerator;

        public TrainerRandomizer(Random rand, PkmnRandomizer pokeRand, EvolutionUtils evoUtils, MovesetGenerator movesetGenerator, IDataTranslator dataT, Settings s)
        {
            this.pokeRand = pokeRand;
            this.rand = rand;
            this.evoUtils = evoUtils;
            this.dataT = dataT;
            this.movesetGenerator = movesetGenerator;
            shouldBanLegendaries = s.BanLegendaries;
            shouldApplyTheming = s.ApplyTheming;
            numBonusPokemon = s.NumBonusPokmon;
        }

        #region Trainer Randomization

        private void RandomizeTrainerPokemon(Trainer trainer, IEnumerable<Pokemon> pokemonSet, PokemonSettings settings, float duplicateMultiplier, WeightedSet<PokemonType> typeOccurence, int? startIndex = null, int? endIndex = null)
        {
            // Get type sample if necessary
            var partyTypeOccurence = typeOccurence ?? GetTrainerTypeOccurence(trainer);
            bool useTheming = shouldApplyTheming(trainer.TrainerCategory);
            // Set ending index
            endIndex ??= trainer.Pokemon.Count;
            for (int i = startIndex ?? 0; i < endIndex; i++)
            {
                var pokemon = trainer.Pokemon[i];
                // Chose pokemon
                var metrics = useTheming ? CreatePokemonMetrics(trainer, pokemonSet, pokemon.species, partyTypeOccurence, settings.Data) : Enumerable.Empty<Metric<Pokemon>>();
                if(duplicateMultiplier != 1)
                {
                    // Add a metric if none exist
                    if (!metrics.Any())
                    {
                        metrics = new List<Metric<Pokemon>>() { new Metric<Pokemon>(new WeightedSet<Pokemon>(pokemonSet), 0, 1)};
                    }
                    // Remove duplicates from all metrics
                    for (int pokemonIndex = 0; pokemonIndex < i; pokemonIndex++)
                    {
                        foreach(var metric in metrics)
                        {
                            metric.Input.Multiply(trainer.Pokemon[pokemonIndex].species, duplicateMultiplier);
                        }
                    }
                    for (int pokemonIndex = endIndex.Value; pokemonIndex < trainer.Pokemon.Count; pokemonIndex++)
                    {
                        foreach (var metric in metrics)
                        {
                            metric.Input.Multiply(trainer.Pokemon[pokemonIndex].species, duplicateMultiplier);
                        }
                    }
                }
                pokemon.species = pokeRand.RandomPokemon(pokemonSet, pokemon.species, metrics, settings, pokemon.level);

                // Reset special moves if necessary
                FinishPokemonRandomization(pokemon);
            }
        }

        private void RandomizeTrainerPokemon(Trainer trainer, IEnumerable<Pokemon> pokemonSet, PokemonSettings settings, float duplicateMultiplier, int? startIndex = null, int? endIndex = null)
        {
            RandomizeTrainerPokemon(trainer, pokemonSet, settings, duplicateMultiplier, null, startIndex, endIndex);
        }

        private IEnumerable<Metric<Pokemon>> CreatePokemonMetrics(Trainer trainer, IEnumerable<Pokemon> all, Pokemon pokemon, WeightedSet<PokemonType> partyTypeOccurence, IReadOnlyList<MetricData> data)
        {
            if(trainer.ThemeData != null)
            {
                return trainer.ThemeData.GetPokemonMetrics(rand, pokeRand, all, pokemon, trainer, dataT);
            }
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

        private WeightedSet<PokemonType> GetTrainerTypeOccurence(Trainer trainer)
        {
            bool useTheming = shouldApplyTheming(trainer.TrainerCategory);
            return useTheming ? PokemonMetrics.TypeOccurence(trainer.Pokemon, p => dataT.GetBaseStats(p.species)) : null;
        }

        private void PropogateThemeData(List<Trainer> allBattles, Trainer.Category priorityCategory)
        {
            TrainerThemeData theme = null;
            // Calculate theme data
            foreach (var battle in allBattles)
            {
                if (battle.ThemeData == null || theme == battle.ThemeData)
                {
                    continue;
                }
                if (theme == null || battle.TrainerCategory == priorityCategory)
                {
                    theme = battle.ThemeData;
                }
            }
            if (theme == null)
                return;
            // Apply theme data
            foreach (var battle in allBattles)
            {
                battle.ThemeData = theme;
            }
        }

        private void AddBonusPokemon(Trainer trainer)
        {
            int bonusPokemon = numBonusPokemon(trainer.TrainerCategory);
            if(bonusPokemon <= 0)
            {
                return;
            }
            for (int i = 0; i < bonusPokemon && trainer.Pokemon.Count < trainer.MaxPokemon; i++)
            {
                trainer.Pokemon.Add(trainer.Pokemon[^1].Clone());
            }
        }

        private void ForceCustomMoves(Trainer trainer)
        {
            if(trainer.DataType == TrainerPokemon.DataType.Basic)
            {
                trainer.DataType = TrainerPokemon.DataType.SpecialMoves;
            }
            else if(trainer.DataType == TrainerPokemon.DataType.HeldItem)
            {
                trainer.DataType = TrainerPokemon.DataType.SpecialMovesAndHeldItem;
            }
        }

        public static Nature GetRandomNature(Random rand, PokemonBaseStats pokemon)
        {
            float maxStat = pokemon.stats[1..].Max();
            var normalizedStats = new float[PokemonBaseStats.numStats];
            normalizedStats[PokemonBaseStats.hpStatIndex] = 0;
            normalizedStats[PokemonBaseStats.atkStatIndex] = pokemon.Attack / maxStat;
            normalizedStats[PokemonBaseStats.defStatIndex] = pokemon.Defense / maxStat;
            normalizedStats[PokemonBaseStats.spdStatIndex] = pokemon.Speed / maxStat;
            normalizedStats[PokemonBaseStats.spAtkStatIndex] = pokemon.SpAttack / maxStat;
            normalizedStats[PokemonBaseStats.spDefStatIndex] = pokemon.SpDefense / maxStat;
            var statWeights = new WeightedSet<int>(PokemonBaseStats.numStats);
            for (int i = 1; i < PokemonBaseStats.numStats; ++i)
            {
                statWeights.Add(i, MathF.Pow(normalizedStats[i], 3));
            }
            int positiveStat = rand.Choice(statWeights);
            int negativeStat;
            if(pokemon.EffectiveAttack * 1.6f <= pokemon.SpAttack)
            {
                negativeStat = PokemonBaseStats.atkStatIndex;
            }
            else if (pokemon.SpAttack * 1.6f <= pokemon.EffectiveAttack)
            {
                negativeStat = PokemonBaseStats.spAtkStatIndex;
            }
            else if (pokemon.Speed <= (int)(pokemon.BST * 0.1f))
            {
                negativeStat = pokemon.Speed;
            }
            else
            {
                statWeights.Clear();
                for (int i = 1; i < PokemonBaseStats.numStats; ++i)
                {
                    statWeights.Add(i, 1 / MathF.Pow(normalizedStats[i], 3));
                }
                negativeStat = rand.Choice(statWeights);
            }
            return NatureUtils.GetNature(positiveStat, negativeStat);
        }


        public void RandomizeAll(List<Trainer> allBattles, IEnumerable<Pokemon> pokemonSet, TrainerSettings settings)
        {
            // No battles, return
            if(allBattles.Count <= 0)
            {
                return;
            }
            // Only one battle (non-recurring trainer)
            if(allBattles.Count == 1)
            {
                Randomize(allBattles[0], pokemonSet, settings);
                return;
            }
            // Recurring trainer
            // Sort battles
            var reoccuringBattles = new List<Trainer>(allBattles);
            reoccuringBattles.Sort(Trainer.AverageLevelComparer);
            // Choose theme data (if applicable)
            PropogateThemeData(reoccuringBattles, settings.PriorityThemeCategory);
            // Randomize the first battle of the sequence
            var firstBattle = reoccuringBattles[0];
            reoccuringBattles.RemoveAt(0);
            Randomize(firstBattle, pokemonSet, settings, false);
            // Generate the recurring sequence
            RandomizeReoccurring(firstBattle, reoccuringBattles, pokemonSet, settings);
        }

        /// <summary> Randomize the given trainer encounter </summary>
        public void Randomize(Trainer trainer, IEnumerable<Pokemon> pokemonSet, TrainerSettings settings, bool safe = true)
        {
            if (settings.RandomizePokemon)
            {
                // Add extra pokemon
                AddBonusPokemon(trainer);
            }
            // Apply level scaling
            ApplyLevelScaling(trainer, settings);
            // Set item stock (if applicable)
            // Set Battle Type
            if (settings.RandomizeBattleType)
            {
                trainer.IsDoubleBattle = rand.RollSuccess(settings.DoubleBattleChance);
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
            if (settings.RandomizePokemon)
            {
                // Set data type
                if (settings.ForceCustomMoves)
                {
                    ForceCustomMoves(trainer);
                }
                RandomizeTrainerPokemon(trainer, pokemonSet, CreatePokemonSettings(trainer, settings), settings.DuplicateMultiplier);
            }
            else
            {
                RemapVariantMovesets(trainer);
            }
        }

        public void RemapVariantMovesets(Trainer trainer)
        {
            if (!trainer.HasSpecialMoves)
                return;
            foreach(var pokemon in trainer.Pokemon)
            {
                RemapVariantMoveset(pokemon);
            }
        }

        public void RemapVariantMoveset(TrainerPokemon pokemon)
        {
            if (!pokemon.HasSpecialMoves)
                return;
            var stats = dataT.GetBaseStats(pokemon.species);
            if (stats.IsVariant)
            {
                pokemon.moves = movesetGenerator.SmartMoveSet(stats, pokemon.level);
            }
        }

        /// <summary>
        /// Randomize A sequence of battles from the same trainer.
        /// Battles is assumed to be in chronological order, and that the first battle has been appropriately randomized.
        /// Use unsafe randomization for randomizing the first battle.
        /// </summary>
        public void RandomizeReoccurring(Trainer firstBattle, List<Trainer> battles, IEnumerable<Pokemon> pokemonSet, TrainerSettings settings)
        {
            if (settings.RandomizePokemon)
            {
                // Add bonus pokemon
                foreach (var battle in battles)
                {
                    AddBonusPokemon(battle);
                }
            }
            // Battle Type
            if (settings.RandomizeBattleType)
            {
                if (settings.BattleTypeStrategy == TrainerSettings.BattleTypePcgStrategy.None)
                {
                    foreach (var battle in battles)
                    {
                        battle.IsDoubleBattle = rand.RollSuccess(settings.DoubleBattleChance);
                    }
                }
                else if (settings.BattleTypeStrategy == TrainerSettings.BattleTypePcgStrategy.KeepSameType)
                {
                    foreach (var battle in battles)
                    {
                        battle.IsDoubleBattle = firstBattle.IsDoubleBattle;
                    }
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
            if (settings.RandomizePokemon)
            {
                // Set data type
                if (settings.ForceCustomMoves)
                {
                    foreach (var battle in battles)
                    {
                        ForceCustomMoves(battle);
                    }
                }
                if (settings.PokemonStrategy == TrainerSettings.PokemonPcgStrategy.None)
                {
                    foreach (var battle in battles)
                    {
                        RandomizeTrainerPokemon(battle, pokemonSet, CreatePokemonSettings(battle, settings), settings.DuplicateMultiplier);
                    }
                }
                else if (settings.PokemonStrategy == TrainerSettings.PokemonPcgStrategy.KeepAce)
                {
                    var lastBattle = firstBattle;
                    foreach (var battle in battles)
                    {
                        var pkmnSettings = CreatePokemonSettings(battle, settings);
                        var typeOccurence = GetTrainerTypeOccurence(battle);
                        // Migrate Ace pokemon from the last battle
                        var currAce = battle.Pokemon[^1];
                        var lastAce = lastBattle.Pokemon[^1];
                        currAce.species = evoUtils.MaxEvolution(lastAce.species, currAce.level, pkmnSettings.RestrictIllegalEvolutions);
                        if (currAce.HasSpecialMoves)
                        {
                            currAce.moves = movesetGenerator.SmartMoveSet(dataT.GetBaseStats(currAce.species), currAce.level);
                        }
                        RandomizeTrainerPokemon(battle, pokemonSet, pkmnSettings, settings.DuplicateMultiplier, typeOccurence, 0, battle.Pokemon.Count - 1);
                        lastBattle = battle;
                    };
                }
                else if (settings.PokemonStrategy == TrainerSettings.PokemonPcgStrategy.KeepParty)
                {
                    var lastBattle = firstBattle;
                    foreach (var battle in battles)
                    {
                        var pkmnSettings = CreatePokemonSettings(battle, settings);
                        var typeOccurence = GetTrainerTypeOccurence(battle);
                        int lastBattleSize = lastBattle.Pokemon.Count;
                        int battleSize = battle.Pokemon.Count;
                        // Migrate pokemon from the last battle
                        for (int i = 0; i < lastBattleSize && i < battleSize; ++i)
                        {
                            var currPokemon = battle.Pokemon[battleSize - (i + 1)];
                            var lastPokemon = lastBattle.Pokemon[lastBattleSize - (i + 1)];
                            currPokemon.species = evoUtils.MaxEvolution(lastPokemon.species, currPokemon.level, pkmnSettings.RestrictIllegalEvolutions);
                            if (currPokemon.HasSpecialMoves)
                            {
                                currPokemon.moves = movesetGenerator.SmartMoveSet(dataT.GetBaseStats(currPokemon.species), currPokemon.level);
                            }
                        }
                        RandomizeTrainerPokemon(battle, pokemonSet, pkmnSettings, settings.DuplicateMultiplier, typeOccurence, 0, battleSize - lastBattleSize);
                        lastBattle = battle;
                    }
                }
            }
            else
            {
                foreach(var battle in battles)
                {
                    RemapVariantMovesets(battle);
                }
            }
        }

        private PokemonSettings CreatePokemonSettings(Trainer trainer, TrainerSettings settings)
        {
            return new PokemonSettings()
            {
                RestrictIllegalEvolutions = settings.RestrictIllegalEvolutions,
                ForceHighestLegalEvolution = settings.ForceHighestLegalEvolution,
                Noise = settings.PokemonNoise,
                BanLegendaries = shouldBanLegendaries(trainer.TrainerCategory),
                Data = settings.MetricType switch
                {
                    TrainerSettings.TrainerTypeDataSource.Individual => new List<MetricData>()
                    {
                        new MetricData(PokemonMetric.typeIndividual, 0),
                    },
                    TrainerSettings.TrainerTypeDataSource.Party => new List<MetricData>()
                    {
                        new MetricData(PokemonMetric.typeTrainerParty, 0, 3, 0.1f),
                    },
                    _ => new List<MetricData>(0)
                },
            };
        }

        private void ApplyLevelScaling(Trainer trainer, TrainerSettings settings)
        {
            if (trainer.Pokemon.Count <= 0)
            {
                return;
            }
            // Apply Difficulty Settings
            if (settings.LevelMultiplier > 0)
            {
                foreach (var pokemon in trainer.Pokemon)
                {
                    pokemon.level = (int)Math.Max(0, Math.Min(100, pokemon.level * settings.LevelMultiplier));
                }
            }
            if (settings.MinIV != 0)
            {
                // All pokemon should have the same maxIV, remap now
                int min;
                int max = trainer.Pokemon[0].MaxIV;
                if (max == PokemonBaseStats.maxIV)
                {
                    min = settings.MinIV;
                }
                else if(max == byte.MaxValue)
                {
                    min = settings.MinIV255;
                }
                else // Remap to desired range from 0-31
                {
                    min = MathUtils.MapInt(PokemonBaseStats.maxIV, max, settings.MinIV);
                }
                foreach (var pokemon in trainer.Pokemon)
                {
                    pokemon.IVLevel = Math.Max(min, pokemon.IVLevel);
                }
            }
        }

        private void ApplyAISettings(Trainer trainer, TrainerSettings settings)
        {
            if (settings.UseSmartAI && trainer is IHasTrainerAI trainerAI)
            {
                // Move failure check (basic)
                trainerAI.AIFlags.Set(0, true);
                // Go for KO
                trainerAI.AIFlags.Set(1, true);
                // Move failure check (advanced)
                trainerAI.AIFlags.Set(2, true);
                // If double battle, set double battle strats
                if (trainer.IsDoubleBattle)
                {
                    trainerAI.AIFlags.Set(7, true);
                }
                // HP Awareness
                trainerAI.AIFlags.Set(8, true);
            }
        }

        #endregion

        public void FinishPokemonRandomization(TrainerPokemon pokemon)
        {
            if (pokemon.HasSpecialMoves)
            {
                pokemon.moves = movesetGenerator.SmartMoveSet(dataT.GetBaseStats(pokemon.species), pokemon.level);
            }
        }
    }
}
