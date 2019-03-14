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
        private Random _rand;
        public Mutator()
        {
            _rand = new Random();
        }
        public Mutator(string seed)
        {
            _rand = new Random(seed.GetHashCode());
        }

        public int RandomInt(int min, int max) => _rand.Next(min, max);
        public double RandomDouble() => _rand.NextDouble(); 

        // Returns an unweighted random choice from the given array
        public T RandomChoice<T>(T[] items)
        {
            return items[_rand.Next(0, items.Length)];
        }
        public T RandomChoice<T>(T[] items, int[] weights, bool isAbsolute)
        {
            // totalWeight is the sum of all weights, or 1 if absolute
            int totalWeight = isAbsolute ? 100 : weights.Aggregate((a, b) => a + b);
#if DEBUG
            if (isAbsolute && weights.Aggregate((a, b) => a + b) != 100)
                throw new Exception("Absolute weights do not add up to 100%! Items: " + items.ToString() + " Weights: " + weights.ToString());
#endif
            int randomNumber = _rand.Next(totalWeight);
            for (int i = 0; i < items.Length; ++i)
            {
                if (randomNumber < weights[i])
                    return items[i];

                randomNumber -= weights[i];
            }
            throw new Exception("No item chosen");
        }
        public T RandomChoice<T>(WeightedSet<T> items, bool isAbsolute = false)
        {
            return RandomChoice<T>(items.Items, items.Weights, isAbsolute);
        }
    }
}
