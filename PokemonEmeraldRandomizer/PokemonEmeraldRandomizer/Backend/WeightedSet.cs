using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer.Backend
{
    public class WeightedSet<T> : IEnumerable<KeyValuePair<T, int>>
    {
        private Dictionary<T, int> _items = new Dictionary<T, int>();
        public T[] Items { get => _items.Keys.ToArray(); }
        public int[] Weights { get => _items.Values.ToArray(); }
        public int Count { get => _items.Count; }

        public WeightedSet() {}
        public WeightedSet(IEnumerable<T> items, IEnumerable<int> weights)
        {
            IEnumerator<int> e = weights.GetEnumerator();
            foreach(T item in items)
            {
                Add(item, e.Current);
                e.MoveNext();
            }
        }
        public WeightedSet(IEnumerable<KeyValuePair<T, int>> pairs)
        {
            foreach (var kvp in pairs)
                Add(kvp.Key, kvp.Value);
        }
        public void Add(T item, int weight = 1)
        {
            if (_items.ContainsKey(item))
                _items[item] += weight;
            else
                _items.Add(item, weight);
        }
        public override string ToString()
        {
            return _items.ToString();
        }

        #region IEnumerable Implementation
        public IEnumerator<KeyValuePair<T, int>> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
        #endregion
    }
}
