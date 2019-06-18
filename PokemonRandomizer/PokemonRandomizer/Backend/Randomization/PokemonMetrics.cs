using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;

namespace PokemonRandomizer.Backend.Randomization
{
    /// <summary>
    /// A class with methods for comparing a pokemon to another through various data
    /// Most functions return a normalize WeightedSet expressing the metric value for all pokemon in the input set
    /// </summary>
    public static class PokemonMetrics
    {
        /// <summary> returns a given pokemon's power similarity to all pokemon in the pokemonSet. Output is normalized </summary>
        public static WeightedSet<PokemonSpecies> PowerSimilarity(IEnumerable<PokemonSpecies> pokemonSet, Dictionary<PokemonSpecies,float> powerScores, PokemonSpecies species, int maxStronger, int maxWeaker)
        {
            float myPowerScore = powerScores[species];
            var powerWeighting = new WeightedSet<PokemonSpecies>();          
            foreach (var pokemon in pokemonSet)
            {
                float similarity = PowerScoreSimilarity(myPowerScore, powerScores[pokemon], maxStronger, maxWeaker);
                if (similarity > 0)
                    powerWeighting.Add(pokemon, similarity);
            }
            powerWeighting.Normalize();
            return powerWeighting;
        }
        /// <summary> The method used to compare one power score to another. Returns higher the more similar the scores are </summary>
        private static float PowerScoreSimilarity(float powerScale, float other, float maxStronger, float maxWeaker)
        {
            float difference = powerScale - other;
            if (difference == 0) // Same power
                return Math.Max(maxStronger, maxWeaker);
            else if(difference < 0) // Other is stronger
            {
                float differenceRating = maxStronger - (difference);
                return differenceRating <= 0 ? 0 : differenceRating;
            }
            else // Other is weaker
            {
                float differenceRating = maxWeaker - (difference);
                return differenceRating <= 0 ? 0 : differenceRating;
            }
        }

        public static WeightedSet<PokemonSpecies> TypeSimilarity(IEnumerable<PokemonSpecies> pokemonSet, RomData data, PokemonSpecies species)
        {
            var myStats = data.PokemonLookup[species];
            var typeWeighting = new WeightedSet<PokemonSpecies>();
            foreach(var pokemon in pokemonSet)
            {
                float similarity = TypeSimilarity(myStats, data.PokemonLookup[pokemon]);
                if (similarity > 0)
                    typeWeighting.Add(pokemon, similarity);
            } 
            return typeWeighting; // Type weighting is already normalized
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
                return matches == 1 ? 0.8f : 0;
            }
        }
    }
}
