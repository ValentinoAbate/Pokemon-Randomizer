using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.DataStructures.Scripts;
using PokemonRandomizer.Backend.Metadata;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;
using PokemonRandomizer.Backend.Utilities.Debug;
using System;
using System.Collections.Generic;
using System.Linq;


namespace PokemonRandomizer.Backend.Randomization
{
    // This class does the randomizing and randomizing by creating a randomized copy of the original ROM data
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
        private readonly MoveCompatibilityRandomizer compatRand;
        private readonly PokemonVariantRandomizer variantRand;
        private readonly BonusMoveGenerator bonusMoveGenerator;
        private readonly WeatherRandomizer weatherRand;
        private readonly List<Action> delayedRandomizationCalls;
        private readonly ScriptRandomizer scriptRand;

        /// <summary>
        /// Create a new randomizer with given data and settings
        /// Input data will be mutated by randomizer calls
        /// </summary>
        public Randomizer(RomData data, Settings settings, string seed)
        {
            this.data = data;
            this.settings = settings;
            // Initialize random generator
            rand = !string.IsNullOrEmpty(seed) ? new Random(seed) : new Random();
            data.Seed = rand.Seed;
            // Intialize evolution helper
            evoUtils = new EvolutionUtils(rand, data);
            //Initialize Species Randomizer
            var powerScores = PowerScaling.Calculate(data.Pokemon, settings.TieringOptions);
            pokeRand = new PkmnRandomizer(evoUtils, rand, data, data.Metrics, powerScores);
            // Initialize item randomizer
            itemRand = new ItemRandomizer(rand, settings.ItemRandomizationSettings, data);
            // Initialize encounter randomizer
            encounterRand = new WildEncounterRandomizer(pokeRand, evoUtils, data.Metrics, data);
            // Initialize Trainer randomizer
            trainerRand = new TrainerRandomizer(rand, pokeRand, evoUtils, data, settings);
            compatRand = new MoveCompatibilityRandomizer(rand, data);
            bonusMoveGenerator = new BonusMoveGenerator(rand, data, settings);
            variantRand = new PokemonVariantRandomizer(rand, data, settings, bonusMoveGenerator, data.PaletteOverrideKey);
            weatherRand = new WeatherRandomizer(rand);
            delayedRandomizationCalls = new List<Action>();
            scriptRand = new ScriptRandomizer(rand, pokeRand, itemRand, data, delayedRandomizationCalls);
        }
        // Apply mutations based on program settings.
        public RomData Randomize()
        {
            Timer.main.Start();
            delayedRandomizationCalls.Clear();
            var pokemonSet = DefinePokemonSet();
            var fossilSet = pokemonSet.Where(PokemonUtils.IsFossil).ToHashSet();
            if (settings.CountRelicanthAsFossil && pokemonSet.Contains(Pokemon.RELICANTH))
                fossilSet.Add(Pokemon.RELICANTH);
            var babySet = pokemonSet.Where(PokemonUtils.IsBaby).ToHashSet();
            var types = DefinePokemonTypes();
            var items = DefineItemSet();

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

            #region Move Definitions

            static void SetAccuracyAndPower(MoveData data, byte power, byte acc)
            {
                data.power = power;
                data.accuracy = acc;
            }
            // Update DOT moves to Gen V Power, ACC, and PP if applicable
            if (settings.UpdateDOTMoves)
            {
                // Bind: 75 -> 85 ACC
                data.GetMoveData(Move.BIND).accuracy = 85;
                // Wrap: 85 -> 90 ACC
                data.GetMoveData(Move.WRAP).accuracy = 90;
                // Fire Spin: 15 -> 35 Power, 70 -> 85 ACC
                SetAccuracyAndPower(data.GetMoveData(Move.FIRE_SPIN), 35, 85);
                // Whirlpool: 15 -> 35 Power, 70 -> 85 ACC
                SetAccuracyAndPower(data.GetMoveData(Move.WHIRLPOOL), 35, 85);
                // Sand Tomb: 15 -> 35 Power, 70 -> 85 ACC
                SetAccuracyAndPower(data.GetMoveData(Move.SAND_TOMB), 35, 85);
                // Sand Tomb: 70 -> 85 ACC, 10 -> 15 PP
                var clamp = data.GetMoveData(Move.CLAMP);
                clamp.pp = 15;
                clamp.accuracy = 85;
            }

            #endregion

            #region TMs, HMs, and Move Tutor Move Mappings

            // Get original TM palettes
            var tmTypePalettes = new Dictionary<PokemonType, int>();
            int tmInd = 0;
            foreach (var item in data.ItemData)
            {
                if (item.IsTM())
                {
                    var moveData = data.GetMoveData(data.TMMoves[tmInd++]);
                    if (!tmTypePalettes.ContainsKey(moveData.type))
                    {
                        tmTypePalettes.Add(moveData.type, item.paletteOffset);
                    }
                }
            }
            if (!tmTypePalettes.ContainsKey(PokemonType.BUG) && tmTypePalettes.ContainsKey(PokemonType.GRS))
            {
                tmTypePalettes.Add(PokemonType.BUG, tmTypePalettes[PokemonType.GRS]);
            }
            // Get Potential Move Choices
            var moves = EnumUtils.GetValues<Move>().ToHashSet();
            moves.Remove(Move.None); // Remove none as a possible choice
            // Remove HM moves if applicable
            if(settings.PreventHmMovesInTMsAndTutors)
            {
                foreach (var move in data.HMMoves)
                    moves.Remove(move);
            }
            // Remove important TMs that will be kept if applicable if no duplicates is on
            if (settings.KeepImportantTMsAndTutors && settings.PreventDuplicateTMsAndTutors)
            {
                moves.RemoveWhere(m => settings.ImportantTMsAndTutors.Contains(m) && data.TMMoves.Contains(m));
            }
            // Randomize TM mappings
            for(int i = 0; i < data.TMMoves.Length; ++i)
            {
                if (settings.KeepImportantTMsAndTutors && settings.ImportantTMsAndTutors.Contains(data.TMMoves[i]))
                    continue; // Important TM moves have already been removed from the move pool, so this will not cause duplicates
                if (rand.RollSuccess(settings.TMRandChance))
                    data.TMMoves[i] = rand.Choice(moves);
                if (settings.PreventDuplicateTMsAndTutors)
                    moves.Remove(data.TMMoves[i]);
            }
            // Log results
            if (settings.TMRandChance > 0)
            {
                data.RandomizationResults.Add("TM Moves", data.TMMoves.Select(EnumUtils.ToDisplayString).ToList());
            }
            // Randomize Move Tutor mappings
            for (int i = 0; i < data.tutorMoves.Length; ++i)
            {
                if (settings.KeepImportantTMsAndTutors && settings.ImportantTMsAndTutors.Contains(data.tutorMoves[i]))
                    continue; // Important Tutor moves have already been removed from the move pool, so this will not cause duplicates
                if (rand.RollSuccess(settings.MoveTutorRandChance))
                    data.tutorMoves[i] = rand.Choice(moves);
                if (settings.PreventDuplicateTMsAndTutors)
                    moves.Remove(data.tutorMoves[i]);
            }
            // Log results
            if(settings.MoveTutorRandChance > 0)
            {
                data.RandomizationResults.Add("Move Tutors", data.tutorMoves.Select(EnumUtils.ToDisplayString).ToList());
            }
            // Remap TM Pallets
            tmInd = 0;
            foreach (var item in data.ItemData)
            {
                if (item.IsTM())
                {
                    var moveData = data.GetMoveData(data.TMMoves[tmInd++]);
                    if (tmTypePalettes.TryGetValue(moveData.type, out int paletteOffset))
                    {
                        item.paletteOffset = paletteOffset;
                    }
                    item.Description = moveData.Description;
                    item.ReformatDescription = true;
                }
            }
            
            #endregion

            #region Item Definitions

            // Modify Custom Shop Item Price (if applicable)
            if (settings.AddCustomItemToPokemarts && settings.OverrideCustomMartItemPrice && settings.CustomMartItem != Item.None)
            {
                data.GetItemData(settings.CustomMartItem).Price = settings.CustomMartItemPrice;
            }

            #endregion

            #region Pokemon Base Attributes

            // Evolution Line Pass
            foreach (PokemonBaseStats pokemon in data.Pokemon)
            {
                if (!pokemon.IsBasic)
                    continue;
                // Variant Generation
                if (rand.RollSuccess(settings.VariantChance))
                {
                    variantRand.CreateVariant(pokemon, settings.VariantSettings);
                }
                // Bonus Move Generation
                if (settings.AddMoves && !pokemon.IsVariant && rand.RollSuccess(settings.AddMovesChance))
                {
                    int numMoves = rand.RandomGaussianPositiveNonZeroInt(settings.NumMovesMean, settings.NumMovesStdDeviation);
                    bonusMoveGenerator.GenerateBonusMoves(pokemon, numMoves, settings.AddMoveSourceWeights);
                }
            }

            var compatSettings = new MoveCompatibilityRandomizer.Data(settings.MoveCompatTrueChance, settings.IntelligentCompatNormalTrueChance, settings.IntelligentCompatTrueChance);

            // Individual Pokemon Pass
            foreach (PokemonBaseStats pokemon in data.Pokemon)
            {

                #region Evolutions
                // Fix Impossible Evolutions
                if (settings.FixImpossibleEvos)
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
                        else if (evo.Type == EvolutionType.Beauty && settings.ConsiderEvolveByBeautyImpossible)
                        {
                            MakeEvolutionByLevelUp(evo);
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
                        if (evo.Type == EvolutionType.LevelUp)
                        {
                            evo.Type = EvolutionType.LevelUpWithPersonality1;
                            int index = FirstEmptyEvo(pokemon.evolvesTo);
                            if (index >= 0)
                            {
                                pokemon.evolvesTo[index].Pokemon = Pokemon.DUNSPARCE;
                                pokemon.evolvesTo[index].Type = EvolutionType.LevelUpWithPersonality2;
                                pokemon.evolvesTo[index].parameter = evo.parameter;
                            }
                        }
                        else if (evo.Type == EvolutionType.Friendship)
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

                // Selfdestruct ban
                if (settings.BanSelfdestruct)
                {
                    pokemon.learnSet.RemoveWhere(m => data.GetMoveData(m.move).IsSelfdestruct);
                }

                #region TM, HM, and Move tutor Compatibility

                compatRand.RandomizeCompatibility(settings.MoveCompatSetting, pokemon.TMCompat, data.TMMoves, pokemon, compatSettings);
                compatRand.RandomizeCompatibility(settings.MoveCompatSetting, pokemon.moveTutorCompat, data.tutorMoves, pokemon, compatSettings);
                // If all on, force all HMs on
                // Else, if intelligent compatibility is on and the pokemon is a variant, intelligently set HM compat
                if(settings.ForceFullHmCompatibility || settings.MoveCompatSetting == Settings.MoveCompatOption.AllOn)
                {
                    compatRand.RandomizeHMCompat(Settings.MoveCompatOption.AllOn, pokemon.HMCompat, data.HMMoves, pokemon);
                }
                else if(settings.MoveCompatSetting == Settings.MoveCompatOption.Intelligent && pokemon.IsVariant)
                {
                    compatRand.RandomizeHMCompat(Settings.MoveCompatOption.Intelligent, pokemon.HMCompat, data.HMMoves, pokemon);
                }


                #endregion

                #region Catch Rates
                if (settings.CatchRateSetting != Settings.CatchRateOption.Unchanged)
                {
                    // Do not change if KeepLegendaryCatchRates is on AND this pokemon is a legendary
                    if (!settings.KeepLegendaryCatchRates || !pokemon.IsLegendary)
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

                #region Exp / EV Yields

                if(settings.BaseExpYieldMultiplier != 1)
                {
                    pokemon.baseExpYield = (byte)Math.Max(byte.MinValue, Math.Min(byte.MaxValue, Math.Floor(pokemon.baseExpYield * settings.BaseExpYieldMultiplier)));
                }

                if (settings.ZeroBaseEVs)
                {
                    for (int i = 0; i < pokemon.evYields.Length; i++)
                    {
                        pokemon.evYields[i] = 0;
                    }
                }

                #endregion
            }

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
                    int numStarters = Math.Min(data.Starters.Count, settings.CustomStarters.Length);
                    for (int i = 0; i < numStarters; i++)
                    {
                        Pokemon starter = settings.CustomStarters[i];
                        if(starter == Pokemon.None)
                        {
                            data.Starters[i] = pokeRand.RandomPokemon(pokemonSet, data.Starters[i], starterSettings, 5);
                        }
                        else
                        {
                            data.Starters[i] = starter;
                        }
                    }
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
                    var recievedPokemonData = data.GetBaseStats(trade.pokemonRecieved);
                    // If the recieved pokemon only has one ability, ensure that the trade data uses ability index 0
                    // If the recieved pokemon has two possible abilities, choose a random ability index
                    if (recievedPokemonData.abilities[1] == Ability.NONE)
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
                    delayedRandomizationCalls.Add(() =>
                    {
                        trade.heldItem = itemRand.RandomItem(items, trade.heldItem, settings.TradeHeldItemSettings);
                        if (trade.heldItem.IsMail())
                        {
                            if (trade.mailNum == 0xFF)
                                trade.mailNum = 0;
                        }
                        else
                        {
                            trade.mailNum = 0xFF;
                        }
                    });
                }
                if (settings.TradePokemonIVOption == Settings.TradePokemonIVSetting.Maximize)
                {
                    for (int i = 0; i < trade.IVs.Length; ++i)
                    {
                        trade.IVs[i] = InGameTrade.maxIV;
                    }
                }
                else if(settings.TradePokemonIVOption == Settings.TradePokemonIVSetting.Randomize)
                {
                    for (int i = 0; i < trade.IVs.Length; ++i)
                    {
                        trade.IVs[i] = (byte)rand.RandomInt(0, InGameTrade.maxIVPlus1);
                    }
                }
            }
            #endregion

            #region Maps
            var scriptRandomizationArgs = new ScriptRandomizer.Args
            {
                babySet = babySet,
                items = items,
                pokemonSet = pokemonSet,
                fossilSet = fossilSet,
            };
            // Initialize gym metadata
            var gymMetadataDict = new Dictionary<string, GymMetadata>(9); // 8 gyms + one temp space for invalid metadata
            // Randomize Maps (currently just iterate though the maps, but may want to construct and traverse a graph later)
            foreach (var map in data.Maps)
            {
                // If the map name is empty, just continue
                if (string.IsNullOrEmpty(map.Name))
                    continue;
                // Randomize Weather
                weatherRand.RandomizeWeather(map, settings);
                // Randomize Hidden Items
                foreach(var sEvent in map.eventData.signEvents)
                {
                    if(sEvent.IsHiddenItem)
                    {
                        if (settings.UseSeperateHiddenItemSettings)
                        {
                            if (rand.RollSuccess(settings.HiddenItemRandChance))
                            {
                                delayedRandomizationCalls.Add(() => sEvent.hiddenItem = itemRand.RandomItem(items, sEvent.hiddenItem, settings.HiddenItemSettings));
                            }
                        }
                        else if(rand.RollSuccess(settings.FieldItemRandChance))
                        {
                            delayedRandomizationCalls.Add(() => sEvent.hiddenItem = itemRand.RandomItem(items, sEvent.hiddenItem, settings.FieldItemSettings));
                        }
                    }
                }

                // Set Proper metadata on script randomizer arguments arg
                if (map.IsGym)
                {
                    // If the dictionary already contains the map name, it means this is a multi-map gym
                    if (!gymMetadataDict.ContainsKey(map.Name))
                    {
                        gymMetadataDict.Add(map.Name, new GymMetadata());
                    }
                    scriptRandomizationArgs.gymMetadata = gymMetadataDict[map.Name];
                }
                else
                {
                    scriptRandomizationArgs.gymMetadata = null;
                }

                // Randomize NPCs
                foreach (var npc in map.eventData.npcEvents)
                {
                    // Randomize NPC scripts
                    if(npc.script != null)
                    {
                        scriptRand.RandomizeScript(npc.script, settings, scriptRandomizationArgs);
                    }
                }

                // Remove invalid metadata
                if (map.IsGym && gymMetadataDict[map.Name].IsValid)
                {
                    gymMetadataDict.Remove(map.Name);
                }
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
                    Settings.DreamTeamSetting.Custom => settings.CustomDreamTeam.Where(p => p != Pokemon.None).ToArray(),
                    Settings.DreamTeamSetting.Random => dreamTeamRandomizer.GenerateDreamTeam(pokemonSet, settings.DreamTeamOptions),
                    _ => Array.Empty<Pokemon>(),
                };
                if(team.Length > 0)
                {
                    dreamTeamRandomizer.ApplyDreamTeam(data.FirstEncounterSet, team);
                    data.RandomizationResults.Add("Dream Team", new List<string> { string.Join(", ", team.Select(EnumUtils.ToDisplayString).ToArray()) });
                }
                else
                {
                    Logger.main.Warning($"Dream Team: Empty team detected with {settings.DreamTeamOption}. Dream Team will not be applied");
                }
            }

