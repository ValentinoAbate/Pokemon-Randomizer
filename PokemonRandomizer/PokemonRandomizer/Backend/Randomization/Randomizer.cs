using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.DataStructures.Scripts;
using PokemonRandomizer.Backend.Metadata;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;
using PokemonRandomizer.Backend.Utilities.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using PokemonRandomizer.Backend.DataStructures.Trainers;

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
        private readonly TrainerOrganizationRandomizer trainerOrgRand;
        private readonly StarterRandomizer starterRandomizer;
        private readonly TypeChartRandomizer typeChartRandomizer;
        private readonly RomMetadata metadata;

        /// <summary>
        /// Create a new randomizer with given data and settings
        /// Input data will be mutated by randomizer calls
        /// </summary>
        public Randomizer(RomData data, RomMetadata metadata, Settings settings, string seed)
        {
            this.data = data;
            this.metadata = metadata;
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
            var paletteModifier = new VariantPaletteModifier();
            variantRand = new PokemonVariantRandomizer(rand, data, settings, bonusMoveGenerator, data.PaletteOverrideKey, paletteModifier);
            weatherRand = new WeatherRandomizer(rand);
            delayedRandomizationCalls = new List<Action>();
            scriptRand = new ScriptRandomizer(rand, pokeRand, itemRand, data, delayedRandomizationCalls);
            trainerOrgRand = new TrainerOrganizationRandomizer(rand, paletteModifier);
            starterRandomizer = new StarterRandomizer(pokeRand);
            typeChartRandomizer = new TypeChartRandomizer();
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
            var items = data.GetAllValidItemData();
            // Apply Allow Mystery Gift Item in Randomization if necessary
            if(settings.MysteryGiftItemAcquisitionSetting == Settings.MysteryGiftItemSetting.AllowInRandomization)
            {
                foreach (var item in data.MysteryGiftEventItems)
                {
                    if (item.IsMysteryGiftEventItem)
                    {
                        item.ItemCategories &= ~ItemData.Categories.KeyItem;
                        item.ItemCategories |= ItemData.Categories.Special;
                    }
                }
            }

            #region Type Definitions
            // Randomize type traits
            // Generate ??? type traits (INCOMPLETE)
            if (settings.ModifyUnknownType)
            {
                data.Types.Add(PokemonType.Unknown);
                foreach (var type in data.Types)
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
            // Modify type chart
            typeChartRandomizer.RandomizeTypeChart(data.TypeDefinitions, settings.TypeChartRandomizationSetting, data.RandomizationResults);
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
            Move[] originalTmMoves = new Move[data.TMMoves.Length];
            Array.Copy(data.TMMoves, originalTmMoves, data.TMMoves.Length);
            int tmInd = 0;
            foreach (var item in data.ItemData)
            {
                if (item.IsTM())
                {
                    var moveData = data.GetMoveData(data.TMMoves[tmInd++]);
                    if (item.paletteOffset != Rom.nullPointer && !tmTypePalettes.ContainsKey(moveData.type))
                    {
                        tmTypePalettes.Add(moveData.type, item.paletteOffset);
                    }
                }
            }
            if (!tmTypePalettes.ContainsKey(PokemonType.BUG) && tmTypePalettes.ContainsKey(PokemonType.GRS))
            {
                tmTypePalettes.Add(PokemonType.BUG, tmTypePalettes[PokemonType.GRS]);
            }
            if (!tmTypePalettes.ContainsKey(PokemonType.Unknown) && tmTypePalettes.ContainsKey(PokemonType.NRM))
            {
                // Set curse to normal palette
                tmTypePalettes.Add(PokemonType.Unknown, tmTypePalettes[PokemonType.NRM]);
            }
            // Get Potential Move Choices
            var moves = data.GetValidMoves(settings.PreventHmMovesInTMsAndTutors, false);
            // Remove important TMs that will be kept if applicable if no duplicates is on
            if (settings.KeepImportantTMsAndTutors && settings.PreventDuplicateTMsAndTutors)
            {
                foreach(var move in settings.ImportantTMsAndTutors)
                {
                    if(moves.Contains(move) && data.TMMoves.Contains(move))
                    {
                        moves.Remove(move);
                    }
                }
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
            if(settings.MoveTutorRandChance > 0 && data.tutorMoves.Length > 0)
            {
                data.RandomizationResults.Add("Move Tutors", data.tutorMoves.Select(EnumUtils.ToDisplayString).ToList());
            }
            // Remap TM Pallets and descriptions (if necessary)
            if(settings.TMRandChance > 0)
            {
                tmInd = 0;
                foreach (var item in data.ItemData)
                {
                    if (item.IsTM())
                    {
                        var originalMove = originalTmMoves[tmInd];
                        var moveData = data.GetMoveData(data.TMMoves[tmInd++]);
                        if (moveData.move == originalMove)
                        {
                            continue;
                        }
                        if (tmTypePalettes.TryGetValue(moveData.type, out int paletteOffset))
                        {
                            item.paletteOffset = paletteOffset;
                        }
                        item.Description = moveData.Description;
                        item.ReformatDescription = true;
                    }
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

                            newEvo.IntParameter = evoUtils.EquivalentLevelReq(evo, pokemon) + rand.RandomGaussianInt(0, settings.ImpossibleEvoLevelStandardDev);
                            newEvo.Type = EvolutionType.LevelUp;
                        }
                        else
                        {
                            evolveByLevelUp.Type = EvolutionType.LevelUpWithPersonality1;
                            newEvo.Type = EvolutionType.LevelUpWithPersonality2;
                            newEvo.IntParameter = evolveByLevelUp.IntParameter;
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
                                newEvo.ItemParamater = evo.ItemParamater;
                                // Log this as a new evolution stone if necessary
                                Item item = newEvo.ItemParamater;
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
                        else if (evo.Type == EvolutionType.FriendshipDay)
                        {
                            if (metadata.IsFireRedOrLeafGreen)
                            {
                                evo.Type = EvolutionType.UseItem;
                                evo.ItemParamater = Item.Sun_Stone;
                            }
                        }
                        else if (evo.Type == EvolutionType.FriendshipNight)
                        {
                            if (metadata.IsFireRedOrLeafGreen)
                            {
                                evo.Type = EvolutionType.UseItem;
                                evo.ItemParamater = Item.Moon_Stone;
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
                        if (evo.Type == EvolutionType.LevelUp)
                        {
                            evo.Type = EvolutionType.LevelUpWithPersonality1;
                            int index = FirstEmptyEvo(pokemon.evolvesTo);
                            if (index >= 0)
                            {
                                pokemon.evolvesTo[index].Pokemon = Pokemon.DUNSPARCE;
                                pokemon.evolvesTo[index].Type = EvolutionType.LevelUpWithPersonality2;
                                pokemon.evolvesTo[index].IntParameter = evo.IntParameter;
                            }
                        }
                        else if (evo.Type == EvolutionType.Friendship && !metadata.IsFireRedOrLeafGreen)
                        {
                            evo.Type = rand.RandomBool() ? EvolutionType.FriendshipDay : EvolutionType.FriendshipNight;
                            int index = FirstEmptyEvo(pokemon.evolvesTo);
                            if (index >= 0)
                            {
                                pokemon.evolvesTo[index].Pokemon = Pokemon.DUNSPARCE;
                                pokemon.evolvesTo[index].Type = evo.Type == EvolutionType.FriendshipDay ? EvolutionType.FriendshipNight : EvolutionType.FriendshipDay;
                                pokemon.evolvesTo[index].IntParameter = evo.IntParameter;
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

                #region Hatch Rates
                if (settings.FastHatching) 
                {
                    pokemon.eggCycles = 0;
                }
                #endregion

                #region Exp / EV Yields

                if (settings.BaseExpYieldMultiplier != 1)
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
            starterRandomizer.Randomize(data, pokemonSet, settings);
            #endregion

            #region In-Game Trades
            foreach (var trade in data.Trades)
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
                staticPokemonMap = new Dictionary<Pokemon, Pokemon>(),
                staticPokemonSet = new HashSet<Pokemon>(pokemonSet),
            };
            // Initialize gym metadata
            var gymMetadataDict = new Dictionary<string, GymMetadata>(16); // 8 gyms + extra room for invalid gyms
            // Randomize Maps (currently just iterate though the maps, but may want to construct and traverse a graph later)
            foreach (var map in data.Maps)
            {
                // If the map name is empty, just continue
                if (string.IsNullOrEmpty(map.Name))
                    continue;
                // Randomize Weather
                weatherRand.RandomizeWeather(map, settings);

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

                // Randomize Sign Events Items
                foreach (var sEvent in map.eventData.signEvents)
                {
                    // Hidden Items
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
                    else if (sEvent.IsScript)
                    {
                        // Randomize Sign Scripts
                        if(sEvent.script != null)
                        {
                            scriptRand.RandomizeScript(sEvent.script, settings, scriptRandomizationArgs);
                        }
                    }
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

                // Randomize Trigger Events
                foreach (var trigger in map.eventData.triggerEvents)
                {
                    if(trigger.script != null)
                    {
                        scriptRand.RandomizeScript(trigger.script, settings, scriptRandomizationArgs);
                    }
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

            // Trainer Orgs: Remove Invalid Data, adn Initialize Trainer Categories (if applicable)
            foreach(var key in gymMetadataDict.Keys)
            {
                if (!gymMetadataDict[key].IsValid)
                {
                    gymMetadataDict.Remove(key);
                }
            }
            foreach(var kvp in gymMetadataDict)
            {
                var gym = kvp.Value;
                gym.InitializeCategory();
                gym.Leaders.Sort(Trainer.AverageLevelComparer);
            }

            // Construct trainer by name map and separate rival + catching tut trainer battles
            var TrainersByName = new Dictionary<string, List<Trainer>>(data.Trainers.Count);
            var rivalTrainers = new Dictionary<string, List<Trainer>>(3);
            var catchingTutorialTrainers = new List<Trainer>();
            var gruntTrainers = new List<Trainer>();
            var eliteFourMetadata = new EliteFourMetadata();
            foreach (var trainer in data.AllTrainers)
            {
                if (trainer.Invalid)
                {
                    continue;
                }
                string name = trainer.Name.ToLower();

                // Theme Override

                if (settings.ApplyTheming(trainer))
                {
                    // If there is a special override for the trainer's class, use it
                    string trainerClass = trainer.ClassName.ToLower();
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
                // Grunts
                if (trainer.TrainerCategory == Trainer.Category.TeamGrunt)
                {
                    gruntTrainers.Add(trainer);
                    continue;
                }
                // All other trainers
                if (!TrainersByName.ContainsKey(name))
                {
                    TrainersByName.Add(name, new List<Trainer>(10));
                }
                TrainersByName[name].Add(trainer);
                if(trainer.TrainerCategory == Trainer.Category.Champion)
                {
                    eliteFourMetadata.Champion.Add(trainer);
                }
                else if(trainer.TrainerCategory == Trainer.Category.EliteFour)
                {
                    if (!eliteFourMetadata.EliteFour.ContainsKey(name))
                    {
                        eliteFourMetadata.EliteFour.Add(name, new EliteFourMetadata.EliteFourMember());
                    }
                    eliteFourMetadata.EliteFour[name].Add(trainer);
                }
            }

            // Trainer orgs: initialize theme data
            eliteFourMetadata.InitializeThemeData(data, settings);
            foreach (var kvp in gymMetadataDict)
            {
                var gym = kvp.Value;
                gym.InitializeThemeData(data, settings);
            }

            #endregion

            #region Trainer Organization Randomization

            // Only apply if trainer pokemon are randomized
            if (settings.RandomizeTrainerPokemon)
            {
                var gymsSorted = new List<GymMetadata>(gymMetadataDict.Values);
                gymsSorted.Sort((g1, g2) => Trainer.AverageLevelComparer(g1.Leaders[0], g2.Leaders[0]));
                trainerOrgRand.RandomizeGymsAndEliteFour(gymsSorted, eliteFourMetadata, data.Types, settings, data.RandomizationResults);
                trainerOrgRand.RandomizeVillainousTeams(data.VillainousTeamMetadata, data.Types, settings, data.RandomizationResults);

                foreach (var kvp in gymMetadataDict)
                {
                    var gym = kvp.Value;
                    gym.ApplyTrainerThemeData(settings);
                }

                foreach (var team in data.VillainousTeamMetadata)
                {
                    team.ApplyTrainerThemeData(settings);
                }

                eliteFourMetadata.ApplyTrainerThemeData(settings);
            }

            #endregion

            #region Trainer Battles
            var trainerSettings = settings.BasicTrainerSettings;
            // Randomize trainers
            foreach (var kvp in TrainersByName)
            {
                trainerRand.RandomizeAll(kvp.Value, pokemonSet, trainerSettings);
            }

            foreach(var trainer in gruntTrainers)
            {
                trainerRand.Randomize(trainer, pokemonSet, trainerSettings);
            }

            #region Rivals

            // Setup Rival Pokemon Settings
            bool originalStarters = settings.StarterSetting == Settings.StarterPokemonOption.Unchanged;
            foreach (var kvp in rivalTrainers)
            {
                var allBattles = kvp.Value;
                allBattles.Sort(Trainer.AverageLevelComparer);
                // Split the rival battles into their three different options
                var starters = new Pokemon[] { allBattles[0].Pokemon[0].species, allBattles[1].Pokemon[0].species, allBattles[2].Pokemon[0].species };
                var rivalBattles = starters.Select(s => allBattles.Where(b => b.Pokemon.Any(p => evoUtils.RelatedToOrSelf(p.species, s))).ToList()).ToArray();
                for (int i = 0; i < rivalBattles.Length; ++i)
                {
                    var battles = new List<Trainer>(rivalBattles[i]);
                    var firstBattle = battles[0];
                    battles.RemoveAt(0);
                    // Randomize the first battle
                    trainerRand.Randomize(firstBattle, pokemonSet, trainerSettings, false);
                    // Set the appropriate starter as the ace
                    firstBattle.Pokemon[^1].species = data.Starters[originalStarters ? i : data.RivalRemap[i]];
                    // Procedurally generate the rest of the battles
                    trainerRand.RandomizeReoccurring(firstBattle, battles, pokemonSet, trainerSettings);
                    if (settings.EasyFirstRivalBattle)
                    {
                        foreach (var pokemon in firstBattle.Pokemon)
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
                wallyBattles.Sort(Trainer.AverageLevelComparer);
                var firstBattle = wallyBattles[0];
                wallyBattles.RemoveAt(0);
                // Set Wally's first pokemon to the catching tut pokemon
                trainerRand.Randomize(firstBattle, pokemonSet, trainerSettings, false);
                var firstBattleAce = firstBattle.Pokemon[^1];
                firstBattleAce.species = evoUtils.MaxEvolution(data.CatchingTutPokemon, firstBattleAce.level, trainerSettings.RestrictIllegalEvolutions);
                // Procedurally generate the rest of Wally's battles
                trainerRand.RandomizeReoccurring(firstBattle, wallyBattles, pokemonSet, trainerSettings);
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
            HashSet<Pokemon> pokemonSet = new(data.Pokemon.Count);
            foreach(var pokemon in data.Pokemon)
            {
                if(pokemon.species != Pokemon.None)
                {
                    pokemonSet.Add(pokemon.species);
                }
            }
            if (pokemonSet.Contains(Pokemon.POKÉMON_EGG))
            {
                pokemonSet.Remove(Pokemon.POKÉMON_EGG);
            }
            if (pokemonSet.Contains(Pokemon.MANAPHY_EGG))
            {
                pokemonSet.Remove(Pokemon.MANAPHY_EGG);
            }
            return pokemonSet;
        }

        #endregion

        private void LogDuplicateTrainerPokemon()
        {
            var pokemonOccurrence = new Dictionary<Pokemon, int>(6);
            var duplicateOccurence = new Dictionary<int, int>(6);
            for (int i = 1; i < 7; i++)
            {
                duplicateOccurence.Add(i, 0);
            }
            foreach (var trainer in data.Trainers)
            {
                pokemonOccurrence.Clear();
                foreach (var pokemon in trainer.Pokemon)
                {
                    if (!pokemonOccurrence.ContainsKey(pokemon.species))
                    {
                        pokemonOccurrence.Add(pokemon.species, 1);
                    }
                    else
                    {
                        pokemonOccurrence[pokemon.species]++;
                    }
                }
                foreach (var pokemon in pokemonOccurrence)
                {
                    if (pokemon.Value > 1)
                    {
                        duplicateOccurence[pokemon.Value]++;
                    }
                }
            }

            Logger.main.Info("Duplicate Trainer Pokemon Count");
            Logger.main.Info($"Duplicates: {duplicateOccurence[2]}");
            Logger.main.Info($"Triplicates: {duplicateOccurence[3]}");
            Logger.main.Info($"Quadruplets: {duplicateOccurence[4]}");
            Logger.main.Info($"Quintuplets: {duplicateOccurence[5]}");
            Logger.main.Info($"Sextuplets: {duplicateOccurence[6]}");
        }

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