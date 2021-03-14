using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.Randomization
{
    public class Metric<T>
    {
        public static WeightedSet<GroupT> ProcessGroup<GroupT>(IEnumerable<Metric<GroupT>> metricData)
        {
            var outputSet = new WeightedSet<GroupT>();
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
            processed.Normalize();
            processed.RemoveWhere(t => input[t] < filter);
            processed.Multiply((float)Math.Pow(10, priorityScale - priority));
            return processed;
        }

        private readonly WeightedSet<T> input;
        private readonly float filter;
        private readonly int priority;

        public Metric(WeightedSet<T> input, float filter, int priority)
        {
            this.input = input;
            this.filter = filter;
            this.priority = priority;
        }
    }
}
