using System;
using System.Collections;
using System.Collections.Generic;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;

namespace PokemonRandomizer.Backend
{
    public class LearnSet : IEnumerable<LearnSet.Entry>
    {
        public int MaxMoves { get; }

        public int Count => items.Count;

        public Entry this[int index] { get => items[index]; set => items[index] = value; }

        private readonly List<Entry> items;

        public LearnSet(int maxMoves)
        {
            MaxMoves = maxMoves;
            items = new List<Entry>(MaxMoves);
        }

        public LearnSet(LearnSet toCopy)
        {
            MaxMoves = toCopy.MaxMoves;
            items = new List<Entry>(toCopy.items.Count);
            foreach(var item in toCopy.items)
            {
                items.Add(new Entry(item));
            }
        }

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

        public Dictionary<Move, int> GetMinimumLearnLevelLookup()
        {
            var lookup = new Dictionary<Move, int>(items.Count);
            foreach (var entry in items)
            {
                // Learnset is sorted so first instance is guaranteed to be first
                if (!lookup.ContainsKey(entry.move))
                {
                    lookup.Add(entry.move, entry.learnLvl);
                }
            }
            return lookup;
        }

        public void Add(Move mv, int learnLvl, bool unsafeAdd = false)
        {
            Add(new Entry(mv, learnLvl), unsafeAdd);
        }

        public void Add(Entry item, bool unsafeAdd = false)
        {
            if (items.Count >= MaxMoves)
            {
                return;
            }
            if (items.Count <= 0 || unsafeAdd)
            {
                items.Add(item);
                return;
            }
            for (int i = 0; i < items.Count; i++)
            {
                if(items[i].learnLvl > item.learnLvl)
                {
                    items.Insert(i, item);
                    return;
                }
            }
            items.Add(item);
        }

        public void RemoveAt(int i)
        {
            items.RemoveAt(i);
        }

        public void RemoveWhere(Predicate<Entry> pred)
        {
            items.RemoveAll(pred);
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

            public Entry(Entry toCopy) : this(toCopy.move, toCopy.learnLvl) { }
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
