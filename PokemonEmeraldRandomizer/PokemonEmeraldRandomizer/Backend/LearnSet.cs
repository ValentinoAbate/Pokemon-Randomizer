using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer.Backend
{
    public class LearnSet : IEnumerable<LearnSet.Item>
    {
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
