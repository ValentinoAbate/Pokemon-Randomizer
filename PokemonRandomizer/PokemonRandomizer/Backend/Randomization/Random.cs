using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.Backend.Randomization
{
    public class Random
    {
        private readonly System.Random rand;
        public Random()
        {
            rand = new System.Random();
        }
        public Random(string seed)
        {
            rand = new System.Random(seed.GetHashCode());
        }

        #region General Random Functions
        /// <summary> Generate an int in between min (inclusive) and max (inclusive) </summary>
        public int RandomInt(int min, int max) => rand.Next(min, max);
        /// <summary> Generate a double in between 0.0 (inclusive) and 1 (exclusive) </summary>
        public double RandomDouble() => rand.NextDouble(); 
        /// <summary> Generate a random float (untested) </summary>
        public float RandomFloat()
        {
            double mantissa = (rand.NextDouble() * 2.0) - 1.0;
            double exponent = Math.Pow(2.0, rand.Next(-127, 128));
            return (float)(mantissa * exponent);
        }
        /// <summary> Generate a byte in between 0 (inclusive) and 255 (inclusive) </summary>
        public byte RandomByte() => (byte)RandomInt(0, 255);
        /// <summary>
        /// Random number with a gaussian distribution. Implemented using the Box-Muller transform.
        /// From this stockoverflow post: https://stackoverflow.com/questions/218060/random-gaussian-variables
        /// May Update to use Ziggurat Method
        /// </summary>
        public double RandomGaussian(double mean, double stdDev)
        {
            double u1 = 1.0 - rand.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0 - rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            return mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
        }
        /// <summary>
        /// Random number with a gaussian distribution (only returns mean or higher).
        /// </summary>
        public double RandomGaussianUp(double mean, double stdDev)
        {
            double num = RandomGaussian(mean, stdDev);
            return num >= mean ? num : RandomGaussianUp(mean, stdDev);
        }
        /// <summary>
        /// Random integer number with a gaussian distribution (only returns mean or higher).
        /// </summary>
        public int RandomGaussianUpInt(double mean, double stdDev)
        {
            int num = (int)Math.Round(RandomGaussian(mean, stdDev));
            return num >= mean ? num : RandomGaussianUpInt(mean, stdDev);
        }
        /// <summary>
        /// Random number with a gaussian distribution (only returns mean or lower).
        /// </summary>
        public double RandomGaussianDown(double mean, double stdDev)
        {
            double num = RandomGaussian(mean, stdDev);
            return num <= mean ? num : RandomGaussianDown(mean, stdDev);
        }
        /// <summary>
        /// Random integer number with a gaussian distribution (only returns mean or higher).
        /// </summary>
        public int RandomGaussianDownInt(double mean, double stdDev)
        {
            int num = (int)Math.Round(RandomGaussian(mean, stdDev));
            return num <= mean ? num : RandomGaussianDownInt(mean, stdDev);
        }
        #endregion

        #region Random Choice from a collection (With options for weighting)
        /// <summary> Returns an unweighted random choice from the given array </summary> 
        public T Choice<T>(T[] items)
        {
            return items[rand.Next(0, items.Length)];
        }
        /// <summary> Returns an unweighted random choice from the given IEnumerable </summary> 
        public T Choice<T>(IEnumerable<T> items)
        {
            if (items.Count() <= 0)
                return default;
            return Choice(items.ToArray());
        }
        /// <summary> Returns a weighted random choice from the given items and float weights </summary> 
        public T Choice<T>(T[] items, float[] weights)
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
        /// <summary> Returns a weighted random choice from the given items and float weights </summary> 
        public T Choice<T>(IEnumerable<T> items, IEnumerable<float> weights)
        {
            float totalWeight = weights.Aggregate((a, b) => a + b);
            float randomNumber = (float)rand.NextDouble() * totalWeight;
            var e = weights.GetEnumerator();
            foreach(var item in items)
            {
                e.MoveNext();
                if (randomNumber < e.Current)
                    return item;
                randomNumber -= e.Current;
            }
            throw new Exception("No item chosen");
        }
        /// <summary> Returns a weighted random choice from the given items and int weights </summary> 
        public T Choice<T>(T[] items, int[] weights, bool isAbsolute)
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
        /// <summary> Returns a weighted random choice from the given WeightedSet </summary> 
        public T Choice<T>(WeightedSet<T> items)
        {
            return Choice(items.Items, items.Weights);
        }
        #endregion
    }
}
