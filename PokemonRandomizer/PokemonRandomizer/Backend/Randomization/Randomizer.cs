using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace PokemonRandomizer.Backend.Randomization
{
    // This class does the actualmutation and randomizing by creating a mutated copy
    // of the original ROM data
    public class Randomizer
    {
        private readonly RomData data;
        private readonly Settings settings;
        private readonly Random rand;
        private Dictionary<PokemonSpecies, float> powerScores;
        /// <summary>
        /// Create a new randomizer with given data and settings
        /// Input data will be mutated by randomizer calls
        /// </summary>
        public Randomizer(RomData data, Settings settings)
        {
            this.data = data;
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
            var items = DefineItemSet();

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
                        data.TypeDefinitions.Set(type, PokemonType.Unknown, te, (type == PokemonType.NRM || type == PokemonType.FTG) && te == TypeEffectiveness.NoEffect);
                    // Type effectiveness of ??? vs other type
                    te = rand.Choice(data.Metrics.TypeEffectivenessRatios);
                    if (te != TypeEffectiveness.Normal) // Only register if not normal effectiveness
                        data.TypeDefinitions.Set(PokemonType.Unknown, type, te, type == PokemonType.GHO);
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
            // Ideas: Strange power: 30-90 power, ??? random target (higher chance of weird targets)
            // Ideas: Enigma: 1-150 power, ???, low chance of any bonus effect
            // Mutate move definitions (should this come before or after hacks (maybe let user choose))
            // Change move type, power, etc. (this would be really lame if not at a low mutation rate)
            #endregion

            #region TMs, HMs, and Move Tutor Move Mappings
            var moves = EnumUtils.GetValues<Move>().ToHashSet();
            moves.Remove(Move.None); // Remove none as a possible choice
            // Remove HM moves if applicable
            if(settings.PreventHmMovesInTMsAndMoveTutors)
                foreach (var move in data.HMMoves)
                    moves.Remove(move);
            // Randomize TM mappings
            for(int i = 0; i < data.TMMoves.Length; ++i)
            {
                if (rand.RandomDouble() < settings.TMRandChance)
                    data.TMMoves[i] = rand.Choice(moves);
                if (settings.PreventDuplicateTMsAndMoveTutors)
                    moves.Remove(data.TMMoves[i]);
            }
            // Randomize Move Tutor mappings
            for (int i = 0; i < data.tutorMoves.Length; ++i)
            {
                if (rand.RandomDouble() < settings.MoveTutorRandChance)
                    data.tutorMoves[i] = rand.Choice(moves);
                if (settings.PreventDuplicateTMsAndMoveTutors)
                    moves.Remove(data.tutorMoves[i]);
            }
            #endregion

            #region Item Definitions (Nothing Yet)

            // Prepare Item Remap Dictionary
            // Find blank item entries with effect
            // Define Item Definitions
            // Hack in new items if applicable
            // Possible Hacks: Add GenIV items (some might not be possible), add fairy-related items
            // Mutate item definitions

            #endregion

            #region Pokemon Base Attributes
            var availableAddMoves = EnumUtils.GetValues<Move>().ToHashSet();
            availableAddMoves.Remove(Move.None);
            if(settings.BanSelfdestruct)
            {
                availableAddMoves.Remove(Move.SELFDESTRUCT);
                availableAddMoves.Remove(Move.EXPLOSION);
            }
            if(settings.DisableAddingHmMoves)
            {
                foreach (var move in data.HMMoves)
                    availableAddMoves.Remove(move);
            }

            // Mutate Pokemon
            foreach (PokemonBaseStats pokemon in data.Pokemon)
            {
                #region Evolutions
                // Fix Impossible Evolutions
                if(settings.FixImpossibleEvos)
                {
                    // Make a non-levelup evoltion into a level up one. Resolves some conflicts but not all
                    void MakeEvolutionByLevelUp(Evolution evo)
                    {
                        var evolveByLevelUp = pokemon.evolvesTo.FirstOrDefault((e) => e.Type == EvolutionType.LevelUp);
                        if (evolveByLevelUp == null)
                        {
                            evo.parameter = EquivalentLevelReq(evo, pokemon) + rand.RandomGaussianInt(0, settings.ImpossibleEvoLevelStandardDev);
                            evo.Type = EvolutionType.LevelUp;
                        }
                        else
                        {
                            evolveByLevelUp.Type = EvolutionType.LevelUpWithPersonality1;
                            evo.Type = EvolutionType.LevelUpWithPersonality2;
                            evo.parameter = evolveByLevelUp.parameter;
                        }
                    }
                    foreach (var evo in pokemon.evolvesTo)
                    {
                        if (evo.Type == EvolutionType.Trade)
                        {
                            MakeEvolutionByLevelUp(evo);
                        }
                        else if (evo.Type == EvolutionType.TradeWithItem)
                        {
                            if (settings.TradeItemEvoSetting == Settings.TradeItemPokemonOption.UseItem)
                            {
                                evo.Type = EvolutionType.UseItem;
                                // Log this as a new evolution stone if necessary
                                Item item = (Item)evo.parameter;
                                if (!data.NewEvolutionStones.Contains(item))
                                    data.NewEvolutionStones.Add(item);
                            }
                            else // Use level up settings
                            {
                                MakeEvolutionByLevelUp(evo);
                            }
                        }
                    }
                }
                foreach (var evo in pokemon.evolvesTo)
                {
                    if (evo.Type == EvolutionType.None)
                        continue;

                    #region Dunsparse Plague
                    if (rand.RollSuccess(settings.DunsparsePlaugeChance))
                    {
                        static int FirstEmptyEvo(Evolution[] evolutions)
                        {
                            for (int i = 0; i < evolutions.Length; i++)
                            {
                                if (evolutions[i].Pokemon == PokemonSpecies.None)
                                    return i;
                            }
                            return -1;
                        }
                        // Add the plague
                        if(evo.Type == EvolutionType.LevelUp)
                        {
                            evo.Type = EvolutionType.LevelUpWithPersonality1;
                            int index = FirstEmptyEvo(pokemon.evolvesTo);
                            if(index >= 0)
                            {
                                pokemon.evolvesTo[index].Pokemon = PokemonSpecies.DUNSPARCE;
                                pokemon.evolvesTo[index].Type = EvolutionType.LevelUpWithPersonality2;
                                pokemon.evolvesTo[index].parameter = evo.parameter;
                            }
                        }
                        else if(evo.Type == EvolutionType.Friendship)
                        {
                            evo.Type = rand.RandomDouble() < 0.5 ? EvolutionType.FriendshipDay : EvolutionType.FriendshipNight;
                            int index = FirstEmptyEvo(pokemon.evolvesTo);
                            if (index >= 0)
                            {
                                pokemon.evolvesTo[index].Pokemon = PokemonSpecies.DUNSPARCE;
                                pokemon.evolvesTo[index].Type = evo.Type == EvolutionType.FriendshipDay ? EvolutionType.FriendshipNight : EvolutionType.FriendshipDay;
                                pokemon.evolvesTo[index].parameter = evo.parameter;
                            }
                        }
                    }
                    #endregion
                }
                #endregion
                // Mutate low-consequence base stats

                #region Types
                // Mutate Pokemon Type
                if (pokemon.IsSingleTyped)
                {
                    if (rand.RandomDouble() < settings.SingleTypeRandChance)
                        pokemon.types[0] = pokemon.types[1] = rand.Choice(data.Metrics.TypeRatiosSingle);
                }
                else
                {
                    if (rand.RandomDouble() < settings.DualTypePrimaryRandChance)
                        pokemon.types[0] = rand.Choice(data.Metrics.TypeRatiosDualPrimary);
                    if (rand.RandomDouble() < settings.DualTypeSecondaryRandChance)
                        pokemon.types[1] = rand.Choice(data.Metrics.TypeRatiosDualSecondary);
                }
                #endregion

                // Mutate battle stats and EVs
                // Mutate Learn Sets
                #region Learn Sets
                if(settings.BanSelfdestruct)
                {
                    pokemon.learnSet.RemoveWhere((m) => m.move == Move.SELFDESTRUCT || m.move == Move.EXPLOSION);
                }
                if(settings.AddMoves && pokemon.IsBasic && rand.RandomDouble() < settings.AddMovesChance)
                {
                    int numMoves = rand.RandomGaussianPositiveNonZeroInt(settings.NumMovesMean, settings.NumMovesStdDeviation);
                    var availableMoves = new List<Move>(availableAddMoves);
                    availableMoves.RemoveAll((m) => pokemon.learnSet.Learns(m));
                    var availableEggMoves = pokemon.eggMoves.Where((m) => availableMoves.Contains(m)).ToList();
                    for (int i = 0; i < numMoves; ++i)
                    {
                        Move move = Move.None;
                        switch (rand.Choice(settings.AddMoveSourceWieghts))
                        {
                            case Settings.AddMoveSource.Random:
                                move = rand.Choice(availableMoves);
                                break;
                            case Settings.AddMoveSource.Damaging:
                                break;
                            case Settings.AddMoveSource.Status:
                                break;
                            case Settings.AddMoveSource.STAB:
                                break;
                            case Settings.AddMoveSource.STABDamaging:
                                break;
                            case Settings.AddMoveSource.STABStatus:
                                break;
                            case Settings.AddMoveSource.EggMoves:
                                if (availableEggMoves.Count > 0)
                                {
                                    move = rand.Choice(availableEggMoves);
                                    availableEggMoves.Remove(move);
                                }
                                break;
                            case Settings.AddMoveSource.CompatibleTms:
                                break;
                        }
                        if(move != Move.None)
                        {
                            void AddMoveToEvoTreeMoveSet(PokemonBaseStats p, Move m, int level, int creep = 0)
                            {
                                p.learnSet.Add(m, level);
                                foreach(var evo in p.evolvesTo)
                                {
                                    if ((int)evo.Pokemon == 0)
                                        return;
                                    // Make this stable with the Dunsparse Plague
                                    if (evo.Pokemon == PokemonSpecies.DUNSPARCE && settings.DunsparsePlaugeChance > 0)
                                        continue;
                                    AddMoveToEvoTreeMoveSet(data.PokemonLookup[evo.Pokemon], m, level + creep, creep);
                                }
                            }
                            availableMoves.Remove(move);
                            if(data.Metrics.LearnLevels.ContainsKey(move))
                            {
                                double mean = data.Metrics.LearnLevelMeans[move];
                                double stdDev = data.Metrics.LearnLevelStandardDeviations[move];
                                int learnLevel = rand.RandomGaussianPositiveNonZeroInt(mean, stdDev);
                                AddMoveToEvoTreeMoveSet(pokemon, move, learnLevel);
                                continue;
                            }
                            int effectivePower = data.MoveData[(int)move].EffectivePower;
                            if(data.Metrics.LearnLevelPowers.ContainsKey(effectivePower))
                            {
                                double mean = data.Metrics.LearnLevelPowerMeans[effectivePower];
                                double stdDev = data.Metrics.LearnLevelPowerStandardDeviations[effectivePower];
                                int learnLevel = rand.RandomGaussianPositiveNonZeroInt(mean, stdDev);
                                AddMoveToEvoTreeMoveSet(pokemon, move, learnLevel);
                            }
                            else if(data.Metrics.LearnLevelPowers.Count > 0)
                            {
                                var powers = data.Metrics.LearnLevelPowers.Keys.ToList();
                                powers.Sort();
                                int closestPower = 0;
                                int last = 0;
                                foreach(var power in powers)
                                {
                                    if(power > effectivePower)
                                    {
                                        closestPower = effectivePower - last > power - effectivePower ? power : last;
                                        break;
                                    }
                                    last = power;
                                }
                                double mean = data.Metrics.LearnLevelPowerMeans[closestPower];
                                double stdDev = data.Metrics.LearnLevelPowerStandardDeviations[closestPower];
                                int learnLevel = rand.RandomGaussianPositiveNonZeroInt(mean, stdDev);
                                AddMoveToEvoTreeMoveSet(pokemon, move, learnLevel);
                            }
                        }
                    }
                }
                #endregion

                #region TM, HM, and Move tutor Compatibility
                if (settings.TmMtCompatSetting != Settings.TmMtCompatOption.Unchanged)
                {
                    if (settings.TmMtCompatSetting == Settings.TmMtCompatOption.AllOn)
                    {
                        pokemon.TMCompat.SetAll(true);
                        pokemon.moveTutorCompat.SetAll(true);
                        for (int i = 0; i < pokemon.TMCompat.Length; ++i)
                        {
                            pokemon.TMCompat[i] = true;
                        }
                        for (int i = 0; i < pokemon.moveTutorCompat.Length; ++i)
                        {
                            pokemon.moveTutorCompat[i] = true;
                        }
                    }
                    else if (settings.TmMtCompatSetting == Settings.TmMtCompatOption.Random)
                    {
                        for (int i = 0; i < pokemon.TMCompat.Length; ++i)
                        {
                            pokemon.TMCompat[i] = rand.RandomDouble() < settings.TmMtTrueChance;
                        }
                        for (int i = 0; i < pokemon.moveTutorCompat.Length; ++i)
                        {
                            pokemon.moveTutorCompat[i] = rand.RandomDouble() < settings.TmMtTrueChance;
                        }
                    }
                    else if (settings.TmMtCompatSetting == Settings.TmMtCompatOption.RandomKeepNumber)
                    {
                        static void RandomKeepNumber(BitArray arr, Random rand)
                        {
                            int compatCount = 0;
                            // Find the number of trues
                            foreach (bool b in arr)
                            {
                                if (b)
                                {
                                    ++compatCount;
                                }
                            }
                            // Wipe the compatibility array
                            arr.SetAll(false);
                            var choices = Enumerable.Range(0, arr.Length).ToList();
                            for (int i = 0; i < compatCount; ++i)
                            {
                                int choice = rand.Choice(choices);
                                arr.Set(choice, true);
                                choices.Remove(choice);
                            }
                        }
                        RandomKeepNumber(pokemon.TMCompat, rand);
                        RandomKeepNumber(pokemon.moveTutorCompat, rand);
                    }
                    else if (settings.TmMtCompatSetting == Settings.TmMtCompatOption.Intelligent)
                    {
                        void SetCompatIntelligent(BitArray arr, int ind, Move[] moveList)
                        {
                            var moveData = data.MoveData[(int)moveList[ind]];
                            if (pokemon.types.Contains(moveData.type))
                                arr[ind] = true;
                            else if (pokemon.learnSet.Any((l) => l.move == moveData.move))
                                arr[ind] = true;
                            else if (pokemon.eggMoves.Contains(moveData.move))
                                arr[ind] = true;
                            else if (moveData.type == PokemonType.NRM)
                                arr[ind] = rand.RandomDouble() < settings.TmMtTrueChance;
                            else
                                arr[ind] = rand.RandomDouble() < settings.TmMtNoise;
                        }
                        for (int i = 0; i < pokemon.TMCompat.Length; ++i)
                        {
                            SetCompatIntelligent(pokemon.TMCompat, i, data.TMMoves);
                        }
                        for (int i = 0; i < pokemon.moveTutorCompat.Length; ++i)
                        {
                            SetCompatIntelligent(pokemon.moveTutorCompat, i, data.tutorMoves);
                        }
                    }
                }
                #endregion

                #region Catch Rates
                if(settings.CatchRateSetting != Settings.CatchRateOption.Unchanged)
                {
                    // Do not change if KeepLegendaryCatchRates is on AND this pokemon is a legendary
                    if(!settings.KeepLegendaryCatchRates || !pokemon.IsLegendary)
                    {
                        if (settings.CatchRateSetting == Settings.CatchRateOption.CompletelyRandom)
                            pokemon.catchRate = rand.RandomByte();
                        else if (settings.CatchRateSetting == Settings.CatchRateOption.AllEasiest)
                            pokemon.catchRate = byte.MaxValue;
                        else if (settings.CatchRateSetting == Settings.CatchRateOption.Constant)
                            pokemon.catchRate = settings.CatchRateConstant;
                        else // Intelligent
                        {
                            // Basic pokemon (or pokemon with only a baby evolution)
                            if (pokemon.evolvesFrom.Count == 0 || pokemon.IsBasicOrEvolvesFromBaby)
                            {
                                if (pokemon.catchRate < settings.IntelligentCatchRateBasicThreshold)
                                    pokemon.catchRate = settings.IntelligentCatchRateBasicThreshold;
                            }
                            else // Evolved pokemon
                            {
                                if (pokemon.catchRate < settings.IntelligentCatchRateEvolvedThreshold)
                                    pokemon.catchRate = settings.IntelligentCatchRateEvolvedThreshold;
                            }
                        }
                    }
                }
                #endregion
            }

            // Set unknown typing if selected
            if (settings.OverrideUnknownType)
            {
                var unknownPokeData = data.PokemonLookup[PokemonSpecies.UNOWN];
                unknownPokeData.types[0] = PokemonType.Unknown;
                if (rand.RandomDouble() < settings.UnknownDualTypeChance)
                    unknownPokeData.types[1] = rand.Choice(data.Metrics.TypeRatiosDualSecondary);
                else
                    unknownPokeData.types[1] = PokemonType.Unknown;
            }
            // Change pallettes to fit new types
            // Recalculate power scoring
            powerScores = PowerScaling.Calculate(data.Pokemon, settings.TieringOptions);
            #endregion

            #region Starters
            if (settings.StarterSetting != Settings.StarterPokemonOption.Unchanged)
            {
                var speciesSettings = settings.StarterSpeciesSettings;
                if(settings.StarterSetting == Settings.StarterPokemonOption.Random)
                {
                    for(int i = 0; i < data.Starters.Count; ++i)
                        data.Starters[i] = RandomSpecies(pokemonSet, data.Starters[i], 5, speciesSettings);
                }
                else if (settings.StarterSetting == Settings.StarterPokemonOption.RandomTypeTriangle)
                {
                    var triangle = RandomTypeTriangle(pokemonSet, speciesSettings, settings.StrongStarterTypeTriangle);
                    if (triangle != null)
                        data.Starters = triangle;
                    else // Fall back on completely random
                    {
                        for (int i = 0; i < data.Starters.Count; ++i)
                            data.Starters[i] = RandomSpecies(pokemonSet, data.Starters[i], 5, speciesSettings);
                    }
                }
                else if(settings.StarterSetting == Settings.StarterPokemonOption.Custom)
                {
                    data.Starters = new List<PokemonSpecies>(settings.CustomStarters);
                }
                // Make sure all starters have attack moves
                if(settings.SafeStarterMovesets)
                {
                    // Hacky tackle fix
                    foreach (var pkmn in data.Starters)
                        if (data.PokemonLookup[pkmn].learnSet[0].move != Move.TACKLE)
                            data.PokemonLookup[pkmn].learnSet.Add(Move.TACKLE, 1);
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

            #region Maps

            // Mutate Maps (currently just iterate though the maps, but may want to construct and traverse a graph later)
            foreach(var map in data.Maps)
            {
                // If the map names is empty, just continue
                if (string.IsNullOrEmpty(map.Name))
                    continue;
                // Mutate Maps
                // Mutate Weather
                RandomizeWeather(map, settings);
                // Mutate tiles and layout (if applicable)
                // Team magma/aqua/rocket takeover mode?
                // Set metadata (environent, etc)
                // Mutate Trainers (should be per map later?)
                // Set Trainer positions (and some base tags maybe) and add scripts
                // Sleeper agents? (Random non-npc trainers become trainers)
                // Set tags (gym trainer, gym leader, elite 4, rival, reoccuring, etc)
                // Set class
                // Natural trainers? (trainer types are based on environment type)
                foreach(var sEvent in map.eventData.signEvents)
                {
                    if(sEvent.IsHiddenItem)
                    {
                        //sEvent.hiddenItem = RandomItem(items, sEvent.hiddenItem, settings.PcItemSettings);
                    }
                }
                //Mutate battle here later?
            }
            #endregion

            #region Wild Pokemon (may happen during maps later)

            if (settings.WildPokemonSetting == Settings.WildPokemonOption.Individual)
            {
                // Get the species randomization settings for wild pokemon
                var wildSpeciesSettings = settings.WildSpeciesSettings;
                if (wildSpeciesSettings.WeightType == Settings.SpeciesSettings.WeightingType.Group)
                {
                    foreach (var encounterSet in data.Encounters)
                    {
                        var typeSample = encounterSet.Select((e) => e.pokemon).ToArray(); //.Distinct();
                        foreach (var enc in encounterSet)
                        {
                            enc.pokemon = RandomSpeciesTypeGroup(pokemonSet, enc.pokemon, enc.level, typeSample, wildSpeciesSettings);
                        }
                    }
                }
                else // Individual Weight Type
                {
                    foreach (var encounterSet in data.Encounters)
                    {
                        foreach (var enc in encounterSet)
                        {
                            enc.pokemon = RandomSpecies(pokemonSet, enc.pokemon, enc.level, wildSpeciesSettings);
                        }
                    }
                }

            }
            else if(settings.WildPokemonSetting == Settings.WildPokemonOption.AreaOneToOne)
            {
                // Get the species randomization settings for wild pokemon
                var wildSpeciesSettings = settings.WildSpeciesSettings;
                foreach (var encounterSet in data.Encounters)
                {
                    var typeSample = encounterSet.Select((e) => e.pokemon).ToArray(); //.Distinct();
                    // Get all unique species in the encounter set
                    var species = encounterSet.Select((e) => e.pokemon).Distinct();
                    var mapping = new Dictionary<PokemonSpecies, PokemonSpecies>();
                    if(wildSpeciesSettings.WeightType == Settings.SpeciesSettings.WeightingType.Group)
                    {
                        foreach (var s in species)
                            mapping.Add(s, RandomSpeciesTypeGroup(pokemonSet, s, typeSample, wildSpeciesSettings));
                    }
                    else // Weight type is individual
                    {
                        foreach (var s in species)
                            mapping.Add(s, RandomSpecies(pokemonSet, s, wildSpeciesSettings));
                    }
                    foreach (var enc in encounterSet)
                    {
                        enc.pokemon = mapping[enc.pokemon];
                        if (wildSpeciesSettings.RestrictIllegalEvolutions)
                            enc.pokemon = CorrectImpossibleEvo(enc.pokemon, enc.level);
                    }
                }
            }
            else if(settings.WildPokemonSetting == Settings.WildPokemonOption.GlobalOneToOne)
            {
                // Get the species randomization settings for wild pokemon
                var wildSpeciesSettings = settings.WildSpeciesSettings;
                var mapping = new Dictionary<PokemonSpecies, PokemonSpecies>();
                foreach (var s in pokemonSet)
                    mapping.Add(s, RandomSpecies(pokemonSet, s, wildSpeciesSettings));
                foreach (var encounterSet in data.Encounters)
                {
                    foreach (var enc in encounterSet)
                    {
                        enc.pokemon = mapping[enc.pokemon];
                        if (wildSpeciesSettings.RestrictIllegalEvolutions)
                        {
                            enc.pokemon = CorrectImpossibleEvo(enc.pokemon, enc.level);
                        }
                    }
                }
            }

            #endregion

            #region Trainer Battles

            var trainerSpeciesSettings = settings.GetTrainerSettings(Settings.TrainerCategory.Trainer);
            // Randomize Normal Trainers
            foreach (var trainer in data.NormalTrainers.Values)
            {
                RandomizeTrainer(trainer, pokemonSet, trainerSpeciesSettings);
            }
            // Randomize Team Grunts (considered normal trainers currently)
            foreach (var trainer in data.GruntBattles)
            {
                RandomizeTrainer(trainer, pokemonSet, trainerSpeciesSettings);
            }

            #region Special Trainers

            #region Rivals

            var rivalSettings = settings.GetTrainerSettings(Settings.TrainerCategory.Rival);
            foreach (var trainer in data.RivalNames)
            {
                var allBattles = data.SpecialTrainers[trainer.ToLower()];
                allBattles.Sort((a, b) => a.AvgLvl.CompareTo(b.AvgLvl));
                // Split the rival battles into their three different options
                var starters = new PokemonSpecies[] { allBattles[0].pokemon[0].species, allBattles[1].pokemon[0].species, allBattles[2].pokemon[0].species };
                var rivalBattles = starters.Select((s) => allBattles.Where((b) => b.pokemon.Any((p) => RelatedToOrSelf(p.species, s)))).ToArray();
                for (int i = 0; i < rivalBattles.Length; ++i)
                {
                    var battles = new List<Trainer>(rivalBattles[i]);
                    var firstBattle = battles[0];
                    battles.RemoveAt(0);
                    // Randomize the first battle
                    RandomizeTrainer(firstBattle, pokemonSet, rivalSettings, false);
                    // Set the appropriate starter as the ace
                    firstBattle.pokemon[firstBattle.pokemon.Length - 1].species = data.Starters[data.RivalRemap[i]];
                    // Procedurally generate the rest of the battles
                    RandomizeTrainerReoccurring(firstBattle, battles, pokemonSet, rivalSettings);
                }
            }

            #endregion

            #region Wally (TODO, make either gen-specific without metadata, or make gen-agnostic)

            const string wallyName = "wally";
            if(data.SpecialTrainers.ContainsKey(wallyName))
            {
                // Randomize Wally starter if applicable
                if (settings.RandomizeWallyAce)
                    data.CatchingTutPokemon = RandomSpecies(pokemonSet, data.CatchingTutPokemon, 5, rivalSettings.SpeciesSettings);
                var wallyBattles = new List<Trainer>(data.SpecialTrainers[wallyName]);
                wallyBattles.Sort((a, b) => a.AvgLvl.CompareTo(b.AvgLvl));
                var firstBattle = wallyBattles[0];
                wallyBattles.RemoveAt(0);
                // Set Wally's first pokemon to the catching tut pokemon
                RandomizeTrainer(firstBattle, pokemonSet, rivalSettings, false);
                var firstBattleAce = firstBattle.pokemon[firstBattle.pokemon.Length - 1];
                firstBattleAce.species = MaxEvolution(data.CatchingTutPokemon, firstBattleAce.level, rivalSettings.SpeciesSettings.RestrictIllegalEvolutions);
                // Procedurally generate the rest of Wally's battles
                RandomizeTrainerReoccurring(firstBattle, wallyBattles, pokemonSet, rivalSettings);
            }

            #endregion

            void SpecialTrainerRandomization(IEnumerable<string> names, Settings.TrainerSettings settings)
            {
                foreach(var name in names)
                {
                    var reoccuringBattles = new List<Trainer>(data.SpecialTrainers[name.ToLower()]);
                    reoccuringBattles.Sort((a, b) => a.AvgLvl.CompareTo(b.AvgLvl));
                    var firstBattle = reoccuringBattles[0];
                    reoccuringBattles.RemoveAt(0);
                    RandomizeTrainer(firstBattle, pokemonSet, settings, false);
                    RandomizeTrainerReoccurring(firstBattle, reoccuringBattles, pokemonSet, settings);
                }
            }

            // Ubers use champion settings for now
            SpecialTrainerRandomization(data.UberNames, settings.GetTrainerSettings(Settings.TrainerCategory.Champion));
            SpecialTrainerRandomization(data.ChampionNames, settings.GetTrainerSettings(Settings.TrainerCategory.Champion));
            SpecialTrainerRandomization(data.EliteFourNames, settings.GetTrainerSettings(Settings.TrainerCategory.EliteFour));
            SpecialTrainerRandomization(data.GymLeaderNames, settings.GetTrainerSettings(Settings.TrainerCategory.GymLeader));
            // Team Leaders use the same settings as gym leaders for now
            SpecialTrainerRandomization(data.TeamLeaderNames, settings.GetTrainerSettings(Settings.TrainerCategory.GymLeader));
            // Team Admins use the same settings as team leaders for now
            SpecialTrainerRandomization(data.TeamAdminNames, settings.GetTrainerSettings(Settings.TrainerCategory.GymLeader));
            SpecialTrainerRandomization(data.AceTrainerNames, settings.GetTrainerSettings(Settings.TrainerCategory.AceTrainer));
            // Reoccurring Trainers use normal trainer settings
            SpecialTrainerRandomization(data.ReoccuringTrainerNames, settings.GetTrainerSettings(Settings.TrainerCategory.Trainer));

            #endregion

            #endregion

            #region Items (NOTHING YET)
            // Mutate Field Items
            #endregion

            #region Misc

            // Randomize PC starting item
            if (settings.PcPotionOption == Settings.PcItemOption.Random)
            {
                data.PcStartItem = RandomItem(items, data.PcStartItem, settings.PcItemSettings);
            }
            else if(settings.PcPotionOption == Settings.PcItemOption.Custom)
            {
                data.PcStartItem = settings.CustomPcItem;
            }

            #endregion

            #region Debugging / Testing
            var tmLearns = new Dictionary<PokemonSpecies, List<Move>>();
            foreach(var pkmn in data.Pokemon)
            {
                var moveList = new List<Move>();
                for (int i = 0; i < pkmn.TMCompat.Length; ++i)
                    if (pkmn.TMCompat[i])
                        moveList.Add(data.TMMoves[i]);
                tmLearns.Add(pkmn.species, moveList);
            }

            var compTypes = new Dictionary<PokemonSpecies, List<PokemonType>>();
            foreach (var pkmn in data.Pokemon)
            {
                var typeList = new List<PokemonType>();
                foreach (var t in EnumUtils.GetValues<PokemonType>())
                    if (IsComplementaryType(pkmn.types, t))
                        typeList.Add(t);
                compTypes.Add(pkmn.species, typeList);
            }

            //data.UseUnknownTypeForMoves = true;
            //data.MoveData[(int)Move.TACKLE].type = PokemonType.Unknown;
            #endregion

            data.CalculateMetrics();

            return data;
        }

        #region Set Definitions

        /// <summary> Define and return the set of valid pokemon (with applicable restrictions)</summary>
        private HashSet<PokemonSpecies> DefinePokemonSet()
        {
            //Start with all for now
            HashSet<PokemonSpecies> pokemonSet = EnumUtils.GetValues<PokemonSpecies>().ToHashSet();
            pokemonSet.Remove(PokemonSpecies.None);
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
            types.Remove(PokemonType.FAI);
            return types;
        }

        private IEnumerable<ItemData> DefineItemSet()
        {
            return data.ItemData.Where((i) => i.Item != Item.None);
        }

        #endregion

        #region Species Randomization

        /// <summary> Chose a random species from the input set based on the given species settings and the type sample given</summary> 
        private PokemonSpecies RandomSpeciesTypeGroup(IEnumerable<PokemonSpecies> possiblePokemon, PokemonSpecies pokemon, int level, IEnumerable<PokemonSpecies> typeGroup, Settings.SpeciesSettings speciesSettings)
        {
            var newSpecies = RandomSpeciesTypeGroup(possiblePokemon, pokemon, typeGroup, speciesSettings);
            if (speciesSettings.ForceHighestLegalEvolution)
                newSpecies = MaxEvolution(newSpecies, level, speciesSettings.RestrictIllegalEvolutions);
            else if (speciesSettings.RestrictIllegalEvolutions)
                newSpecies = CorrectImpossibleEvo(newSpecies, level);
            // Actually choose the species
            return newSpecies;
        }
        /// <summary> Chose a random species from the input set based on the given species settings and the type sample given</summary> 
        private PokemonSpecies RandomSpeciesTypeGroup(IEnumerable<PokemonSpecies> possiblePokemon, PokemonSpecies pokemon, IEnumerable<PokemonSpecies> typeGroup, Settings.SpeciesSettings speciesSettings)
        {
            var combinedWeightings = SpeciesWeightedSetTypeGroup(possiblePokemon, pokemon, typeGroup, speciesSettings);
            // Actually choose the species
            return rand.Choice(combinedWeightings);
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
            if (speciesSettings.ForceHighestLegalEvolution)
                newSpecies = MaxEvolution(newSpecies, level, speciesSettings.RestrictIllegalEvolutions);
            else if(speciesSettings.RestrictIllegalEvolutions)
                newSpecies = CorrectImpossibleEvo(newSpecies, level);
            // Actually choose the species
            return newSpecies;
        }
        
        private Func<PokemonSpecies, float> GetTypeBalanceFunction(IEnumerable<PokemonSpecies> possiblePokemon)
        {
            var typeOccurenceLookup = new Dictionary<PokemonType, float>();
            foreach (var type in EnumUtils.GetValues<PokemonType>())
            {
                typeOccurenceLookup.Add(type, 0);
            }
            foreach (var pokemon in possiblePokemon)
            {
                var pData = data.PokemonLookup[pokemon];
                typeOccurenceLookup[pData.types[0]] += 1;
                if (pData.IsSingleTyped)
                    continue;
                typeOccurenceLookup[pData.types[1]] += 1;
            }
            foreach (var type in EnumUtils.GetValues<PokemonType>())
            {
                float val = typeOccurenceLookup[type];
                typeOccurenceLookup[type] = val == 0 ? 0 : 1 / val;
            }
            float TypeBalanceMetric(PokemonSpecies s)
            {
                var pData = data.PokemonLookup[s];
                if (pData.IsSingleTyped)
                    return typeOccurenceLookup[pData.types[0]];
                return (typeOccurenceLookup[pData.types[0]] + typeOccurenceLookup[pData.types[1]]) / 2;
            }
            return TypeBalanceMetric;
        }
        /// <summary> Get a weighted and culled list of possible pokemon</summary>
        private WeightedSet<PokemonSpecies> SpeciesWeightedSet(IEnumerable<PokemonSpecies> possiblePokemon, PokemonSpecies pokemon, Settings.SpeciesSettings speciesSettings)
        {
            var combinedWeightings = new WeightedSet<PokemonSpecies>(possiblePokemon);
            // Power level similarity
            if (speciesSettings.PowerScaleSimilarityMod > 0)
            {
                var powerWeighting = PokemonMetrics.PowerSimilarity(combinedWeightings.Items, powerScores, pokemon, speciesSettings.PowerThresholdStronger, speciesSettings.PowerThresholdWeaker);
                combinedWeightings.Add(powerWeighting, speciesSettings.PowerScaleSimilarityMod);
                // Cull if necessary
                if (speciesSettings.PowerScaleCull)
                    combinedWeightings.RemoveWhere((p) => !powerWeighting.Contains(p));
            }
            // Type similarity
            if (speciesSettings.TypeSimilarityMod > 0)
            {
                var typeWeighting = PokemonMetrics.TypeSimilarity(combinedWeightings.Items, data, pokemon);
                typeWeighting.Multiply(GetTypeBalanceFunction(combinedWeightings.Items)(pokemon));
                typeWeighting.Normalize();
                combinedWeightings.Add(typeWeighting, speciesSettings.TypeSimilarityMod);
                // Cull if necessary
                if (speciesSettings.TypeSimilarityCull)
                    combinedWeightings.RemoveWhere((p) => !typeWeighting.Contains(p));
            }
            // Sharpen the data if necessary
            if (speciesSettings.Sharpness > 0)
            {
                combinedWeightings.Map((p) => (float)Math.Pow(combinedWeightings[p], speciesSettings.Sharpness));
                combinedWeightings.Normalize();
            }
            // Normalize combined weightings and add noise
            if (speciesSettings.Noise > 0)
            {
                combinedWeightings.Normalize();
                var noise = new WeightedSet<PokemonSpecies>(possiblePokemon, speciesSettings.Noise);
                combinedWeightings.Add(noise);
            }
            // Remove Legendaries
            if (speciesSettings.BanLegendaries)
                combinedWeightings.RemoveWhere((p) => PokemonBaseStats.legendaries.Contains(p));
            combinedWeightings.RemoveWhere((p) => combinedWeightings[p] <= 0);
            return combinedWeightings;
        }
        private WeightedSet<PokemonSpecies> SpeciesWeightedSetTypeGroup(IEnumerable<PokemonSpecies> possiblePokemon, PokemonSpecies pokemon, IEnumerable<PokemonSpecies> typeGroup, Settings.SpeciesSettings speciesSettings)
        {
            var combinedWeightings = new WeightedSet<PokemonSpecies>(possiblePokemon);
            // Power level similarity
            if (speciesSettings.PowerScaleSimilarityMod > 0)
            {
                var powerWeighting = PokemonMetrics.PowerSimilarity(combinedWeightings.Items, powerScores, pokemon, speciesSettings.PowerThresholdStronger, speciesSettings.PowerThresholdWeaker);
                combinedWeightings.Add(powerWeighting, speciesSettings.PowerScaleSimilarityMod);
                // Cull if necessary
                if (speciesSettings.PowerScaleCull)
                    combinedWeightings.RemoveWhere((p) => !powerWeighting.Contains(p));
            }
            // Type similarity
            if (speciesSettings.TypeSimilarityMod > 0)
            {
                var typeBalanceMetric = GetTypeBalanceFunction(combinedWeightings.Items);
                var typeWeighting = PokemonMetrics.TypeSimilarity(combinedWeightings.Items, data, pokemon);
                typeWeighting.Multiply(typeBalanceMetric);
                foreach (var sample in typeGroup)
                {
                    typeWeighting.Add(PokemonMetrics.TypeSimilarity(combinedWeightings.Items, data, sample), typeBalanceMetric(sample));
                }
                var sampleTypes = typeGroup.SelectMany((s) => data.PokemonLookup[s].types).Distinct();
                Tuple<PokemonType, PokemonType> Map(PokemonSpecies p)
                {
                    var types = data.PokemonLookup[p].types.Intersect(sampleTypes).ToList();
                    types.Sort();
                    if (types.Count == 0)
                        return null;
                    if (types.Count == 1)
                        return new Tuple<PokemonType, PokemonType>(types[0], types[0]);
                    return new Tuple<PokemonType, PokemonType>(types[0], types[1]);
                }
                var typeDistribution = typeWeighting.Distribution(Map);
                typeWeighting.Normalize();
                combinedWeightings.Add(typeWeighting, speciesSettings.TypeSimilarityMod);
                // Cull if necessary
                if (speciesSettings.TypeSimilarityCull)
                    combinedWeightings.RemoveWhere((p) => !typeWeighting.Contains(p));
            }
            // Sharpen the data if necessary
            if(speciesSettings.Sharpness > 0)
            {
                combinedWeightings.Map((p) => (float)Math.Pow(combinedWeightings[p], speciesSettings.Sharpness));
                combinedWeightings.Normalize();
            }
            // Normalize combined weightings and add noise
            if (speciesSettings.Noise > 0)
            {
                combinedWeightings.Normalize();
                var noise = new WeightedSet<PokemonSpecies>(possiblePokemon, speciesSettings.Noise);
                combinedWeightings.Add(noise);
            }
            // Remove Legendaries
            if (speciesSettings.BanLegendaries)
                combinedWeightings.RemoveWhere((p) => PokemonBaseStats.legendaries.Contains(p));
            combinedWeightings.RemoveWhere((p) => combinedWeightings[p] <= 0);
            return combinedWeightings;
        }
        #endregion

        #region Typing

        /// <summary> Return 3 pokemon that form a valid type traingle, or null if none exist in the input set.
        /// Type triangles require one-way weakness, but allow neutral relations in reverse order (unless strong is true) </summary>
        private List<PokemonSpecies> RandomTypeTriangle(IEnumerable<PokemonSpecies> possiblePokemon, Settings.SpeciesSettings speciesSettings, bool strong = false)
        {
            var set = SpeciesWeightedSet(possiblePokemon, data.Starters[0], speciesSettings);
            if(speciesSettings.RestrictIllegalEvolutions)
                set.RemoveWhere((p) => !IsPokemonValidLevel(data.PokemonLookup[p], 5));
            var pool = new WeightedSet<PokemonSpecies>(set);
            while(pool.Count > 0)
            {
                var first = rand.Choice(pool);
                pool.Remove(first);
                // Get potential second pokemon
                var secondPossiblities = SpeciesWeightedSet(set.Items, data.Starters[1], speciesSettings);
                secondPossiblities.RemoveWhere((p) => !OneWayWeakness(first, p, strong));
                // Finish the traiangle if possible
                var triangle = FinishTriangle(set, secondPossiblities, first, speciesSettings, strong);
                if (triangle != null)
                    return triangle;

            }
            return null; // No viable triangle with input spcifications
        }
        /// <summary> Helper method for the RandomTypeTriangle method </summary>
        private List<PokemonSpecies> FinishTriangle(WeightedSet<PokemonSpecies> set, WeightedSet<PokemonSpecies> possibleSeconds, PokemonSpecies first, Settings.SpeciesSettings speciesSettings, bool strong)
        {
            while (possibleSeconds.Count > 0)
            {
                var second = rand.Choice(possibleSeconds);
                possibleSeconds.Remove(second);
                // Get third pokemon
                var thirdPossiblities = SpeciesWeightedSet(set.Items, data.Starters[2], speciesSettings);
                thirdPossiblities.RemoveWhere((p) => !(OneWayWeakness(second, p, strong) && OneWayWeakness(p, first, strong)));
                // If at least one works, choose one randomly
                if (thirdPossiblities.Count > 0)
                    return new List<PokemonSpecies> { first, second, rand.Choice(thirdPossiblities) };
            }
            return null;
        }
        /// <summary> Return true if b is weak to a AND a is not weak to b. 
        /// If strong is true, b must also not be normally effective against a </summary>
        private bool OneWayWeakness(PokemonSpecies a, PokemonSpecies b, bool strong = true)
        {
            var aTypes = data.PokemonLookup[a].types;
            var bTypes = data.PokemonLookup[b].types;
            var aVsB = data.TypeDefinitions.GetEffectiveness(aTypes[0], aTypes[1], bTypes[0], bTypes[1]);
            var bVsA = data.TypeDefinitions.GetEffectiveness(bTypes[0], bTypes[1], aTypes[0], aTypes[1]);
            if(strong)
                return aVsB == TypeEffectiveness.SuperEffective && !(bVsA == TypeEffectiveness.SuperEffective || bVsA == TypeEffectiveness.Normal);
            return aVsB == TypeEffectiveness.SuperEffective && !(bVsA == TypeEffectiveness.SuperEffective);
        }
        private bool IsComplementaryType(PokemonType[] pokemonType, PokemonType atkType)
        {
            var allTypes = EnumUtils.GetValues<PokemonType>();
            var weakTypes = allTypes.Where((t) => data.TypeDefinitions.GetEffectiveness(t, t, pokemonType[0], pokemonType[1]) == TypeEffectiveness.SuperEffective);           
            foreach (var t in weakTypes)
                if (data.TypeDefinitions.GetEffectiveness(atkType, t) == TypeEffectiveness.SuperEffective)
                    return true;
            //var resistTypes = allTypes.Where((t) => data.TypeDefinitions.GetEffectiveness(pokemonType[0], pokemonType[1], t, t) == TypeEffectiveness.NotVeryEffective);
            //foreach (var t in resistTypes)
            //    if (data.TypeDefinitions.GetEffectiveness(atkType, t) == TypeEffectiveness.SuperEffective)
            //        return true;
            return false;
        }

        #endregion

        #region Evolution

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
        /// If the pokemon is an invalid level due to evolution state, revert to an earlier evolution
        private PokemonSpecies CorrectImpossibleEvo(PokemonSpecies species, int level)
        {
            var pokemon = data.PokemonLookup[species];
            while (!IsPokemonValidLevel(pokemon, level))
            {
                // Choose a random element from the pokemon this pokemon evolves from
                pokemon = data.PokemonLookup[rand.Choice(pokemon.evolvesFrom).Pokemon];
            }
            return pokemon.species;
        }
        /// <summary> returns false if the pokemon is an invalid level. 
        /// (due to not being high enough level to evolve to the current species) </summary>
        private bool IsPokemonValidLevel(PokemonBaseStats pokemon, int level)
        {
            if (pokemon.evolvesFrom.Count == 0) // basic pokemon
                return true;
            // Is there at least one valid evolution
            foreach (var evo in pokemon.evolvesFrom)
            {
                if (EquivalentLevelReq(evo, pokemon) <= level)
                    return true;
            }
            return false;
        }
        /// <summary> Returns the equivalent required level of an evolution for a give pokemon (including non-leveling evolutions if applicable) </summary>
        public int EquivalentLevelReq(Evolution evo, PokemonBaseStats pokemon)
        {
            if (evo.EvolvesByLevel)
                return evo.parameter;
            if(evo.EvolvesByFriendship && pokemon.IsBaby)
            {
                return 18;
            }
            // For any other type Calculate level based on evolution tree
            if (pokemon.evolvesFrom.Count > 0)
            {
                return EquivalentLevelReq(pokemon.evolvesFrom[0], data.PokemonLookup[pokemon.evolvesFrom[0].Pokemon]) + 12;
            }
            else
            {
                int baseLevel = 32;
                // Is this pokemon a middle stage evolution?
                if (data.PokemonLookup[evo.Pokemon].evolvesTo.Count((e) => e.Pokemon != PokemonSpecies.None) > 0)
                    baseLevel -= 8;
                return baseLevel;
            }
        }

        /// <summary> Return the maximum evolved form of the pokemon at the given level.
        /// returns a lower form if the pokemon is an invalid level.
        /// returns a random branch for evolution trees that branch </summary>
        private PokemonSpecies MaxEvolution(PokemonSpecies p, int level, bool restrictIllegalEvolutions)
        {
            var stats = data.PokemonLookup[p];
            // If illegal evolutions are disabled, and the pokemon is an illegal level, correct the impossible evolution
            if (restrictIllegalEvolutions && !IsPokemonValidLevel(stats, level))
                return CorrectImpossibleEvo(p, level);
            // Else evolve the pokemon until you can't anymore
            var evos = stats.evolvesTo.Where((evo) => evo.Type != EvolutionType.None && EquivalentLevelReq(evo, stats) <= level);
            while (evos.Count() > 0)
            {
                stats = data.PokemonLookup[rand.Choice(evos).Pokemon];
                evos = stats.evolvesTo.Where((evo) => evo.Type != EvolutionType.None && EquivalentLevelReq(evo, stats) <= level);
            }
            return stats.species;
        }

        #endregion

        #region Trainer Randomization

        private void RandomizeTrainerPokemon(Trainer trainer, IEnumerable<PokemonSpecies> pokemonSet, Settings.SpeciesSettings settings)
        {
            // Class based?
            // Local environment based
            // Get type sample
            var typeSample = trainer.pokemon.Select((p) => p.species).ToArray();
            foreach (var pokemon in trainer.pokemon)
            {
                if (settings.WeightType == Settings.SpeciesSettings.WeightingType.Group)
                {
                    pokemon.species = RandomSpeciesTypeGroup(pokemonSet, pokemon.species, pokemon.level, typeSample, settings);
                }
                else
                {

                    pokemon.species = RandomSpecies(pokemonSet, pokemon.species, pokemon.level, settings);
                }
                // Reset special moves if necessary
                if (pokemon.HasSpecialMoves)
                {
                    //pokemon.moves = MovesetGenerator.DefaultMoveset(data.PokemonLookup[pokemon.species], pokemon.level);
                    pokemon.moves = MovesetGenerator.SmartMoveSet(rand, data, data.PokemonLookup[pokemon.species], pokemon.level);
                }
            }
        }

        /// <summary> Radnomize the given trainer encounter </summary>
        private void RandomizeTrainer(Trainer trainer, IEnumerable<PokemonSpecies> pokemonSet, Settings.TrainerSettings settings, bool safe = true)
        {
            // Set data type
            // Set AI flags
            // Set item stock (if applicable)
            // Set Battle Type
            if (rand.RollSuccess(settings.BattleTypeRandChance))
                trainer.isDoubleBattle = rand.RandomDouble() < settings.DoubleBattleChance;
            // Set pokemon
            if(rand.RollSuccess(settings.PokemonRandChance))
            {
                RandomizeTrainerPokemon(trainer, pokemonSet, settings.SpeciesSettings);
            }
            // Fix any unsafe values if safe is set to true
            if(safe)
            {
                // Set 1-pokemon battle to solo if appropriate
                if (settings.MakeSoloPokemonBattlesSingle && trainer.pokemon.Length == 1)
                    trainer.isDoubleBattle = false;
            }
        }

        /// <summary>
        /// Randomize A sequence of battles from the same trainer.
        /// Battles is assumed to be in chronological order, and that the first battle has been appropriately randomized.
        /// Use unsafe randomization for randomizing the first battle.
        /// </summary>
        private void RandomizeTrainerReoccurring(Trainer firstBattle, List<Trainer> battles, IEnumerable<PokemonSpecies> pokemonSet, Settings.TrainerSettings settings)
        {
            var speciesSettings = settings.SpeciesSettings;

            // Battle Type
            if(settings.BattleTypeStrategy == Settings.TrainerSettings.BattleTypePcgStrategy.None)
            {
                for (int i = 1; i < battles.Count; i++)
                {
                    if (rand.RollSuccess(settings.BattleTypeRandChance))
                    {
                        battles[i].isDoubleBattle = rand.RollSuccess(settings.DoubleBattleChance);
                    }
                }
            }
            else if(settings.BattleTypeStrategy == Settings.TrainerSettings.BattleTypePcgStrategy.KeepSameType)
            {
                for (int i = 1; i < battles.Count; i++)
                {
                    battles[i].isDoubleBattle = battles[0].isDoubleBattle;
                }
            }

            // Pokemon
            if (settings.PokemonStrategy == Settings.TrainerSettings.PokemonPcgStrategy.None)
            {
                for (int i = 1; i < battles.Count; i++)
                {
                    RandomizeTrainerPokemon(battles[i], pokemonSet, speciesSettings);
                }
            }
            else if (settings.PokemonStrategy == Settings.TrainerSettings.PokemonPcgStrategy.KeepAce)
            {
                var lastBattle = firstBattle;
                foreach (var battle in battles)
                {
                    RandomizeTrainerPokemon(battle, pokemonSet, speciesSettings);
                    // Migrate Ace pokemon from the last battle
                    var currAce = battle.pokemon[battle.pokemon.Length - 1];
                    var lastAce = lastBattle.pokemon[lastBattle.pokemon.Length - 1];
                    currAce.species = MaxEvolution(lastAce.species, currAce.level, speciesSettings.RestrictIllegalEvolutions);
                    if (currAce.HasSpecialMoves)
                    {
                        currAce.moves = MovesetGenerator.SmartMoveSet(rand, data, data.PokemonLookup[currAce.species], currAce.level);
                    }
                    lastBattle = battle;
                };
            }
            else if (settings.PokemonStrategy == Settings.TrainerSettings.PokemonPcgStrategy.KeepParty)
            {
                var lastBattle = firstBattle;
                foreach (var battle in battles)
                {
                    RandomizeTrainerPokemon(battle, pokemonSet, speciesSettings);
                    int lastBattleSize = lastBattle.pokemon.Length;
                    int battleSize = battle.pokemon.Length;
                    // Migrate pokemon from the last battle
                    for (int i = 0; i < lastBattleSize && i < battleSize; ++i)
                    {
                        var currPokemon = battle.pokemon[battleSize - (i + 1)];
                        var lastPokemon = lastBattle.pokemon[lastBattleSize - (i + 1)];
                        currPokemon.species = MaxEvolution(lastPokemon.species, currPokemon.level, speciesSettings.RestrictIllegalEvolutions);
                        if(currPokemon.HasSpecialMoves)
                        {
                            currPokemon.moves = MovesetGenerator.SmartMoveSet(rand, data, data.PokemonLookup[currPokemon.species], currPokemon.level);
                        }
                    }
                    lastBattle = battle;
                }
            }

            // Fixes
            if(settings.MakeSoloPokemonBattlesSingle)
            {
                foreach (var battle in battles)
                {
                    if (battle.isDoubleBattle && battle.pokemon.Length <= 1)
                        battle.isDoubleBattle = false;
                }
                if (firstBattle.isDoubleBattle && firstBattle.pokemon.Length <= 1)
                    firstBattle.isDoubleBattle = false;
            }
        }

        #endregion

        private void RandomizeWeather(Map map, Settings s)
        {
            if (s.WeatherSetting == Settings.WeatherOption.Unchanged)
                return;
            // Local method to finish choosing the weather
            void ChooseWeather(Map m, Settings s2, bool gymOverride)
            {
                var choices = new WeightedSet<Map.Weather>(s2.WeatherWeights);
                if (s2.SafeUnderwaterWeather && !(m.mapType == Map.Type.Underwater))
                {
                    choices.RemoveIfContains(Map.Weather.UnderwaterMist);
                }
                if (!gymOverride && s2.SafeInsideWeather && !m.IsOutdoors)
                {
                    choices.RemoveIfContains(Map.Weather.ClearWithCloudsInWater);
                    choices.RemoveIfContains(Map.Weather.Clear);
                    choices.RemoveIfContains(Map.Weather.Cloudy);
                    choices.RemoveIfContains(Map.Weather.Rain);
                    choices.RemoveIfContains(Map.Weather.RainThunderstorm);
                    choices.RemoveIfContains(Map.Weather.RainHeavyThunderstrorm);
                    choices.RemoveIfContains(Map.Weather.Sandstorm);
                    choices.RemoveIfContains(Map.Weather.Snow);
                    choices.RemoveIfContains(Map.Weather.SnowSteady);
                    choices.RemoveIfContains(Map.Weather.StrongSunlight);
                }
                if (choices.Count > 0)
                    m.weather = rand.Choice(choices);
            }
            // If Gym override is set and the map is a gym, proceed with randomization
            if (s.OverrideAllowGymWeather && map.battleField == 0x01)
            {
                if(rand.RandomDouble() < s.GymWeatherRandChance)
                {
                    ChooseWeather(map, s, true);
                }
            }
            else if((!s.OnlyChangeClearWeather || Map.IsWeatherClear(map.weather)) && s.WeatherRandChance.ContainsKey(map.mapType))
            {
                if(rand.RandomDouble() < s.WeatherRandChance[map.mapType])
                {
                    ChooseWeather(map, s, false);
                }
            }
        }

        private Item RandomItem(IEnumerable<ItemData> possibleItems, Item input, Settings.ItemSettings settings)
        {
            var inputData = data.ItemData[(int)input];
            var itemWeights = new WeightedSet<ItemData>(possibleItems, 1);
            if(rand.RollSuccess(settings.SamePocketChance))
            {
                itemWeights.RemoveWhere((i) => i.pocket != inputData.pocket);
            }
            return itemWeights.Count <= 0 ? input : rand.Choice(itemWeights).Item;
        }
    }
}