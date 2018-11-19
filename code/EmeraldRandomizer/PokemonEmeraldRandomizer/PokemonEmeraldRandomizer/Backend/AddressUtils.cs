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
        public const int pokemonBaseStatsAddy = 0x3203e8;
        //The address where pokemon TM compatiblilties start
        public const int TMCompatAddy = 0x31e8a0;
        //The address where pokemon move tutor compats starts
        public const int tutorCompatAddy = 0x61504c;
        //The address where pokemon movesets start
        public const int movesetAddy = 0x3230dc;
        #endregion

        //Converts and address' hex string to an integer
        public static Int32 hexToInt(string addy)
        {
            return Convert.ToInt32(addy, 16);
        }
    }
}
