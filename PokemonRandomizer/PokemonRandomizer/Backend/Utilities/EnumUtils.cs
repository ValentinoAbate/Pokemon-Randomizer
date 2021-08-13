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
        public const char enumSpaceChar = '_';
        public static string ToDisplayString<T>(this T e) where T : Enum
        {
            return e.ToString().Replace(enumSpaceChar, ' ').Replace(enumDashChar, '-');
        }
        public static IEnumerable<T> GetValues<T>() where T : Enum
        {
            return (T[])Enum.GetValues(typeof(T));
        }

        public static IEnumerable<string> GetDisplayValues<T>() where T : Enum
        {
            return GetValues<T>().Select(e => e.ToDisplayString());
        }
    }
}
