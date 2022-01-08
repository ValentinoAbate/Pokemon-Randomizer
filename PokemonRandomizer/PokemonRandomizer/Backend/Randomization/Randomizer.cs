using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.DataStructures.Scripts;
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
        private readonly List<Action> delayedRandomizationCalls;

        /// <summary>
        /// Create a new randomizer with given data and settings
        /// Input data will be mutated by randomizer calls
        /// </summary>
        public Randomizer(RomData data, Settings settings)
        {
            this.data = data;
            this.settings = settings;
            // Initialize random generator
            rand = settings.SetSeed ? new Random(settings.Seed.Trim()) : new Random();
            data.Seed = rand.Seed;
            // Intialize evolution helper
            evoUtils = new EvolutionUtils(rand, data);
            //Initialize Species Randomizer
            var powerScores = PowerScaling.Calculate(data.Pokemon, settings.TieringOptions);
            pokeRand = new PkmnRandomizer(evoUtils, rand, data, data.Metrics, powerScores);
            // Initialize item randomizer
            itemRand = new ItemRandomizer(rand, new ItemRandomizer.RandomizerSettings(), data);
            // Initialize encounter randomizer
            encounterRand = new WildEncounterRandomizer(pokeRand, evoUtils, data.Metrics, data);
            // Initialize Trainer randomizer
            trainerRand = new TrainerRandomizer(rand, pokeRand, evoUtils, data);
            compatRand = new MoveCompatibilityRandomizer(rand, data);
            bonusMoveGenerator = new BonusMoveGenerator(rand, data, settings);
            variantRand = new PokemonVariantRandomizer(rand, data, bonusMoveGenerator);
            delayedRandomizationCalls = new List<Action>();
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
                // Remap TM Pallets
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

            // Define Item Definitions
            // Hack in new items if applicable
            // Possible Hacks: Add GenIV items (some might not be possible), add fairy-related items
            // Mutate item definitions
            // Modify Custom Shop Item Price (if applicable)
            if (settings.AddCustomItemToPokemarts && settings.OverrideCustomMartItemPrice && settings.CustomMartItem != Item.None)
            {
                data.GetItemData(settings.CustomMartItem).Price = settings.CustomMartItemPrice;
            }

            #endregion

            #region Pokemon Base Attributes
            var availableAddMoves = EnumUtils.GetValues<Move>().ToHashSet();
            availableAddMoves.Remove(Move.None);
            if(settings.BanSelfdestruct)
            {
                availableAddMoves.RemoveWhere(m => data.GetMoveData(m).IsSelfdestruct);
            }
            if(settings.DisableAddingHmMoves)
            {
                foreach (var move in data.HMMoves)
                    availableAddMoves.Remove(move);
            }

            // Mutate Pokemon
            foreach (PokemonBaseStats pokemon in data.Pokemon)
            {
                if (pokemon.IsBasic && rand.RollSuccess(settings.VariantChance)) // || !pokemon.isBasic and not already a variant and chance
                {
                    variantRand.CreateVariant(pokemon, settings.VariantSettings);
                }

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
                        else if(evo.Type == EvolutionType.Beauty && settings.ConsiderEvolveByBeautyImpossible)
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

                #region Types
                // Mutate Pokemon Type
                if (pokemon.IsSingleTyped)
                {
                    if (rand.RollSuccess(settings.SingleTypeRandChance))
                    {
                        pokemon.SetSingleType(rand.Choice(data.Metrics.TypeRatiosSingle));
                    };
                }
                else
                {
                    if (rand.RollSuccess(settings.DualTypePrimaryRandChance))
                        pokemon.PrimaryType = rand.Choice(data.Metrics.TypeRatiosDualPrimary);
                    if (rand.RollSuccess(settings.DualTypeSecondaryRandChance))
                        pokemon.SecondaryType = rand.Choice(data.Metrics.TypeRatiosDualSecondary);
                }
                #endregion

                #region Learn Sets
                if(settings.BanSelfdestruct)
                {
                    pokemon.learnSet.RemoveWhere(m => data.GetMoveData(m.move).IsSelfdestruct);
                }
                if(settings.AddMoves && pokemon.IsBasic && !pokemon.IsVariant && rand.RollSuccess(settings.AddMovesChance))
                {
                    int numMoves = rand.RandomGaussianPositiveNonZeroInt(settings.NumMovesMean, settings.NumMovesStdDeviation);
                    bonusMoveGenerator.GenerateBonusMoves(pokemon, numMoves, settings.AddMoveSourceWeights);
                }
                #endregion

                #region TM, HM, and Move tutor Compatibility

                compatRand.RandomizeCompatibility(pokemon.TMCompat, new MoveCompatibilityRandomizer.Data(settings.TmCompatSetting, settings.MoveCompatTrueChance, settings.MoveCompatNoise, pokemon, data.TMMoves));
                compatRand.RandomizeCompatibility(pokemon.moveTutorCompat, new MoveCompatibilityRandomizer.Data(settings.MtCompatSetting, settings.MoveCompatTrueChance, settings.MoveCompatNoise, pokemon, data.tutorMoves));
                compatRand.RandomizeCompatibility(pokemon.HMCompat, new MoveCompatibilityRandomizer.Data(settings.HmCompatSetting, settings.MoveCompatTrueChance, settings.MoveCompatNoise, pokemon, data.HMMoves));

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
                        case GivePokedexCommand givePokedex:
                            if (settings.StartWithNationalDex)
                            {
                                givePokedex.Type = GivePokedexCommand.PokedexType.National;
                            }
                            break;
                        case GiveItemCommand giveItem:
                            if (giveItem.type == GiveItemCommand.Type.Normal && rand.RollSuccess(settings.FieldItemRandChance))
                            {
                                delayedRandomizationCalls.Add(() => giveItem.item = itemRand.RandomItem(items, giveItem.item, settings.FieldItemSettings));
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
                        case ShopCommand shopCommand:
                            if(settings.AddCustomItemToPokemarts && settings.CustomMartItem != Item.None && shopCommand.shop.items.Any(i => i == Item.Potion || i.IsPokeBall()))
                            {
                                shopCommand.shop.items.Add(settings.CustomMartItem);
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
                                delayedRandomizationCalls.Add(() => sEvent.hiddenItem = itemRand.RandomItem(items, sEvent.hiddenItem, settings.HiddenItemSettings));
                            }
                        }
                        else if(rand.RollSuccess(settings.FieldItemRandChance))
                        {
                            delayedRandomizationCalls.Add(() => sEvent.hiddenItem = itemRand.RandomItem(items, sEvent.hiddenItem, settings.FieldItemSettings));
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
                    Settings.DreamTeamSetting.Custom => settings.CustomDreamTeam.Where(p => p != Pokemon.None).ToArray(),
                    Settings.DreamTeamSetting.Random => dreamTeamRandomizer.GenerateDreamTeam(pokemonSet, settings.DreamTeamOptions),
                    _ => new Pokemon[0],
                };
                if(team.Length > 0)
                {
                    dreamTeamRandomizer.ApplyDreamTeam(data.FirstEncounterSet, team);
                }
                else
                {
                    Logger.main.Warning($"Dream Team: Empty team detected with {settings.DreamTeamOption}. Dream Team will not be applied");
                }
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
            bool originalStarters = settings.StarterSetting == Settings.StarterPokemonOption.Unchanged;
            foreach (var trainer in data.RivalNames)
            {
                var allBattles = data.SpecialTrainers[trainer.ToLower()];
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
                    firstBattle.pokemon[firstBattle.pokemon.Length - 1].species = data.Starters[originalStarters ? i: data.RivalRemap[i]];
                    // Procedurally generate the rest of the battles
                    trainerRand.RandomizeReoccurring(firstBattle, battles, pokemonSet, rivalSettings);
                    if (settings.EasyFirstRivalBattle)
                    {
                        foreach(var pokemon in firstBattle.pokemon)
                        {
                            pokemon.level = 1;
                        }
                    }
                }
            }

            #endregion

            #region Wally (TODO, make either gen-specific without metadata, or make gen-agnostic)

            const string wallyName = "wally";
            if(data.SpecialTrainers.ContainsKey(wallyName))
            {
                // Randomize Wally starter if applicable
                if (settings.RandomizeWallyAce)
                {
                    // Remove all pokemon that cannot be male (the tutorial cutscen crashes if the catching tut pokemon isn't able to be male)
                    var possibleCatchingTutPokemon = new List<Pokemon>(pokemonSet);
                    possibleCatchingTutPokemon.RemoveAll(p => data.GetBaseStats(p).genderRatio > 0xFD);
                    data.CatchingTutPokemon = pokeRand.RandomPokemon(possibleCatchingTutPokemon, data.CatchingTutPokemon, rivalSettings.PokemonSettings, 5);
                }

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
                        //data.PickupItems.Items[i] = itemRand.RandomItem(items, data.PickupItems.Items[i], settings.PickupItemSettings);
                    }
                    for (int i = 0; i < data.PickupItems.RareItems.Count; i++)
                    {
                        int index = i;
                        delayedRandomizationCalls.Add(() =>
                        {
                            data.PickupItems.RareItems[index] = itemRand.RandomItem(items, data.PickupItems.RareItems[index], settings.PickupItemSettings);
                        });
                        //data.PickupItems.RareItems[i] = itemRand.RandomItem(items, data.PickupItems.RareItems[i], settings.PickupItemSettings);
                    }
                }
            }

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