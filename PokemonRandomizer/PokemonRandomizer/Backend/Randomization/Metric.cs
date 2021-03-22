using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.Backend.Randomization
{
    public class Metric<T>
    {
        public const int power = 5;
        public static WeightedSet<T> ProcessGroup(IEnumerable<Metric<T>> metricData)
        {
            var outputSet = new WeightedSet<T>();
            int scale = metricData.Max(metric => metric.priority);
            foreach(var metric in metricData)
            {
                outputSet.Add(metric.Processed(scale));
            }
            return outputSet;
        }

        public WeightedSet<T> Processed(int priorityScale)
        {
            var processed = new WeightedSet<T>(input);
            if(sharpness > 0 && sharpness != 1)
            {
                processed.Map((p) => (float)Math.Pow(processed[p], sharpness));
            }
            processed.Normalize();
            processed.RemoveWhere(t => processed[t] <= filter);
            processed.Multiply((float)Math.Pow(power, priorityScale - priority));
            return processed;
        }

        private readonly WeightedSet<T> input;
        private readonly float filter;
        private readonly int priority;
        private readonly float sharpness;

        public Metric(WeightedSet<T> input, float filter, float sharpness, int priority)
        {
            this.input = input;
            this.filter = filter;
            this.priority = priority;
            this.sharpness = sharpness;
        }
    }
}
