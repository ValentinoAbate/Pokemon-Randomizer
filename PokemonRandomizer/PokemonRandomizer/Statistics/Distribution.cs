using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.Statistics
{
    public static class Distribution
    {
        public static double Variance(IEnumerable<double> values)
        {
            double mean = values.Average();
            return values.Select((v) => Math.Pow((v - mean), 2)).Average();
        }

        public static double Variance(IEnumerable<int> values)
        {
            double mean = values.Average();
            return values.Select((v) => Math.Pow((v - mean), 2)).Average();
        }

        public static double StandardDeviation(IEnumerable<double> values)
        {
            return Math.Sqrt(Variance(values));
        }

        public static double StandardDeviation(IEnumerable<int> values)
        {
            return Math.Sqrt(Variance(values));
        }
    }
}
