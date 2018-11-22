using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer.Backend
{
    public class WeightedSet<T>
    {
        private Dictionary<T, int> _items = new Dictionary<T, int>();
        public T[] Items { get { return _items.Keys.ToArray(); } }
        public int[] Weights { get { return _items.Values.ToArray(); } }
        public void AddWeight(T item, int weight = 1)
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
    }
}
