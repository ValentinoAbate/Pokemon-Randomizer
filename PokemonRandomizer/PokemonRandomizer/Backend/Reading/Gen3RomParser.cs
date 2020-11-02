using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;
using System.Collections;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.Reading
{
    public static class Gen3RomParser
    {
        // Parse the ROM bytes into a RomData object
        public static RomData Parse(byte[] rawRom)
        {
            RomData data = new RomData(rawRom);
            var info = data.Info;

            #region Move Mappings (TMs/HMs/Tutors)
            //Read the TM move mappings from the ROM
            data.TMMoves = ReadMoveMappings("tmMoves", data.Rom, info);
            //Read the HM move mappings from the ROM
            data.HMMoves = ReadMoveMappings("hmMoves", data.Rom, info);
            //Read the move tutor move mappings from the ROM
            data.tutorMoves = ReadMoveMappings("moveTutorMoves", data.Rom, info); ;
            #endregion

            data.MoveData = ReadMoves(data.Rom, info);

            #region Base Stats
            // Read the pokemon base stats from the Rom
            data.Pokemon = ReadPokemonBaseStats(data.Rom, info, out byte[] skippedData);
            data.SkippedLearnSetData = skippedData;
            data.PokemonLookup = new Dictionary<PokemonSpecies, PokemonBaseStats>();
            foreach (var pokemon in data.Pokemon)
                data.PokemonLookup.Add(pokemon.species, pokemon);
            // Link Pokemon to what they evolved from
            data.LinkEvolutions();
            data.LinkEggMoves();

            #endregion

            // Read Starters
            data.Starters = ReadStarters(data.Rom, info);
            //data.StarterItems = ReadStarterItems(data.Rom, info);
            // Read Catching tutorial pokemon
            data.CatchingTutPokemon = ReadCatchingTutOpponent(data.Rom, info);
            // Read the PC item
            data.PcStartItem = (Item)data.Rom.ReadUInt16(info.Offset("pcPotion"));
            // Trainers and associated data
            data.ClassNames = ReadTrainerClassNames(data.Rom, info);
            data.Trainers = ReadTrainers(data.Rom, info, data.ClassNames);
            // Read type definitions
            data.TypeDefinitions = ReadTypeEffectivenessData(data.Rom, info);
            // Read in the map data
            data.Maps = new MapManager(data, info);
            data.Encounters = ReadEncounters(data.Rom, info, data.Maps);
            // Calculate the balance metrics from the loaded data
            data.CalculateMetrics();
            return data;
        }
        // Read the move definitions
        private static List<MoveData> ReadMoves(Rom rom, XmlManager info)
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
        private static Move[] ReadMoveMappings(string element, Rom rom, XmlManager info)
        {
            if (!info.FindAndSeekOffset(element, rom))
                return new Move[0];
            int numToRead = info.Num(element);
            Move[] moves = new Move[numToRead];
            for (int i = 0; i < numToRead; i++)
                moves[i] = (Move)rom.ReadUInt16();
            return moves;
        }

        private static Dictionary<PokemonSpecies, List<Move>> ReadEggMoves(Rom rom, XmlManager info)
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
        private static List<PokemonBaseStats> ReadPokemonBaseStats(Rom rom, XmlManager info, out byte[] skippedData)
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

        private static PokemonBaseStats ReadBaseStatsSingle(Rom rom, int offset, PokemonSpecies species)
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
        private static int ReadAttacks(Rom rom, int offset, out LearnSet moves)
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
        private static void ReadTMHMCompat(Rom rom, XmlManager info, int offset, out BitArray tmCompat, out BitArray hmCompat)
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
        private static void ReadTutorCompat(Rom rom, XmlManager info, int offset, out BitArray tutCompat)
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
        private static void ReadEvolutions(Rom rom, XmlManager info, int offset, out Evolution[] evolutions)
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
        private static List<PokemonSpecies> ReadStarters(Rom rom, XmlManager info)
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
        private static List<Item> ReadStarterItems(Rom rom, XmlManager info)
        {
            throw new System.NotImplementedException();
        }
        //Read the catching tut pokemon
        private static PokemonSpecies ReadCatchingTutOpponent(Rom rom, XmlManager info)
        {
            // Currently have no idea how to actually read this so just return RALTS
            // Maybe add a constant in the ROM info later
            return PokemonSpecies.RALTS; 
        }
        // Read the Trainer Class names
        private static List<string> ReadTrainerClassNames(Rom rom, XmlManager info)
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
        // Read the Trainers
        private static List<Trainer> ReadTrainers(Rom rom, XmlManager info, List<string> classNames)
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
        // Read encounters
        private static List<EncounterSet> ReadEncounters(Rom rom, XmlManager info, MapManager maps)
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
        private static TypeEffectivenessChart ReadTypeEffectivenessData(Rom rom, XmlManager info)
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