            #endregion

            #region Trainer + Trainer Organization Metadata Preprocessing

            foreach(var kvp in gymMetadataDict)
            {
                var gym = kvp.Value;
                gym.InitializeCategory();
                gym.InitializeThemeData(data);
            }

            // Construct trainer by name map and separate rival + catching tut trainer battles
            var normalTrainersByName = new Dictionary<string, List<Trainer>>(data.Trainers.Count);
            var rivalTrainers = new Dictionary<string, List<Trainer>>(3);
            var catchingTutorialTrainers = new List<Trainer>();
            foreach (var trainer in data.Trainers)
            {
                if (trainer.Invalid)
                {
                    continue;
                }
                string name = trainer.name.ToLower();

                // Theme Override

                if (settings.ApplyTheming(trainer))
                {
                    // If there is a special override for the trainer's class, use it
                    string trainerClass = trainer.Class.ToLower();
                    if (data.TrainerClassTypeOverrides.ContainsKey(trainerClass))
                    {
                        var overrideTypes = data.TrainerClassTypeOverrides[trainerClass];
                        trainer.ThemeData = new TrainerThemeData()
                        {
                            Theme = overrideTypes.Length > 0 ? TrainerThemeData.TrainerTheme.Typed : TrainerThemeData.TrainerTheme.Untyped,
                            Types = overrideTypes
                        };
                    }
                    else if (data.TrainerNameTypeOverrides.ContainsKey(name)) // If there a special type override for the trainer's name, use it
                    {
                        var overrideTypes = data.TrainerNameTypeOverrides[name];
                        trainer.ThemeData = new TrainerThemeData()
                        {
                            Theme = overrideTypes.Length > 0 ? TrainerThemeData.TrainerTheme.Typed : TrainerThemeData.TrainerTheme.Untyped,
                            Types = overrideTypes
                        };
                    }
                }

                // Sorting

                // Rivals
                if (trainer.TrainerCategory == Trainer.Category.Rival)
                {
                    if (!rivalTrainers.ContainsKey(name))
                    {
                        rivalTrainers.Add(name, new List<Trainer>(10));
                    }
                    rivalTrainers[name].Add(trainer);
                    continue;
                }
                // Catching Tut Trainers (Wally, Etc.)
                if (trainer.TrainerCategory == Trainer.Category.CatchingTutTrainer)
                {
                    catchingTutorialTrainers.Add(trainer);
                    continue;
                }
                // All other trainers
                if (!normalTrainersByName.ContainsKey(name))
                {
                    normalTrainersByName.Add(name, new List<Trainer>(10));
                }
                normalTrainersByName[name].Add(trainer);
            }

