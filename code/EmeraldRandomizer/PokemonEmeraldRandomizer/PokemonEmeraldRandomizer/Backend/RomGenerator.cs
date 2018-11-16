using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PokemoneEmeraldRandomizer.Backend
{
    //This class takes a modified ROMData object and applies the changes made to
    public static class ROMGenerator
    {
        public static byte[] GenerateROM(ROMData data)
        {
            //changeStarters(); //must come before trainers
            //changeTMs();      //must come before trainers
            //randomizePokeData(); //must come before trainers
            //changeWildPokemon();
            //changePkmnColors();
            //fixDisobedience();
            //changeIntroPokemon();
            //changeTradeEvolutions();
            //randomizePickup();
            //changeTrainerPokemon();
            //changeTrainerClasses();
            //heartScales();
            //metronomeHyperdrive();
            //changeItems();
            //changeDex();
            return data.RawROM;
        }
    }
}
