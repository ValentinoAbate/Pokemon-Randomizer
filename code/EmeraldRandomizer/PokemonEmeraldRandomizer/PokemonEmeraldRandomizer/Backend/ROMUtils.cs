using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer.Backend
{
    public static class ROMUtils
    {
        // Reads a word from the rom (a 16-bit number)
        public static int ReadWord(this byte[] rom, int offset)
        {
            return rom[offset] + (rom[offset + 1] << 8);
        }
        // Reads a pointer from the rom (a 24-bit number)
        public static int ReadPointer(this byte[] rom, int offset)
        {
            return rom[offset] + (rom[offset + 1] << 8) + (rom[offset + 2] << 16);
        }
    }
}
