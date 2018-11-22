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
        public static ROMData Mutate(ROMData orig, MainWindow window)
        {
            ROMData copy = ROMParser.Parse(orig.ROM);
            Mutator mut = (bool)window.cbSeed.IsChecked ? new Mutator(window.tbSeed.Text) : new Mutator();
            foreach(PokemonBaseStats pkmn in copy.Pokemon)
            {
                if(pkmn.IsSingleTyped)
                    pkmn.types[0] = pkmn.types[1] = mut.RandomChoice(orig.Metrics.typeRatiosPrimary);
                else
                {
                    pkmn.types[0] = mut.RandomChoice(orig.Metrics.typeRatiosPrimary);
                    pkmn.types[1] = mut.RandomChoice(orig.Metrics.typeRatiosSecondary);
                }
            }
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
            copy.CalculateMetrics();
            return copy;
        }
    }
}
