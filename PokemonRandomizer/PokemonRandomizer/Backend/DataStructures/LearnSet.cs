using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;

namespace PokemonRandomizer.Backend
{
    public class LearnSet : IEnumerable<LearnSet.Entry>
    {
        /// <summary>
        /// The original number of moves in this moveset when read from the rom.
        /// If -1, this value was never set.
        /// </summary>
        public int OriginalCount { get; private set; } = -1;
        public int OriginalOffset { get; set; }

        public int Count => items.Count;

        public Entry this[int index] { get => items[index]; set => items[index] = value; }

        private readonly List<Entry> items = new List<Entry>();

        public HashSet<Move> GetMovesLookup()
        {
            var lookup = new HashSet<Move>(items.Count);
            foreach(var entry in items)
            {
                if (lookup.Contains(entry.move))
                {
                    continue;
                }
                lookup.Add(entry.move);
            }
            return lookup;
        }

        public void Add(Move mv, int learnLvl)
        {
            Add(new Entry(mv, learnLvl));
        }

        public void Add(Entry item)
        {
            items.Add(item);
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
            return string.Join(", ", items);
        }

        #region IEnumerable<Entry> Implementation
        public IEnumerator<Entry> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
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
