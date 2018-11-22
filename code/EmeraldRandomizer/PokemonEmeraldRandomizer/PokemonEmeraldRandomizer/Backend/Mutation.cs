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
        private Random Rand;
        public Mutator()
        {
            Rand = new Random();
        }
        public Mutator(string seed)
        {
            Rand = new Random(seed.GetHashCode());
        }

        public T RandomChoice<T>(T[] items, int[] weights, bool isAbsolute)
        {
            // totalWeight is the sum of all weights, or 1 if absolute
            int totalWeight = isAbsolute ? 1 : weights.Aggregate((a, b) => a + b);
#if DEBUG
            if (isAbsolute && weights.Aggregate((a, b) => a + b) != 1)
                throw new Exception("Absolute weights do not add up to 100%! Items: " + items.ToString() + " Weights: " + weights.ToString());
#endif
            int randomNumber = Rand.Next(totalWeight);
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

        public delegate T MappingFunc<T>(T input);
        public delegate T[] MappingFuncWeighted<T>(T input);
        public static T Map<T>(T item, Dictionary<T, T> map)
        {
#if DEBUG
            if (!map.ContainsKey(item))
                throw new System.Exception("Key not found in map: " + item.ToString());
#endif
            return map[item];
        }
    }
}
