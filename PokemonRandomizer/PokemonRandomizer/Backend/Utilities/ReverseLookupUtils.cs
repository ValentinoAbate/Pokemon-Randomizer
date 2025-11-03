using System.Collections.Generic;

namespace PokemonRandomizer.Backend.Utilities
{
    public static class ReverseLookupUtils
    {
        public static Dictionary<T,int> BuildReverseLookup<T>(IReadOnlyList<T> items)
        {
            var reverseLookup = new Dictionary<T, int>(items.Count);
            for (int i = 0; i < items.Count; ++i)
            {
                var item = items[i];
                if (reverseLookup.ContainsKey(item))
                    continue;
                reverseLookup.Add(item, i);
            }
            return reverseLookup;
        }
    }
}
