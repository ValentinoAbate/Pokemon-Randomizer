using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer.Backend
{
    // This class does the actualmutation and randomizing by creating a mutated copy
    // of the original ROM data
    public class Randomizer
    {
        public RomData data;
        public Settings settings;
        public Random rand;
        public Dictionary<PokemonSpecies, float> powerScores;

        public Randomizer(RomData orig, Settings settings)
        {
            // Initialize copy data to mutate and mutator with seed if applicable
            data = orig.Clone();
            rand = settings.SetSeed ? new Random(settings.Seed) : new Random();
            this.settings = settings;
            //Calculate original power scores
            powerScores = PowerScaling.Calculate(data.Pokemon, settings.TieringOptions);
        }
        // Apply mutations based on program settings.
        public RomData Randomize()
        {
            var pokemonSet = DefinePokemonSet();
            var types = DefinePokemonTypes();

            // Randomize type traits
            // Generate ??? type traits (INCOMPLETE)
            if (settings.ModifyUnknownType)
            {
                foreach (var type in types)
                {
                    // Type effectiveness of other type vs ???
                    var te = rand.RandomChoice(data.Metrics.TypeEffectivenessRatios);
                    if (te != TypeEffectiveness.Normal) // Only register if not normal effectiveness
                        data.TypeDefinitions.Add(type, PokemonType.Unknown, te, (type == PokemonType.NRM || type == PokemonType.FTG) && te == TypeEffectiveness.NoEffect);
                    // Type effectiveness of ??? vs other type
                    te = rand.RandomChoice(data.Metrics.TypeEffectivenessRatios);
                    if (te != TypeEffectiveness.Normal) // Only register if not normal effectiveness
                        data.TypeDefinitions.Add(PokemonType.Unknown, type, te, type == PokemonType.GHO);
                }
            }
            // Combat Hacks
            // Hack combat if applicable
                // Possible Hacks: Upgrade combat AI, Special/Physical split
            // Define Move Definitions
            // Hack in new moves if applicable
                // Possible Hacks: Add GenIV moves (idk if this is possible), Add Fairy moves (should be OK),
                // add some ???-type moves, procedurally generate new moves. Animations would be a problem
                    // Ideas: Mystery Power: 60 power ??? move, random chance of a randomly chosen stat buff/debuff or special condition
                    // Ideas: Unknown Power: 75-80 power ??? move, random chance of a randomly chosen stat buff/debuff or special condition
                    // Ideas: Cryptic Power: 30 power ???, chance of weirder effects like multi-hit, much higher chance of multiple effects
            // Mutate move definitions (should this come before or after hacks (maybe let user choose))
                // Change move type, power, etc. (this would be really lame if not at a low mutation rate)
            // Define Item Definitions
                // Hack in new items if applicable
                    // Possible Hacks: Add GenIV items (some might not be possible), add fairy-related items
            // Mutate item definitions
            // Mutate Pokemon
            foreach (PokemonBaseStats pkmn in data.Pokemon)
            {
                // Mutate Evolution trees
                // Set Pokemon Tags (legendary, etc)
                // Mutate low-consequence base stats
                // Mutate Pokemon Type
                if (pkmn.IsSingleTyped)
                {
                    if (rand.RandomDouble() < settings.SingleTypeMutationRate)
                        pkmn.types[0] = pkmn.types[1] = RandomType(data.Metrics, "None");
                }
                else
                {
                    if (rand.RandomDouble() < settings.DualTypePrimaryMutationRate)
                        pkmn.types[0] = rand.RandomChoice(data.Metrics.TypeRatiosDualPrimary);
                    if (rand.RandomDouble() < settings.DualTypeSecondaryMutationRate)
                        pkmn.types[1] = rand.RandomChoice(data.Metrics.TypeRatiosDualSecondary);
                }
                // Mutate battle states and EVs
                // Mutate Learn Sets
            }
            // Change pallettes to fit new types
            // Recalculate power scoring
            powerScores = PowerScaling.Calculate(data.Pokemon, settings.TieringOptions);
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
            // Mutate Trainers (should be per map later?)
            // Set Trainer positions (and some base tags maybe) and add scripts
            // Sleeper agents? (Random non-npc trainers become trainers)
            // Set tags (gym trainer, gym leader, elite 4, rival, reoccuring, etc)
            // Set class
            // Natural trainers? (trainer types are based on environment type)
            //Mutate battle here later?

            #region Randomize Wild Pokemon (may happen during maps later)

            if(settings.WildPokemonSetting == Settings.WildPokemonOption.CompletelyRandom)
            {
                foreach (var encounterSet in data.Encounters)
                {
                    foreach (var encounter in encounterSet)
                    {
                        encounter.pokemon = RandomSpecies(pokemonSet, encounter.pokemon, settings.GetSpeciesSettings("wild"));
                    }
                }
            }
            else if(settings.WildPokemonSetting == Settings.WildPokemonOption.AreaOneToOne)
            {
                foreach (var encounterSet in data.Encounters)
                {
                    // Get all unique species in the encounter set
                    var species = encounterSet.Select((e) => e.pokemon).Distinct();
                    var mapping = new Dictionary<PokemonSpecies, PokemonSpecies>();
                    foreach (var s in species)
                        mapping.Add(s, RandomSpecies(pokemonSet, s, settings.GetSpeciesSettings("wild")));
                    foreach (var encounter in encounterSet)
                    {
                        encounter.pokemon = mapping[encounter.pokemon];
                    }
                }
            }
            else if(settings.WildPokemonSetting == Settings.WildPokemonOption.GlobalOneToOne)
            {
                var mapping = new Dictionary<PokemonSpecies, PokemonSpecies>();
                foreach (var s in pokemonSet)
                    mapping.Add(s, RandomSpecies(pokemonSet, s, settings.GetSpeciesSettings("wild")));
                foreach (var encounterSet in data.Encounters)
                {
                    foreach (var encounter in encounterSet)
                    {
                        encounter.pokemon = mapping[encounter.pokemon];
                    }
                }
            }

            #endregion

            #region Randomize Trainer Battles

            foreach (var trainer in data.Trainers)
            {
                // Set data type
                // Set AI flags
                // Set item stock (if applicable)
                // Set pokemon
                foreach (var pokemon in trainer.pokemon)
                {
                    pokemon.species = RandomSpecies(pokemonSet, pokemon.species, settings.GetSpeciesSettings("trainer"));
                    // Reset special moves if necessary
                    if (pokemon.dataType == TrainerPokemon.DataType.SpecialMoves || pokemon.dataType == TrainerPokemon.DataType.SpecialMovesAndHeldItem)
                    {
                        pokemon.moves = MovesetGenerator.DefaultMoveset(data.PokemonLookup[pokemon.species], pokemon.level);
                    }                 
                }
                // Class based?
                // Local environment based?
            }

            #endregion

            // Mutate Field Items
            // Misc Hacks
            // Potential hacks:
            // Randomize pickup items, natl pokedex, text speed hack, Lower case name hacks, Dunsparse GOD MODE, Dunsparse Plague mode, Unknown Mods

            data.CalculateMetrics();
            return data;
        }

        /// <summary>Define and return the set of valid pokemon (with applicable restrictions)</summary>
        private HashSet<PokemonSpecies> DefinePokemonSet()
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
        /// <summary>Define and return the set of valid types (with applicable restrictions)</summary> 
        private HashSet<PokemonType> DefinePokemonTypes()
        {
            HashSet<PokemonType> types = EnumUtils.GetValues<PokemonType>().ToHashSet();
            // Remove the FAIRY type if we are Gen V or below and have not enabled the add fairy type hack
            if (data.Gen < RomData.Generation.VI && !settings.AddFairyType)
                types.Remove(PokemonType.FAI);
            return types;
        }

        /// <summary>Chose a random type based on the given metric (OUT OF DATE)</summary> 
        private PokemonType RandomType(RomMetrics metrics, string metric)
        {
            switch (metric)
            {
                case "None":
                    return rand.RandomChoice(metrics.TypeRatiosAll.Items);
                case "Type Occurence (Any)":
                    return rand.RandomChoice(metrics.TypeRatiosAll);
                case "Type Occurence (Single)":
                    return rand.RandomChoice(metrics.TypeRatiosSingle);
                case "Type Occurence (Primary)":
                    return rand.RandomChoice(metrics.TypeRatiosDualPrimary);
                case "Type Occurence (Secondary)":
                    return rand.RandomChoice(metrics.TypeRatiosDualSecondary);
                default:
                    throw new System.NotImplementedException(metric + " is not a valid metric.");
            }
        }
        /// <summary>Chose a random species from the input set based on the given species settings</summary> 
        private PokemonSpecies RandomSpecies(IEnumerable<PokemonSpecies> possiblePokemon, PokemonSpecies pokemon, Settings.SpeciesSettings speciesSettings)
        {
            var combinedWeightings = new WeightedSet<PokemonSpecies>(possiblePokemon);
            // Power level similarity
            if (speciesSettings.PowerScaleSimilarityMod > 0)
            {
                var powerWeighting = PokemonMetrics.PowerSimilarity(combinedWeightings.Items, powerScores, pokemon);
                combinedWeightings.Add(powerWeighting, speciesSettings.PowerScaleSimilarityMod);
                // Cull if necessary
                if (speciesSettings.PowerScaleCull)
                    combinedWeightings.RemoveWhere((p) => !powerWeighting.Contains(p));
            }
            // Type similarity
            if (speciesSettings.TypeSimilarityMod > 0)
            {
                var typeWeighting = PokemonMetrics.TypeSimilarity(combinedWeightings.Items, data, pokemon);
                combinedWeightings.Add(typeWeighting, speciesSettings.TypeSimilarityMod);
                // Cull if necessary
                if (speciesSettings.PowerScaleCull)
                    combinedWeightings.RemoveWhere((p) => !typeWeighting.Contains(p));
            }
            // Normalize combined weightings and add noise
            if (speciesSettings.Noise > 0)
            {
                combinedWeightings.Normalize();
                var noise = new WeightedSet<PokemonSpecies>(possiblePokemon, speciesSettings.Noise);
                combinedWeightings.Add(noise);
            }

            // Actually choose the species
            return rand.RandomChoice(combinedWeightings);
        }
    }
}
