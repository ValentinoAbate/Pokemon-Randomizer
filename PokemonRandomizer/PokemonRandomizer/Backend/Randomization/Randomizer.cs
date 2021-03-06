﻿using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.DataStructures.Scripts;
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
        private readonly EvolutionUtils evoUtils;
        private readonly PkmnRandomizer pokeRand;
        private readonly ItemRandomizer itemRand;
        private readonly WildEncounterRandomizer encounterRand;
        private readonly TrainerRandomizer trainerRand;
        /// <summary>
        /// Create a new randomizer with given data and settings
        /// Input data will be mutated by randomizer calls
        /// </summary>
        public Randomizer(RomData data, Settings settings)
        {
            this.data = data;
            this.settings = settings;
            // Initialize random generator
            rand = settings.SetSeed ? new Random(settings.Seed) : new Random();
            // Intialize evolution helper
            evoUtils = new EvolutionUtils(rand, data);
            //Initialize Species Randomizer
            var powerScores = PowerScaling.Calculate(data.Pokemon, settings.TieringOptions);
            pokeRand = new PkmnRandomizer(evoUtils, rand, data, data.Metrics, powerScores);
            // Initialize item randomizer
            itemRand = new ItemRandomizer(rand, data);
            // Initialize encounter randomizer
            encounterRand = new WildEncounterRandomizer(pokeRand, evoUtils, data.Metrics, data);
            // Initialize Trainer randomizer
            trainerRand = new TrainerRandomizer(rand, pokeRand, evoUtils, data);
        }
        // Apply mutations based on program settings.
        public RomData Randomize()
        {
            var pokemonSet = DefinePokemonSet();
            var fossilSet = pokemonSet.Where(PokemonUtils.IsFossil).ToHashSet();
            if (settings.CountRelicanthAsFossil && pokemonSet.Contains(Pokemon.RELICANTH))
                fossilSet.Add(Pokemon.RELICANTH);
            var babySet = pokemonSet.Where(PokemonUtils.IsBaby).ToHashSet();
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
            if(settings.PreventHmMovesInTMsAndTutors)
            {
                foreach (var move in data.HMMoves)
                    moves.Remove(move);
            }
            // Randomize TM mappings
            for(int i = 0; i < data.TMMoves.Length; ++i)
            {
                bool skipImportant = settings.KeepImportantTMsAndTutors && settings.ImportantTMsAndTutors.Contains(data.TMMoves[i]);
                if (!skipImportant && rand.RollSuccess(settings.TMRandChance))
                    data.TMMoves[i] = rand.Choice(moves);
                if (settings.PreventDuplicateTMsAndTutors)
                    moves.Remove(data.TMMoves[i]);
            }
            // Randomize Move Tutor mappings
            for (int i = 0; i < data.tutorMoves.Length; ++i)
            {
                bool skipImportant = settings.KeepImportantTMsAndTutors && settings.ImportantTMsAndTutors.Contains(data.tutorMoves[i]);
                if (!skipImportant && rand.RollSuccess(settings.MoveTutorRandChance))
                    data.tutorMoves[i] = rand.Choice(moves);
                if (settings.PreventDuplicateTMsAndTutors)
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
                        var evolveByLevelUp = pokemon.evolvesTo.FirstOrDefault(e => e.Type == EvolutionType.LevelUp);
                        var newEvo = pokemon.evolvesTo.FirstOrDefault(e => !e.IsRealEvolution) ?? evo;
                        newEvo.Pokemon = evo.Pokemon;
                        if (evolveByLevelUp == null)
                        {

                            newEvo.parameter = evoUtils.EquivalentLevelReq(evo, pokemon) + rand.RandomGaussianInt(0, settings.ImpossibleEvoLevelStandardDev);
                            newEvo.Type = EvolutionType.LevelUp;
                        }
                        else
                        {
                            evolveByLevelUp.Type = EvolutionType.LevelUpWithPersonality1;
                            newEvo.Type = EvolutionType.LevelUpWithPersonality2;
                            newEvo.parameter = evolveByLevelUp.parameter;
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
                                var newEvo = pokemon.evolvesTo.FirstOrDefault(e => !e.IsRealEvolution) ?? evo;
                                newEvo.Pokemon = evo.Pokemon;
                                newEvo.Type = EvolutionType.UseItem;
                                newEvo.parameter = evo.parameter;
                                // Log this as a new evolution stone if necessary
                                Item item = (Item)newEvo.parameter;
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
                    if (!evo.IsRealEvolution)
                        continue;

                    #region Dunsparse Plague
                    if (rand.RollSuccess(settings.DunsparsePlaugeChance))
                    {
                        static int FirstEmptyEvo(Evolution[] evolutions)
                        {
                            for (int i = 0; i < evolutions.Length; i++)
                            {
                                if (!evolutions[i].IsRealEvolution)
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
                                pokemon.evolvesTo[index].Pokemon = Pokemon.DUNSPARCE;
                                pokemon.evolvesTo[index].Type = EvolutionType.LevelUpWithPersonality2;
                                pokemon.evolvesTo[index].parameter = evo.parameter;
                            }
                        }
                        else if(evo.Type == EvolutionType.Friendship)
                        {
                            evo.Type = rand.RandomBool() ? EvolutionType.FriendshipDay : EvolutionType.FriendshipNight;
                            int index = FirstEmptyEvo(pokemon.evolvesTo);
                            if (index >= 0)
                            {
                                pokemon.evolvesTo[index].Pokemon = Pokemon.DUNSPARCE;
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
                    if (rand.RollSuccess(settings.SingleTypeRandChance))
                        pokemon.types[0] = pokemon.types[1] = rand.Choice(data.Metrics.TypeRatiosSingle);
                }
                else
                {
                    if (rand.RollSuccess(settings.DualTypePrimaryRandChance))
                        pokemon.types[0] = rand.Choice(data.Metrics.TypeRatiosDualPrimary);
                    if (rand.RollSuccess(settings.DualTypeSecondaryRandChance))
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
                if(settings.AddMoves && pokemon.IsBasic && rand.RollSuccess(settings.AddMovesChance))
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
                                    if (!evo.IsRealEvolution)
                                        return;
                                    // Make this stable with the Dunsparse Plague
                                    if (evo.Pokemon == Pokemon.DUNSPARCE && settings.DunsparsePlaugeChance > 0)
                                        continue;
                                    AddMoveToEvoTreeMoveSet(data.GetBaseStats(evo.Pokemon), m, level + creep, creep);
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
                            int effectivePower = data.GetMoveData(move).EffectivePower;
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
                            pokemon.TMCompat[i] = rand.RollSuccess(settings.TmMtTrueChance);
                        }
                        for (int i = 0; i < pokemon.moveTutorCompat.Length; ++i)
                        {
                            pokemon.moveTutorCompat[i] = rand.RollSuccess(settings.TmMtTrueChance);
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
                            var moveData = data.GetMoveData(moveList[ind]);
                            if (pokemon.types.Contains(moveData.type))
                                arr[ind] = true;
                            else if (pokemon.learnSet.Any((l) => l.move == moveData.move))
                                arr[ind] = true;
                            else if (pokemon.eggMoves.Contains(moveData.move))
                                arr[ind] = true;
                            else if (moveData.type == PokemonType.NRM)
                                arr[ind] = rand.RollSuccess(settings.TmMtTrueChance);
                            else
                                arr[ind] = rand.RollSuccess(settings.TmMtNoise);
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
                            if (pokemon.IsBasicOrEvolvesFromBaby)
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
                var unknownPokeData = data.GetBaseStats(Pokemon.UNOWN);
                unknownPokeData.types[0] = PokemonType.Unknown;
                if (rand.RollSuccess(settings.UnknownDualTypeChance))
                    unknownPokeData.types[1] = rand.Choice(data.Metrics.TypeRatiosDualSecondary);
                else
                    unknownPokeData.types[1] = PokemonType.Unknown;
            }
            // Change pallettes to fit new types
            #endregion

            #region Starters
            if (settings.StarterSetting != Settings.StarterPokemonOption.Unchanged)
            {
                var starterSettings = settings.StarterPokemonSettings;
                if(settings.StarterSetting == Settings.StarterPokemonOption.Random)
                {
                    for(int i = 0; i < data.Starters.Count; ++i)
                    {
                        data.Starters[i] = pokeRand.RandomPokemon(pokemonSet, data.Starters[i], starterSettings, 5);
                    }
                }
                else if (settings.StarterSetting == Settings.StarterPokemonOption.RandomTypeTriangle)
                {
                    var triangle = pokeRand.RandomTypeTriangle(pokemonSet, data.Starters, data.TypeDefinitions, starterSettings, settings.StrongStarterTypeTriangle);
                    if (triangle != null)
                    {
                        data.Starters = triangle;
                    }
                    else // Fall back on completely random
                    {
                        for (int i = 0; i < data.Starters.Count; ++i)
                        {
                            data.Starters[i] = pokeRand.RandomPokemon(pokemonSet, data.Starters[i], starterSettings, 5);
                        }
                    }
                }
                else if(settings.StarterSetting == Settings.StarterPokemonOption.Custom)
                {
                    data.Starters = new List<Pokemon>(settings.CustomStarters);
                }
                // Make sure all starters have attack moves
                if(settings.SafeStarterMovesets)
                {
                    // Hacky tackle fix
                    foreach (var pkmn in data.Starters)
                    {
                        var learnSet = data.GetBaseStats(pkmn).learnSet;
                        if (learnSet[0].move != Move.TACKLE)
                            learnSet.Add(Move.TACKLE, 1);
                    }

                }
            }
            #endregion

            #region In-Game Trades
            foreach(var trade in data.Trades)
            {
                if (rand.RollSuccess(settings.TradePokemonGiveRandChance))
                {
                    trade.pokemonWanted = pokeRand.RandomPokemon(pokemonSet, trade.pokemonWanted, settings.TradeSpeciesSettingsGive);
                }
                if (rand.RollSuccess(settings.TradePokemonRecievedRandChance))
                {
                    trade.pokemonRecieved = pokeRand.RandomPokemon(pokemonSet, trade.pokemonRecieved, settings.TradeSpeciesSettingsReceive);
                    var pokemonData = data.GetBaseStats(trade.pokemonRecieved);
                    if (pokemonData.abilities[0] == pokemonData.abilities[1])
                    {
                        trade.abilityNum = 0;
                    }
                    else
                    {
                        trade.abilityNum = rand.RandomInt(0, 2);
                    }
                }
                if (rand.RollSuccess(settings.TradeHeldItemRandChance))
                {
                    trade.heldItem = itemRand.RandomItem(items, trade.heldItem, settings.TradeHeldItemSettings);
                    if (ItemData.IsMail(trade.heldItem))
                    {
                        if (trade.mailNum == 0xFF)
                            trade.mailNum = 0;
                    }
                    else
                    {
                        trade.mailNum = 0xFF;
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

            #region Maps
            void RandomizeScript(Script script)
            {
                foreach (var command in script)
                {
                    switch (command)
                    {
                        case GotoCommand @goto:
                            RandomizeScript(@goto.script);
                            break;
                        case GiveItemCommand giveItem:
                            if (giveItem.type == GiveItemCommand.Type.Normal && rand.RollSuccess(settings.FieldItemRandChance))
                            {
                                giveItem.item = itemRand.RandomItem(items, giveItem.item, settings.FieldItemSettings);
                            }
                            break;
                        case GivePokemonCommand givePokemon:
                            if(givePokemon.type == GivePokemonCommand.Type.Normal && rand.RollSuccess(settings.GiftPokemonRandChance))
                            {
                                // Should choose from fossil set?
                                bool fossil = settings.EnsureFossilRevivesAreFossilPokemon && fossilSet.Count > 0 && givePokemon.pokemon.IsFossil();
                                givePokemon.pokemon = pokeRand.RandomPokemon(fossil ? fossilSet : pokemonSet, givePokemon.pokemon, settings.GiftSpeciesSettings, givePokemon.level);

                            }
                            break;
                        case GiveEggCommand giveEgg:
                            if(rand.RollSuccess(settings.GiftPokemonRandChance))
                            {
                                bool baby = settings.EnsureGiftEggsAreBabyPokemon && babySet.Count > 0;
                                giveEgg.pokemon = pokeRand.RandomPokemon(baby ? babySet : pokemonSet, giveEgg.pokemon, settings.GiftSpeciesSettings, 1);
                            }
                            break;
                    }
                }
            }
            // Mutate Maps (currently just iterate though the maps, but may want to construct and traverse a graph later)
            foreach (var map in data.Maps)
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
                // Randomize Hidden Items
                foreach(var sEvent in map.eventData.signEvents)
                {
                    if(sEvent.IsHiddenItem)
                    {
                        if (settings.UseSeperateHiddenItemSettings)
                        {
                            if (rand.RollSuccess(settings.HiddenItemRandChance))
                            {
                                sEvent.hiddenItem = itemRand.RandomItem(items, sEvent.hiddenItem, settings.HiddenItemSettings);
                            }
                        }
                        else if(rand.RollSuccess(settings.FieldItemRandChance))
                        {
                            sEvent.hiddenItem = itemRand.RandomItem(items, sEvent.hiddenItem, settings.FieldItemSettings);
                        }
                    }
                }
                // Randomize NPCs
                foreach (var npc in map.eventData.npcEvents)
                {
                    // Randomize NPC scripts
                    if(npc.script != null)
                    {
                        RandomizeScript(npc.script);
                    }
                }
                //Mutate battle here later?
            }
            #endregion

            #region Wild Encounters

            encounterRand.RandomizeEncounters(pokemonSet, data.Encounters, settings.EncounterSettings, settings.EncounterStrategy);

            // Apply dream team to first encounter if desired
            if(settings.DreamTeamOption != Settings.DreamTeamSetting.None && data.FirstEncounterSet != null)
            {
                var dreamTeamRandomizer = new DreamTeamRandomizer(rand, data);
                var team = settings.DreamTeamOption switch
                {
                    Settings.DreamTeamSetting.Custom => settings.CustomDreamTeam,
                    Settings.DreamTeamSetting.Random => dreamTeamRandomizer.GenerateDreamTeam(pokemonSet, settings.DreamTeamOptions),
                    _ => new Pokemon[0],
                };
                dreamTeamRandomizer.ApplyDreamTeam(data.FirstEncounterSet, team);
            }

            #endregion

            #region Trainer Battles

            var trainerSpeciesSettings = settings.GetTrainerSettings(Settings.TrainerCategory.Trainer);
            // Randomize Normal Trainers
            foreach (var trainer in data.NormalTrainers.Values)
            {
                trainerRand.Randomize(trainer, pokemonSet, trainerSpeciesSettings);
            }
            // Randomize Team Grunts (considered normal trainers currently)
            foreach (var trainer in data.GruntBattles)
            {
                trainerRand.Randomize(trainer, pokemonSet, trainerSpeciesSettings);
            }

            #region Special Trainers

            #region Rivals

            var rivalSettings = settings.GetTrainerSettings(Settings.TrainerCategory.Rival);
            foreach (var trainer in data.RivalNames)
            {
                var allBattles = data.SpecialTrainers[trainer.ToLower()];
                allBattles.Sort((a, b) => a.AvgLvl.CompareTo(b.AvgLvl));
                // Split the rival battles into their three different options
                var starters = new Pokemon[] { allBattles[0].pokemon[0].species, allBattles[1].pokemon[0].species, allBattles[2].pokemon[0].species };
                var rivalBattles = starters.Select((s) => allBattles.Where(b => b.pokemon.Any(p => evoUtils.RelatedToOrSelf(p.species, s)))).ToArray();
                for (int i = 0; i < rivalBattles.Length; ++i)
                {
                    var battles = new List<Trainer>(rivalBattles[i]);
                    var firstBattle = battles[0];
                    battles.RemoveAt(0);
                    // Randomize the first battle
                    trainerRand.Randomize(firstBattle, pokemonSet, rivalSettings, false);
                    // Set the appropriate starter as the ace
                    firstBattle.pokemon[firstBattle.pokemon.Length - 1].species = data.Starters[data.RivalRemap[i]];
                    // Procedurally generate the rest of the battles
                    trainerRand.RandomizeReoccurring(firstBattle, battles, pokemonSet, rivalSettings);
                }
            }

            #endregion

            #region Wally (TODO, make either gen-specific without metadata, or make gen-agnostic)

            const string wallyName = "wally";
            if(data.SpecialTrainers.ContainsKey(wallyName))
            {
                // Randomize Wally starter if applicable
                if (settings.RandomizeWallyAce)
                    data.CatchingTutPokemon = pokeRand.RandomPokemon(pokemonSet, data.CatchingTutPokemon, rivalSettings.PokemonSettings, 5);
                var wallyBattles = new List<Trainer>(data.SpecialTrainers[wallyName]);
                wallyBattles.Sort((a, b) => a.AvgLvl.CompareTo(b.AvgLvl));
                var firstBattle = wallyBattles[0];
                wallyBattles.RemoveAt(0);
                // Set Wally's first pokemon to the catching tut pokemon
                trainerRand.Randomize(firstBattle, pokemonSet, rivalSettings, false);
                var firstBattleAce = firstBattle.pokemon[firstBattle.pokemon.Length - 1];
                firstBattleAce.species = evoUtils.MaxEvolution(data.CatchingTutPokemon, firstBattleAce.level, rivalSettings.PokemonSettings.RestrictIllegalEvolutions);
                // Procedurally generate the rest of Wally's battles
                trainerRand.RandomizeReoccurring(firstBattle, wallyBattles, pokemonSet, rivalSettings);
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
                    trainerRand.Randomize(firstBattle, pokemonSet, settings, false);
                    trainerRand.RandomizeReoccurring(firstBattle, reoccuringBattles, pokemonSet, settings);
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

            #region Items
            // Field items happen in map scripts

            // Randomize PC starting item
            if (settings.PcPotionOption == Settings.PcItemOption.Random)
            {
                data.PcStartItem = itemRand.RandomItem(items, data.PcStartItem, settings.PcItemSettings);
            }
            else if (settings.PcPotionOption == Settings.PcItemOption.Custom)
            {
                data.PcStartItem = settings.CustomPcItem;
            }
            #endregion

            #region Debugging / Testing
            var tmLearns = new Dictionary<Pokemon, List<Move>>();
            foreach(var pkmn in data.Pokemon)
            {
                var moveList = new List<Move>();
                for (int i = 0; i < pkmn.TMCompat.Length; ++i)
                    if (pkmn.TMCompat[i])
                        moveList.Add(data.TMMoves[i]);
                tmLearns.Add(pkmn.species, moveList);
            }

            //data.UseUnknownTypeForMoves = true;
            //data.MoveData[(int)Move.TACKLE].type = PokemonType.Unknown;
            #endregion

            data.CalculateMetrics();

            return data;
        }

        #region Set Definitions

        /// <summary> Define and return the set of valid pokemon (with applicable restrictions)</summary>
        private HashSet<Pokemon> DefinePokemonSet()
        {
            //Start with all for now
            HashSet<Pokemon> pokemonSet = EnumUtils.GetValues<Pokemon>().ToHashSet();
            pokemonSet.Remove(Pokemon.None);
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
                if(rand.RollSuccess(s.GymWeatherRandChance))
                {
                    ChooseWeather(map, s, true);
                }
            }
            else if((!s.OnlyChangeClearWeather || Map.IsWeatherClear(map.weather)) && s.WeatherRandChance.ContainsKey(map.mapType))
            {
                if(rand.RollSuccess(s.WeatherRandChance[map.mapType]))
                {
                    ChooseWeather(map, s, false);
                }
            }
        }
    }
}