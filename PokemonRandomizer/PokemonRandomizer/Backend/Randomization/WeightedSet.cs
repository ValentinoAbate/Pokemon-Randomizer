using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.Backend.Randomization
{

    public class WeightedSet<T> : IReadOnlyCollection<KeyValuePair<T, float>>
    {
        public IEnumerable<T> Items { get => items.Keys; }
        public IEnumerable<float> Weights { get => items.Values; }
        public int Count { get => items.Count; }
        public Dictionary<T, float> Percentages
        {
            get
            {
                var ret = new Dictionary<T, float>(items.Count);
                float totalWeight = Weights.Sum();
                foreach (var kvp in items)
                    ret.Add(kvp.Key, (kvp.Value / totalWeight) * 100);
                return ret;
            }
        }

        private readonly Dictionary<T, float> items;

        #region Constructors
        public WeightedSet() { items = new Dictionary<T, float>(); }
        public WeightedSet(int capacity) { items = new Dictionary<T, float>(capacity); }
        public WeightedSet(IReadOnlyDictionary<T,float> items)
        {
            this.items = new Dictionary<T,float>(items);
        }
        public WeightedSet(WeightedSet<T> toCopy) : this(toCopy.items) { }
        public WeightedSet(IEnumerable<T> items, float weight = 1)
        {
            this.items = new Dictionary<T, float>(items.Count());
            AddRange(items, weight);
        }
        public WeightedSet(ICollection<T> items, float weight = 1)
        {
            this.items = new Dictionary<T, float>(items.Count);
            AddRange(items, weight);
        }
        /// <summary>
        /// Creates a weighted set where the weight of each value given is the value of the metric function evaluated with the item
        /// </summary>
        public WeightedSet(IEnumerable<T> items, Func<T, float> weight)
        {
            this.items = new Dictionary<T, float>(items.Count());
            AddRange(items, weight);
        }
        public WeightedSet(ICollection<T> items, Func<T, float> weight)
        {
            this.items = new Dictionary<T, float>(items.Count);
            AddRange(items, weight);
        }
        #endregion

        public void Add(WeightedSet<T> set)
        {
            foreach (var item in set.Items)
                Add(item, set[item]);
        }
        public void Add(WeightedSet<T> set, float multiplier)
        {
            foreach (var item in set.Items)
                Add(item, set[item] * multiplier);
        }
        public void Add(T item, float weight = 1)
        {
            if (!float.IsFinite(weight))
            {
                return;
            }
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
            else if (weight <= 0)
            {
                return;
            }
            else
            {
                items.Add(item, weight);
            }
        }
        public void Add(Func<T, float> m)
        {
            var keys = new List<T>(Items);
            foreach (var key in keys)
                Add(key, m(key));
        }
        public void AddRange(IEnumerable<T> items, float weight = 1)
        {
            foreach (var item in items)
            {
                Add(item, weight);
            }
        }
        public void AddRange(IEnumerable<T> items, Func<T, float> weight)
        {
            foreach(var item in items)
            {
                Add(item, weight(item));
            }
        }
        public void Multiply(float multiplier)
        {
            foreach (var item in items.Keys.ToArray())
                Multiply(item, multiplier);
        }
        public void Multiply(T item, float multiplier)
        {
            if (items.ContainsKey(item))
            {
                if (items[item] * multiplier <= 0)
                {
                    Remove(item);
                }
                else
                {
                    items[item] *= multiplier;
                }
            }
        }
        public void Multiply(Func<T, float> m)
        {
            var keys = new List<T>(Items);
            foreach (var key in keys)
                Multiply(key, m(key));
        }

        public void Pow(float power)
        {
            if(power > 0 && power != 1)
            {
                Map(i => (float)Math.Pow(items[i], power));
            }
        }
        public void Map(Func<T, float> m)
        {
            var keys = new List<T>(Items);
            foreach (var key in keys)
                items[key] = m(key);
        }

        public void Remove(T item)
        {
            items.Remove(item);
        }
        public void RemoveIfContains(T item)
        {
            if (items.ContainsKey(item))
                items.Remove(item);
        }
        public void RemoveWhere(Predicate<T> predicate)
        {
            foreach (var key in items.Keys.ToArray())
                if (predicate(key))
                    items.Remove(key);
        }
        public void Clear()
        {
            items.Clear();
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
        public WeightedSet<T2> Distribution<T2>(Func<T, T2> selector)
        {
            var ret = new WeightedSet<T2>();
            foreach(var kvp in items)
            {
                ret.Add(selector(kvp.Key), kvp.Value);
            }
            return ret;
        }

        public override string ToString()
        {
            var ret = new List<string>(Count);
            float totalWeight = Weights.Sum();
            foreach (var kvp in items)
            {
                ret.Add(kvp.Key.ToString() + ": " + ((kvp.Value / totalWeight) * 100).ToString("#.0") + "%");
            }
            return string.Join("\n", ret);
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