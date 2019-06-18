using System;
using System.Collections;
using System.Collections.Generic;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;

namespace PokemonRandomizer.Backend
{
    public class LearnSet : IEnumerable<LearnSet.Item>
    {
        /// <summary>
        /// The original number of moves in this moveset when read from the rom.
        /// If -1, this value was never set.
        /// </summary>
        public int OriginalCount { get; private set; } = -1;
        private readonly List<Item> items = new List<Item>();
        public void Add(Move mv, int learnLvl)
        {
            items.Add(new Item(mv, learnLvl));
            items.Sort();
        }
        public void Sort()
        {
            items.Sort();
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

        #region IEnumerable<MoveSet.Item> Implementation
        public IEnumerator<Item> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }
        #endregion

        public class Item : IComparable<Item>
        {
            public Move move;
            public int learnLvl;
            public Item(Move move, int learnLvl)
            {
                this.move = move;
                this.learnLvl = learnLvl;
            }
            public int CompareTo(Item other)
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
