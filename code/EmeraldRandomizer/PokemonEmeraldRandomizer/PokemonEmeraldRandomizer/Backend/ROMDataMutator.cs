using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer.Backend
{
    //This class does the actualmutation and randomizing by creating a mutated copy 
    //of the original ROM data
    public static class ROMDataMutator
    {
        //Apply mutations based on program settings.
        public static ROMData mutate(ROMData orig, MainWindow window)
        {
            ROMData mut = new ROMData(orig.RawROM);
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
            return mut;
        }
    }
}
