using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;
using PokemonRandomizer.Backend.Utilities.Debug;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using PokemonRandomizer.Backend.DataStructures.Scripts;
using PokemonRandomizer.Backend.Metadata;
using static PokemonRandomizer.Backend.DataStructures.MoveData;
using PokemonRandomizer.Backend.Constants;
using PokemonRandomizer.Backend.RomHandling.IndexTranslators;
using PokemonRandomizer.Backend.DataStructures.Trainers;
using static PokemonRandomizer.Backend.Randomization.VariantPaletteModifier;

namespace PokemonRandomizer.Backend.RomHandling.Parsing
{
    public class Gen3RomParser : RomParser
    {
        protected override IIndexTranslator IndexTranslator => Gen3IndexTranslator.Main;

        private readonly Gen3ScriptParser scriptParser;
        private readonly Gen3MapParser mapParser;
        private readonly Gen3PaletteParser paletteParser;

        public Gen3RomParser()
        {
            scriptParser = new Gen3ScriptParser();
            mapParser = new Gen3MapParser(scriptParser);
            paletteParser = new Gen3PaletteParser();
        }

        // Parse the ROM bytes into a RomData object
        public override RomData Parse(Rom rom, RomMetadata metadata, XmlManager info)
        {
            Timer.main.Start();
            scriptParser.Clear();
            RomData data = new RomData();

            // Type Definitions
            DefineTypes(data);

            #region Move Mappings (TMs/HMs/Tutors)
            //Read the TM move mappings from the ROM
            data.TMMoves = ReadMoveMappings(ElementNames.tmMoves, rom, info);
            //Read the HM move mappings from the ROM
            data.HMMoves = ReadMoveMappings(ElementNames.hmMoves, rom, info);
            //Read the move tutor move mappings from the ROM
            data.tutorMoves = ReadMoveMappings(ElementNames.tutorMoves, rom, info);
            #endregion

            data.MoveData = ReadMoves(rom, info);

            #region Base Stats
            // Read the pokemon base stats from the Rom and link dex orders
            var pokemon = ReadPokemonBaseStats(rom, info);
            // Find the offset of the eggMoves if we have the data
            int? eggMovesOffset = info.FindOffset(ElementNames.eggMoves, rom);
            if (eggMovesOffset.HasValue && eggMovesOffset.Value != Rom.nullPointer)
            {
                ReadEggMoves(rom, eggMovesOffset.Value, info, pokemon);
            }
            ReadNationalDexOrder(pokemon, rom, info);
            // Set the data on the RomData
            data.Pokemon = pokemon;
            #endregion

            // Read Starters
            data.Starters = ReadStarters(rom, info);
            //data.StarterItems = ReadStarterItems(rom, info);
            // Read Catching tutorial pokemon
            data.CatchingTutPokemon = ReadCatchingTutOpponent(rom, info);
            // Read in-game trades
            data.Trades = ReadInGameTrades(rom, info);
            // Read the PC item
            int pcPotionOffset = info.FindOffset(ElementNames.pcPotion, rom);
            if (pcPotionOffset != Rom.nullPointer)
            {
                data.PcStartItem = (Item)rom.ReadUInt16(pcPotionOffset);
            }
            // Trainers and associated data
            data.TrainerClasses = ReadTrainerClasses(rom, info);
            data.TrainerSprites = ReadTrainerSprites(rom, info);
            data.Trainers = ReadTrainers(rom, info, data.TrainerClasses, data.TrainerSprites);
            ReadStevenAllyTrainerBattle(rom, data, info);
            SetTrainerCategoryData(data, info);
            SetTrainerThemeOverrides(data, info);
            // Read type definitions
            data.TypeDefinitions = ReadTypeEffectivenessData(rom, info);
            // Read in the map data
            data.MapBanks = mapParser.ReadMapBanks(rom, info, metadata);
            data.Encounters = ReadEncounters(rom, info);
            data.FirstEncounterSet = FindFirstEncounter(data.Encounters, info);
            // Read in the item data
            data.ItemData = ReadItemData(rom, info, metadata, data.MysteryGiftEventItems);
            // Read in the pickup items
            data.PickupItems = ReadPickupData(rom, info, metadata);
            // Read berry tree script (if applicable)
            data.SetBerryTreeScript = ReadSetBerryTreeScript(rom, info, metadata);
            // Calculate the balance metrics from the loaded data
            data.CalculateMetrics();
            // Set PaletteOverrideKey for variant generator until I figure out a better way to do this
            data.PaletteOverrideKey = metadata.IsFireRedOrLeafGreen ? Randomization.PokemonVariantRandomizer.FRLGPalKey : string.Empty;
            Timer.main.Stop();
            Timer.main.Log("ROM Parsing");
            return data;
        }

        private void DefineTypes(RomData data)
        {
            data.Types.Clear();
            data.Types.AddRange(EnumUtils.GetValues<PokemonType>());
            data.Types.Remove(PokemonType.FAI);
            data.Types.Remove(PokemonType.Unknown);
        }