            #endregion

            #region Trainer Organizations

            foreach (var kvp in gymMetadataDict)
            {
                var gym = kvp.Value;
                gym.ApplyTrainerThemeData(settings);
            }

            foreach (var team in data.VillainousTeamMetadata)
            {
                team.ApplyTrainerThemeData(settings);
            }

            #endregion

            #region Trainer Battles
            var trainerSettings = settings.BasicTrainerSettings;
            // Randomize trainers
            foreach (var kvp in normalTrainersByName)
            {
                var battles = new List<Trainer>(kvp.Value);
                if (battles.Count <= 0)
                    continue;
                var firstBattle = battles[0];
                // Randomize the first battle
                trainerRand.Randomize(firstBattle, pokemonSet, trainerSettings, battles.Count <= 1);
                if (battles.Count <= 1)
                    continue;
                battles.RemoveAt(0);
                // Procedurally generate the rest of the battles
                trainerRand.RandomizeReoccurring(firstBattle, battles, pokemonSet, trainerSettings);
            }

            #region Rivals

            // Setup Rival Pokemon Settings
            var rivalSettings = settings.BasicTrainerSettings;
            bool originalStarters = settings.StarterSetting == Settings.StarterPokemonOption.Unchanged;
            foreach (var kvp in rivalTrainers)
            {
                var allBattles = kvp.Value;
                allBattles.Sort((a, b) => a.AvgLvl.CompareTo(b.AvgLvl));
                // Split the rival battles into their three different options
                var starters = new Pokemon[] { allBattles[0].pokemon[0].species, allBattles[1].pokemon[0].species, allBattles[2].pokemon[0].species };
                var rivalBattles = starters.Select(s => allBattles.Where(b => b.pokemon.Any(p => evoUtils.RelatedToOrSelf(p.species, s))).ToList()).ToArray();
                for (int i = 0; i < rivalBattles.Length; ++i)
                {
                    var battles = new List<Trainer>(rivalBattles[i]);
                    var firstBattle = battles[0];
                    battles.RemoveAt(0);
                    // Randomize the first battle
                    trainerRand.Randomize(firstBattle, pokemonSet, rivalSettings, false);
                    // Set the appropriate starter as the ace
                    firstBattle.pokemon[firstBattle.pokemon.Length - 1].species = data.Starters[originalStarters ? i : data.RivalRemap[i]];
                    // Procedurally generate the rest of the battles
                    trainerRand.RandomizeReoccurring(firstBattle, battles, pokemonSet, rivalSettings);
                    if (settings.EasyFirstRivalBattle)
                    {
                        foreach (var pokemon in firstBattle.pokemon)
                        {
                            pokemon.level = 1;
                        }
                    }
                }
            }

