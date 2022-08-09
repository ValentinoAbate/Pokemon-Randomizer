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
            var processed = new WeightedSet<T>(Input);
            processed.Normalize();
            processed.RemoveWhere(t => processed[t] <= filter);
            processed.Multiply((float)Math.Pow(power, priorityScale - priority));
            processed.Multiply(1f / processed.Count); // Normalize for amount of entries
            return processed;
        }

        public WeightedSet<T> Input { get; }
        private readonly float filter;
        private readonly int priority;

        public Metric(WeightedSet<T> input, float filter, int priority)
        {
            this.Input = input;
            this.filter = filter;
            this.priority = priority;
        }
    }
}
