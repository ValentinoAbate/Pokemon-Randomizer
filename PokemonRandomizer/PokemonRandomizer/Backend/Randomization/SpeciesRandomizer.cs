using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.Randomization
{
    using EnumTypes;
    using Utilities;
    using DataStructures;
    public class SpeciesRandomizer
    {
        private readonly EvolutionUtils evoUtils;
        private readonly Random rand;
        private readonly Func<PokemonSpecies, PokemonBaseStats> baseStats;
        private readonly Dictionary<PokemonSpecies, float> powerScores;

        public SpeciesRandomizer(EvolutionUtils evoUtils, Random rand, Func<PokemonSpecies, PokemonBaseStats> baseStats, Dictionary<PokemonSpecies, float> powerScores)
        {
            this.evoUtils = evoUtils;
            this.rand = rand;
            this.baseStats = baseStats;
            this.powerScores = powerScores;
        }
        #region Species Randomization

        /// <summary> Chose a random species from the input set based on the given species settings and the type sample given</summary> 
        public PokemonSpecies RandomSpeciesTypeGroup(IEnumerable<PokemonSpecies> possiblePokemon, PokemonSpecies pokemon, int level, IEnumerable<PokemonSpecies> typeGroup, Settings.SpeciesSettings settings)
        {
            var newSpecies = RandomSpeciesTypeGroup(possiblePokemon, pokemon, typeGroup, settings);
            if (settings.ForceHighestLegalEvolution)
                newSpecies = evoUtils.MaxEvolution(newSpecies, level, settings.RestrictIllegalEvolutions);
            else if (settings.RestrictIllegalEvolutions)
                newSpecies = evoUtils.CorrectImpossibleEvo(newSpecies, level);
            // Actually choose the species
            return newSpecies;
        }
        /// <summary> Chose a random species from the input set based on the given species settings and the type sample given</summary> 
        public PokemonSpecies RandomSpeciesTypeGroup(IEnumerable<PokemonSpecies> possiblePokemon, PokemonSpecies pokemon, IEnumerable<PokemonSpecies> typeGroup, Settings.SpeciesSettings settings)
        {
            var combinedWeightings = SpeciesWeightedSetTypeGroup(possiblePokemon, pokemon, typeGroup, settings);
            // Actually choose the species
            return rand.Choice(combinedWeightings);
        }
        /// <summary> Chose a random species from the input set based on the given species settings</summary> 
        public PokemonSpecies RandomSpecies(IEnumerable<PokemonSpecies> possiblePokemon, PokemonSpecies pokemon, Settings.SpeciesSettings settings)
        {
            var combinedWeightings = SpeciesWeightedSet(possiblePokemon, pokemon, settings);
            // Actually choose the species
            return rand.Choice(combinedWeightings);
        }
        /// <summary> Chose a random species from the input set based on the given species settings.
        /// If speciesSettings.DisableIllegalEvolutions is true, scale impossible evolutions down to their less evolved forms </summary> 
        public PokemonSpecies RandomSpecies(IEnumerable<PokemonSpecies> possiblePokemon, PokemonSpecies pokemon, int level, Settings.SpeciesSettings settings)
        {
            var newSpecies = RandomSpecies(possiblePokemon, pokemon, settings);
            if (settings.ForceHighestLegalEvolution)
                newSpecies = evoUtils.MaxEvolution(newSpecies, level, settings.RestrictIllegalEvolutions);
            else if (settings.RestrictIllegalEvolutions)
                newSpecies = evoUtils.CorrectImpossibleEvo(newSpecies, level);
            // Actually choose the species
            return newSpecies;
        }

        private Func<PokemonSpecies, float> GetTypeBalanceFunction(IEnumerable<PokemonSpecies> possiblePokemon)
        {
            var typeOccurenceLookup = new Dictionary<PokemonType, float>();
            foreach (var type in EnumUtils.GetValues<PokemonType>())
            {
                typeOccurenceLookup.Add(type, 0);
            }
            foreach (var pokemon in possiblePokemon)
            {
                var pData = baseStats(pokemon);
                typeOccurenceLookup[pData.types[0]] += 1;
                if (pData.IsSingleTyped)
                    continue;
                typeOccurenceLookup[pData.types[1]] += 1;
            }
            foreach (var type in EnumUtils.GetValues<PokemonType>())
            {
                float val = typeOccurenceLookup[type];
                typeOccurenceLookup[type] = val == 0 ? 0 : 1 / val;
            }
            float TypeBalanceMetric(PokemonSpecies s)
            {
                var pData = baseStats(s);
                if (pData.IsSingleTyped)
                    return typeOccurenceLookup[pData.types[0]];
                return (typeOccurenceLookup[pData.types[0]] + typeOccurenceLookup[pData.types[1]]) / 2;
            }
            return TypeBalanceMetric;
        }
        /// <summary> Get a weighted and culled list of possible pokemon (TODO: MAKE PRIVATE)</summary>
        public WeightedSet<PokemonSpecies> SpeciesWeightedSet(IEnumerable<PokemonSpecies> possiblePokemon, PokemonSpecies pokemon, Settings.SpeciesSettings settings)
        {
            var combinedWeightings = new WeightedSet<PokemonSpecies>(possiblePokemon);
            // Power level similarity
            if (settings.PowerScaleSimilarityMod > 0)
            {
                var powerWeighting = PokemonMetrics.PowerSimilarity(combinedWeightings.Items, powerScores, pokemon, settings.PowerThresholdStronger, settings.PowerThresholdWeaker);
                combinedWeightings.Add(powerWeighting, settings.PowerScaleSimilarityMod);
                // Cull if necessary
                if (settings.PowerScaleCull)
                    combinedWeightings.RemoveWhere((p) => !powerWeighting.Contains(p));
            }
            // Type similarity
            if (settings.TypeSimilarityMod > 0)
            {
                var typeWeighting = PokemonMetrics.TypeSimilarity(combinedWeightings.Items, pokemon, baseStats);
                typeWeighting.Multiply(GetTypeBalanceFunction(combinedWeightings.Items)(pokemon));
                typeWeighting.Normalize();
                combinedWeightings.Add(typeWeighting, settings.TypeSimilarityMod);
                // Cull if necessary
                if (settings.TypeSimilarityCull)
                    combinedWeightings.RemoveWhere((p) => !typeWeighting.Contains(p));
            }
            // Sharpen the data if necessary
            if (settings.Sharpness > 0)
            {
                combinedWeightings.Map((p) => (float)Math.Pow(combinedWeightings[p], settings.Sharpness));
                combinedWeightings.Normalize();
            }
            // Normalize combined weightings and add noise
            if (settings.Noise > 0)
            {
                combinedWeightings.Normalize();
                var noise = new WeightedSet<PokemonSpecies>(possiblePokemon, settings.Noise);
                combinedWeightings.Add(noise);
            }
            // Remove Legendaries
            if (settings.BanLegendaries)
                combinedWeightings.RemoveWhere(SpeciesUtils.IsLegendary);
            combinedWeightings.RemoveWhere(p => combinedWeightings[p] <= 0);
            return combinedWeightings;
        }
        private WeightedSet<PokemonSpecies> SpeciesWeightedSetTypeGroup(IEnumerable<PokemonSpecies> possiblePokemon, PokemonSpecies pokemon, IEnumerable<PokemonSpecies> typeGroup, Settings.SpeciesSettings settings)
        {
            var combinedWeightings = new WeightedSet<PokemonSpecies>(possiblePokemon);
            // Power level similarity
            if (settings.PowerScaleSimilarityMod > 0)
            {
                var powerWeighting = PokemonMetrics.PowerSimilarity(combinedWeightings.Items, powerScores, pokemon, settings.PowerThresholdStronger, settings.PowerThresholdWeaker);
                combinedWeightings.Add(powerWeighting, settings.PowerScaleSimilarityMod);
                // Cull if necessary
                if (settings.PowerScaleCull)
                    combinedWeightings.RemoveWhere((p) => !powerWeighting.Contains(p));
            }
            // Type similarity
            if (settings.TypeSimilarityMod > 0)
            {
                var typeBalanceMetric = GetTypeBalanceFunction(combinedWeightings.Items);
                var typeWeighting = PokemonMetrics.TypeSimilarity(combinedWeightings.Items, pokemon, baseStats);
                typeWeighting.Multiply(typeBalanceMetric);
                foreach (var sample in typeGroup)
                {
                    typeWeighting.Add(PokemonMetrics.TypeSimilarity(combinedWeightings.Items, sample, baseStats), typeBalanceMetric(sample));
                }
                var sampleTypes = typeGroup.SelectMany((s) => baseStats(s).types).Distinct();
                Tuple<PokemonType, PokemonType> Map(PokemonSpecies p)
                {
                    var types = baseStats(p).types.Intersect(sampleTypes).ToList();
                    types.Sort();
                    if (types.Count == 0)
                        return null;
                    if (types.Count == 1)
                        return new Tuple<PokemonType, PokemonType>(types[0], types[0]);
                    return new Tuple<PokemonType, PokemonType>(types[0], types[1]);
                }
                var typeDistribution = typeWeighting.Distribution(Map);
                typeWeighting.Normalize();
                combinedWeightings.Add(typeWeighting, settings.TypeSimilarityMod);
                // Cull if necessary
                if (settings.TypeSimilarityCull)
                    combinedWeightings.RemoveWhere((p) => !typeWeighting.Contains(p));
            }
            // Sharpen the data if necessary
            if (settings.Sharpness > 0)
            {
                combinedWeightings.Map((p) => (float)Math.Pow(combinedWeightings[p], settings.Sharpness));
                combinedWeightings.Normalize();
            }
            // Normalize combined weightings and add noise
            if (settings.Noise > 0)
            {
                combinedWeightings.Normalize();
                var noise = new WeightedSet<PokemonSpecies>(possiblePokemon, settings.Noise);
                combinedWeightings.Add(noise);
            }
            // Remove Legendaries
            if (settings.BanLegendaries)
                combinedWeightings.RemoveWhere(SpeciesUtils.IsLegendary);
            combinedWeightings.RemoveWhere((p) => combinedWeightings[p] <= 0);
            return combinedWeightings;
        }
        #endregion
    }
}
