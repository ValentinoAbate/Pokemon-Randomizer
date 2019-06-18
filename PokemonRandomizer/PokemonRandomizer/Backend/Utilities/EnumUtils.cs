using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.Utilities
{
    public static class EnumUtils
    {
        public const char enumDashChar = 'ー';
        public static string ToDisplayString<T>(this T e) where T : Enum
        {
            return e.ToString().Replace('_', ' ').Replace(enumDashChar, '-');
        }
        public static IEnumerable<T> GetValues<T>() where T : Enum
        {
            return (T[])Enum.GetValues(typeof(T));
        }
    }
}
