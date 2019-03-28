using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer.Backend
{
    // This class does the actualmutation and randomizing by creating a mutated copy
    // of the original ROM data
    public static class RomDataMutator
    {
        // Apply mutations based on program settings.
        public static RomData Mutate(RomData orig, ApplicationData appData)
        {
            // Initialize copy data to mutate and mutator with seed if applicable
            RomData copy = orig.Clone();
            Mutator mut = appData.SetSeed ? new Mutator(appData.Seed) : new Mutator();

            var pokemonSet = DefinePokemonSet(copy, appData, mut);
            var types = DefinePokemonTypes(copy, appData, mut);

            // Randomize type traits
            // Generate ??? type traits (INCOMPLETE)
            if(appData.ModifyUnknownType)
            {
                foreach (var type in types)
                {
                    // Type effectiveness of other type vs ???
                    var te = mut.RandomChoice(orig.Metrics.TypeEffectivenessRatios);
                    if (te != TypeEffectiveness.Normal) // Only register if not normal effectiveness
                        copy.TypeDefinitions.Add(type, PokemonType.Unknown, te, (type == PokemonType.NRM || type == PokemonType.FTG) && te == TypeEffectiveness.NoEffect);
                    // Type effectiveness of ??? vs other type
                    te = mut.RandomChoice(orig.Metrics.TypeEffectivenessRatios);
                    if (te != TypeEffectiveness.Normal) // Only register if not normal effectiveness
                        copy.TypeDefinitions.Add(PokemonType.Unknown, type, te, type == PokemonType.GHO);
                }
            }
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
                    if (mut.RandomDouble() < appData.SingleTypeMutationRate)
                        pkmn.types[0] = pkmn.types[1] = randomType(mut, copy.Metrics, "None");
                }
                else
                {
                    if (mut.RandomDouble() < appData.DualTypePrimaryMutationRate)
                        pkmn.types[0] = mut.RandomChoice(orig.Metrics.TypeRatiosDualPrimary);
                    if (mut.RandomDouble() < appData.DualTypeSecondaryMutationRate)
                        pkmn.types[1] = mut.RandomChoice(orig.Metrics.TypeRatiosDualSecondary);
                }
                // Mutate battle states and EVs
                // Mutate Learn Sets
            }
            // Recalculate power scoring
            var powerScores = PowerScaling.Calculate(copy.Pokemon, appData.TieringOptions);
            // Mutate Starter Pokemon
            // Determine trainer class set
                // Only double battles?
                // Include FRLG classes in emerald (and vice-versa)?
            // Mutate Trainer classes
                // Theme: type?
                // Theme: move strategy?
                // Theme: hold item?
                // Ace trainers have random theme?
            // Mutate Maps
                // Mutate tiles and layout (if applicable)
                    // Team magma/aqua/rocket takeover mode?
                // Set metadata (environent, etc)
                // Mutate wild pokemon
                // Mutate Trainers (should be per map later?)
                    // Set Trainer positions (and some base tags maybe) and add scripts
                        // Sleeper agents? (Random non-npc trainers become trainers)
                    // Set tags (gym trainer, gym leader, elite 4, rival, reoccuring, etc)
                    // Set class
                        // Natural trainers? (trainer types are based on environment type)
                    //Mutate battle here later?
            // Mutate Trainer battles
            foreach(var trainer in copy.Trainers)
            {
                // Set data type
                // Set AI flags
                // Set item stock (if applicable)
                // Set pokemon
                foreach (var pokemon in trainer.pokemon)
                {
                    var possiblePokemon = new HashSet<PokemonSpecies>(pokemonSet);
                    var combinedWeightings = new WeightedSet<PokemonSpecies>();
                    // Power level similarity
                    if(appData.TrainerPowerScaleSimilarityMod > 0)
                    {
                        var powerWeighting = PokemonMetrics.PowerSimilarity(possiblePokemon, powerScores, pokemon.species);
                        combinedWeightings.Add(powerWeighting, appData.TrainerPowerScaleSimilarityMod);
                        // Cull if necessary
                        if (appData.TrainerPowerScaleCull)
                        {
                            possiblePokemon.RemoveWhere((p) => !powerWeighting.Contains(p));
                            // combinedWeightings.RemoveWhere((p) => !powerWeighting.Contains(p)); no need to back-propagate (first metric)
                        }                           
                    }
                    // Type similarity
                    if(appData.TrainerTypeSimilarityMod > 0)
                    {
                        var typeWeighting = PokemonMetrics.TypeSimilarity(possiblePokemon, copy, pokemon.species);
                        combinedWeightings.Add(typeWeighting, appData.TrainerTypeSimilarityMod);
                        // Cull if necessary
                        if (appData.TrainerPowerScaleCull)
                        {
                            possiblePokemon.RemoveWhere((p) => !typeWeighting.Contains(p));
                            combinedWeightings.RemoveWhere((p) => !typeWeighting.Contains(p));
                        }
                    }
                    // Normalize combined weightings and add noise
                    if(appData.TrainerPokemonNoise > 0)
                    {
                        combinedWeightings.Normalize();
                        var noise = new WeightedSet<PokemonSpecies>(pokemonSet, appData.TrainerPokemonNoise);
                        combinedWeightings.Add(noise);
                    }

                    // Actually choose the species
                    pokemon.species = mut.RandomChoice(combinedWeightings);
                    // Reset special moves if necessary
                }
                    // Class based?
                    // Local environment based?
            }

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

        // Define and return the set of valid pokemon (with applicable restrictions)
        private static HashSet<PokemonSpecies> DefinePokemonSet(RomData romData, ApplicationData appData, Mutator mut)
        {                        
            //Start with all for now
            HashSet<PokemonSpecies> pokemonSet = EnumUtils.GetValues<PokemonSpecies>().ToHashSet();          
                // Restrict pokemon if applicable
            // Possible restrictions any combination of: GenI, GenI+ (GenI related pokemon from GenII, and/or possibly GenIV), GenII,
                // GenII+ (GenII related from GenI, GenII and/or possilby GenIV), GenIII, GenIII+ (Gen II related pokemon from GenI and/or GenII
            // Possibly other pkmn groups like starters, legendaries, maybe even arbitrary groups
                // Hack in new pokemon if applicable
                // Possible Hacks: Gen IV
            return pokemonSet;
        }

        // Define and return the set of valid types (with applicable restrictions)
        private static HashSet<PokemonType> DefinePokemonTypes(RomData romData, ApplicationData appData, Mutator mut)
        {
            HashSet<PokemonType> types = EnumUtils.GetValues<PokemonType>().ToHashSet();
            // Remove the FAIRY type if we are Gen V or below and have not enabled the add fairy type hack
            if (romData.Gen < RomData.Generation.VI && !appData.AddFairyType)
                types.Remove(PokemonType.FAI);
            return types;
        }
        


        private static PokemonType randomType(Mutator mut, RomMetrics metrics, string metric)
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