        // Read national dex order
        private void ReadNationalDexOrder(IList<PokemonBaseStats> pokemon, Rom rom, XmlManager info)
        {
            int[] order = new int[info.Num(ElementNames.pokemonBaseStats) + 1];
            int offset = info.FindOffset(ElementNames.nationalDexOrder, rom);
            if (offset == Rom.nullPointer)
                return;
            foreach (var p in pokemon)
            {
                p.NationalDexIndex = rom.ReadUInt16(offset + ((int)p.species - 1) * 2);
            }
        }
        // Read the move definitions
        private List<MoveData> ReadMoves(Rom rom, XmlManager info)
        {
            // Exit early if the offset doesn't exist (feature unsupported)
            if (!info.FindAndSeekOffset(ElementNames.moveData, rom))
                return new List<MoveData>();
            int moveCount = info.Num(ElementNames.moveData);
            var moveData = new List<MoveData>(moveCount);
            for (int i = 0; i <= moveCount; ++i)
            {
                var move = new MoveData()
                {
                    move = (Move)i,
                    effect = (MoveEffect)rom.ReadByte(),
                    power = rom.ReadByte(),
                    type = (PokemonType)rom.ReadByte(),
                    accuracy = rom.ReadByte(),
                    pp = rom.ReadByte(),
                    effectChance = rom.ReadByte(),
                    targets = (Targets)rom.ReadByte(),
                    priority = rom.ReadByte(),
                    flags = new BitArray(new byte[] { rom.ReadByte() }),
                };
                rom.Skip(3); // three bytes of 0x00
                moveData.Add(move);
            }
            if (!info.FindAndSeekOffset(ElementNames.moveDescriptions, rom))
                return moveData;
            for (int i = 1; i <= moveCount; ++i)
            {
                var move = moveData[i];
                move.Description = rom.ReadVariableLengthString(rom.ReadPointer());
                move.OrigininalDescriptionLength = move.Description.Length;
            }
            return moveData;
        }
        // Read TM, HM, or Move tutor definitions from the rom (depending on args)
        private Move[] ReadMoveMappings(string element, Rom rom, XmlManager info)
        {
            if (!info.FindAndSeekOffset(element, rom))
                return Array.Empty<Move>();
            int numToRead = info.Num(element);
            Move[] moves = new Move[numToRead];
            for (int i = 0; i < numToRead; i++)
                moves[i] = (Move)rom.ReadUInt16();
            return moves;
        }

        #region Read Pokemon Base Stats
        // Read the Pokemon base stat definitions from the ROM
        private List<PokemonBaseStats> ReadPokemonBaseStats(Rom rom, XmlManager info)
        {
            int numPokemonBaseStats = info.Num(ElementNames.pokemonBaseStats);
            int skipNum = info.IntAttr(ElementNames.pokemonBaseStats, "skip");
            var pokemon = new List<PokemonBaseStats>(numPokemonBaseStats - skipNum);

            #region Setup Offsets

            // Setup pokemon offset
            int pkmnOffset = info.FindOffset(ElementNames.pokemonBaseStats, rom);
            if (pkmnOffset == Rom.nullPointer)
                return pokemon;
            int pkmnSize = info.Size(ElementNames.pokemonBaseStats);
            // Setup evolution offset
            int evolutionOffset = info.FindOffset(ElementNames.evolutions, rom);
            if (evolutionOffset == Rom.nullPointer)
                return pokemon;
            int evolutionSize = info.Size(ElementNames.evolutions);
            int evolutionsPerPokemon = info.IntAttr(ElementNames.evolutions, AttributeNames.evolutionsPerPokemon);
            int evolutionPadding = info.Padding(ElementNames.evolutions);
            // setup TmHmCompat offset
            int tmHmCompatOffset = info.FindOffset(ElementNames.tmHmCompat, rom);
            if (tmHmCompatOffset == Rom.nullPointer)
                return pokemon;
            int tmHmSize = info.Size(ElementNames.tmHmCompat);
            int numTms = info.Num(ElementNames.tmMoves);
            int numHms = info.Num(ElementNames.hmMoves);
            // Setup Move Tutor Compat offset
            int tutorCompatOffset = info.FindOffset(ElementNames.tutorCompat, rom);
            int tutorSize = 0, numTutorMoves = 0;
            if (tutorCompatOffset != Rom.nullPointer)
            {
                tutorSize = info.Size(ElementNames.tutorCompat);
                numTutorMoves = info.Num(ElementNames.tutorMoves);
            }
            // Setup moveset offset
            int movesetTableOffset = info.FindOffset(ElementNames.movesets, rom);
            if (movesetTableOffset == Rom.nullPointer)
                return pokemon;
            // Setup Name offset
            int namesOffset = info.FindOffset(ElementNames.pokemonNames, rom);
            int nameLength = 0;
            if (namesOffset != Rom.nullPointer)
            {
                nameLength = info.Length(ElementNames.pokemonNames);
            }
            // Setup palette offsets
            int pokemonPaletteSize = info.Size(ElementNames.pokemonPalettes);
            int normalPaletteOffset = info.FindOffset(ElementNames.pokemonPalettes, rom);
            int shinyPaletteOffset = info.FindOffset(ElementNames.pokemonPalettesShiny, rom);
            #endregion

            // Find skip index if one exists
            int skipAt = info.HasElementWithAttr(ElementNames.pokemonBaseStats, "skipAt") ? info.IntAttr(ElementNames.pokemonBaseStats, "skipAt") : -1;
            for (int i = 1; i <= numPokemonBaseStats; i++)
            {
                if (i == skipAt) // potentially skip empty slots
                {
                    i += skipNum;
                }
                // Create Pokemon
                PokemonBaseStats pkmn = ReadBaseStatsSingle(rom, pkmnOffset + i * pkmnSize, InternalIndexToPokemon(i));
                // Read name
                pkmn.Name = namesOffset != Rom.nullPointer ? rom.ReadFixedLengthString(namesOffset + i * nameLength, nameLength) : pkmn.species.ToDisplayString();
                // Read Learn Set
                ReadLearnSet(rom, rom.ReadPointer(movesetTableOffset + (Rom.pointerSize * i)), out pkmn.learnSet);
                // Read Tm/Hm/Mt compat
                ReadTMHMCompat(rom, tmHmCompatOffset + i * tmHmSize, numTms, numHms, tmHmSize, out pkmn.TMCompat, out pkmn.HMCompat);
                ReadTutorCompat(rom, tutorCompatOffset + i * tutorSize, numTutorMoves, tutorSize, out pkmn.moveTutorCompat);
                ReadEvolutions(rom, evolutionOffset + i * evolutionSize, evolutionsPerPokemon, evolutionPadding, out pkmn.evolvesTo);
                ReadPalettes(rom, normalPaletteOffset + i * pokemonPaletteSize, shinyPaletteOffset + i * pokemonPaletteSize, pkmn);
                pokemon.Add(pkmn);
            }
            return pokemon;
        }

