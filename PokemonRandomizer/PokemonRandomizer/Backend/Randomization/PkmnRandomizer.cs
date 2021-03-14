using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.Backend.Randomization
{
    using DataStructures;
    using EnumTypes;
    using Utilities;
    public class PkmnRandomizer
    {
        public const string powerDataSource = "power";
        public const string typeDataSource = "type";
        public const string typeGroupDataSource = typeDataSource + "group";
        private readonly EvolutionUtils evoUtils;
        private readonly Random rand;
        private readonly Func<Pokemon, PokemonBaseStats> baseStats;
        private readonly Dictionary<Pokemon, float> powerScores;

        public PkmnRandomizer(EvolutionUtils evoUtils, Random rand, Func<Pokemon, PokemonBaseStats> baseStats, Dictionary<Pokemon, float> powerScores)
        {
            this.evoUtils = evoUtils;
            this.rand = rand;
            this.baseStats = baseStats;
            this.powerScores = powerScores;
        }

        #region Pokemon Randomization

        public Pokemon RandomPokemon(IEnumerable<Pokemon> all, IEnumerable<Metric<Pokemon>> data, Settings settings, int level)
        {
            // Get a random pokemon (could possibly prefilter for level later to even out evolution stage odds unbalance)
            var newPokemon = RandomPokemon(all, data, settings);
            // Restrict the evolution if specified in the settings
            if (settings.ForceHighestLegalEvolution)
                newPokemon = evoUtils.MaxEvolution(newPokemon, level, settings.RestrictIllegalEvolutions);
            else if (settings.RestrictIllegalEvolutions)
                newPokemon = evoUtils.CorrectImpossibleEvo(newPokemon, level);
            return newPokemon;
        }
        public Pokemon RandomPokemon(IEnumerable<Pokemon> all, IEnumerable<Metric<Pokemon>> data, Settings settings)
        {
            // If we roll on the noise
            if (rand.RollSuccess(settings.Noise))
            {
                if (settings.BanLegendaries) // Remove legendaries if banned
                    return rand.Choice(all.Where(p => !p.IsLegendary()));
                return rand.Choice(all); 
            }
            // Process the metrics
            var set = Metric<Pokemon>.ProcessGroup(data);
            if (settings.BanLegendaries) // Remove legendaries if banned
            {
                set.RemoveWhere(PokemonUtils.IsLegendary);
            }
            return rand.Choice(set);
        }

        public WeightedSet<Pokemon> TypeSimilarityIndividual(IEnumerable<Pokemon> all, Pokemon pokemon)
        {
            var set = PokemonMetrics.TypeSimilarity(all, pokemon, baseStats);
            set.Multiply(GetTypeBalanceFunction(all)(pokemon));
            return set;
        }

        public WeightedSet<Pokemon> TypeSimilarityGroup(IEnumerable<Pokemon> all, Pokemon pokemon, WeightedSet<PokemonType> typeData)
        {
            var set = new WeightedSet<Pokemon>(all, 1);
            set.Multiply(GetTypeBalanceFunction(all)(pokemon));
            float TypeMultiplier(Pokemon p)
            {
                var data = baseStats(p);
                float type1Val = typeData.Contains(data.types[0]) ? typeData[data.types[0]] : 0;
                if (data.IsSingleTyped) // If single typed, just return the first type value
                {
                    return type1Val;
                }
                // For dual-typed pokemon, return the sum of their type values
                return type1Val + (typeData.Contains(data.types[1]) ? typeData[data.types[1]] : 0);
            }
            set.Multiply(TypeMultiplier);
            set.RemoveWhere(p => set[p] <= 0);
            return set;
        }

        public WeightedSet<Pokemon> PowerSimilarityIndividual(IEnumerable<Pokemon> all, Pokemon pokemon)
        {
            return PokemonMetrics.PowerSimilarity(all, powerScores, pokemon, 300, 300);
        }

        /// <summary> Chose a random species from the input set based on the given species settings and the type sample given</summary> 
        public Pokemon RandomTypeGroup(IEnumerable<Pokemon> possiblePokemon, Pokemon pokemon, int level, IEnumerable<Pokemon> typeGroup, Settings settings)
        {
            var newPokemon = RandomTypeGroup(possiblePokemon, pokemon, typeGroup, settings);
            if (settings.ForceHighestLegalEvolution)
                newPokemon = evoUtils.MaxEvolution(newPokemon, level, settings.RestrictIllegalEvolutions);
            else if (settings.RestrictIllegalEvolutions)
                newPokemon = evoUtils.CorrectImpossibleEvo(newPokemon, level);
            // Actually choose the species
            return newPokemon;
        }
        /// <summary> Chose a random species from the input set based on the given species settings and the type sample given</summary> 
        public Pokemon RandomTypeGroup(IEnumerable<Pokemon> possiblePokemon, Pokemon pokemon, IEnumerable<Pokemon> typeGroup, Settings settings)
        {
            var combinedWeightings = GetWeightedSetTypeGroup(possiblePokemon, pokemon, typeGroup, settings);
            // Actually choose the species
            return rand.Choice(combinedWeightings);
        }
        /// <summary> Chose a random species from the input set based on the given species settings</summary> 
        public Pokemon Random(IEnumerable<Pokemon> possiblePokemon, Pokemon pokemon, Settings settings)
        {
            var combinedWeightings = GetWeightedSet(possiblePokemon, pokemon, settings);
            // Actually choose the species
            return rand.Choice(combinedWeightings);
        }
        /// <summary> Chose a random species from the input set based on the given species settings.
        /// If speciesSettings.DisableIllegalEvolutions is true, scale impossible evolutions down to their less evolved forms </summary> 
        public Pokemon Random(IEnumerable<Pokemon> possiblePokemon, Pokemon pokemon, int level, Settings settings)
        {
            var newPokemon = Random(possiblePokemon, pokemon, settings);
            if (settings.ForceHighestLegalEvolution)
                newPokemon = evoUtils.MaxEvolution(newPokemon, level, settings.RestrictIllegalEvolutions);
            else if (settings.RestrictIllegalEvolutions)
                newPokemon = evoUtils.CorrectImpossibleEvo(newPokemon, level);
            // Actually choose the species
            return newPokemon;
        }

        /// <summary>
        /// Function that balance weightings with the occurence rate of the types in all
        /// Makes water types less likely to dominate just cause there are so many of them, etc
        /// </summary>
        private Func<Pokemon, float> GetTypeBalanceFunction(IEnumerable<Pokemon> all)
        {
            var typeOccurenceLookup = new Dictionary<PokemonType, float>();
            foreach (var type in EnumUtils.GetValues<PokemonType>())
            {
                typeOccurenceLookup.Add(type, 0);
            }
            foreach (var pokemon in all)
            {
                var pData = baseStats(pokemon);
                typeOccurenceLookup[pData.types[0]] += 1;
                if (!pData.IsSingleTyped)
                {
                    typeOccurenceLookup[pData.types[1]] += 1;
                }
            }
            foreach (var type in EnumUtils.GetValues<PokemonType>())
            {
                float val = typeOccurenceLookup[type];
                typeOccurenceLookup[type] = val == 0 ? 0 : 1 / val;
            }
            float TypeBalanceMetric(Pokemon s)
            {
                var pData = baseStats(s);
                if (pData.IsSingleTyped)
                    return typeOccurenceLookup[pData.types[0]];
                return (typeOccurenceLookup[pData.types[0]] + typeOccurenceLookup[pData.types[1]]) / 2;
            }
            return TypeBalanceMetric;
        }
        /// <summary> Get a weighted and culled list of possible pokemon</summary>
        private WeightedSet<Pokemon> GetWeightedSet(IEnumerable<Pokemon> possiblePokemon, Pokemon pokemon, Settings settings)
        {
            var combinedWeightings = new WeightedSet<Pokemon>(possiblePokemon);
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
                var noise = new WeightedSet<Pokemon>(possiblePokemon, settings.Noise);
                combinedWeightings.Add(noise);
            }
            // Remove Legendaries
            if (settings.BanLegendaries)
                combinedWeightings.RemoveWhere(PokemonUtils.IsLegendary);
            combinedWeightings.RemoveWhere(p => combinedWeightings[p] <= 0);
            return combinedWeightings;
        }
        private WeightedSet<Pokemon> GetWeightedSetTypeGroup(IEnumerable<Pokemon> possiblePokemon, Pokemon pokemon, IEnumerable<Pokemon> typeGroup, Settings settings)
        {
            var combinedWeightings = new WeightedSet<Pokemon>(possiblePokemon);
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
                Tuple<PokemonType, PokemonType> Map(Pokemon p)
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
                var noise = new WeightedSet<Pokemon>(possiblePokemon, settings.Noise);
                combinedWeightings.Add(noise);
            }
            // Remove Legendaries
            if (settings.BanLegendaries)
                combinedWeightings.RemoveWhere(PokemonUtils.IsLegendary);
            combinedWeightings.RemoveWhere((p) => combinedWeightings[p] <= 0);
            return combinedWeightings;
        }
        #endregion

        #region Type Triangles

        /// <summary> Return 3 pokemon that form a valid type traingle, or null if none exist in the input set.
        /// Type triangles require one-way weakness, but allow neutral relations in reverse order (unless strong is true) </summary>
        public List<Pokemon> RandomTypeTriangle(IEnumerable<Pokemon> possiblePokemon, IList<Pokemon> input, TypeEffectivenessChart typeDefinitions, Settings settings, bool strong = false)
        {
            // invalid input
            if (input.Count < 3)
                return null; // TODO: Log
            var set = GetWeightedSet(possiblePokemon, input[0], settings);
            if (settings.RestrictIllegalEvolutions)
                set.RemoveWhere((p) => !evoUtils.IsPokemonValidLevel(baseStats(p), 5));
            var pool = new WeightedSet<Pokemon>(set);
            while (pool.Count > 0)
            {
                var first = rand.Choice(pool);
                pool.Remove(first);
                // Get potential second pokemon
                var secondPossiblities = GetWeightedSet(set.Items, input[1], settings);
                secondPossiblities.RemoveWhere((p) => !OneWayWeakness(typeDefinitions, first, p, strong));
                // Finish the triangle if possible
                var triangle = FinishTriangle(set, secondPossiblities, first, input[2], typeDefinitions, settings, strong);
                if (triangle != null)
                    return triangle;
            }
            return null; // No viable triangle with input spcifications
        }
        /// <summary> Helper method for the RandomTypeTriangle method </summary>
        private List<Pokemon> FinishTriangle(WeightedSet<Pokemon> set, WeightedSet<Pokemon> possibleSeconds, Pokemon first, Pokemon lastInput, TypeEffectivenessChart typeDefinitions, Settings settings, bool strong)
        {
            while (possibleSeconds.Count > 0)
            {
                var second = rand.Choice(possibleSeconds);
                possibleSeconds.Remove(second);
                // Get third pokemon
                var thirdPossiblities = GetWeightedSet(set.Items, lastInput, settings);
                thirdPossiblities.RemoveWhere((p) => !(OneWayWeakness(typeDefinitions, second, p, strong) && OneWayWeakness(typeDefinitions, p, first, strong)));
                // If at least one works, choose one randomly
                if (thirdPossiblities.Count > 0)
                    return new List<Pokemon> { first, second, rand.Choice(thirdPossiblities) };
            }
            return null;
        }

        /// <summary> Return true if b is weak to a AND a is not weak to b. 
        /// If strong is true, b must also not be normally effective against a </summary>
        private bool OneWayWeakness(TypeEffectivenessChart typeDefinitions, Pokemon a, Pokemon b, bool strong = true)
        {
            var aTypes = baseStats(a).types;
            var bTypes = baseStats(b).types;
            var aVsB = typeDefinitions.GetEffectiveness(aTypes[0], aTypes[1], bTypes[0], bTypes[1]);
            var bVsA = typeDefinitions.GetEffectiveness(bTypes[0], bTypes[1], aTypes[0], aTypes[1]);
            if (strong)
                return aVsB == TypeEffectiveness.SuperEffective && !(bVsA == TypeEffectiveness.SuperEffective || bVsA == TypeEffectiveness.Normal);
            return aVsB == TypeEffectiveness.SuperEffective && !(bVsA == TypeEffectiveness.SuperEffective);
        }

        #endregion

        public class Settings
        {
            public enum WeightingType
            {
                Individual,
                Group,
            }

            public bool RestrictIllegalEvolutions { get; set; } = true;
            public bool ForceHighestLegalEvolution { get; set; } = false;
            public bool BanLegendaries { get; set; } = false;
            public WeightingType WeightType { get; set; } = WeightingType.Individual;
            public float Sharpness { get; set; } = 0;
            public float Noise { get; set; } = 0;
            public float PowerScaleSimilarityMod { get; set; } = 0;
            public bool PowerScaleCull { get; set; } = false;
            public int PowerThresholdStronger { get; set; } = 100;
            public int PowerThresholdWeaker { get; set; } = 100;
            public float TypeSimilarityMod { get; set; } = 0;
            public bool TypeSimilarityCull { get; set; } = false;
        }
    }
}
