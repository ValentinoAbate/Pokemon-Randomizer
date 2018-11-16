using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PokemonEmeraldRandomizer.Backend
{
    //This class takes a modified ROMData object and converts it to a byte[]
    //to write to a file
    public static class ROMCreator
    {
        public static byte[] GenerateROM(ROMData data)
        {
            return data.RawROM;
        }
    }
}