        private PokemonBaseStats ReadBaseStatsSingle(Rom rom, int offset, Pokemon species)
        {
            var pkmn = new PokemonBaseStats
            {
                // Set species
                species = species
            };
            // Seek the offset of the pokemon base stats data structure
            rom.Seek(offset);
            // fill in stats (hp/at/df/sp/sa/sd)
            pkmn.stats = rom.ReadBlock(6);
            // fill in types
            pkmn.types[0] = (PokemonType)rom.ReadByte();
            pkmn.types[1] = (PokemonType)rom.ReadByte();
            pkmn.catchRate = rom.ReadByte();
            pkmn.baseExpYield = rom.ReadByte();
            // fill in ev yields (stored in the first 12 bits of data[10-11])
            pkmn.evYields = rom.ReadBits(12, 2);
            pkmn.heldItems[0] = (Item)rom.ReadUInt16(); // (data[13] * 256 + data[12]);
            pkmn.heldItems[1] = (Item)rom.ReadUInt16(); // (data[15] * 256 + data[14]);
            pkmn.genderRatio = rom.ReadByte();
            pkmn.eggCycles = rom.ReadByte();
            pkmn.baseFriendship = rom.ReadByte();
            pkmn.growthType = (ExpGrowthType)rom.ReadByte();
            // fill in egg groups
            pkmn.eggGroups[0] = (EggGroup)rom.ReadByte();
            pkmn.eggGroups[1] = (EggGroup)rom.ReadByte();
            // fill in abilities
            pkmn.abilities[0] = (Ability)rom.ReadByte();
            pkmn.abilities[1] = (Ability)rom.ReadByte();
            pkmn.safariZoneRunRate = rom.ReadByte();
            byte searchFlip = rom.ReadByte();
            // read color
            pkmn.searchColor = (SearchColor)((searchFlip & 0b1111_1110) >> 1);
            // read flip
            pkmn.flip = (searchFlip & 0b0000_0001) == 1;
            return pkmn;
        }
        // Read the TMcompat and HM compat BitArrays starting at given offset
        private void ReadTMHMCompat(Rom rom, int offset, int numTms, int numHms, int compatSize, out BitArray tmCompat, out BitArray hmCompat)
        {
            tmCompat = new BitArray(numTms);
            hmCompat = new BitArray(numHms);
            byte[] tmHmChunk = rom.ReadBlock(offset, compatSize);
            int mask = 0;
            for (int p = 0; p < numTms + numHms; ++p)
            {
                if (p % 8 == 0) mask = 1;
                BitArray set = p < numTms ? tmCompat : hmCompat;
                set.Set(p < numTms ? p : p - numTms, (tmHmChunk[p / 8] & mask) > 0);
                mask <<= 1;
            }
        }
        // Read the move tutor compatibility BitArray at offset
        private void ReadTutorCompat(Rom rom, int offset, int numMoveTutors, int tutorCompatSize, out BitArray tutCompat)
        {
            if (numMoveTutors == 0)
            {
                tutCompat = new BitArray(numMoveTutors);
                return;
            }
            tutCompat = new BitArray(numMoveTutors);
            byte[] tutChunk = rom.ReadBlock(offset, tutorCompatSize);
            int mask = 0;
            for (int p = 0; p < numMoveTutors; ++p)
            {
                if (p % 8 == 0) mask = 1;
                tutCompat.Set(p, (tutChunk[p / 8] & mask) > 0);
                mask <<= 1;
            }
        }

        // Read Palettes
        private void ReadPalettes(Rom rom, int normalOffset, int shinyOffset, PokemonBaseStats pokemon)
        {
            rom.SaveAndSeekOffset(normalOffset);
            pokemon.palette = paletteParser.ParseCompressed(rom.ReadPointer(), rom);
            pokemon.paletteIndex = rom.ReadUInt16();
            rom.Seek(shinyOffset);
            pokemon.shinyPalette = paletteParser.ParseCompressed(rom.ReadPointer(), rom);
            pokemon.shinyPaletteIndex = rom.ReadUInt16();
            rom.LoadOffset();
        }
        #endregion

