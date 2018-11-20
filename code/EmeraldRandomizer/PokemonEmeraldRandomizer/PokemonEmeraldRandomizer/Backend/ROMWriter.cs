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
    public static class ROMWriter
    {
        public static byte[] GenerateROM(ROMData data)
        {
            //Unlock National pokedex
            //if (data.NationalDexUnlocked)
            //{
            //    writeText(addy("e40004"), "[3172016732AC1F083229610825F00129E40825F30116CD40010003]");
            //    writeText(addy("1fa301"), "[0400e4]");
            //}
            return data.ROM;
        }
    }
}
