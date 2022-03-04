using PokemonRandomizer.Backend.EnumTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.Utilities
{
    public static class PokemonTypeUtils
    {
        private static readonly Dictionary<string, PokemonType> typeMap;
        static PokemonTypeUtils()
        {
            typeMap = new(18);
            foreach (var type in Enum.GetValues<PokemonType>())
            {
                typeMap.Add(type.ToString().ToLower(), type);
            }
        }
        public static PokemonType FromString(string typeString)
        {
            if (string.IsNullOrEmpty(typeString))
            {
                return PokemonType.Unknown;
            }
            return typeMap.ContainsKey(typeString) ? typeMap[typeString] : PokemonType.Unknown;
        }
    }
}
