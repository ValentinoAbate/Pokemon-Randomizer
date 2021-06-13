using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;
using PokemonRandomizer.Backend.GenIII.Constants.ElementNames;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.Backend.Reading
{
    public class Gen3RomParser : RomParser
    {
        private readonly Gen3ScriptParser scriptParser;
        private readonly Gen3MapParser mapParser;

        public Gen3RomParser()
        {
            scriptParser = new Gen3ScriptParser();
            mapParser = new Gen3MapParser(scriptParser);
        }

        // Parse the ROM bytes into a RomData object
        public override RomData Parse(Rom rom, RomMetadata metadata, XmlManager info)
        {
            RomData data = new RomData();

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
            var pokemon = ReadPokemonBaseStats(rom, info, out byte[] skippedData);
            ReadNationalDexOrder(pokemon, rom, info);
            // Set the data on the RomData
            data.Pokemon = pokemon;
            data.SkippedLearnSetData = skippedData;
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
            data.ClassNames = ReadTrainerClassNames(rom, info);
            data.Trainers = ReadTrainers(rom, info, data.ClassNames);
            SetSpecialTrainerData(data, metadata, info);
            // Read type definitions
            data.TypeDefinitions = ReadTypeEffectivenessData(rom, info);
            // Read in the map data
            data.MapBanks = mapParser.ReadMapBanks(rom, info, metadata);
            data.Encounters = ReadEncounters(rom, info);
            data.FirstEncounterSet = FindFirstEncounter(data.Encounters, info);
            // Read in the item data
            data.ItemData = ReadItemData(rom, info);
            // Read in the pickup items
            data.PickupItems = ReadPickupData(rom, info, metadata);
            // Calculate the balance metrics from the loaded data
            data.CalculateMetrics();
            return data;
        }

        // Read national dex order
        private void ReadNationalDexOrder(IList<PokemonBaseStats> pokemon, Rom rom, XmlManager info)
        {
            int[] order = new int[info.Num(ElementNames.pokemonBaseStats) + 1];
            int offset  = info.FindOffset(ElementNames.nationalDexOrder, rom);
            if (offset == Rom.nullPointer)
                return;
            foreach (var p in pokemon)
            {
                p.NationalDexIndex = rom.ReadUInt16(offset + (((int)p.species - 1) * 2));
            }
        }
        // Read the move definitions
        private List<MoveData> ReadMoves(Rom rom, XmlManager info)
        {
            List<MoveData> moveData = new List<MoveData>();
            // Exit early if the offset doesn't exist (feature unsupported)
            if (!info.FindAndSeekOffset(ElementNames.moveData, rom))
                return moveData;
            int moveCount = info.Num(ElementNames.moveData);
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

        private Dictionary<Pokemon, List<Move>> ReadEggMoves(Rom rom, XmlManager info)
        {
            const string eggMoveElt = "eggMoves";
            rom.SaveOffset();
            // Find the offset of the eggMoves if we have the data
            if (!info.FindAndSeekOffset(eggMoveElt, rom))
            {
                rom.LoadOffset();
                return new Dictionary<Pokemon, List<Move>>();
            }
            int pkmnSigniture = info.HexAttr(eggMoveElt, "pokemonSigniture");
            var moves = new Dictionary<Pokemon, List<Move>>();
            var pkmn = (Pokemon)0;
            int counter = 0;
            // Limit on loop just in case we are at the wrong place
            while (++counter < 3000)
            {
                int number = rom.ReadUInt16();
                if (number > pkmnSigniture + 1000 || number < 0)
                    break;
                if(number >= pkmnSigniture)
                {
                    pkmn = (Pokemon)(number - pkmnSigniture);
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

        #region Read Pokemon Base Stats
        // Read the Pokemon base stat definitions from the ROM
        private List<PokemonBaseStats> ReadPokemonBaseStats(Rom rom, XmlManager info, out byte[] skippedData)
        {
            skippedData = null;
            List<PokemonBaseStats> pokemon = new List<PokemonBaseStats>();

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
            // Add evolution size to skip the null pokemon
            evolutionOffset += evolutionSize;
            // setup TmHmCompat offset
            int tmHmCompatOffset = info.FindOffset(ElementNames.tmHmCompat, rom);
            if (tmHmCompatOffset == Rom.nullPointer)
                return pokemon;
            int tmHmSize = info.Size(ElementNames.tmHmCompat);
            // Skip over the null pokemon
            tmHmCompatOffset += tmHmSize;
            // Setup Move Tutor Compat offset
            int tutorCompatOffset = info.FindOffset(ElementNames.tutorMoves, rom);
            if (tutorCompatOffset == Rom.nullPointer)
                return pokemon;
            int tutorSize = info.Size(ElementNames.tutorCompat);
            // Skip over the tutor move definitions to get to the compatibilities, and +tutorSize to skip the null pkmn
            tutorCompatOffset += (info.Num(ElementNames.tutorMoves) * info.Size(ElementNames.tutorMoves)) + tutorSize;
            // Setup moveset offset
            int movesetOffset = info.FindOffset(ElementNames.movesets, rom);
            if (movesetOffset == Rom.nullPointer)
                return pokemon;
            // Setup Name offset
            int namesOffset = info.FindOffset(ElementNames.pokemonNames, rom);
            if (namesOffset == Rom.nullPointer)
                return pokemon;
            int nameLength = info.Length(ElementNames.pokemonNames);
            namesOffset += nameLength;
            #endregion

            // Read Egg Moves
            var eggMoves = ReadEggMoves(rom, info);
            // Find skip index if one exists
            int skipAt = info.HasElementWithAttr(ElementNames.pokemonBaseStats, "skipAt") ? info.IntAttr(ElementNames.pokemonBaseStats, "skipAt") : -1; 
            for (int i = 0; i < info.Num(ElementNames.pokemonBaseStats); i++)
            {
                if (i == skipAt) // potentially skip empty slots
                {
                    int skipNum = info.IntAttr(ElementNames.pokemonBaseStats, "skip");
                    skippedData = rom.ReadBlock(movesetOffset, skipNum * 4);
                    i += skipNum;
                    movesetOffset += skipNum * 4; // (don't know why this is 4, cuz move segments are variable lengths possibly terminators?)
                }
                // Create Pokemon
                PokemonBaseStats pkmn = ReadBaseStatsSingle(rom, pkmnOffset + (i * pkmnSize), (Pokemon)(i + 1));
                // Read name
                pkmn.Name = rom.ReadString(namesOffset + (i * nameLength), nameLength);
                // Set Egg Moves
                pkmn.eggMoves = eggMoves.ContainsKey(pkmn.species) ? eggMoves[pkmn.species] : new List<Move>();
                // Read Learn Set
                movesetOffset = ReadAttacks(rom, movesetOffset, out pkmn.learnSet);
                // Read Tm/Hm/Mt compat
                ReadTMHMCompat(rom, info, tmHmCompatOffset + (i * tmHmSize), out pkmn.TMCompat, out pkmn.HMCompat);
                ReadTutorCompat(rom, info, tutorCompatOffset + (i * tutorSize), out pkmn.moveTutorCompat);
                ReadEvolutions(rom, info, evolutionOffset + (i * evolutionSize), out pkmn.evolvesTo);
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
            int bytesPerEvolution = info.IntAttr(ElementNames.evolutions, "sizePerEvolution");
            evolutions = new Evolution[info.IntAttr(ElementNames.evolutions, "evolutionsPerPokemon")];
            for(int i = 0; i < evolutions.Length; ++i, offset += bytesPerEvolution)
            {
                byte[] evolutionBlock = rom.ReadBlock(offset, bytesPerEvolution);
                evolutions[i] = new Evolution(evolutionBlock);
            }

        }
        #endregion

        // Read the starter pokemon
        private List<Pokemon> ReadStarters(Rom rom, XmlManager info)
        {
            var starters = new List<Pokemon>();
            if (!info.FindAndSeekOffset(ElementNames.starterPokemon, rom))
                return starters;
            starters.Add((Pokemon)rom.ReadUInt16());
            rom.Skip(info.IntAttr(ElementNames.starterPokemon, "skip1"));
            starters.Add((Pokemon)rom.ReadUInt16());
            rom.Skip(info.IntAttr(ElementNames.starterPokemon, "skip2"));
            starters.Add((Pokemon)rom.ReadUInt16());
            return starters;
        }
        // Read the starter items
        private List<Item> ReadStarterItems(Rom rom, XmlManager info)
        {
            throw new System.NotImplementedException();
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
                t.pokemonRecieved = (Pokemon)rom.ReadUInt16();
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
                t.pokemonWanted = (Pokemon)rom.ReadUInt32();
                trades.Add(t);
            }
            return trades;
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
            int nameLength = info.Length(trainerClassNameElt);
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
            // If fail, reading trainer battles is not supported for this ROM
            if (!info.FindAndSeekOffset(ElementNames.trainerBattles, rom))
                return new List<Trainer>();
            int numTrainers = info.Num(ElementNames.trainerBattles);
            List<Trainer> ret = new List<Trainer>(numTrainers);
            for (int i = 0; i < numTrainers; ++i)
            {
                ret.Add(new Trainer(rom, classNames));
            }
            return ret;
        }
        /// <summary>
        /// Read all the preset trainer data from the info file into the ROM data, and find normal grunt, ace, and reocurring trainers
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

        private EncounterSet FindFirstEncounter(IEnumerable<EncounterSet> encounters, XmlManager info)
        {
            if (!info.HasElement(ElementNames.firstEncounter))
            {
                // TODO: log
                return null;
            }
            if(!info.HasElementWithAttr(ElementNames.firstEncounter, "map"))
            {
                // TODO: log
                return null;
            }
            if(!info.HasElementWithAttr(ElementNames.firstEncounter, "bank"))
            {
                // TODO: log
                return null;
            }
            int map = info.IntAttr(ElementNames.firstEncounter, "map");
            int bank = info.IntAttr(ElementNames.firstEncounter, "bank");
            foreach(var encounter in encounters)
            {
                if (encounter.map == map && encounter.bank == bank && encounter.type == EncounterSet.Type.Grass)
                    return encounter;
            }
            return null;
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

        private List<ItemData> ReadItemData(Rom rom, XmlManager info)
        {
            if (!info.FindAndSeekOffset(ElementNames.itemData, rom))
                return new List<ItemData>();
            int numItemDefinitions = info.Num(ElementNames.itemData);
            var itemData = new List<ItemData>(numItemDefinitions);
            for(int i = 0; i < numItemDefinitions; ++i)
            {
                itemData.Add(new ItemData
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
                });
                itemData[i].Description = rom.ReadString(itemData[i].descriptionOffset);
            }
            // If we have the offset for the item sprites, read the item sprite data
            if (!info.FindAndSeekOffset(ElementNames.itemSprites, rom))
                return itemData;
            foreach(var item in itemData)
            {
                item.spriteOffset = rom.ReadPointer();
                item.paletteOffset = rom.ReadPointer();
            }
            return itemData;
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
    }
}
