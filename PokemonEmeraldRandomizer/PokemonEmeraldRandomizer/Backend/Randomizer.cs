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
                foreach (var evo in pkmn.evolvesTo)
                {
                    if(settings.FixImpossibleEvos)
                    {
                        if(evo.Type == EvolutionType.Trade)
                        {
                            evo.Type = EvolutionType.LevelUp;
                            evo.parameter = 32;
                        }
                        else if(evo.Type == EvolutionType.TradeWithItem)
                            evo.Type = EvolutionType.UseItem;
                    }
                }
                // Set Pokemon Tags (legendary, etc)
                // Mutate low-consequence base stats
                // Mutate Pokemon Type
                if (pkmn.IsSingleTyped)
                {
                    if (rand.RandomDouble() < settings.SingleTypeRandChance)
                        pkmn.types[0] = pkmn.types[1] = RandomType(data.Metrics, "None");
                }
                else
                {
                    if (rand.RandomDouble() < settings.DualTypePrimaryRandChance)
                        pkmn.types[0] = rand.RandomChoice(data.Metrics.TypeRatiosDualPrimary);
                    if (rand.RandomDouble() < settings.DualTypeSecondaryRandChance)
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
            // Get the species randomization settings for wild pokemon
            var wildSpeciesSettings = settings.GetSpeciesSettings("wild");
            if (settings.WildPokemonSetting == Settings.WildPokemonOption.CompletelyRandom)
            {
                foreach (var encounterSet in data.Encounters)
                {
                    foreach (var enc in encounterSet)
                    {
                        enc.pokemon = RandomSpecies(pokemonSet, enc.pokemon, enc.level, wildSpeciesSettings);
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
                        mapping.Add(s, RandomSpecies(pokemonSet, s, wildSpeciesSettings));
                    foreach (var enc in encounterSet)
                    {
                        enc.pokemon = mapping[enc.pokemon];
                        if (wildSpeciesSettings.DisableIllegalEvolutions)
                            enc.pokemon = CorrectImpossibleEvo(enc.pokemon, enc.level, wildSpeciesSettings);
                    }
                }
            }
            else if(settings.WildPokemonSetting == Settings.WildPokemonOption.GlobalOneToOne)
            {
                var mapping = new Dictionary<PokemonSpecies, PokemonSpecies>();
                foreach (var s in pokemonSet)
                    mapping.Add(s, RandomSpecies(pokemonSet, s, wildSpeciesSettings));
                foreach (var encounterSet in data.Encounters)
                {
                    foreach (var enc in encounterSet)
                    {
                        enc.pokemon = mapping[enc.pokemon];
                        if (wildSpeciesSettings.DisableIllegalEvolutions)
                            enc.pokemon = CorrectImpossibleEvo(enc.pokemon, enc.level, wildSpeciesSettings);
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
                // Set Battle Type
                if (rand.RandomDouble() < settings.BattleTypeRandChance)
                    trainer.isDoubleBattle = rand.RandomDouble() < settings.DoubleBattleChance;
                // Set pokemon
                foreach (var pokemon in trainer.pokemon)
                {
                    pokemon.species = RandomSpecies(pokemonSet, pokemon.species, pokemon.level, settings.GetSpeciesSettings("trainer"));
                    // Reset special moves if necessary
                    if (pokemon.dataType == TrainerPokemon.DataType.SpecialMoves || pokemon.dataType == TrainerPokemon.DataType.SpecialMovesAndHeldItem)
                    {
                        pokemon.moves = MovesetGenerator.DefaultMoveset(data.PokemonLookup[pokemon.species], pokemon.level);
                    }                 
                }
                // Set 1-pokemon battle to solo if appropriate
                if (settings.MakeSoloPokemonBattlesSingle && trainer.pokemon.Length == 1)
                    trainer.isDoubleBattle = false;
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
                var powerWeighting = PokemonMetrics.PowerSimilarity(combinedWeightings.Items, powerScores, pokemon, speciesSettings.PowerScaleThreshold);
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
        /// <summary>Chose a random species from the input set based on the given species settings.
        /// If speciesSettings.DisableIllegalEvolutions is true, scale impossible evolutions down to their less evolved forms </summary> 
        private PokemonSpecies RandomSpecies(IEnumerable<PokemonSpecies> possiblePokemon, PokemonSpecies pokemon, int level, Settings.SpeciesSettings speciesSettings)
        {
            var newSpecies = RandomSpecies(possiblePokemon, pokemon, speciesSettings);
            if(speciesSettings.DisableIllegalEvolutions)
                newSpecies = CorrectImpossibleEvo(newSpecies, level, speciesSettings);
            // Actually choose the species
            return newSpecies;
        }
        /// If the pokemon is an invalid level due to evolution state, revert to an earlier evolution
        private PokemonSpecies CorrectImpossibleEvo(PokemonSpecies species, int level, Settings.SpeciesSettings speciesSettings)
        {
            var newSpecies = species;
            var evolvesFrom = data.PokemonLookup[newSpecies].evolvesFrom;
            while (!IsPokemonValidLevel(evolvesFrom, level, speciesSettings))
            {
                // Choose a random element from the pokemon this pokemon evolves from
                newSpecies = rand.RandomChoice(evolvesFrom).Pokemon;
                evolvesFrom = data.PokemonLookup[newSpecies].evolvesFrom;
            }
            return newSpecies;
        }
        /// <summary> returns false if the pokemon is an invalid level 
        /// (due to not being high enough level to evolve to the current species) </summary>
        private static bool IsPokemonValidLevel(List<Evolution> evolvesFrom, int level, Settings.SpeciesSettings speciesSettings)
        {
            if (evolvesFrom.Count == 0) // basic pokemon
                return true;
            // Is there at least one valid evolution
            foreach (var evo in evolvesFrom)
            {
                if (!evo.EvolvesByLevel) // evolution is valid if not by level
                {
                    if (!speciesSettings.SetLevelsOnArtificialEvos)
                        return true;
                    int levelReq = 0; // Calculate a level req for randomizer purposes
                    if (evo.EvolvesByTrade)
                        levelReq = speciesSettings.TradeEvolutionLevel;
                    else if (evo.Type == EvolutionType.UseItem)
                        levelReq = speciesSettings.ItemEvolutionLevel;
                    else if (evo.Type == EvolutionType.Beauty)
                        levelReq = speciesSettings.BeautyEvolutionLevel;
                    else
                        levelReq = speciesSettings.FriendshipEvolutionLevel;
                    if (levelReq <= level)
                        return true;
                }
                else if (evo.parameter <= level) // evolution is valid if required level is <= level
                    return true;
            }
            return false;
        }
    }
}
