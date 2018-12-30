using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer.Backend
{
    // This class does the actualmutation and randomizing by creating a mutated copy
    // of the original ROM data
    public static class ROMDataMutator
    {
        // Apply mutations based on program settings.
        public static ROMData Mutate(ROMData orig, MainWindow window)
        {
            // Initialize copy data to mutate and mutator with seed if applicable
            ROMData copy = ROMParser.Parse(orig.ROM);
            Mutator mut = (bool)window.cbSeed.IsChecked ? new Mutator(window.tbSeed.Text) : new Mutator();

            // Define pokemon set
                // Restrict pokemon if applicable
                    // Possible restrictions any combination of: GenI, GenI+ (GenI related pokemon from GenII, and/or possibly GenIV), GenII,
                    // GenII+ (GenII related from GenI, GenII and/or possilby GenIV), GenIII, GenIII+ (Gen II related pokemon from GenI and/or GenII
                    // Possibly other pkmn groups like starters, legendaries, maybe even arbitrary groups
                // Hack in new pokemon if applicable
                    // Possible Hacks: Gen IV
            // Define Type set
                // Hack in new types if applicable
                    // Possible Hacks: Add Fairy Type
            // Define Type Traits

            // Randomize type traits
            foreach(PokemonType t in EnumUtils.GetValues<PokemonType>())
            {
                TypeEffectiveness te = mut.RandomChoice(orig.Metrics.TypeEffectivenessRatios);
                if (te != TypeEffectiveness.Normal)
                    copy.TypeDefinitions.Add(t, PokemonType.Unknown, te, (t == PokemonType.NRM || t == PokemonType.FTG) && te == TypeEffectiveness.NoEffect);
                te = mut.RandomChoice(orig.Metrics.TypeEffectivenessRatios);
                if (te != TypeEffectiveness.Normal)
                    copy.TypeDefinitions.Add(PokemonType.Unknown, t, te, t == PokemonType.GHO);
            }
            // Generate ??? type traits
            copy.TypeDefinitions.Set(PokemonType.NRM, PokemonType.Unknown, TypeEffectiveness.SuperEffective);
            // Combat Hacks
                // Hack combat if applicable
                    // Possible Hacks: Upgrade combat AI, Special/Physical split
            // Define Move Definitions
                // Hack in new moves if applicable
                    // Possible Hacks: Add GenIV moves (idk if this is possible), Add Fairy moves (should be OK),
                    // add some ???-type moves, procedurally generate new moves. Animations would be a problem
                // Mutate move definitions (should this come before or after hacks (maybe let user choose))
                    // Change move type, power, etc. (this would be really lame if not at a low mutation rate)
            // Define Item Definitions
                // Hack in new items if applicable
                    // Possible Hacks: Add GenIV items (some might not be possible), add fairy-related items
                // Mutate item definitions
            // Mutate Pokemon
            foreach (PokemonBaseStats pkmn in copy.Pokemon)
            {
                // Mutate Evolution trees
                // Set Pokemon Tags (legendary, etc)
                // Mutate low-consequence base stats
                // Mutate Pokemon Type
                if (pkmn.IsSingleTyped)
                {
                    var rate = window.mutSlSingleType.Value;
                    if (mut.RandomDouble() < rate)
                        pkmn.types[0] = pkmn.types[1] = randomType(mut, copy.Metrics, "None");
                }
                else
                {
                    if (mut.RandomDouble() < 0.05)
                        pkmn.types[0] = mut.RandomChoice(orig.Metrics.TypeRatiosDualPrimary);
                    if (mut.RandomDouble() < 0.05)
                        pkmn.types[1] = mut.RandomChoice(orig.Metrics.TypeRatiosDualSecondary);
                }
                // Mutate battle states and EVs
                // Mutate Learn Sets
            }
            // Mutate Starter Pokemon
            // Mutate Trainers
            // Mutate Field Items
            // Misc Hacks
                // Potential hacks:
                    // Randomize pickup items, natl pokedex, text speed hack, Lower case name hacks, Dunsparse GOD MODE, Dunsparse Plague modeX
            #region Old Arty mutFlow
            // changeStarters(); // must come before trainers
            // changeTMs();      // must come before trainers
            // randomizePokeData(); // must come before trainers
            // changeWildPokemon();
            // changePkmnColors();
            // fixDisobedience();
            // changeIntroPokemon();
            // changeTradeEvolutions();
            // randomizePickup();
            // changeTrainerPokemon();
            // changeTrainerClasses();
            // heartScales();
            // metronomeHyperdrive();
            // changeItems();
            // changeDex();
            #endregion
            copy.CalculateMetrics();
            return copy;
        }
        private static PokemonType randomType(Mutator mut, BalanceMetrics metrics, string metric)
        {
            switch (metric)
            {
                case "None":
                    return mut.RandomChoice(metrics.TypeRatiosAll.Items);
                case "Type Occurence (Any)":
                    return mut.RandomChoice(metrics.TypeRatiosAll);
                case "Type Occurence (Single)":
                    return mut.RandomChoice(metrics.TypeRatiosSingle);
                case "Type Occurence (Primary)":
                    return mut.RandomChoice(metrics.TypeRatiosDualPrimary);
                case "Type Occurence (Secondary)":
                    return mut.RandomChoice(metrics.TypeRatiosDualSecondary);
                default:
                    throw new System.NotImplementedException(metric + " is not a valid metric.");
            }
        }
    }
}
