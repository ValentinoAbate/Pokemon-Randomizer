using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace PokemonRandomizer.CultureUtils
{
    public static class StringUtils
    {
        public static string RemovePercent(this string s)
        {
            return s.Replace(CultureInfo.CurrentCulture.NumberFormat.PercentSymbol, string.Empty);
        }
    }
}
