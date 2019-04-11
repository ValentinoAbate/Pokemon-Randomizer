﻿using System;
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

            #region Combat Hacks (NOTHING YET)
            // Combat Hacks
            // Hack combat if applicable
            // Possible Hacks: Upgrade combat AI, Special/Physical split
            #endregion

            #region Type Definitions
            // Randomize type traits
            // Generate ??? type traits (INCOMPLETE)
            if (settings.ModifyUnknownType)
            {
                foreach (var type in types)
                {
                    // Type effectiveness of other type vs ???
                    var te = rand.Choice(data.Metrics.TypeEffectivenessRatios);
                    if (te != TypeEffectiveness.Normal) // Only register if not normal effectiveness
                        data.TypeDefinitions.Add(type, PokemonType.Unknown, te, (type == PokemonType.NRM || type == PokemonType.FTG) && te == TypeEffectiveness.NoEffect);
                    // Type effectiveness of ??? vs other type
                    te = rand.Choice(data.Metrics.TypeEffectivenessRatios);
                    if (te != TypeEffectiveness.Normal) // Only register if not normal effectiveness
                        data.TypeDefinitions.Add(PokemonType.Unknown, type, te, type == PokemonType.GHO);
                }
            }
            #endregion

            #region Move Definitions (NOTHING YET)
            // Define Move Definitions
            // Hack in new moves if applicable
            // Possible Hacks: Add GenIV moves (idk if this is possible), Add Fairy moves (should be OK),
            // add some ???-type moves, procedurally generate new moves. Animations would be a problem
            // Ideas: Mystery Power: 60 power ??? move, random chance of a randomly chosen stat buff/debuff or special condition
            // Ideas: Unknown Power: 75-80 power ??? move, random chance of a randomly chosen stat buff/debuff or special condition
            // Ideas: Cryptic Power: 30 power ???, chance of weirder effects like multi-hit, much higher chance of multiple effects
            // Mutate move definitions (should this come before or after hacks (maybe let user choose))
            // Change move type, power, etc. (this would be really lame if not at a low mutation rate)
            #endregion

            #region Item Definitions (NOTHING YET)
            // Define Item Definitions
            // Hack in new items if applicable
            // Possible Hacks: Add GenIV items (some might not be possible), add fairy-related items
            // Mutate item definitions
            #endregion

            #region Pokemon Base Attributes
            // Mutate Pokemon
            foreach (PokemonBaseStats pkmn in data.Pokemon)
            {
                // Mutate Evolution trees
                foreach (var evo in pkmn.evolvesTo)
                {
                    if(rand.RandomDouble() < settings.DunsparsePlaugeChance)
                    {
                        // Add the plague
                        if(evo.Type == EvolutionType.LevelUp)
                        {
                            evo.Type = EvolutionType.LevelUpWithPersonality1;
                            if(pkmn.evolvesTo[1].Pokemon == PokemonSpecies.SLOWKING)
                            {
                                pkmn.evolvesTo[2].Pokemon = PokemonSpecies.DUNSPARCE;
                                pkmn.evolvesTo[2].Type = EvolutionType.LevelUpWithPersonality2;
                                pkmn.evolvesTo[2].parameter = evo.parameter;
                            }
                            else
                            {
                                pkmn.evolvesTo[1].Pokemon = PokemonSpecies.DUNSPARCE;
                                pkmn.evolvesTo[1].Type = EvolutionType.LevelUpWithPersonality2;
                                pkmn.evolvesTo[1].parameter = evo.parameter;
                            }
                        }
                    }
                    if(settings.FixImpossibleEvos)
                    {
                        if(evo.Type == EvolutionType.Trade)
                        {
                            evo.Type = EvolutionType.LevelUp;
                            evo.parameter = 32;
                        }
                        else if(evo.Type == EvolutionType.TradeWithItem)
                        {
                            evo.Type = EvolutionType.UseItem;
                            evo.parameter = (int)Item.Fire_Stone;
                        }
                            
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
                        pkmn.types[0] = rand.Choice(data.Metrics.TypeRatiosDualPrimary);
                    if (rand.RandomDouble() < settings.DualTypeSecondaryRandChance)
                        pkmn.types[1] = rand.Choice(data.Metrics.TypeRatiosDualSecondary);
                }

                // Mutate battle states and EVs
                // Mutate Learn Sets
            }
            // Change pallettes to fit new types
            // Recalculate power scoring
            powerScores = PowerScaling.Calculate(data.Pokemon, settings.TieringOptions);
            #endregion

            #region Starters
            if (settings.RandomizeStarters)
            {
                var speciesSettings = settings.GetSpeciesSettings("starter");
                if(settings.StarterSetting == Settings.StarterPokemonOption.CompletelyRandom)
                {
                    for(int i = 0; i < data.Starters.Count; ++i)
                        data.Starters[i] = RandomSpecies(pokemonSet, data.Starters[i], 5, speciesSettings);
                }
                else if (settings.StarterSetting == Settings.StarterPokemonOption.TypeTriangle)
                {
                    var triangle = RandomTypeTriangle(pokemonSet, speciesSettings);
                    if (triangle != null)
                        data.Starters = triangle;
                    else // Fall back on completely random
                    {
                        for (int i = 0; i < data.Starters.Count; ++i)
                            data.Starters[i] = RandomSpecies(pokemonSet, data.Starters[i], 5, speciesSettings);
                    }
                }
            }
            #endregion

            #region Trainer Classes (NOTHING YET)
            // Determine trainer class set
            // Include FRLG classes in emerald (and vice-versa)?
            // Mutate Trainer classes
            // Theme: type?
            // Theme: move strategy?
            // Theme: hold item?
            // Ace trainers have random theme?
            #endregion

            #region Maps (NOTHING YET)
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
            #endregion

            #region Wild Pokemon (may happen during maps later)
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

            #region Trainer Battles

            #region Set Up Special Trainers
            var specialTrainers = new Dictionary<string, List<Trainer>>();
            var rivals = data.Info.ArrayAttr("rivals", "names");
            foreach (var name in rivals)
                specialTrainers.Add(name.ToLower(), new List<Trainer>());
            var reoccuring = data.Info.ArrayAttr("reoccuring", "names");
            foreach (var name in data.Info.ArrayAttr("reoccuring", "names"))
                specialTrainers.Add(name.ToLower(), new List<Trainer>());
            //foreach (var name in data.Info.ArrayAttr("gymLeaders", "names"))
            //    specialTrainers.Add(name.ToLower(), new List<Trainer>());
            //foreach (var name in data.Info.ArrayAttr("eliteFour", "names"))
            //    specialTrainers.Add(name.ToLower(), new List<Trainer>());
            //foreach (var name in data.Info.ArrayAttr("champion", "names"))
            //    specialTrainers.Add(name.ToLower(), new List<Trainer>());
            //foreach (var name in data.Info.ArrayAttr("uber", "names"))
            //    specialTrainers.Add(name.ToLower(), new List<Trainer>());
            //foreach (var name in data.Info.ArrayAttr("teamAdmins", "names"))
            //    specialTrainers.Add(name.ToLower(), new List<Trainer>());
            //foreach (var name in data.Info.ArrayAttr("teamLeaders", "names"))
            //    specialTrainers.Add(name.ToLower(), new List<Trainer>());
            int[] aceTrainerClasses = data.Info.IntArrayAttr("aceTrainers", "classNums");
            List<Trainer> aceTrainers = new List<Trainer>();
            #endregion

            var trainerSpeciesSettings = settings.GetSpeciesSettings("trainer");

            foreach (var trainer in data.Trainers)
            {
                // Set aside special trainers
                if (specialTrainers.ContainsKey(trainer.name.ToLower()))
                    specialTrainers[trainer.name.ToLower()].Add(trainer);
                else if (aceTrainerClasses.Contains(trainer.trainerClass))
                    aceTrainers.Add(trainer);
                else
                    RandomizeBattle(trainer, pokemonSet, trainerSpeciesSettings);     
            }

            #region Special Trainers

            #region Rivals

            var rivalSpeciesSettings = settings.GetSpeciesSettings("rival");
            foreach (var trainer in rivals)
            {
                var battles = specialTrainers[trainer.ToLower()];
                battles.Sort((a, b) => a.AvgLvl.CompareTo(b.AvgLvl));
                if (settings.RivalSetting == Settings.TrainerOption.CompletelyRandom)
                {
                    foreach (var battle in battles)
                        RandomizeBattle(battle, pokemonSet, rivalSpeciesSettings);
                }
                else if (settings.RivalSetting == Settings.TrainerOption.KeepAce)
                {
                    var starters = new PokemonSpecies[] { battles[0].pokemon[0].species, battles[1].pokemon[0].species, battles[2].pokemon[0].species };
                    var rival1Battles = battles.Where((b) => b.pokemon.Any((p) => RelatedToOrSelf(p.species, starters[0]))).ToArray();
                    var rival2Battles = battles.Where((b) => b.pokemon.Any((p) => RelatedToOrSelf(p.species, starters[1]))).ToArray();
                    var rival3Battles = battles.Where((b) => b.pokemon.Any((p) => RelatedToOrSelf(p.species, starters[2]))).ToArray();
                    var rivalBattleArr = new Trainer[][] { rival1Battles, rival2Battles, rival3Battles };
                    int[] rivalRemap = { 1, 2, 0 };
                    for (int i = 0; i < rivalBattleArr.Length; ++i)
                    {
                        var battleSet = rivalBattleArr[i];
                        foreach (var battle in battleSet)
                        {
                            RandomizeBattle(battle, pokemonSet, rivalSpeciesSettings);
                            var pokemon = battle.pokemon[battle.pokemon.Length - 1];
                            pokemon.species = MaxEvolution(data.Starters[rivalRemap[i]], pokemon.level, rivalSpeciesSettings);
                            if (pokemon.HasSpecialMoves)
                            {
                                pokemon.moves = MovesetGenerator.DefaultMoveset(data.PokemonLookup[pokemon.species], pokemon.level);
                            }
                        }
                    }                     
                }
                else if(settings.RivalSetting == Settings.TrainerOption.Procedural)
                {
                    var starters = new PokemonSpecies[] { battles[0].pokemon[0].species, battles[1].pokemon[0].species, battles[2].pokemon[0].species };
                    var rival1Battles = battles.Where((b) => b.pokemon.Any((p) => RelatedToOrSelf(p.species, starters[0]))).ToArray();
                    var rival2Battles = battles.Where((b) => b.pokemon.Any((p) => RelatedToOrSelf(p.species, starters[1]))).ToArray();
                    var rival3Battles = battles.Where((b) => b.pokemon.Any((p) => RelatedToOrSelf(p.species, starters[2]))).ToArray();
                    PcgBattles(rival1Battles, new PokemonSpecies[] { data.Starters[1] }, pokemonSet, rivalSpeciesSettings);
                    PcgBattles(rival2Battles, new PokemonSpecies[] { data.Starters[2] }, pokemonSet, rivalSpeciesSettings);
                    PcgBattles(rival3Battles, new PokemonSpecies[] { data.Starters[0] }, pokemonSet, rivalSpeciesSettings);
                }              
            }

            #endregion

            #region Reoccuring Trainers

            foreach(var trainer in reoccuring)
            {
                var battles = specialTrainers[trainer.ToLower()];
                battles.Sort((a, b) => a.AvgLvl.CompareTo(b.AvgLvl));
                if (settings.RivalSetting == Settings.TrainerOption.CompletelyRandom)
                {
                    foreach (var battle in battles)
                        RandomizeBattle(battle, pokemonSet, trainerSpeciesSettings);
                }
                else if (settings.RivalSetting == Settings.TrainerOption.Procedural)
                {
                    RandomizeBattle(battles[0], pokemonSet, trainerSpeciesSettings);
                    PcgBattles(battles, battles[0].pokemon.Select((p) => p.species), pokemonSet, trainerSpeciesSettings);
                }
                
            }

            #endregion

            #region Ace Trainers

            var aceTrainerSpeciesSettings = settings.GetSpeciesSettings("aceTrainer");
            foreach (var trainer in aceTrainers)
            {
                // Ace Trainers are as strong as rivals for now
                RandomizeBattle(trainer, pokemonSet, aceTrainerSpeciesSettings);
            }

            #endregion

            #endregion

            #endregion

            #region Field Items (NOTHING YET)
            // Mutate Field Items
            #endregion

            #region Misc

            // Randomize PC starting item
            if (settings.RandomizePcPotion)
            {
                data.PcStartItem = rand.Choice(EnumUtils.GetValues<Item>());
            }
           
            #endregion

            data.CalculateMetrics();
            return data;
        }

        /// <summary> Define and return the set of valid pokemon (with applicable restrictions)</summary>
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
        /// <summary> Define and return the set of valid types (with applicable restrictions)</summary> 
        private HashSet<PokemonType> DefinePokemonTypes()
        {
            HashSet<PokemonType> types = EnumUtils.GetValues<PokemonType>().ToHashSet();
            // Remove the FAIRY type if we are Gen V or below and have not enabled the add fairy type hack
            if (data.Gen < RomData.Generation.VI && !settings.AddFairyType)
                types.Remove(PokemonType.FAI);
            return types;
        }

        /// <summary> Chose a random type based on the given metric (OUT OF DATE)</summary> 
        private PokemonType RandomType(RomMetrics metrics, string metric)
        {
            switch (metric)
            {
                case "None":
                    return rand.Choice(metrics.TypeRatiosAll.Items);
                case "Type Occurence (Any)":
                    return rand.Choice(metrics.TypeRatiosAll);
                case "Type Occurence (Single)":
                    return rand.Choice(metrics.TypeRatiosSingle);
                case "Type Occurence (Primary)":
                    return rand.Choice(metrics.TypeRatiosDualPrimary);
                case "Type Occurence (Secondary)":
                    return rand.Choice(metrics.TypeRatiosDualSecondary);
                default:
                    throw new System.NotImplementedException(metric + " is not a valid metric.");
            }
        }
        /// <summary> Get a wieghted and culled list of possible pokemon</summary>
        private WeightedSet<PokemonSpecies> SpeciesWeightedSet(IEnumerable<PokemonSpecies> possiblePokemon, PokemonSpecies pokemon, Settings.SpeciesSettings speciesSettings)
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
            return combinedWeightings;
        }
        /// <summary> Chose a random species from the input set based on the given species settings</summary> 
        private PokemonSpecies RandomSpecies(IEnumerable<PokemonSpecies> possiblePokemon, PokemonSpecies pokemon, Settings.SpeciesSettings speciesSettings)
        {
            var combinedWeightings = SpeciesWeightedSet(possiblePokemon, pokemon, speciesSettings);
            // Actually choose the species
            return rand.Choice(combinedWeightings);
        }
        /// <summary> Chose a random species from the input set based on the given species settings.
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
                newSpecies = rand.Choice(evolvesFrom).Pokemon;
                evolvesFrom = data.PokemonLookup[newSpecies].evolvesFrom;
            }
            return newSpecies;
        }
        /// <summary> returns false if the pokemon is an invalid level 
        /// (due to not being high enough level to evolve to the current species) </summary>
        private bool IsPokemonValidLevel(List<Evolution> evolvesFrom, int level, Settings.SpeciesSettings speciesSettings)
        {
            if (evolvesFrom.Count == 0) // basic pokemon
                return true;
            // Is there at least one valid evolution
            foreach (var evo in evolvesFrom)
                if (EquivalentLevelReq(evo, speciesSettings) <= level)
                    return true;
            return false;
        }
        /// <summary> Returns the equivalent required level of an evolution (including non-leveling evolutions if applicable) </summary>
        private int EquivalentLevelReq(Evolution evo, Settings.SpeciesSettings speciesSettings)
        {
            if (evo.EvolvesByLevel)
                return evo.parameter;
            else if (!speciesSettings.SetLevelsOnArtificialEvos)
                return 0;
            else if (evo.EvolvesByTrade)
                return speciesSettings.TradeEvolutionLevel;
            else if (evo.Type == EvolutionType.UseItem)
                return speciesSettings.ItemEvolutionLevel;
            else if (evo.Type == EvolutionType.Beauty)
                return speciesSettings.BeautyEvolutionLevel;
            else
                return speciesSettings.FriendshipEvolutionLevel;
        }
        /// <summary> Return 3 pokemon that form a valid type traingle, or null if none exist in the input set 
        /// Currently type triangles just requires one-way weakness, but allow neutral relations </summary>
        private List<PokemonSpecies> RandomTypeTriangle(IEnumerable<PokemonSpecies> possiblePokemon, Settings.SpeciesSettings speciesSettings)
        {
            var set = SpeciesWeightedSet(possiblePokemon, data.Starters[0], speciesSettings);
            if(speciesSettings.DisableIllegalEvolutions)
                set.RemoveWhere((p) => !IsPokemonValidLevel(data.PokemonLookup[p].evolvesFrom, 5, speciesSettings));
            var pool = new WeightedSet<PokemonSpecies>(set);
            while(pool.Count > 0)
            {
                var first = rand.Choice(pool);
                pool.Remove(first);
                // Get potential second pokemon
                var secondPossiblities = SpeciesWeightedSet(set.Items, data.Starters[1], speciesSettings);
                secondPossiblities.RemoveWhere((p) => !OneWayWeakness(first, p));
                // Finish the traiangle if possible
                var triangle = FinishTriangle(set, secondPossiblities, first, speciesSettings);
                if (triangle != null)
                    return triangle;

            }
            return null; // No viable triangle with input spcifications
        }
        /// <summary> Helper method for the RandomTypeTriangle method </summary>
        private List<PokemonSpecies> FinishTriangle(WeightedSet<PokemonSpecies> set, WeightedSet<PokemonSpecies> possibleSeconds, PokemonSpecies first, Settings.SpeciesSettings speciesSettings)
        {
            while (possibleSeconds.Count > 0)
            {
                var second = rand.Choice(possibleSeconds);
                possibleSeconds.Remove(second);
                // Get third pokemon
                var thirdPossiblities = SpeciesWeightedSet(set.Items, data.Starters[2], speciesSettings);
                thirdPossiblities.RemoveWhere((p) => !(OneWayWeakness(second, p) && OneWayWeakness(p, first)));
                // If at least one works, choose one randomly
                if (thirdPossiblities.Count > 0)
                    return new List<PokemonSpecies> { first, second, rand.Choice(thirdPossiblities) };
            }
            return null;
        }

        /// <summary> Return true if b is weak to a AND a is not weak to b </summary>
        private bool OneWayWeakness(PokemonSpecies a, PokemonSpecies b)
        {
            var aTypes = data.PokemonLookup[a].types;
            var bTypes = data.PokemonLookup[b].types;
            var aVsB = data.TypeDefinitions.GetEffectiveness(aTypes[0], aTypes[1], bTypes[0], bTypes[1]);
            var bVsA = data.TypeDefinitions.GetEffectiveness(bTypes[0], bTypes[1], aTypes[0], aTypes[1]);
            return aVsB == TypeEffectiveness.SuperEffective ? !(bVsA == TypeEffectiveness.SuperEffective) : false;
        }
        /// <summary> return true if pokemon a evolves into or from pokemon b or IS pokemon b</summary>
        private bool RelatedToOrSelf(PokemonSpecies a, PokemonSpecies b)
        {
            return (a == b) || RelatedTo(a,b);
        }
        /// <summary> return true if pokemon a evolves into or from pokemon b</summary>
        private bool RelatedTo(PokemonSpecies a, PokemonSpecies b)
        {
            return EvolvesInto(a, b) || EvolvesFrom(a, b);
        }
        /// <summary> return true if pokemon a evolves into pokemon b</summary>
        private bool EvolvesInto(PokemonSpecies a, PokemonSpecies b)
        {
            var stats = data.PokemonLookup[a];
            var evos = stats.evolvesTo;
            foreach(var evo in evos)
            {
                if (evo.Type == EvolutionType.None)
                    continue;
                if (evo.Pokemon == b)
                    return true;
                if (EvolvesInto(evo.Pokemon, b))
                    return true;
            }
            return false;
        }
        /// <summary> return true if pokemon a evolves from pokemon b</summary>
        private bool EvolvesFrom(PokemonSpecies a, PokemonSpecies b)
        {
            var stats = data.PokemonLookup[a];
            var evos = stats.evolvesFrom;
            foreach (var evo in evos)
            {
                if (evo.Type == EvolutionType.None)
                    continue;
                if (evo.Pokemon == b)
                    return true;
                if (EvolvesFrom(evo.Pokemon, b))
                    return true;
            }
            return false;
        }
        /// <summary> Radnomize the given trainer encounter </summary>
        private void RandomizeBattle(Trainer trainer, IEnumerable<PokemonSpecies> pokemonSet, Settings.SpeciesSettings speciesSettings)
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
                pokemon.species = RandomSpecies(pokemonSet, pokemon.species, pokemon.level, speciesSettings);
                // Reset special moves if necessary
                if (pokemon.HasSpecialMoves)
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
        /// <summary> Procedurally generate a sequence of battles from a given team seed and a list of template battles </summary>
        private void PcgBattles(IEnumerable<Trainer> battles, IEnumerable<PokemonSpecies> seed, IEnumerable<PokemonSpecies> pokemonSet, Settings.SpeciesSettings speciesSettings)
        {
            var team = new List<PokemonSpecies>(seed);
            foreach(var battle in battles)
            {
                RandomizeBattle(battle, pokemonSet, speciesSettings);
                for(int i = 0; i < battle.pokemon.Length; ++i)
                {
                    // Go from the back of the battle so the ace is last
                    int j = battle.pokemon.Length - (i + 1);
                    if (i < team.Count)
                    {
                        var pokemon = battle.pokemon[j];
                        pokemon.species = MaxEvolution(team[i], pokemon.level, speciesSettings);
                        if (pokemon.HasSpecialMoves)
                        {
                            pokemon.moves = MovesetGenerator.DefaultMoveset(data.PokemonLookup[pokemon.species], pokemon.level);
                        }
                    }                       
                    else
                        team.Add(battle.pokemon[j].species);
                }
            }
        }
        /// <summary> Return the maximum evolved form of the pokemon at the given level
        /// returns a lower form if the pokemon is an invalid level 
        /// returns a random branch for evolution trees that branch </summary>
        private PokemonSpecies MaxEvolution(PokemonSpecies p, int level, Settings.SpeciesSettings speciesSettings)
        {
            var stats = data.PokemonLookup[p];
            if (!IsPokemonValidLevel(stats.evolvesFrom, level, speciesSettings))
                return CorrectImpossibleEvo(p, level, speciesSettings);
            var evos = stats.evolvesTo.Where((evo) => evo.Type != EvolutionType.None && EquivalentLevelReq(evo, speciesSettings) <= level);
            while(evos.Count() > 0)
            {
                stats = data.PokemonLookup[rand.Choice(evos).Pokemon];
                evos = stats.evolvesTo.Where((evo) => evo.Type != EvolutionType.None && EquivalentLevelReq(evo, speciesSettings) <= level);
            }
            return stats.species;
        }
    }
}