        // Read the starter pokemon
        private List<Pokemon> ReadStarters(Rom rom, XmlManager info)
        {
            var starters = new List<Pokemon>();
            if (!info.FindAndSeekOffset(ElementNames.starterPokemon, rom))
                return starters;
            starters.Add(InternalIndexToPokemon(rom.ReadUInt16()));
            rom.Skip(info.IntAttr(ElementNames.starterPokemon, "skip1"));
            starters.Add(InternalIndexToPokemon(rom.ReadUInt16()));
            rom.Skip(info.IntAttr(ElementNames.starterPokemon, "skip2"));
            starters.Add(InternalIndexToPokemon(rom.ReadUInt16()));
            return starters;
        }
        // Read the starter items
        private List<Item> ReadStarterItems(Rom rom, XmlManager info)
        {
            throw new NotImplementedException();
        }
        //Read the catching tut pokemon
        private Pokemon ReadCatchingTutOpponent(Rom rom, XmlManager info)
        {
            // Currently have no idea how to actually read this so just return RALTS
            // Maybe add a constant in the ROM info later
            return Pokemon.RALTS;
        }

        private List<InGameTrade> ReadInGameTrades(Rom rom, XmlManager info)
        {
            if (!info.FindAndSeekOffset(ElementNames.trades, rom))
                return new List<InGameTrade>();
            int numTrades = info.Num(ElementNames.trades);
            var trades = new List<InGameTrade>(numTrades);
            for (int i = 0; i < numTrades; ++i)
            {
                var t = new InGameTrade();
                t.pokemonName = rom.ReadFixedLengthString(InGameTrade.pokemonNameLength);
                t.pokemonRecieved = InternalIndexToPokemon(rom.ReadUInt16());
                t.IVs = rom.ReadBlock(6);
                t.abilityNum = rom.ReadUInt32();
                t.trainerID = rom.ReadUInt32();
                t.contestStats = rom.ReadBlock(5);
                rom.Skip(3);
                t.personality = rom.ReadUInt32();
                t.heldItem = (Item)rom.ReadUInt16();
                t.mailNum = rom.ReadByte();
                t.trainerName = rom.ReadFixedLengthString(InGameTrade.trainerNameLength);
                t.trainerGender = rom.ReadByte();
                t.sheen = rom.ReadByte();
                t.pokemonWanted = InternalIndexToPokemon(rom.ReadUInt32());
                trades.Add(t);
            }
            return trades;
        }

        private List<TrainerClass> ReadTrainerClasses(Rom rom, XmlManager info)
        {
            if (!info.FindAndSeekOffset(ElementNames.trainerClassNames, rom))
                return new List<TrainerClass>();
            int numClasses = info.Num(ElementNames.trainerClassNames);
            int nameLength = info.Length(ElementNames.trainerClassNames);
            var classes = new List<TrainerClass>(numClasses);
            for (int i = 0; i < numClasses; ++i)
            {
                var trainerClass = new TrainerClass()
                {
                    ClassNum = i,
                    Name = rom.ReadFixedLengthString(nameLength)
                };
                classes.Add(trainerClass);
            }
            return classes;
        }

        private List<TrainerSprite> ReadTrainerSprites(Rom rom, XmlManager info)
        {
            // Read front palettes
            if (!info.FindAndSeekOffset(ElementNames.trainerPalettesFront, rom))
                return new List<TrainerSprite>();
            int numSprites = info.Num(ElementNames.trainerSprites);
            var sprites = new List<TrainerSprite>(numSprites);
            for (int i = 0; i < numSprites; ++i)
            {
                sprites.Add(new TrainerSprite { 
                    FrontPalette = paletteParser.ParseCompressed(rom.ReadPointer(), rom),
                });
                rom.Skip(4); // uint16 palette index and uint16 unused
            }
            return sprites;
        }

