using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.Backend.Randomization
{

    public class WeightedSet<T> : IEnumerable<KeyValuePair<T, float>>
    {
        public delegate float Metric(T item);

        public IEnumerable<T> Items { get => items.Keys; }
        public IEnumerable<float> Weights { get => items.Values; }
        public float Count { get => items.Count; }
        public Dictionary<T, float> Percentages
        {
            get
            {
                var ret = new Dictionary<T, float>();
                float totalWieght = Weights.Aggregate((a, b) => a + b);
                foreach (var kvp in items)
                    ret.Add(kvp.Key, (kvp.Value / totalWieght) * 100);
                return ret;
            }
        }

        private Dictionary<T, float> items = new Dictionary<T, float>();

        #region Constructors
        public WeightedSet() { }
        public WeightedSet(WeightedSet<T> toCopy) : this(toCopy.items) { }
        public WeightedSet(IEnumerable<T> items, float weight = 0)
        {
            foreach (var item in items)
                Add(item, weight);
        }
        public WeightedSet(IEnumerable<T> items, IEnumerable<float> weights)
        {
            if (items.Count() != weights.Count())
                throw new Exception("Weighted Set Construction Exception: Items and weights are not the same length");
            IEnumerator<float> e = weights.GetEnumerator();
            e.MoveNext();
            foreach (T item in items)
            {
                Add(item, e.Current);
                e.MoveNext();
            }
        }
        public WeightedSet(IEnumerable<KeyValuePair<T, float>> pairs)
        {
            foreach (var kvp in pairs)
                Add(kvp.Key, kvp.Value);
        }
        /// <summary>
        /// Creates a weighted set where the weight of each value given is the value of the metric function evaluated with the item
        /// </summary>
        public WeightedSet(IEnumerable<T> items, Metric weight)
        {
            foreach (var item in items)
                Add(item, weight(item));
        }
        #endregion

        public void Add(WeightedSet<T> set, float multiplier = 1)
        {
            foreach (var item in set.Items)
                Add(item, set[item] * multiplier);
        }
        public void Add(T item, float weight = 1)
        {
            if (items.ContainsKey(item))
            {
                if (items[item] + weight < 0)
                {
                    Remove(item);
                }
                else
                {
                    items[item] += weight;
                }

            }
            else if (weight < 0)
            {
                return;
            }
            else
            {
                items.Add(item, weight);
            }
        }
        public void Remove(T item)
        {
            items.Remove(item);
        }
        public void RemoveWhere(Predicate<T> predicate)
        {
            foreach (var key in items.Keys.ToArray())
                if (predicate(key))
                    items.Remove(key);
        }

        public bool Contains(T item)
        {
            return items.ContainsKey(item);
        }

        public void Normalize()
        {
            if (Count <= 0)
                return;
            float max = items.Max((kvp) => kvp.Value);
            foreach (var key in items.Keys.ToArray())
                items[key] = items[key] / max;
        }
        public void ApplyMetric(Metric m)
        {
            var keys = new List<T>(Items);
            foreach (var key in keys)
                Add(key, m(key));
        }

        public override string ToString()
        {
            List<string> ret = new List<string>();
            float totalWieght = Weights.Aggregate((a, b) => a + b);
            foreach (var kvp in items)
            {
                ret.Add(kvp.Key.ToString() + ": " + ((kvp.Value / totalWieght) * 100).ToString("#.0") + "%");
            }
            return ret.Aggregate((a, b) => a + b + "\n");
        }
        public float this[T item]
        {
            get => items[item];
        }

        #region IEnumerable Implementation
        public IEnumerator<KeyValuePair<T, float>> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }
        #endregion
    }
}