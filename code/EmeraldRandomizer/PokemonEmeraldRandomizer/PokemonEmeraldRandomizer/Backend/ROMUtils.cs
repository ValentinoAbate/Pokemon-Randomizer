using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer.Backend
{
    public static class ROMUtils
    {
        public static int GetWord(this byte[] rom, int index)
        {
            return (rom[index + 1] % 2) * 256 + rom[index];
        }
    }
}