        // Readainers
        private List<BasicTrainer> ReadTrainers(Rom rom, XmlManager info, List<TrainerClass> trainerClasses, List<TrainerSprite> trainerSprites)
        {
            // If fail, reading trainer battles is not supported for this ROM
            if (!info.FindAndSeekOffset(ElementNames.trainerBattles, rom))
                return new List<BasicTrainer>();
            int numTrainers = info.Num(ElementNames.trainerBattles);
            var ret = new List<BasicTrainer>(numTrainers);
            for (int trainerInd = 0; trainerInd < numTrainers; ++trainerInd)
            {
                var trainer = new BasicTrainer();
                var dataType = (TrainerPokemon.DataType)rom.ReadByte();
                trainer.trainerClass = rom.ReadByte();
                trainer.Class = trainerClasses[trainer.trainerClass];
                // Read Gender (byte 2 bit 0)
                trainer.gender = (Gender)((rom.Peek() & 0x80) >> 7);
                // Read music track index (byte 2 bits 1-7)
                trainer.musicIndex = (byte)(rom.ReadByte() & 0x7F);
                // Read sprite index (byte 3)
                trainer.spriteIndex = rom.ReadByte();
                trainer.Sprite = trainerSprites[trainer.spriteIndex];
                // Read name (I think bytes 4 - 15?)
                trainer.Name = rom.ReadFixedLengthString(Trainer.nameLength);
                // Read items (bytes 16-23)
                for (int itemInd = 0; itemInd < 4; ++itemInd)
                {
                    trainer.useItems[itemInd] = InternalIndexToItem(rom.ReadUInt16());
                }
                // Read double battle (byte 24)
                trainer.IsDoubleBattle = rom.ReadByte() == 1;
                // What is in bytes 25-27?
                rom.Skip(3);
                // Read AI flags
                trainer.AIFlags = new BitArray(new int[] { rom.ReadUInt32() });
                int numPokemon = rom.ReadByte();
                // What is in bytes 33-35?
                rom.Skip(3);
                // Bytes 36-39 (end of data)
                trainer.pokemonOffset = rom.ReadPointer();

                // Save the internal offset before chasing pointers
                rom.SaveOffset();
                #region Read pokemon from pokemonOffset
                rom.Seek(trainer.pokemonOffset);
                trainer.PokemonData = new Trainer.TrainerPokemonData(dataType, numPokemon);
                // The pokemon data structures will be either 8 or 16 bytes depending on the dataType of the trainer
                for (int pokemonInd = 0; pokemonInd < numPokemon; ++pokemonInd)
                {
                    var p = new TrainerPokemon();
                    p.dataType = dataType;
                    p.IVLevel = rom.ReadUInt16();
                    p.level = rom.ReadUInt16();
                    p.species = InternalIndexToPokemon(rom.ReadUInt16());
                    if (dataType == TrainerPokemon.DataType.Basic)
                    {
                        rom.Skip(2); // Skip padding
                    }
                    else if (dataType == TrainerPokemon.DataType.HeldItem)
                    {
                        p.heldItem = InternalIndexToItem(rom.ReadUInt16());
                    }
                    else if (dataType == TrainerPokemon.DataType.SpecialMoves)
                    {
                        for (int moveInd = 0; moveInd < TrainerPokemon.numMoves; ++moveInd)
                            p.moves[moveInd] = InternalIndexToMove(rom.ReadUInt16());
                        rom.Skip(2);
                    }
                    else if (dataType == TrainerPokemon.DataType.SpecialMovesAndHeldItem)
                    {
                        p.heldItem = InternalIndexToItem(rom.ReadUInt16());
                        for (int moveInd = 0; moveInd < TrainerPokemon.numMoves; ++moveInd)
                            p.moves[moveInd] = InternalIndexToMove(rom.ReadUInt16());
                    }
                    trainer.PokemonData.Pokemon.Add(p);
                }
                #endregion
                // Return to the trainers
                rom.LoadOffset();
                ret.Add(trainer);
            }
            return ret;
        }

        private void ReadStevenAllyTrainerBattle(Rom rom, RomData data, XmlManager info)
        {
            if (!info.FindAndSeekOffset(ElementNames.GenIII.stevenAllyBattle, rom))
            {
                return;
            }
            var stevenAllyBattle = new StevenAllyTrainer() 
            { 
                pokemonOffset = rom.InternalOffset
            };
            stevenAllyBattle.PokemonData = new Trainer.TrainerPokemonData(TrainerPokemon.DataType.SpecialMoves, Trainer.multiBattlePartySize);
            for (int i = 0; i < Trainer.multiBattlePartySize; ++i)
            {
                var pokemon = new StevenAllyTrainerPokemon()
                {
                    dataType = TrainerPokemon.DataType.SpecialMoves,
                    species = InternalIndexToPokemon(rom.ReadUInt16()),
                    IVLevel = rom.ReadByte(),
                    level = rom.ReadByte(),
                    Nature = rom.ReadByte(),
                    EVs = rom.ReadBlock(PokemonBaseStats.numStats),
                };
                rom.Skip(); // padding 0x00
                for (int moveInd = 0; moveInd < TrainerPokemon.numMoves; ++moveInd)
                {
                    pokemon.moves[moveInd] = InternalIndexToMove(rom.ReadUInt16());
                }
                stevenAllyBattle.PokemonData.Pokemon.Add(pokemon);
            }
            data.SpecialTrainers.Add(StevenAllyTrainer.specialTrainerKey, new List<Trainer>() { stevenAllyBattle });
        }
        private class VillainousTeamInfo
        {
            public string LeaderClassName { get; set; }
            public string AdminClassName { get; set; }
            public string GruntClassName { get; set; }
            public List<string> AdminNames { get; } = new();
            public VillainousTeamMetadata TeamData { get; } = new();

            public bool TryProcessMember(Trainer trainer)
            {
                string className = trainer.ClassName.ToLower();
                if (className == GruntClassName)
                {
                    TeamData.TeamGrunts.Add(trainer);
                    trainer.TrainerCategory = Trainer.Category.TeamGrunt;
                }
                if (className == LeaderClassName)
                {
                    TeamData.TeamLeaders.Add(trainer);
                    trainer.TrainerCategory = Trainer.Category.TeamLeader;
                    return true;
                }
                if (className == AdminClassName || AdminNames.Contains(trainer.Name.ToLower()))
                {
                    TeamData.TeamAdmins.Add(trainer);
                    trainer.TrainerCategory = Trainer.Category.TeamAdmin;
                    return true;
                }
                return false;
            }
        }

