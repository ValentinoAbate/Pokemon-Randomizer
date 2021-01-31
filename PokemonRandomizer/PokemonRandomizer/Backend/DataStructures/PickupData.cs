using System.Collections.Generic;
using PokemonRandomizer.Backend.EnumTypes;

namespace PokemonRandomizer.Backend.DataStructures
{
    public class PickupData
    {
        public enum Type
        {
            ItemsWithChances,
            ItemsAndRareItems,
        }
        public Type DataType => Items.Count > 0 ? Type.ItemsAndRareItems : Type.ItemsWithChances;
        public List<Item> Items { get; } = new List<Item>(18);
        public List<Item> RareItems { get; } = new List<Item>(11);
        public SortedList<byte, Item> ItemChances { get; } = new SortedList<byte, Item>(16);
    }
}