            #endregion

            #region Catching Tut Trainers (Wally)

            if (catchingTutorialTrainers.Count > 0)
            {
                // Randomize Wally starter if applicable
                if (settings.RandomizeWallyAce)
                {
                    // Remove all pokemon that cannot be male (the tutorial cutscene crashes if the catching tut pokemon isn't able to be male)
                    var possibleCatchingTutPokemon = new List<Pokemon>(pokemonSet);
                    possibleCatchingTutPokemon.RemoveAll(p => data.GetBaseStats(p).genderRatio > 0xFD);
                    var catchingTutSettings = new Settings.PokemonSettings()
                    {
                        BanLegendaries = settings.BanLegendaries(Trainer.Category.CatchingTutTrainer),
                        RestrictIllegalEvolutions = settings.TrainerRestrictIllegalEvolutions,
                        ForceHighestLegalEvolution = settings.TrainerForceHighestLegalEvolution,
                        Noise = 0,
                    };
                    data.CatchingTutPokemon = pokeRand.RandomPokemon(possibleCatchingTutPokemon, data.CatchingTutPokemon, catchingTutSettings, 5);
                }
                var wallyBattles = new List<Trainer>(catchingTutorialTrainers);
                wallyBattles.Sort((a, b) => a.AvgLvl.CompareTo(b.AvgLvl));
                var firstBattle = wallyBattles[0];
                wallyBattles.RemoveAt(0);
                // Set Wally's first pokemon to the catching tut pokemon
                trainerRand.Randomize(firstBattle, pokemonSet, rivalSettings, false);
                var firstBattleAce = firstBattle.pokemon[firstBattle.pokemon.Length - 1];
                firstBattleAce.species = evoUtils.MaxEvolution(data.CatchingTutPokemon, firstBattleAce.level, rivalSettings.RestrictIllegalEvolutions);
                // Procedurally generate the rest of Wally's battles
                trainerRand.RandomizeReoccurring(firstBattle, wallyBattles, pokemonSet, rivalSettings);
            }

