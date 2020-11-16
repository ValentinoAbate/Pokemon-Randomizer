using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.Backend.Reading
{
    public class Gen3RomParser : RomParser
    {
        // Parse the ROM bytes into a RomData object
        public override RomData Parse(Rom rom, RomMetadata metadata, XmlManager info)
        {
            RomData data = new RomData();

            #region Move Mappings (TMs/HMs/Tutors)
            //Read the TM move mappings from the ROM
            data.TMMoves = ReadMoveMappings("tmMoves", rom, info);
            //Read the HM move mappings from the ROM
            data.HMMoves = ReadMoveMappings("hmMoves", rom, info);
            //Read the move tutor move mappings from the ROM
            data.tutorMoves = ReadMoveMappings("moveTutorMoves", rom, info); ;
            #endregion

            data.MoveData = ReadMoves(rom, info);

            #region Base Stats
            // Read the pokemon base stats from the Rom
            data.Pokemon = ReadPokemonBaseStats(rom, info, out byte[] skippedData);
            data.SkippedLearnSetData = skippedData;
            data.PokemonLookup = new Dictionary<PokemonSpecies, PokemonBaseStats>();
            foreach (var pokemon in data.Pokemon)
                data.PokemonLookup.Add(pokemon.species, pokemon);
            // Link Pokemon to what they evolved from
            data.LinkEvolutions();
            data.LinkEggMoves();

            #endregion

            // Read Starters
            data.Starters = ReadStarters(rom, info);
            //data.StarterItems = ReadStarterItems(rom, info);
            // Read Catching tutorial pokemon
            data.CatchingTutPokemon = ReadCatchingTutOpponent(rom, info);
            // Read the PC item
            data.PcStartItem = (Item)rom.ReadUInt16(info.Offset("pcPotion"));
            // Trainers and associated data
            data.ClassNames = ReadTrainerClassNames(rom, info);
            data.Trainers = ReadTrainers(rom, info, data.ClassNames);
            SetSpecialTrainerData(data, metadata, info);
            // Read type definitions
            data.TypeDefinitions = ReadTypeEffectivenessData(rom, info);
            // Read in the map data
            data.MapBanks = ReadMapBanks(rom, info, metadata);
            data.Encounters = ReadEncounters(rom, info);
            // Calculate the balance metrics from the loaded data
            data.CalculateMetrics();
            return data;
        }
        // Read the move definitions
        private List<MoveData> ReadMoves(Rom rom, XmlManager info)
        {
            List<MoveData> moveData = new List<MoveData>();
            int moveCount = int.Parse(info.Attr("moveData", "num", info.Constants).Value);
            int dataOffset = rom.ReadPointer(HexUtils.HexToInt(info.Attr("moveData", "ptr", info.Constants).Value));
            rom.Seek(dataOffset);
            for (int i = 0; i <= moveCount; i++)
            {
                var move = new MoveData(rom)
                {
                    move = (Move)i
                };
                moveData.Add(move);
            }
            return moveData;
        }
        // Read TM, HM, or Move tutor definitions from the rom (depending on args)
        private Move[] ReadMoveMappings(string element, Rom rom, XmlManager info)
        {
            if (!info.FindAndSeekOffset(element, rom))
                return new Move[0];
            int numToRead = info.Num(element);
            Move[] moves = new Move[numToRead];
            for (int i = 0; i < numToRead; i++)
                moves[i] = (Move)rom.ReadUInt16();
            return moves;
        }

        private Dictionary<PokemonSpecies, List<Move>> ReadEggMoves(Rom rom, XmlManager info)
        {
            const string eggMoveElt = "eggMoves";
            rom.SaveOffset();
            // Find the offset of the eggMoves if we have the data
            if (!info.FindAndSeekOffset(eggMoveElt, rom))
            {
                rom.LoadOffset();
                return new Dictionary<PokemonSpecies, List<Move>>();
            }
            int pkmnSigniture = info.HexAttr(eggMoveElt, "pokemonSigniture");
            var moves = new Dictionary<PokemonSpecies, List<Move>>();
            var pkmn = (PokemonSpecies)0;
            int counter = 0;
            // Limit on loop just in case we are at the wrong place
            while (++counter < 3000)
            {
                int number = rom.ReadUInt16();
                if (number > pkmnSigniture + 1000 || number < 0)
                    break;
                if(number >= pkmnSigniture)
                {
                    pkmn = (PokemonSpecies)(number - pkmnSigniture);
                    if ((int)pkmn > 0)
                        moves.Add(pkmn, new List<Move>());
                }
                else
                {
                    moves[pkmn].Add((Move)number);
                }
            }
            rom.LoadOffset();
            return moves;
        }

        //private void loadPokemonNames()
        //{
        //    int offs = romEntry.getValue("PokemonNames");
        //    int nameLen = romEntry.getValue("PokemonNameLength");
        //    int numInternalPokes = romEntry.getValue("PokemonCount");
        //    pokeNames = new String[numInternalPokes + 1];
        //    for (int i = 1; i <= numInternalPokes; i++)
        //    {
        //        pokeNames[i] = readFixedLengthString(offs + i * nameLen, nameLen);
        //    }
        //}

        #region Read Pokemon Base Stats
        // Read the Pokemon base stat definitions from the ROM
        private List<PokemonBaseStats> ReadPokemonBaseStats(Rom rom, XmlManager info, out byte[] skippedData)
        {
            const string evolutionsElt = "evolutions";
            const string pokemonBaseStatsElt = "pokemonBaseStats";
            skippedData = null;
            int? offsetCheck = null;
            List<PokemonBaseStats> pokemon = new List<PokemonBaseStats>();
            int pkmnPtr = info.Offset(pokemonBaseStatsElt);
            int pkmnSize = info.Size(pokemonBaseStatsElt);
            int tmHmSize = info.Size("tmHmCompat");
            // Skip over the null pokemon
            int tmPtr = info.Offset("tmHmCompat") + tmHmSize;
            int tutorSize = info.Size("moveTutorCompat");
            // Need an extra +4 to skip the null pokemon. Determined from the move tutor move table's offset
            int tutorPtr = info.Offset("moveTutorMoves") + info.Num("moveTutorMoves") * info.Size("moveTutorMoves") + tutorSize;
            int movePtr = info.Offset("movesets");
            // Setup evolution offset
            offsetCheck = info.FindOffset(evolutionsElt, rom);
            if (offsetCheck == null)
                return pokemon;
            int evolutionSize = info.Size(evolutionsElt);
            // Add evolution size to skip the null pokemon
            int evolutionOffset = (int)offsetCheck + evolutionSize;
            // Read Egg Moves
            var eggMoves = ReadEggMoves(rom, info);
            // Find skip index if one exists
            int skipAt = info.HasElementWithAttr(pokemonBaseStatsElt, "skipAt") ? info.IntAttr(pokemonBaseStatsElt, "skipAt") : -1; 
            for (int i = 0; i < info.Num(pokemonBaseStatsElt); i++)
            {
                if (i == skipAt) // potentially skip empty slots
                {
                    int skipNum = info.IntAttr(pokemonBaseStatsElt, "skip");
                    skippedData = rom.ReadBlock(movePtr, skipNum * 4);
                    i += skipNum;
                    movePtr += skipNum * 4; // (don't know why this is 4, cuz move segments are variable lengths possibly terminators?)
                }
                // Create Pokemon
                PokemonBaseStats pkmn = ReadBaseStatsSingle(rom, pkmnPtr + (i * pkmnSize), (PokemonSpecies)(i + 1));
                // Set Egg Moves
                pkmn.eggMoves = eggMoves.ContainsKey(pkmn.species) ? eggMoves[pkmn.species] : new List<Move>();
                // Read Learn Set
                movePtr = ReadAttacks(rom, movePtr, out pkmn.learnSet);
                // Read Tm/Hm/Mt compat
                ReadTMHMCompat(rom, info, tmPtr + (i * tmHmSize), out pkmn.TMCompat, out pkmn.HMCompat);
                ReadTutorCompat(rom, info, tutorPtr + (i * tutorSize), out pkmn.moveTutorCompat);
                ReadEvolutions(rom, info, evolutionOffset + (i * evolutionSize), out pkmn.evolvesTo);
                pokemon.Add(pkmn);
            }
            return pokemon;
        }

        private PokemonBaseStats ReadBaseStatsSingle(Rom rom, int offset, PokemonSpecies species)
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
        // Read the attacks starting at offset (returns the index after completion)
        private int ReadAttacks(Rom rom, int offset, out LearnSet moves)
        {
            moves = new LearnSet
            {
                OriginalOffset = offset
            };
            byte curr = rom.ReadByte(offset);
            byte next = rom.ReadByte(offset + 1);
            while (curr != 255 || next != 255)
            {
                // lvl is in the lvl byte but divided by 2 (lose the last bit)
                int lvl = next >> 1;
                // if the move number is over 255, the last bit of the learn level byte is set to 1
                Move move = (Move)((next % 2) * 256 + curr);
                moves.Add(move, lvl);
                offset += 2;
                curr = rom.ReadByte(offset);
                next = rom.ReadByte(offset + 1);
            }
            moves.SetOriginalCount();
            offset += 2;    //pass final FFFF
            return offset;
        }
        // Read the TMcompat and HM compat BitArrays starting at given offset
        private void ReadTMHMCompat(Rom rom, XmlManager info, int offset, out BitArray tmCompat, out BitArray hmCompat)
        {
            int numTms = info.Num("tmMoves");
            int numHms = info.Num("hmMoves");
            tmCompat = new BitArray(numTms);
            hmCompat = new BitArray(numHms);
            byte[] tmHmChunk = rom.ReadBlock(offset, info.Size("tmHmCompat"));
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
        private void ReadTutorCompat(Rom rom, XmlManager info, int offset, out BitArray tutCompat)
        {
            int numMoveTutors = info.Num("moveTutorMoves");
            int tutorCompatSize = info.Size("moveTutorCompat");
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
        // Read evolutions
        private void ReadEvolutions(Rom rom, XmlManager info, int offset, out Evolution[] evolutions)
        {
            const string evolutionElt = "evolutions";
            int bytesPerEvolution = info.IntAttr(evolutionElt, "sizePerEvolution");
            evolutions = new Evolution[info.IntAttr(evolutionElt, "evolutionsPerPokemon")];
            for(int i = 0; i < evolutions.Length; ++i, offset += bytesPerEvolution)
            {
                byte[] evolutionBlock = rom.ReadBlock(offset, bytesPerEvolution);
                evolutions[i] = new Evolution(evolutionBlock);
            }

        }
        #endregion

        // Read the starter pokemon
        private List<PokemonSpecies> ReadStarters(Rom rom, XmlManager info)
        {
            const string starterPokemonElt = "starterPokemon";
            var starters = new List<PokemonSpecies>();
            if (!info.FindAndSeekOffset(starterPokemonElt, rom))
                return starters;
            starters.Add((PokemonSpecies)rom.ReadUInt16());
            rom.Skip(info.IntAttr(starterPokemonElt, "skip1"));
            starters.Add((PokemonSpecies)rom.ReadUInt16());
            rom.Skip(info.IntAttr(starterPokemonElt, "skip2"));
            starters.Add((PokemonSpecies)rom.ReadUInt16());
            return starters;
        }
        // Read the starter items
        private List<Item> ReadStarterItems(Rom rom, XmlManager info)
        {
            throw new System.NotImplementedException();
        }
        //Read the catching tut pokemon
        private PokemonSpecies ReadCatchingTutOpponent(Rom rom, XmlManager info)
        {
            // Currently have no idea how to actually read this so just return RALTS
            // Maybe add a constant in the ROM info later
            return PokemonSpecies.RALTS; 
        }
        // Read the Trainer Class names
        private List<string> ReadTrainerClassNames(Rom rom, XmlManager info)
        {
            const string trainerClassNameElt = "trainerClassNames";
            int? offset = info.Offset(trainerClassNameElt);
            // Trainer class names are not supported
            if (offset == null)
                return new List<string>();
            int numClasses = info.Num(trainerClassNameElt);
            int nameLength = (int)info.Attr(trainerClassNameElt, "length");
            int realOffset = (int)offset;
            List<string> classNames = new List<string>(numClasses);
            for(int i = 0; i < numClasses; ++i)
            {
                classNames.Add(rom.ReadString(realOffset + (i * nameLength), nameLength));
            }
            return classNames;
        }
        // Readainers
        private List<Trainer> ReadTrainers(Rom rom, XmlManager info, List<string> classNames)
        {
            const string trainerBattleElt = "trainerBattles";
            // If fail, reading trainer battles is not supported for this ROM
            if (!info.FindAndSeekOffset(trainerBattleElt, rom))
                return new List<Trainer>();
            int numTrainers = info.Num(trainerBattleElt);
            List<Trainer> ret = new List<Trainer>(numTrainers);
            for (int i = 0; i < numTrainers; ++i)
            {
                ret.Add(new Trainer(rom, classNames));
            }
            return ret;
        }
        /// <summary>
        /// Read all the preset trainer data from the info file into the ROM data, and find normal crunt, ace, and reocurring trainers
        /// </summary>
        private void SetSpecialTrainerData(RomData data, RomMetadata metadata, XmlManager info)
        {
            data.SpecialTrainers = new Dictionary<string, List<Trainer>>();
            string[] AddTrainersFromArrayAttr(string element)
            {
                var names = info.SafeArrayAttr(element, "names", info.ArrayAttr);
                foreach (var name in names)
                    data.SpecialTrainers.Add(name.ToLower(), new List<Trainer>());
                return names;
            };
            // Add trainers from preset names in info file
            data.RivalNames = AddTrainersFromArrayAttr("rivals");
            data.RivalRemap = info.SafeArrayAttr("rivals", "remap", info.IntArrayAttr);
            data.GymLeaderNames = AddTrainersFromArrayAttr("gymLeaders");
            data.EliteFourNames = AddTrainersFromArrayAttr("eliteFour");
            data.ChampionNames = AddTrainersFromArrayAttr("champion");
            data.UberNames = AddTrainersFromArrayAttr("uber");
            data.TeamAdminNames = AddTrainersFromArrayAttr("teamAdmins");
            data.TeamLeaderNames = AddTrainersFromArrayAttr("teamLeaders");
            // Find the normal trainers and calculated special trainers
            data.ReoccuringTrainerNames = new List<string>();
            data.AceTrainerNames = new List<string>();
            // Fetch the Ace Trainer Class Numbers for this ROM
            data.AceTrainerClasses = info.SafeArrayAttr("aceTrainers", "classNums", info.IntArrayAttr);
            // Safely initialize the team grunt names if supported
            data.TeamGruntNames = info.SafeArrayAttr("teamGrunts", "names", info.ArrayAttr).Select((name) => name.ToLower()).ToArray();
            data.GruntBattles = new List<Trainer>();
            const string wallyName = "wally";
            // Add Wally as their own special category if in RSE
            if (metadata.IsRubySapphireOrEmerald)
            {
                data.SpecialTrainers.Add(wallyName, new List<Trainer>());
            }
            // Initialize Normal Trainer List
            data.NormalTrainers = new Dictionary<string, Trainer>();
            // Classify trainers
            foreach (var trainer in data.Trainers)
            {
                string name = trainer.name.ToLower();
                if (string.IsNullOrWhiteSpace(name) || name == "??????")
                    continue;
                // All grunts have the same names but are not reoccuring trainers
                if (data.TeamGruntNames.Contains(name))
                {
                    data.GruntBattles.Add(trainer);
                }
                else if (data.SpecialTrainers.ContainsKey(name)) // Already a known special trainer, add to the special trainers dictionary
                {
                    data.SpecialTrainers[name].Add(trainer);
                }
                else if (data.AceTrainerClasses.Contains(trainer.trainerClass)) // new Ace trainer, add to speical trainers
                {
                    if (!data.SpecialTrainers.ContainsKey(name))
                    {
                        data.SpecialTrainers.Add(name, new List<Trainer>());
                        data.AceTrainerNames.Add(name);
                    }
                    data.SpecialTrainers[name].Add(trainer);
                }
                else if (data.NormalTrainers.ContainsKey(name)) // new reocurring trainer
                {
                    data.SpecialTrainers.Add(name, new List<Trainer> { data.NormalTrainers[name], trainer });
                    data.NormalTrainers.Remove(name);
                    data.ReoccuringTrainerNames.Add(name);
                }
                else // Normal (or potentially reocurring trainer)
                {
                    data.NormalTrainers.Add(name, trainer);
                }
            }
        }

        #region Map Reading

        // Read Map Banks
        private Map[][] ReadMapBanks(Rom rom, XmlManager info, RomMetadata metadata)
        {
            // Read data from XML file
            int bankPtrOffset = info.Offset("mapBankPointers");
            int ptrSize = info.Size("mapBankPointers");
            Map[][] mapBanks = new Map[info.Num("mapBankPointers")][];
            int[] bankLengths = info.IntArrayAttr("maps", "bankLengths");
            int labelOffset = rom.ReadPointer(rom.FindFromPrefix(info.Attr("mapLabels", "ptrPrefix").Value));
            // Construct map data structures
            for (int i = 0; i < mapBanks.Length; ++i)
            {
                int bankPtr = rom.ReadPointer(bankPtrOffset + (i * ptrSize));
                mapBanks[i] = ReadMapBank(rom, metadata, bankPtr, bankLengths[i], labelOffset);
            }
            return mapBanks;
        }
        private Map[] ReadMapBank(Rom rom, RomMetadata metadata, int offset, int numMaps, int labelOffset)
        {
            Map[] maps = new Map[numMaps];
            for (int i = 0; i < maps.Length; ++i)
            {
                int mapAddy = rom.ReadPointer(offset + (i * 4));
                maps[i] = ReadMap(rom, metadata, mapAddy, labelOffset);
            }
            return maps;
        }
        private Map ReadMap(Rom rom, RomMetadata metadata, int offset, int labelOffset)
        {
            rom.SaveOffset();
            rom.Seek(offset);

            #region Construct Map With Header Data

            var map = new Map()
            {
                mapDataOffset = rom.ReadPointer(),
                eventDataOffset = rom.ReadPointer(),
                mapScriptsOffset = rom.ReadPointer(),
                connectionOffset = rom.ReadPointer(),
                music = rom.ReadUInt16(),
                mapIndex = rom.ReadUInt16(),
                labelIndex = rom.ReadByte(),
                visibility = rom.ReadByte(),
                weather = (Map.Weather)rom.ReadByte(),
                mapType = (Map.Type)rom.ReadByte(),
                unknown = rom.ReadByte(),
                unknown2 = rom.ReadByte(),
                showLabelOnEntry = rom.ReadByte(),
                battleField = rom.ReadByte(),
            };

            #endregion

            #region Read Non-Header Data

            // Map Label (name)
            if (metadata.IsRubySapphireOrEmerald)
            {
                // Read Map Label (RSE)
                rom.Seek(rom.ReadPointer(labelOffset + map.labelIndex * 8 + 4));
                map.Name = rom.ReadVariableLengthString();
            }
            else if (metadata.IsFireRedOrLeafGreen)
            {
                // Don't know why this magic number is here
                const int frlgMapLabelsStart = 0x58;
                // Read Map Label (FRLG)
                rom.Seek(rom.ReadPointer(labelOffset + (map.labelIndex - frlgMapLabelsStart) * 4));
                map.Name = rom.ReadVariableLengthString();
            }
            // Map Data
            if (map.mapDataOffset != Rom.nullPointer)
                map.mapData = ReadMapData(rom, metadata, map.mapDataOffset);
            // Event Data
            if (map.eventDataOffset != Rom.nullPointer)
                map.eventData = ReadMapEventData(rom, map.eventDataOffset);
            // Connections
            if (map.connectionOffset != Rom.nullPointer)
                map.connections = ReadMapConnectionData(rom, map.connectionOffset);

            #endregion

            rom.LoadOffset();
            return map;
        }
        private MapData ReadMapData(Rom rom, RomMetadata metadata, int offset)
        {
            var mapData = new MapData();
            rom.Seek(offset);
            mapData.width = rom.ReadUInt32();
            mapData.height = rom.ReadUInt32();
            mapData.borderTileOffset = rom.ReadPointer();
            mapData.mapTilesOffset = rom.ReadPointer();
            mapData.globalTileSetOffset = rom.ReadPointer();
            mapData.localTileSetOffset = rom.ReadPointer();
            var border = rom.ReadBits(4, 2);
            mapData.borderWidth = border[0];
            mapData.borderHeight = border[1];
            mapData.secondarySize = mapData.borderWidth + 0xA0;
            if(metadata.IsRubySapphireOrEmerald)
            {
                const int rseBorderWidthAndHeight = 2;
                mapData.borderWidth = rseBorderWidthAndHeight;
                mapData.borderHeight = rseBorderWidthAndHeight;
            }
            return mapData;
        }
        private MapEventData ReadMapEventData(Rom rom, int offset)
        {
            var eventData = new MapEventData();
            rom.Seek(offset);
            eventData.numNpcEvents = rom.ReadByte();
            eventData.numWarpEvents = rom.ReadByte();
            eventData.numTriggerEvents = rom.ReadByte();
            eventData.numSignEvents = rom.ReadByte();
            eventData.npcEventOffset = rom.ReadPointer();
            eventData.warpEventOffset = rom.ReadPointer();
            eventData.triggerEventOffset = rom.ReadPointer();
            eventData.signEventOffset = rom.ReadPointer();

            // Read Non-Header Data

            return eventData;
        }
        private ConnectionData ReadMapConnectionData(Rom rom, int offset)
        {
            rom.SaveOffset();
            rom.Seek(offset);
            ConnectionData data = new ConnectionData
            {
                initialNumConnections = rom.ReadUInt32(),
                dataOffset = rom.ReadPointer()
            };
            rom.Seek(data.dataOffset);
            for (int i = 0; i < data.initialNumConnections; ++i)
            {
                // Read the connection
                var connection = new Connection
                {
                    type = (Connection.Type)rom.ReadUInt32(),
                    offset = rom.ReadUInt32(),
                    bankId = rom.ReadByte(),
                    mapId = rom.ReadByte(),
                    unknown = rom.ReadUInt16()
                };
                data.connections.Add(connection);
            }
            rom.LoadOffset();
            return data;
        }

        #endregion

        // Read encounters
        private List<EncounterSet> ReadEncounters(Rom rom, XmlManager info)
        {
            #region Find and Seek encounter table
            var encounterPtrPrefix = info.Attr("wildPokemon", "ptrPrefix", info.Constants).Value;
            var ptr = rom.FindFromPrefix(encounterPtrPrefix);
            rom.Seek(rom.ReadPointer(ptr));
            #endregion

            #region Encounter Slots
            int grassSlots = int.Parse(info.Attr("wildPokemon", "grassSlots", info.Constants).Value);
            int surfSlots = int.Parse(info.Attr("wildPokemon", "surfSlots", info.Constants).Value);
            int rockSmashSlots = int.Parse(info.Attr("wildPokemon", "rockSmashSlots", info.Constants).Value);
            int fishSlots = int.Parse(info.Attr("wildPokemon", "fishSlots", info.Constants).Value);
            #endregion

            var encounters = new List<EncounterSet>();

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
                if(grassPtr > 0 && grassPtr < rom.Length)
                {
                    var grassPokemon = new EncounterSet(EncounterSet.Type.Grass, bank, map, rom, grassPtr, grassSlots);
                    encounters.Add(grassPokemon);
                    // TODO: Log in map
                }
                if(surfPtr > 0 && surfPtr < rom.Length)
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
                if(fishPtr > 0 && fishPtr < rom.Length)
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

        // Read Type Effectiveness data
        private TypeEffectivenessChart ReadTypeEffectivenessData(Rom rom, XmlManager info)
        {
            TypeEffectivenessChart ret = new TypeEffectivenessChart();
            if (!info.FindAndSeekOffset("typeEffectiveness", rom))
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
    }
}