        private const string wallyName = "wally";

        private static readonly Dictionary<string, PaletteData> villaneousTeamPaletteData = new()
        {
            { "aquaGruntMasc", new PaletteData(Range(5, 8)) },
            { "aquaGruntFem", new PaletteData(Range(5, 8)) },
            { "aquaAdminMatt", new PaletteData(Range(5, 8)) },
            { "aquaAdminShelley", new PaletteData(Range(5, 8)) },
            { "betaArchie", new PaletteData(Range(5, 8)) },
            { "archie", new PaletteData(PalRange(5, 6, 7, 8, 11), null, PalRange(10, 12, 13)) },
            { "magmaGruntMasc", new PaletteData(PalRange(4, 10, 12, 13)) },
            { "magmaGruntFem", new PaletteData(PalRange(4, 10, 12, 13)) },
            { "maxie", new PaletteData(PalRange(4, 10, 12, 13)) },
            { "magmaAdminTabitha", new PaletteData(PalRange(4, 10, 12, 13)) },
            { "magmaAdminCourtney", new PaletteData(PalRange(4, 10, 12, 13)) },
            { "rocketGruntMasc", new PaletteData(PalRange(5, 6, 7, 12, 13, 14)) },
            { "rocketGruntFem", new PaletteData(PalRange(5, 6, 7, 12, 13, 14)) },
            { "giovanni", new PaletteData(Range(9, 12)) }, // maybe don't use
        };
        /// <summary>
        /// Read all the preset trainer data from the info file into the ROM data, and find normal grunt, ace, and reocurring trainers
        /// </summary>
        private void SetTrainerCategoryData(RomData data, XmlManager info)
        {
            // Setup team metadata
            var teamNames = info.ArrayAttr(ElementNames.teamData, AttributeNames.elementNames);
            var allTeams = new List<VillainousTeamInfo>(2);
            foreach (var name in teamNames)
            {
                if (!info.HasElement(name))
                {
                    Logger.main.Error($"Parsing Error: Invalid villainous team name (element not found) {name}");
                    continue;
                }
                var teamData = new VillainousTeamInfo()
                {
                    LeaderClassName = info.AttrLowerCase(name, "leaderClass"),
                    AdminClassName = info.AttrLowerCase(name, "adminClass"),
                    GruntClassName = info.AttrLowerCase(name, "gruntClass")
                };
                var adminNames = info.ArrayAttrLowerCase(name, "adminNames");
                if (adminNames.Length > 0)
                {
                    teamData.AdminNames.AddRange(adminNames);
                }
                var spriteNums = info.IntArrayAttr(name, "sprites");
                var spriteKeys = info.ArrayAttr(name, "spriteKeys");
                for(int i = 0; i < spriteNums.Length && i < spriteKeys.Length; i++)
                {
                    var palKey = spriteKeys[i];
                    if (!villaneousTeamPaletteData.ContainsKey(palKey))
                    {
                        continue;
                    }
                    var palette = data.TrainerSprites[spriteNums[i]].FrontPalette;
                    var palData = villaneousTeamPaletteData[palKey];
                    teamData.TeamData.Palettes.Add((palette, palData, palKey == "giovanni"));
                }
                teamData.TeamData.DefaultPrimaryTypes = info.TypeArrayAttr(name, "primaryTypes");
                teamData.TeamData.DefaultSecondaryTypes = info.TypeArrayAttr(name, "secondaryTypes");
                allTeams.Add(teamData);
            }
            // Get leader, elite four, and champion class names
            var leaderClass = info.AttrLowerCase(ElementNames.gymLeaders, AttributeNames.className);
            var eliteFourClass = info.AttrLowerCase(ElementNames.eliteFour, AttributeNames.className);
            var championClass = info.AttrLowerCase(ElementNames.champion, AttributeNames.className);
            // Get rival names and remap info
            var rivalNames = info.ArrayAttrLowerCase(ElementNames.rivals, AttributeNames.names);
            data.RivalRemap = info.IntArrayAttr(ElementNames.rivals, "remap");
            // Get special boss info
            var specialBossNames = info.ArrayAttrLowerCase(ElementNames.specialBosses, AttributeNames.names);
            // Fetch the Ace Trainer Class Numbers for this ROM
            var aceTrainersClasses = info.IntArrayAttr(ElementNames.aceTrainers, "classNums");
            foreach (var trainer in data.AllTrainers)
            {
                if (trainer.Invalid)
                    continue;
                string trainerClass = trainer.ClassName.ToLower();
                string name = trainer.Name.ToLower();
                if (name == wallyName)
                {
                    trainer.TrainerCategory = Trainer.Category.CatchingTutTrainer;
                }
                else if (rivalNames.Contains(name)) // First rival battle will fit the "placeholder" criteria
                {
                    trainer.TrainerCategory = Trainer.Category.Rival;
                }
                else if (aceTrainersClasses.Contains(trainer.trainerClass))
                {
                    trainer.TrainerCategory = Trainer.Category.AceTrainer;
                }
                else if (IsPlaceholder(trainer))
                {
                    continue;
                }
                else if (trainerClass == leaderClass)
                {
                    trainer.TrainerCategory = Trainer.Category.GymLeader;
                }
                else if (trainerClass == eliteFourClass)
                {
                    trainer.TrainerCategory = Trainer.Category.EliteFour;
                }
                else if (trainerClass == championClass)
                {
                    trainer.TrainerCategory = Trainer.Category.Champion;
                }
                else if (specialBossNames.Contains(name))
                {
                    trainer.TrainerCategory = Trainer.Category.SpecialBoss;
                }
                else // See if this trainer is a member of a villainous team
                {
                    foreach (var team in allTeams)
                    {
                        if (team.TryProcessMember(trainer))
                        {
                            break;
                        }
                    }
                }
            }
            // Add valid team data to ROM data
            foreach (var team in allTeams)
            {
                if (team.TeamData.IsValid)
                {
                    data.VillainousTeamMetadata.Add(team.TeamData);
                }
            }
        }

