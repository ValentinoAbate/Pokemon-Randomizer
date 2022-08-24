using System;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.Utilities
{
    public static class HashSetUtils
    {
        public static void RemoveIfContains<T>(this HashSet<T> set, T value)
        {
            if (set.Contains(value))
            {
                set.Remove(value);
            }
        }
    }
}
