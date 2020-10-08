using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;

namespace PokemonRandomizer.Backend
{
    public class LearnSet : IList<LearnSet.Entry>
    {
        /// <summary>
        /// The original number of moves in this moveset when read from the rom.
        /// If -1, this value was never set.
        /// </summary>
        public int OriginalCount { get; private set; } = -1;
        public int OriginalOffset { get; set; }

        public int Count => items.Count;

        int ICollection<Entry>.Count => items.Count;

        public bool IsReadOnly => ((IList<Entry>)items).IsReadOnly;

        public Entry this[int index] { get => items[index]; set => items[index] = value; }

        private readonly List<Entry> items = new List<Entry>();

        public LearnSet()
        {

        }

        public LearnSet(LearnSet toCopy)
        {
            OriginalCount = toCopy.OriginalCount;
            OriginalOffset = toCopy.OriginalOffset;
            items.AddRange(toCopy);
        }

        public bool Learns(Move move)
        {
            foreach (var entry in items)
                if (entry.move == move)
                    return true;
            return false;
        }

        public void Add(Move mv, int learnLvl)
        {
            items.Add(new Entry(mv, learnLvl));
            items.Sort();
        }
        public void Sort()
        {
            items.Sort();
        }
        public void RemoveWhere(Predicate<Entry> pred)
        {
            items.RemoveAll(pred);
        }
        /// <summary>
        /// Sets the original count value to the current count
        /// </summary>
        public void SetOriginalCount()
        {
            OriginalCount = items.Count;
        }
        public override string ToString()
        {
            string ret = string.Empty;
            foreach (var item in items)
                ret += item.ToString() + ", ";
            return ret;
        }

        #region IList<MoveSet.Item> Implementation
        public IEnumerator<Entry> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        public int IndexOf(Entry item)
        {
            return items.IndexOf(item);
        }

        public void Insert(int index, Entry item)
        {
            items.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            items.RemoveAt(index);
        }

        public void Add(Entry item)
        {
            items.Add(item);
            items.Sort();
        }

        public void Clear()
        {
            items.Clear();
        }

        public bool Contains(Entry item)
        {
            return items.Contains(item);
        }

        public void CopyTo(Entry[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        public bool Remove(Entry item)
        {
            return items.Remove(item);
        }
        #endregion

        public class Entry : IComparable<Entry>
        {
            public Move move;
            public int learnLvl;
            public Entry(Move move, int learnLvl)
            {
                this.move = move;
                this.learnLvl = learnLvl;
            }
            public int CompareTo(Entry other)
            {
                return learnLvl.CompareTo(other.learnLvl);
            }

            public override string ToString()
            {
                return learnLvl + ": " + move.ToDisplayString();
            }
        }
    }
}
