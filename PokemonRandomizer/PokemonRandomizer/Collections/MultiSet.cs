using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Collections
{
    public class MultiSet<T> : IEnumerable<T>
    {
        private Dictionary<T, int> _dictionary = new Dictionary<T, int>(); // Internal dictionary interface

        #region Dictionary Implementation
        // The number of distict items (not including duplicate) in the multiset 
        public int Count
        {
            get
            {
                return _dictionary.Count;
            }
        }
        // The number of items in the multiset including duplicates: O(n)
        public int TotalCount
        {
            get
            {
                int count = 0;
                foreach (int freq in _dictionary.Values)
                    count += freq;
                return count;
            }
        }
        // Clear the whole multiset
        public void Clear()
        {
            _dictionary.Clear();
        }
        // Removes the item from the multiset regardless of its frequency
        public void ClearItem(T item)
        {
            _dictionary.Remove(item);
        }
        // Return if there is at least one of the item in the set
        public bool Contains(T item)
        {
            return _dictionary.ContainsKey(item);
        }
        // Return the frequency of the item in the multiset (read-only)
        public int Frequency(T item)
        {
            return _dictionary[item];
        }
        // Adds the item to the multiset if not already an element, else increments its frequency by amount
        public void Add(T item, int amount = 1)
        {
            if (_dictionary.ContainsKey(item))
                _dictionary[item] += amount;
            else
                _dictionary.Add(item, amount);
        }
        // Removes the item from the multiset if its frequency is less than amount, else lowers the frequency by amount
        public void Remove(T item, int amount = 1)
        {
            if (_dictionary[item] <= amount)
                _dictionary.Remove(item);
            else
                _dictionary[item] -= amount;
        }
        #endregion

        #region IEnumarable Implementation
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _dictionary.Keys.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _dictionary.Keys.GetEnumerator();
        }
        #endregion
    }
}