            #endregion

            #endregion

            #region Items

            // Field items happen in map scripts

            // Randomize PC starting item
            if (settings.PcPotionOption == Settings.PcItemOption.Random)
            {
                delayedRandomizationCalls.Add(() => data.PcStartItem = itemRand.RandomItem(items, data.PcStartItem, settings.PcItemSettings));                
            }
            else if (settings.PcPotionOption == Settings.PcItemOption.Custom)
            {
                data.PcStartItem = settings.CustomPcItem;
            }

            // Randomize Pickup Items
            if (settings.PickupItemRandChance > 0) // if randomize
            {
                if(data.PickupItems.DataType == PickupData.Type.ItemsWithChances)
                {
                    for (int i = 0; i < data.PickupItems.ItemChances.Count; i++)
                    {
                        int index = i;
                        var itemChance = data.PickupItems.ItemChances[index];
                        delayedRandomizationCalls.Add(() =>
                        {
                            data.PickupItems.ItemChances[index] = new PickupData.ItemChance()
                            {
                                item = itemRand.RandomItem(items, itemChance.item, settings.PickupItemSettings),
                                chance = itemChance.chance
                            };
                        });
                    }
                }
                else // Rare + Common table
                {
                    for (int i = 0; i < data.PickupItems.Items.Count; i++)
                    {
                        int index = i;
                        delayedRandomizationCalls.Add(() =>
                        {
                            data.PickupItems.Items[index] = itemRand.RandomItem(items, data.PickupItems.Items[index], settings.PickupItemSettings);
                        });
                    }
                    for (int i = 0; i < data.PickupItems.RareItems.Count; i++)
                    {
                        int index = i;
                        delayedRandomizationCalls.Add(() =>
                        {
                            data.PickupItems.RareItems[index] = itemRand.RandomItem(items, data.PickupItems.RareItems[index], settings.PickupItemSettings);
                        });
                    }
                }
            }

