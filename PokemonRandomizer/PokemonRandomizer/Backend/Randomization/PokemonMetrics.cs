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
        public static WeightedSet<Pokemon> PowerSimilarity(IEnumerable<Pokemon> pokemonSet, Dictionary<Pokemon,float> powerScores, Pokemon species, int maxStronger, int maxWeaker)
        {
            float myPowerScore = powerScores[species];
            var powerWeighting = new WeightedSet<Pokemon>();          
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
            float maxDeviation = Math.Max(maxStronger, maxWeaker);
            if (difference == 0) // Same power
                return maxDeviation;
            else if (difference < 0) // Other is stronger
            {
                float differenceRating = maxStronger + difference; // Difference must be negative
                return differenceRating <= 0 ? 0 : maxDeviation + difference;
            }
            else // Other is weaker
            {
                float differenceRating = maxWeaker - difference; // Difference must be positive
                return differenceRating <= 0 ? 0 : maxDeviation - difference;
            }
        }

        public static WeightedSet<Pokemon> TypeSimilarity(IEnumerable<Pokemon> pokemonSet, Pokemon species, IDataTranslator dataT)
        {
            var myStats = dataT.GetBaseStats(species);
            var typeWeighting = new WeightedSet<Pokemon>();
            foreach(var pokemon in pokemonSet)
            {
                float similarity = TypeSimilarity(myStats, dataT.GetBaseStats(pokemon));
                if (similarity > 0)
                    typeWeighting.Add(pokemon, similarity);
            } 
            return typeWeighting; // Type weighting is already normalized
        }
        /// <summary> method used to compare the types of one pokemon to another 
        /// Returns 1 if single typed and the other is the same single type, or 0.8</summary>
        private static float TypeSimilarity(PokemonBaseStats self, PokemonBaseStats other)
        {
            if (self.OriginallySingleTyped)
            {
                return other.IsType(self.OriginalPrimaryType) ? 1 : 0;
            }
            var matches = self.OriginalTypes.Intersect(other.types).Count();
            if (matches == 2)
                return 1;
            return matches == 1 ? 0.95f : 0;
        }

        public static WeightedSet<PokemonType> TypeOccurence(IEnumerable<PokemonBaseStats> pokemon)
        {
            var set = new WeightedSet<PokemonType>(16);
            foreach(var p in pokemon)
            {
                set.Add(p.OriginalPrimaryType);
                if (!p.OriginallySingleTyped)
                {
                    set.Add(p.OriginalSecondaryType);
                }
            }
            return set;
        }
    }
}
