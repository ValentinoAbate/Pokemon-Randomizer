using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer.Backend
{
    /// <summary>
    /// A class with methods for comparing a pokemon to another through various data
    /// Most functions return a normalize WeightedSet expressing the metric value for all pokemon in the input set
    /// </summary>
    public static class PokemonMetrics
    {
        /// <summary> returns a given pokemon's power similarity to all pokemon in the pokemonSet. Output is normalized </summary>
        public static WeightedSet<PokemonSpecies> PowerSimilarity(HashSet<PokemonSpecies> pokemonSet, Dictionary<PokemonSpecies,float> powerScores, PokemonSpecies species)
        {
            var powerScoreSimilarity = pokemonSet.Select((p) => PowerScaleSimilarity(powerScores[species], powerScores[p]));
            var powerWeighting = new WeightedSet<PokemonSpecies>(pokemonSet, powerScoreSimilarity);
            powerWeighting.Normalize();
            return powerWeighting;
        }
        /// <summary> The method used to compare one power score to another. Returns higher the more similar the scores are </summary>
        private static float PowerScaleSimilarity(float powerScale, float other)
        {
            float maxDifference = 700;
            float difference = Math.Abs(powerScale - other);
            float differenceRating = maxDifference - (difference * 5);
            return differenceRating <= 0 ? 1 : differenceRating;
        }

        public static WeightedSet<PokemonSpecies> TypeSimilarity(HashSet<PokemonSpecies> pokemonSet, RomData data, PokemonSpecies species)
        {
            var stats = data.PokemonLookup[species];
            var typeSimilarity = pokemonSet.Select((p) => TypeSimilarity(stats, data.PokemonLookup[p]));
            var typeWeighting = new WeightedSet<PokemonSpecies>(pokemonSet, typeSimilarity);
            typeWeighting.Normalize();
            return typeWeighting;
        }
        /// <summary> method used to compare the types of one pokemon to another 
        /// Returns 1 if single typed and the other is the same single type, or 0.8</summary>
        private static float TypeSimilarity(PokemonBaseStats self, PokemonBaseStats other)
        {
            if (self.IsSingleTyped)
                return other.types.Contains(self.types[0]) ? 1 : 0;
            else
            {
                var matches = self.types.Intersect(other.types).Count();
                if (matches == 2)
                    return 1;
                return matches == 1 ? 0.75f : 0;
            }
        }
    }
}
