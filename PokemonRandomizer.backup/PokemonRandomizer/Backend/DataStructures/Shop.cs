using System;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.DataStructures
{
    using EnumTypes;
    public class Shop
    {
        public int OriginalSize { get; private set; }
        public readonly List<Item> items = new List<Item>();

        public void SetOriginalSize()
        {
            OriginalSize = items.Count;
        }
    }
}
