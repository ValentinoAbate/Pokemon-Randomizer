using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.Backend.Randomization
{
    using DataStructures;
    using EnumTypes;
    using Utilities;
    using static Settings;
    public class PkmnRandomizer
    {
        public const string powerDataSource = "power";
        public const string typeDataSource = "type";
        public const string typeGroupDataSource = typeDataSource + "group";
        private readonly EvolutionUtils evoUtils;
        private readonly Random rand;
        private readonly IDataTranslator dataT;
        private readonly Dictionary<Pokemon, float> powerScores;
        private readonly RomMetrics romMetrics;

        public PkmnRandomizer(EvolutionUtils evoUtils, Random rand, IDataTranslator dataT, RomMetrics romMetrics, Dictionary<Pokemon, float> powerScores)
        {
            this.evoUtils = evoUtils;
            this.rand = rand;
            this.dataT = dataT;
            this.powerScores = powerScores;
            this.romMetrics = romMetrics;
        }

        #region Pokemon Randomization

        private Pokemon SafePokemon(Pokemon original, Pokemon newPokemon) => newPokemon != Pokemon.None ? newPokemon : original;

        // Safe methods (if there is no valid pokemon the original will be returned

        public Pokemon RandomPokemon(List<Pokemon> all, Pokemon pokemon, PokemonSettings settings, int level)
        {
            return SafePokemon(pokemon, RandomPokemonUnsafe(all, CreateBasicMetrics(all, pokemon, settings.Data), settings, level));
        }
        public Pokemon RandomPokemon(List<Pokemon> all, Pokemon pokemon, PokemonSettings settings)
        {
            return SafePokemon(pokemon, RandomPokemonUnsafe(all, CreateBasicMetrics(all, pokemon, settings.Data), settings));
        }
        public Pokemon RandomPokemon(List<Pokemon> all, Pokemon pokemon, IEnumerable<Metric<Pokemon>> data, PokemonSettings settings, int level)
        {
            return SafePokemon(pokemon, RandomPokemonUnsafe(all, data, settings, level));
        }
        public Pokemon RandomPokemon(List<Pokemon> all, Pokemon pokemon, IEnumerable<Metric<Pokemon>> data, PokemonSettings settings)
        {
            return SafePokemon(pokemon, RandomPokemonUnsafe(all, data, settings));
        }
        public Pokemon RandomPokemonRestricted(List<Pokemon> all, List<Pokemon> restricted, Pokemon pokemon, PokemonSettings settings, int level)
        {
            return SafePokemon(pokemon, RandomPokemonUnsafe(all, restricted, CreateBasicMetrics(all, pokemon, settings.Data), settings, level));
        }
        public Pokemon RandomPokemonRestricted(List<Pokemon> all, List<Pokemon> restricted, Pokemon pokemon, PokemonSettings settings)
        {
            return SafePokemon(pokemon, RandomPokemonUnsafe(all, restricted, CreateBasicMetrics(all, pokemon, settings.Data), settings));
        }
        public Pokemon RandomPokemonRestricted(List<Pokemon> all, List<Pokemon> restricted, Pokemon pokemon, IEnumerable<Metric<Pokemon>> data, PokemonSettings settings, int level)
        {
            return SafePokemon(pokemon, RandomPokemonUnsafe(all, restricted, data, settings, level));
        }
        public Pokemon RandomPokemonRestricted(List<Pokemon> all, List<Pokemon> restricted, Pokemon pokemon, IEnumerable<Metric<Pokemon>> data, PokemonSettings settings)
        {
            return SafePokemon(pokemon, RandomPokemonUnsafe(all, restricted, data, settings));
        }

        // Unsafe methods (if there is no valid pokemon, Pokemon.None will be returned)



        private Pokemon RandomPokemonUnsafe(List<Pokemon> all, List<Pokemon> restricted, IEnumerable<Metric<Pokemon>> data, PokemonSettings settings, int level)
        {
            if (settings.RestrictIllegalEvolutions)
            {
                restricted.RemoveAll(p => !evoUtils.IsPokemonValidLevel(dataT.GetBaseStats(p), level));
            }
            if (settings.ForceHighestLegalEvolution)
            {
                restricted.RemoveAll(p => !evoUtils.IsMaxEvolution(p, level));
            }
            var newPokemon = RandomPokemonUnsafe(all, restricted, data, settings);
            // If we got a null result, propegate that
            if (newPokemon == Pokemon.None)
                return Pokemon.None;
            return newPokemon;
        }
        private Pokemon RandomPokemonUnsafe(List<Pokemon> all, List<Pokemon> restricted, IEnumerable<Metric<Pokemon>> data, PokemonSettings settings)
        {
            if (!restricted.Any())
            {
                return Pokemon.None;
            }
            // If we roll noise, choose a completely random pick
            if (rand.RollSuccess(settings.Noise))
            {
                return rand.Choice(all);
            }
            // If there is no metric data
            if (!data.Any())
            {
                if (settings.BanLegendaries) // Remove legendaries if banned
                {
                    restricted.RemoveAll(PokemonUtils.IsLegendary);
                }
                return rand.Choice(restricted); 
            }
            // Process the metrics
            var metricChoices = Metric<Pokemon>.ProcessGroup(data);
            // Remove any pokemon that were not in the input set
            var restrictedHashSet = restricted.ToHashSet();
            metricChoices.RemoveWhere(p => !restrictedHashSet.Contains(p));
            if (settings.BanLegendaries) // Remove legendaries if banned
            {
                metricChoices.RemoveWhere(PokemonUtils.IsLegendary);
            }
            return metricChoices.Count > 0 ? rand.Choice(metricChoices) : Pokemon.None;
        }
        private Pokemon RandomPokemonUnsafe(List<Pokemon> all, IEnumerable<Metric<Pokemon>> data, PokemonSettings settings, int level)
        {
            return RandomPokemonUnsafe(all, all, data, settings, level);
        }
        private Pokemon RandomPokemonUnsafe(List<Pokemon> all, IEnumerable<Metric<Pokemon>> data, PokemonSettings settings)
        {
            return RandomPokemonUnsafe(all, all, data, settings);
        }

        #endregion

        #region Type Triangles

        /// <summary> Return 3 pokemon that form a valid type traingle, or null if none exist in the input set.
        /// Type triangles require one-way weakness, but allow neutral relations in reverse order (unless strong is true) </summary>
        public List<Pokemon> RandomTypeTriangle(IEnumerable<Pokemon> possiblePokemon, IList<Pokemon> input, TypeEffectivenessChart typeDefinitions, PokemonSettings settings, bool strong = false)
        {
            // invalid input
            if (input.Count < 3)
                return null; // TODO: Log
            var pool = possiblePokemon.Where(p => evoUtils.IsPokemonValidLevel(dataT.GetBaseStats(p), 5)).ToHashSet();
            var list = pool.ToList();
            var metrics = new List<Metric<Pokemon>>[]
            {
                CreateBasicMetrics(pool, input[0], settings.Data),
                CreateBasicMetrics(pool, input[1], settings.Data),
                CreateBasicMetrics(pool, input[2], settings.Data),
            };

            while (pool.Count > 0)
            {
                var first = RandomPokemonUnsafe(list, metrics[0], settings);
                // No valid pokemon for first pokemon so no valid triangle
                if (first == Pokemon.None)
                    return null;
                pool.Remove(first);
                list.Clear();
                list.AddRange(pool);
                // Get potential second pokemon
                var secondPossiblities = list.Where(p => OneWayWeakness(typeDefinitions, first, p, strong)).ToHashSet();
                // Finish the triangle if possible
                var triangle = FinishTriangle(pool, secondPossiblities, first, metrics, typeDefinitions, settings, strong);
                if (triangle != null)
                    return triangle;
            }
            return null; // No viable triangle with input spcifications TODO: Log
        }
        /// <summary> Helper method for the RandomTypeTriangle method </summary>
        private List<Pokemon> FinishTriangle(HashSet<Pokemon> pool, HashSet<Pokemon> possibleSeconds, Pokemon first, List<Metric<Pokemon>>[] metrics, TypeEffectivenessChart typeDefinitions, PokemonSettings settings, bool strong)
        {
            var list = new List<Pokemon>(possibleSeconds);
            while (possibleSeconds.Count > 0)
            {
                var second = RandomPokemonUnsafe(list, metrics[1], settings);
                // No valid pokemon for first pokemon so no valid triangle
                if (second == Pokemon.None)
                    return null;
                possibleSeconds.Remove(second);
                list.Clear();
                list.AddRange(possibleSeconds);
                // Get third pokemon
                var possibleThirds = list.Where(p => OneWayWeakness(typeDefinitions, second, p, strong) && OneWayWeakness(typeDefinitions, p, first, strong)).ToList();
                // If at least one works, choose one randomly
                if (possibleThirds.Count > 0)
                {
                    var third = RandomPokemonUnsafe(possibleThirds, metrics[2], settings);
                    // No valid pokemon for first pokemon so no valid triangle
                    if (third != Pokemon.None)
                    {
                        return new List<Pokemon> { first, second, third };
                    }
                }
            }
            return null;
        }

        /// <summary> Return true if b is weak to a AND a is not weak to b. 
        /// If strong is true, b must also not be normally effective against a </summary>
        private bool OneWayWeakness(TypeEffectivenessChart typeDefinitions, Pokemon a, Pokemon b, bool strong = true)
        {
            var aTypes = dataT.GetBaseStats(a).types;
            var bTypes = dataT.GetBaseStats(b).types;
            var aVsB = typeDefinitions.GetEffectiveness(aTypes[0], aTypes[1], bTypes[0], bTypes[1]);
            var bVsA = typeDefinitions.GetEffectiveness(bTypes[0], bTypes[1], aTypes[0], aTypes[1]);
            if (strong)
                return aVsB == TypeEffectiveness.SuperEffective && !(bVsA == TypeEffectiveness.SuperEffective || bVsA == TypeEffectiveness.Normal);
            return aVsB == TypeEffectiveness.SuperEffective && !(bVsA == TypeEffectiveness.SuperEffective);
        }

        #endregion

        #region Metric Helper Functions

        public WeightedSet<Pokemon> TypeSimilarityIndividual(IEnumerable<Pokemon> all, Pokemon pokemon)
        {
            var set = PokemonMetrics.TypeSimilarity(all, pokemon, dataT);
            set.Multiply(TypeBalanceFunction);
            return set;
        }

        public WeightedSet<Pokemon> TypeSimilarityGroup(IEnumerable<Pokemon> all, WeightedSet<PokemonType> typeData)
        {
            var set = new WeightedSet<Pokemon>(all);
            var modTypeData = new WeightedSet<PokemonType>(typeData);
            modTypeData.Multiply(TypeBalanceFunction);
            //set.Multiply(TypeBalanceFunction);
            float TypeMultiplier(Pokemon p)
            {
                var data = dataT.GetBaseStats(p);
                float type1Val = modTypeData.Contains(data.PrimaryType) ? modTypeData[data.PrimaryType] : 0;
                if (data.IsSingleTyped) // If single typed, just return the first type value
                {
                    return type1Val;
                }
                // For dual-typed pokemon, return the sum of their type values
                return type1Val + (modTypeData.Contains(data.SecondaryType) ? modTypeData[data.SecondaryType] : 0);
            }
            set.Multiply(TypeMultiplier);
            set.RemoveWhere(p => set[p] <= 0);
            return set;
        }

        public WeightedSet<Pokemon> TypeSimilarityGroup(IEnumerable<Pokemon> all, WeightedSet<PokemonType> typeData, float sharpness)
        {
            var newTypeData = new WeightedSet<PokemonType>(typeData);
            newTypeData.Pow(sharpness);
            return TypeSimilarityGroup(all, newTypeData);
        }

        public WeightedSet<Pokemon> PowerSimilarityIndividual(IEnumerable<Pokemon> all, Pokemon pokemon)
        {
            return PokemonMetrics.PowerSimilarity(all, powerScores, pokemon, 300, 300);
        }

        /// <summary>
        /// Function that balance weightings with the occurence rate of the types in all
        /// Makes water types less likely to dominate just cause there are so many of them, etc
        /// </summary>
        private float TypeBalanceFunction(Pokemon p)
        {
            var pData = dataT.GetBaseStats(p);
            float type1 = TypeBalanceFunction(pData.OriginalPrimaryType);
            if (pData.IsSingleTyped)
            {
                return type1;
            }
            float type2 = TypeBalanceFunction(pData.OriginalSecondaryType);
            return (type1 + type2) / 2;
        }

        private float TypeBalanceFunction(PokemonType t)
        {
            var typeOccurence = romMetrics.TypeOccurenceAll;
            return typeOccurence.Contains(t) ? 1 / typeOccurence[t] : 0;
        }

        public List<Metric<Pokemon>> CreateBasicMetrics(IEnumerable<Pokemon> all, Pokemon pokemon, IReadOnlyList<MetricData> data)
        {
            return CreateBasicMetrics(all, pokemon, data, out var _); // Optimize later
        }

        public List<Metric<Pokemon>> CreateBasicMetrics(IEnumerable<Pokemon> all, Pokemon pokemon, IReadOnlyList<MetricData> data, out List<MetricData> nonBasic)
        {
            var metrics = new List<Metric<Pokemon>>(data.Count);
            nonBasic = new List<MetricData>(data.Count);
            foreach (var d in data)
            {
                WeightedSet<Pokemon> input = d.DataSource switch
                {
                    PokemonMetric.typeIndividual => TypeSimilarityIndividual(all, pokemon),
                    PokemonMetric.powerIndividual => PowerSimilarityIndividual(all, pokemon),
                    _ => null,
                };
                if (input != null)
                {
                    metrics.Add(new Metric<Pokemon>(input, d.Filter, d.Priority));
                }
                else if(d.DataSource != MetricData.emptyMetric)
                {
                    nonBasic.Add(d);
                }
            }
            return metrics;
        }

        #endregion
    }
}
