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
        public List<ItemChance> ItemChances { get; } = new List<ItemChance>(16);

        public struct ItemChance
        {
            public Item item;
            public int chance;
            public override string ToString()
            {
                return $"{item}: {chance}";
            }
        }
    }
}
