using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer.Backend
{
    public static class AddressUtils
    {
        #region Special Addresses
        //The address where pokemon definitions start
        public const int pokemonDefinitionStartAddy = 0x3203e8;
        #endregion

        //Converts and address' hex string to an integer
        public static Int32 hexToInt(string addy)
        {
            return Convert.ToInt32(addy, 16);
        }
    }
}