        private void SetTrainerThemeOverrides(RomData data, XmlManager info)
        {
            var nameOverrides = info.ArrayAttr(ElementNames.nameTrainerTypeOverrides, AttributeNames.elementNames);
            foreach (var nameOverride in nameOverrides)
            {
                string name = info.Attr(nameOverride, AttributeNames.name);
                var types = info.TypeArrayAttr(nameOverride, AttributeNames.types);
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }
                data.TrainerNameTypeOverrides.Add(name, types);
            }
            var classOverrides = info.ArrayAttr(ElementNames.classTrainerTypeOverrides, AttributeNames.elementNames);
            foreach (var classOverride in classOverrides)
            {
                string className = info.Attr(classOverride, AttributeNames.className);
                var types = info.TypeArrayAttr(classOverride, AttributeNames.types);
                if (string.IsNullOrWhiteSpace(className))
                {
                    continue;
                }
                data.TrainerClassTypeOverrides.Add(className, types);
            }
        }

        private bool IsPlaceholder(Trainer trainer)
        {
            return trainer.Pokemon.Count == 1 && trainer.Pokemon[0].level == 5;
        }

        // Read encounters
        private List<EncounterSet> ReadEncounters(Rom rom, XmlManager info)
        {
            const string wildPokemonElt = "wildPokemon";
            var encounters = new List<EncounterSet>();
            // Seek data offset or return early if not found
            if (!info.FindAndSeekOffset(wildPokemonElt, rom))
                return encounters;

            // Get encounter slot sizes
            int grassSlots = info.IntAttr(wildPokemonElt, "grassSlots");
            int surfSlots = info.IntAttr(wildPokemonElt, "surfSlots");
            int rockSmashSlots = info.IntAttr(wildPokemonElt, "rockSmashSlots");
            int fishSlots = info.IntAttr(wildPokemonElt, "fishSlots");

            // Iterate until the ending marker (0xff, 0xff)
            while (rom.Peek() != 0xff || rom.Peek(1) != 0xff)
            {
                int bank = rom.ReadByte();
                int map = rom.ReadByte();
                // Idk what the next two bytes are
                rom.Skip(2);
                int grassPtr = rom.ReadPointer();
                int surfPtr = rom.ReadPointer();
                int rockSmashPtr = rom.ReadPointer();
                int fishPtr = rom.ReadPointer();
                // Save the internal offset before chasing pointers
                rom.SaveOffset();

                #region Load the actual Encounter sets for this area
                if (grassPtr > 0 && grassPtr < rom.Length)
                {
                    var grassPokemon = new EncounterSet(EncounterSet.Type.Grass, bank, map, rom, grassPtr, grassSlots);
                    encounters.Add(grassPokemon);
                    // TODO: Log in map
                }
                if (surfPtr > 0 && surfPtr < rom.Length)
                {
                    var surfPokemon = new EncounterSet(EncounterSet.Type.Surf, bank, map, rom, surfPtr, surfSlots);
                    encounters.Add(surfPokemon);
                    // TODO: Log in map
                }
                if (rockSmashPtr > 0 && rockSmashPtr < rom.Length)
                {
                    var rockSmashPokemon = new EncounterSet(EncounterSet.Type.RockSmash, bank, map, rom, rockSmashPtr, rockSmashSlots);
                    encounters.Add(rockSmashPokemon);
                    // TODO: Log in map
                }
                if (fishPtr > 0 && fishPtr < rom.Length)
                {
                    var fishPokemon = new EncounterSet(EncounterSet.Type.Fish, bank, map, rom, fishPtr, fishSlots);
                    encounters.Add(fishPokemon);
                    // TODO: Log in map
                }
                #endregion

                // Load the saved offset to check the next header
                rom.LoadOffset();
            }

            return encounters;
        }

        private EncounterSet FindFirstEncounter(IEnumerable<EncounterSet> encounters, XmlManager info)
        {
            if (!info.HasElement(ElementNames.firstEncounter))
            {
                // TODO: log
                return null;
            }
            if (!info.HasElementWithAttr(ElementNames.firstEncounter, "map"))
            {
                // TODO: log
                return null;
            }
            if (!info.HasElementWithAttr(ElementNames.firstEncounter, "bank"))
            {
                // TODO: log
                return null;
            }
            int map = info.IntAttr(ElementNames.firstEncounter, "map");
            int bank = info.IntAttr(ElementNames.firstEncounter, "bank");
            foreach (var encounter in encounters)
            {
                if (encounter.map == map && encounter.bank == bank && encounter.type == EncounterSet.Type.Grass)
                    return encounter;
            }
            return null;
        }

