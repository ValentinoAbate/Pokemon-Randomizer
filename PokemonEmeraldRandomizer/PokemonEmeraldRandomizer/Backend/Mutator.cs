using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer.Backend
{
    public class Mutator
    {
        private Random rand;
        public Mutator()
        {
            rand = new Random();
        }
        public Mutator(string seed)
        {
            rand = new Random(seed.GetHashCode());
        }

        #region General Random Functions
        public int RandomInt(int min, int max) => rand.Next(min, max);
        public double RandomDouble() => rand.NextDouble(); 
        public float RandomFloat(float min, float max)
        {
            double mantissa = (rand.NextDouble() * 2.0) - 1.0;
            double exponent = Math.Pow(2.0, rand.Next(-127, 128));
            return (float)(mantissa * exponent);
        }
        #endregion

        #region Random Choice from a collection (With options for weighting)
        /// <summary> Returns an unweighted random choice from the given array </summary> 
        public T RandomChoice<T>(T[] items)
        {
            return items[rand.Next(0, items.Length)];
        }
        /// <summary> Returns an unweighted random choice from the given IEnumerable </summary> 
        public T RandomChoice<T>(IEnumerable<T> items)
        {
            if (items.Count() <= 0)
                return default;
            return RandomChoice(items.ToArray());
        }
        /// <summary> Returns a weighted random choice from the given items and float weights </summary> 
        public T RandomChoice<T>(T[] items, float[] weights)
        {
            float totalWeight = weights.Aggregate((a, b) => a + b);
            float randomNumber = (float)rand.NextDouble() * totalWeight;
            for (int i = 0; i < items.Length; ++i)
            {
                if (randomNumber < weights[i])
                    return items[i];
                randomNumber -= weights[i];
            }
            throw new Exception("No item chosen");
        }
        /// <summary> Returns a weighted random choice from the given items and int weights </summary> 
        public T RandomChoice<T>(T[] items, int[] weights, bool isAbsolute)
        {
            // totalWeight is the sum of all weights, or 1 if absolute
            int totalWeight = isAbsolute ? 100 : weights.Aggregate((a, b) => a + b);
#if DEBUG
            if (isAbsolute && weights.Aggregate((a, b) => a + b) != 100)
                throw new Exception("Absolute weights do not add up to 100%! Items: " + items.ToString() + " Weights: " + weights.ToString());
#endif
            int randomNumber = rand.Next(totalWeight);
            for (int i = 0; i < items.Length; ++i)
            {
                if (randomNumber < weights[i])
                    return items[i];
                randomNumber -= weights[i];
            }
            throw new Exception("No item chosen");
        }
        /// <summary> Returns a wrighted random choice from the given </summary> 
        public T RandomChoice<T>(WeightedSet<T> items)
        {
            return RandomChoice(items.Items, items.Weights);
        }
        #endregion
    }
}