            // Randomize Berry Trees
            RandomizeBerryTress(data.SetBerryTreeScript, settings, items);

            #endregion

            // Invoke delayed item randomizations in a random order
            while (delayedRandomizationCalls.Count > 0)
            {
                int i = rand.RandomInt(0, delayedRandomizationCalls.Count);
                delayedRandomizationCalls[i].Invoke();
                delayedRandomizationCalls.RemoveAt(i);
            }

#if DEBUG
            #region Debugging / Testing

            itemRand.LogOccurrences();

            #endregion
#endif

            Timer.main.Stop();
            Timer.main.Log("Randomization");

            return data;
        }

        #region Set Definitions

        /// <summary> Define and return the set of valid pokemon (with applicable restrictions)</summary>
        private HashSet<Pokemon> DefinePokemonSet()
        {
            //Start with all for now
            HashSet<Pokemon> pokemonSet = EnumUtils.GetValues<Pokemon>().ToHashSet();
            pokemonSet.Remove(Pokemon.None);
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

        private void RandomizeBerryTress(Script berryTreeScript, Settings s, IEnumerable<ItemData> allItems)
        {
            if (berryTreeScript == null || s.BerryTreeRandChance <= 0)
                return;
            var berries = allItems.Where(i => i.ItemCategories.HasFlag(ItemData.Categories.Berry)).ToList();
            // Remove EV berries if applicable
            if (s.BanEvBerries)
            {
                berries.RemoveAll(i => i.ItemCategories.HasFlag(ItemData.Categories.EVBerry));
            }
            // Remove Minigame berries if applicable
            if (s.BanMinigameBerries)
            {
                berries.RemoveAll(i => i.ItemCategories.HasFlag(ItemData.Categories.MinigameBerry));
            }
            if (s.RemapBerries)
            {
                var berrySet = berries.Select(i => i.Item).ToHashSet();
                var map = new Dictionary<Item, Item>(berrySet.Count);
                // Construct Remapping
                foreach (var command in data.SetBerryTreeScript)
                {
                    if (command is SetBerryTreeCommand berryTreeCommand && !map.ContainsKey(berryTreeCommand.berry))
                    {
                        var berry = berryTreeCommand.berry;
                        if (rand.RollSuccess(s.BerryTreeRandChance))
                        {
                            bool addBack = berrySet.Contains(berry);
                            if (addBack)
                            {
                                berrySet.Remove(berry);
                            }
                            if(berrySet.Count > 0)
                            {
                                var newBerry = rand.Choice(berrySet);
                                map.Add(berry, newBerry);
                                berrySet.Remove(newBerry);
                                if (addBack)
                                {
                                    berrySet.Add(berry);
                                }
                            }
                            else
                            {
                                map.Add(berryTreeCommand.berry, berryTreeCommand.berry);
                            }
                        }
                        else
                        {
                            map.Add(berryTreeCommand.berry, berryTreeCommand.berry);
                        }
                    }
                }
                // Perform remap
                foreach (var command in data.SetBerryTreeScript)
                {
                    if (command is SetBerryTreeCommand berryTreeCommand && map.ContainsKey(berryTreeCommand.berry))
                    {
                        berryTreeCommand.berry = map[berryTreeCommand.berry];
                    }
                }
            }
            else
            {
                // Randomize berry tree commands
                foreach (var command in data.SetBerryTreeScript)
                {
                    if (command is SetBerryTreeCommand berryTreeCommand && rand.RollSuccess(s.BerryTreeRandChance))
                    {
                        berryTreeCommand.berry = rand.Choice(berries).Item;
                    }
                }
            }
        }
    }
}