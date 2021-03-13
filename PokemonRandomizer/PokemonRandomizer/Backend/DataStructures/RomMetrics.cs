using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Randomization;
using PokemonRandomizer.Backend.Utilities;
using PokemonRandomizer.Statistics;

namespace PokemonRandomizer.Backend.DataStructures
{
    public class RomMetrics
    {

        public WeightedSet<PokemonType> TypeRatiosAll { get; }
        public WeightedSet<PokemonType> TypeRatiosSingle { get; }
        public WeightedSet<PokemonType> TypeRatiosDualPrimary { get; }
        public WeightedSet<PokemonType> TypeRatiosDualSecondary { get; }
        public float DualTypePercentage { get; }
        public WeightedSet<TypeEffectiveness> TypeEffectivenessRatios { get; }

        #region Catch Rate Data

        public Dictionary<byte, HashSet<PokemonSpecies>> CatchRateCategories { get; } = new Dictionary<byte, HashSet<PokemonSpecies>>();

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

        // Calculates the balancing metrics based on the given data
        public RomMetrics(RomData data)
        {
            #region Pokemon Base Stat Metrics

            #region Type Metrics
            TypeRatiosAll = new WeightedSet<PokemonType>();
            TypeRatiosSingle = new WeightedSet<PokemonType>();
            TypeRatiosDualPrimary = new WeightedSet<PokemonType>();
            TypeRatiosDualSecondary = new WeightedSet<PokemonType>();
            foreach (PokemonBaseStats pkmn in data.Pokemon)
            {
                // if pkmn is single-typed
                if (pkmn.IsSingleTyped)
                {
                    TypeRatiosAll.Add(pkmn.types[0]);
                    TypeRatiosSingle.Add(pkmn.types[0]);
                }
                else
                {
                    TypeRatiosAll.Add(pkmn.types[0]);
                    TypeRatiosAll.Add(pkmn.types[1]);
                    TypeRatiosDualPrimary.Add(pkmn.types[0]);
                    TypeRatiosDualSecondary.Add(pkmn.types[1]);
                }
            }
            DualTypePercentage = data.Pokemon.Count / TypeRatiosDualPrimary.Count;
            #endregion

            #region Type Effectiveness Metrics
            TypeEffectivenessRatios = new WeightedSet<TypeEffectiveness>();
            foreach (var atkType in EnumUtils.GetValues<PokemonType>())
                foreach (var defType in EnumUtils.GetValues<PokemonType>())
                    TypeEffectivenessRatios.Add(data.TypeDefinitions.GetEffectiveness(atkType, defType));
            #endregion

            #region Catch Rate Metrics
            foreach (PokemonBaseStats pkmn in data.Pokemon)
            {
                if (!CatchRateCategories.ContainsKey(pkmn.catchRate))
                    CatchRateCategories.Add(pkmn.catchRate, new HashSet<PokemonSpecies>());
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
                    int effectivePower = data.MoveData[(int)entry.move].EffectivePower;
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
                if (!TrainerClassTypeOccurence.ContainsKey(trainer.Class))
                {
                    TrainerClassTypeOccurence.Add(trainer.Class, new WeightedSet<PokemonType>());
                }
                var set = TrainerClassTypeOccurence[trainer.Class];
                foreach(var pokemon in trainer.pokemon)
                {
                    var pData = data.PokemonLookup[pokemon.species];
                    set.Add(pData.types[0]);
                    if (!pData.IsSingleTyped)
                    {
                        set.Add(pData.types[1]);
                    }
                }
            }
        }
    }
}
