using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Randomization;
using PokemonRandomizer.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.Backend.DataStructures
{
    public class RomMetrics
    {

        public WeightedSet<PokemonType> TypeOccurenceAll { get; }
        public WeightedSet<PokemonType> TypeRatiosSingle { get; }
        public WeightedSet<PokemonType> TypeRatiosDualPrimary { get; }
        public WeightedSet<PokemonType> TypeRatiosDualSecondary { get; }
        public float DualTypePercentage { get; }
        public WeightedSet<TypeEffectiveness> TypeEffectivenessRatios { get; }

        #region Catch Rate Data

        public Dictionary<byte, HashSet<Pokemon>> CatchRateCategories { get; } = new Dictionary<byte, HashSet<Pokemon>>();

        #endregion

        #region Learnset Data

        public Dictionary<Move, List<int>> LearnLevels { get; } = new Dictionary<Move, List<int>>();
        public Dictionary<Move, double> LearnLevelMeans { get; } = new Dictionary<Move, double>();
        public Dictionary<Move, double> LearnLevelStandardDeviations { get; } = new Dictionary<Move, double>();
        public Dictionary<int, List<int>> LearnLevelPowers { get; } = new Dictionary<int, List<int>>();
        public Dictionary<int, double> LearnLevelPowerMeans { get; } = new Dictionary<int, double>();
        public Dictionary<int, double> LearnLevelPowerStandardDeviations { get; } = new Dictionary<int, double>();

        #endregion

        public Dictionary<string, WeightedSet<PokemonType>> TrainerClassTypeOccurence { get; } = new Dictionary<string, WeightedSet<PokemonType>>();

        public Dictionary<EncounterSet.Type, WeightedSet<PokemonType>> EncounterSlotTypeOccurence { get; } = new Dictionary<EncounterSet.Type, WeightedSet<PokemonType>>();

        // Calculates the balancing metrics based on the given data
        public RomMetrics(RomData data)
        {
            #region Pokemon Base Stat Metrics

            #region Type Metrics
            TypeOccurenceAll = new WeightedSet<PokemonType>();
            TypeRatiosSingle = new WeightedSet<PokemonType>();
            TypeRatiosDualPrimary = new WeightedSet<PokemonType>();
            TypeRatiosDualSecondary = new WeightedSet<PokemonType>();
            foreach (PokemonBaseStats pkmn in data.Pokemon)
            {
                // if pkmn is single-typed
                if (pkmn.IsSingleTyped)
                {
                    TypeOccurenceAll.Add(pkmn.PrimaryType);
                    TypeRatiosSingle.Add(pkmn.PrimaryType);
                }
                else
                {
                    TypeOccurenceAll.Add(pkmn.PrimaryType);
                    TypeOccurenceAll.Add(pkmn.SecondaryType);
                    TypeRatiosDualPrimary.Add(pkmn.PrimaryType);
                    TypeRatiosDualSecondary.Add(pkmn.SecondaryType);
                }
            }
            DualTypePercentage = data.Pokemon.Count / TypeRatiosDualPrimary.Count;
            #endregion

            #region Type Effectiveness Metrics
            TypeEffectivenessRatios = new WeightedSet<TypeEffectiveness>();
            foreach (var atkType in data.Types)
                foreach (var defType in data.Types)
                    TypeEffectivenessRatios.Add(data.TypeDefinitions.GetEffectiveness(atkType, defType));
            #endregion

            #region Catch Rate Metrics
            foreach (PokemonBaseStats pkmn in data.Pokemon)
            {
                if (!CatchRateCategories.ContainsKey(pkmn.catchRate))
                    CatchRateCategories.Add(pkmn.catchRate, new HashSet<Pokemon>());
                CatchRateCategories[pkmn.catchRate].Add(pkmn.species);
            }

            #endregion

            #endregion

            #region LearnSet Metrics
            foreach (PokemonBaseStats pkmn in data.Pokemon)
            {
                foreach (var entry in pkmn.learnSet)
                {
                    if (entry.learnLvl == 1 && !pkmn.IsBasicOrEvolvesFromBaby)
                        continue;
                    if (!LearnLevels.ContainsKey(entry.move))
                        LearnLevels.Add(entry.move, new List<int>());
                    LearnLevels[entry.move].Add(entry.learnLvl);
                    int effectivePower = data.GetMoveData(entry.move).EffectivePower;
                    if (!LearnLevelPowers.ContainsKey(effectivePower))
                        LearnLevelPowers.Add(effectivePower, new List<int>());
                    LearnLevelPowers[effectivePower].Add(entry.learnLvl);
                }
            }
            foreach(var kvp in LearnLevels)
            {
                LearnLevelMeans.Add(kvp.Key, kvp.Value.Average());
                LearnLevelStandardDeviations.Add(kvp.Key, Distribution.StandardDeviation(kvp.Value));
            }
            foreach (var kvp in LearnLevelPowers)
            {
                LearnLevelPowerMeans.Add(kvp.Key, kvp.Value.Average());
                LearnLevelPowerStandardDeviations.Add(kvp.Key, Distribution.StandardDeviation(kvp.Value));
            }
            #endregion

            foreach(var trainer in data.Trainers)
            {
                if (!TrainerClassTypeOccurence.ContainsKey(trainer.ClassName))
                {
                    TrainerClassTypeOccurence.Add(trainer.ClassName, new WeightedSet<PokemonType>(16));
                }
                var set = TrainerClassTypeOccurence[trainer.ClassName];
                foreach(var pokemon in trainer.Pokemon)
                {
                    var pData = data.GetBaseStats(pokemon.species);
                    set.Add(pData.PrimaryType);
                    if (!pData.IsSingleTyped)
                    {
                        set.Add(pData.SecondaryType);
                    }
                }
            }

            foreach (var encounterSet in data.Encounters)
            {
                if (!EncounterSlotTypeOccurence.ContainsKey(encounterSet.type))
                {
                    EncounterSlotTypeOccurence.Add(encounterSet.type, new WeightedSet<PokemonType>(16));
                }
                var set = EncounterSlotTypeOccurence[encounterSet.type];
                foreach (var enc in encounterSet.RealEncounters)
                {
                    var pData = data.GetBaseStats(enc.Pokemon);
                    set.Add(pData.PrimaryType);
                    if (!pData.IsSingleTyped)
                    {
                        set.Add(pData.SecondaryType);
                    }
                }
            }
        }
    }
}
