using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;

namespace PokemonRandomizer.Backend.Reading
{
    public static class RomParser
    {
        // Parse the ROM bytes into a RomData object
        public static RomData Parse(byte[] rawRom)
        {
            RomData data = new RomData(rawRom);
            var info = data.Info;

            #region Move Mappings (TMs/HMs/Tutors)
            //Read the TM move mappings from the ROM
            data.TMMoves = ReadMoveMappings(data.Rom, info.Offset("tmMoves"), info.Num("tmMoves"));
            //Read the HM move mappings from the ROM
            data.HMMoves = ReadMoveMappings(data.Rom, info.Offset("hmMoves"), info.Num("hmMoves"));
            //Read the move tutor move mappings from the ROM
            data.tutorMoves = ReadMoveMappings(data.Rom, info.Offset("moveTutorMoves"), info.Num("moveTutorMoves"));
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
            data.Maps = new MapManager(data.Rom, info);
            data.Encounters = ReadEncounters(data.Rom, info, data.Maps);
            // Calculate the balance metrics from the loaded data
            data.CalculateMetrics();
            return data;
        }
        // Read the move definitions
        private static List<MoveData> ReadMoves(Rom rom, XmlManager data)
        {
            List<MoveData> moveData = new List<MoveData>();
            int moveCount = int.Parse(data.Attr("moveData", "num", data.Constants).Value);
            int dataOffset = rom.ReadPointer(HexUtils.HexToInt(data.Attr("moveData", "ptr", data.Constants).Value));
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
        private static Move[] ReadMoveMappings(Rom rom, int offset, int numToRead)
        {
            rom.Seek(offset);
            Move[] moves = new Move[numToRead];
            for (int i = 0; i < numToRead; i++)
                moves[i] = (Move)rom.ReadUInt16();
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
            skippedData = null;
            List<PokemonBaseStats> pokemon = new List<PokemonBaseStats>();
            int pkmnPtr = info.Offset("pokemonBaseStats");
            int pkmnSize = info.Size("pokemonBaseStats");
            int tmPtr = info.Offset("tmHmCompat");
            int tmHmSize = info.Size("tmHmCompat");
            int tutorPtr = info.Offset("moveTutorCompat");
            int tutorSize = info.Size("moveTutorCompat");
            int movePtr = info.Offset("movesets");
            int evolutionPtr = info.Offset("evolutions");
            int evolutionSize = info.Size("evolutions");
            for (int i = 0; i < info.Num("pokemonBaseStats"); i++)
            {
                if (i == (int)info.Attr("pokemonBaseStats", "skipAt")) // potentially skip empty slots
                {
                    int skipNum = (int)info.Attr("pokemonBaseStats", "skip");
                    skippedData = rom.ReadBlock(movePtr, skipNum * 4);
                    i += skipNum;
                    movePtr += skipNum * 4; // (don't know why this is 4, cuz move segments are variable lengths possibly terminators?)
                }
                // Create Pokemon
                PokemonBaseStats pkmn = new PokemonBaseStats(rom, pkmnPtr + (i * pkmnSize), (PokemonSpecies)(i + 1));
                movePtr = ReadAttacks(rom, movePtr, out pkmn.learnSet);
                ReadTMHMCompat(rom, info, tmPtr + (i * tmHmSize), out pkmn.TMCompat, out pkmn.HMCompat);
                ReadTutorCompat(rom, info, tutorPtr + (i * tutorSize), out pkmn.moveTutorCompat);
                ReadEvolutions(rom, info, evolutionPtr + (i * evolutionSize), out pkmn.evolvesTo);
                pokemon.Add(pkmn);
            }
            return pokemon;
        }
        // Read the attacks starting at offset (returns the index after completion)
        private static int ReadAttacks(Rom rom, int offset, out LearnSet moves)
        {
            moves = new LearnSet();
            moves.OriginalOffset = offset;
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
            int bytesPerEvolution = (int)info.Attr("evolutions", "sizePerEvolution");
            evolutions = new Evolution[(int)info.Attr("evolutions", "evolutionsPerPokemon")];
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
            var starters = new List<PokemonSpecies>();
            rom.Seek(info.Offset("starterPokemon"));
            starters.Add((PokemonSpecies)rom.ReadUInt16());
            rom.Skip(info.IntAttr("starterPokemon", "skip1"));
            starters.Add((PokemonSpecies)rom.ReadUInt16());
            rom.Skip(info.IntAttr("starterPokemon", "skip2"));
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
            int addy = info.Offset("trainerClassNames");
            int numClasses = info.Num("trainerClassNames");
            int nameLength = (int)info.Attr("trainerClassNames", "length");

            List<string> classNames = new List<string>(numClasses);
            for(int i = 0; i < numClasses; ++i)
                classNames.Add(rom.ReadString(addy + (i * nameLength), nameLength));
            return classNames;
        }
        // Read the Trainers
        private static List<Trainer> ReadTrainers(Rom rom, XmlManager info, List<string> classNames)
        {
            List<Trainer> ret = new List<Trainer>();
            int numTrainers = info.Num("trainerBattles");
            rom.Seek(info.Offset("trainerBattles"));
            for (int i = 0; i < numTrainers; ++i)
                ret.Add(new Trainer(rom, classNames));
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
            rom.Seek(info.Offset("typeEffectiveness"));
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

        // Checks the hash of the rom to see if its the right version (INVALID DUE TO VERSION CHECKING)
        private static void CheckHash(byte[] rawRom, XmlManager info)
        {
            MD5 mD5 = MD5.Create();
            byte[] bytes = mD5.ComputeHash(rawRom);
            // Create a new Stringbuilder to collect the byte and create a string.
            StringBuilder sBuilder = new StringBuilder();
            // Loop through each byte of the hashed data and format each one as a hexadecimal string.
            for (int i = 0; i < bytes.Length; i++)
            {
                sBuilder.Append(bytes[i].ToString("x2"));
            }
            string hash = sBuilder.ToString();
            if (hash != info.StringElt("hash"))
            {
                System.Windows.MessageBox.Show("The base ROM does not match the target ROM. This program was intended for!\n\nMD5: "
                                                + hash + "\nExpected: " + info.StringElt("hash"), "WARNING: ", System.Windows.MessageBoxButton.OK);
            }
        }
    }
}
