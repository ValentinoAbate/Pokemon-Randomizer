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
                if (pkmn.species == PokemonSpecies.TREECKO)
                {
                    pkmn.types[0] = pkmn.types[1] = PokemonType.Unknown;
                    continue;
                }
                if (pkmn.IsSingleTyped)
                {
                    if (mut.RandomDouble() < 0.05)
                        pkmn.types[0] = pkmn.types[1] = mut.RandomChoice(orig.Metrics.TypeRatiosSingle);
                }
                else
                {
                    if (mut.RandomDouble() < 0.05)
                        pkmn.types[0] = mut.RandomChoice(orig.Metrics.TypeRatiosDualPrimary);
                    if (mut.RandomDouble() < 0.05)
                        pkmn.types[1] = mut.RandomChoice(orig.Metrics.TypeRatiosDualSecondary);
                }
            }
            foreach(PokemonType t in EnumUtils.GetValues<PokemonType>())
            {
                TypeEffectiveness te = mut.RandomChoice(orig.Metrics.TypeEffectivenessRatios);
                if (te != TypeEffectiveness.Normal)
                    copy.TypeDefinitions.Add(t, PokemonType.Unknown, te, (t == PokemonType.NRM || t == PokemonType.FTG) && te == TypeEffectiveness.NoEffect);
                te = mut.RandomChoice(orig.Metrics.TypeEffectivenessRatios);
                if (te != TypeEffectiveness.Normal)
                    copy.TypeDefinitions.Add(PokemonType.Unknown, t, te, t == PokemonType.GHO);
            }
            copy.TypeDefinitions.Set(PokemonType.NRM, PokemonType.Unknown, TypeEffectiveness.SuperEffective);
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