        // Read Type Effectiveness data
        private TypeEffectivenessChart ReadTypeEffectivenessData(Rom rom, XmlManager info)
        {
            var ret = new TypeEffectivenessChart();
            if (!info.FindAndSeekOffset(ElementNames.typeEffectiveness, rom))
                return ret;
            bool ignoreAfterForesight = false;
            // Run until the end of structure sequence (0xff 0xff 0x00)
            while (rom.Peek() != 0xff)
            {
                // Skip the ignoreAfterForesight separator (0xfe 0xfe 0x00)
                if (rom.Peek() == 0xfe)
                {
                    ignoreAfterForesight = true;
                    rom.Skip(3);
                }
                PokemonType attackingType = (PokemonType)rom.ReadByte();
                PokemonType defendingType = (PokemonType)rom.ReadByte();
                TypeEffectiveness ae = (TypeEffectiveness)rom.ReadByte();
                ret.Add(attackingType, defendingType, ae, ignoreAfterForesight);
            }
            ret.InitCount = ret.Count;
            return ret;
        }

        private List<ItemData> ReadItemData(Rom rom, XmlManager info, RomMetadata metadata, List<ItemData> mysteryGiftEventItems)
        {
            if (!info.FindAndSeekOffset(ElementNames.itemData, rom))
                return new List<ItemData>();
            int numItemDefinitions = info.Num(ElementNames.itemData);
            var itemData = new List<ItemData>(numItemDefinitions);
            for (int i = 0; i < numItemDefinitions; ++i)
            {
                var item = new ItemData
                {
                    Name = rom.ReadFixedLengthString(ItemData.nameLength),
                    Item = (Item)rom.ReadUInt16(),
                    Price = rom.ReadUInt16(),
                    holdEffect = rom.ReadByte(),
                    param = rom.ReadByte(),
                    descriptionOffset = rom.ReadPointer(),
                    keyItemValue = rom.ReadByte(),
                    RegisterableKeyItem = rom.ReadByte() == 1,
                    pocket = rom.ReadByte(),
                    type = rom.ReadByte(),
                    fieldEffectOffset = rom.ReadPointer(),
                    battleUsage = rom.ReadUInt32(),
                    battleEffectOffset = rom.ReadPointer(),
                    extraData = rom.ReadUInt32(),
                };
                if(item.IsMysteryGiftEventItem = IsMysteryGiftEventItem(item, metadata))
                {
                    mysteryGiftEventItems.Add(item);
                }
                item.Description = rom.ReadVariableLengthString(item.descriptionOffset);
                item.SetCategoryFlags();
                item.SetOriginalValues();
                itemData.Add(item);
            }
            // If we have the offset for the item sprites, read the item sprite data
            if (!info.FindAndSeekOffset(ElementNames.itemSprites, rom))
                return itemData;
            foreach (var item in itemData)
            {
                item.spriteOffset = rom.ReadPointer();
                item.paletteOffset = rom.ReadPointer();
            }
            return itemData;
        }

        private bool IsMysteryGiftEventItem(ItemData item, RomMetadata metadata)
        {
            return item.Item switch
            {
                Item.Eon_Ticket => metadata.IsRubySapphireOrEmerald,
                Item.MysticTicket => metadata.IsEmerald || metadata.IsFireRedOrLeafGreen,
                Item.AuroraTicket => metadata.IsEmerald || metadata.IsFireRedOrLeafGreen,
                Item.Old_Sea_Map => metadata.IsEmerald,
                _ => false,
            };
        }

        private PickupData ReadPickupData(Rom rom, XmlManager info, RomMetadata metadata)
        {
            var data = new PickupData();
            if (!info.FindAndSeekOffset(ElementNames.pickupItems, rom))
                return data;
            int numItems = info.Num(ElementNames.pickupItems);
            if (metadata.IsEmerald) // Use the two item tables
            {
                for (int i = 0; i < numItems; i++)
                {
                    data.Items.Add((Item)rom.ReadUInt16());
                }
                if (!info.FindAndSeekOffset(ElementNames.pickupRareItems, rom))
                    return data;
                numItems = info.Num(ElementNames.pickupRareItems);
                for (int i = 0; i < numItems; i++)
                {
                    data.RareItems.Add((Item)rom.ReadUInt16());
                }
            }
            else // Is RS or FRLG, use items + chances
            {
                for (int i = 0; i < numItems; i++)
                {
                    data.ItemChances.Add(new PickupData.ItemChance()
                    {
                        item = (Item)rom.ReadUInt16(),
                        chance = rom.ReadUInt16(),
                    });
                }
            }
            return data;
        }

        private Script ReadSetBerryTreeScript(Rom rom, XmlManager info, RomMetadata metadata)
        {
            // Read the set berry tre script (if applicable)
            int offset = info.FindOffset(ElementNames.setBerryTreeScript, rom);
            if (offset != Rom.nullPointer)
            {
                return scriptParser.Parse(rom, offset, metadata);
            }
            return null;
        }
    }
}
