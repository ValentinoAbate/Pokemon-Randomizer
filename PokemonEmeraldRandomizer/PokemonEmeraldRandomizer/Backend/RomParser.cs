using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace PokemonEmeraldRandomizer.Backend
{
    public static class RomParser
    {
        // Parse the ROM bytes into a RomData object
        public static RomData Parse(byte[] rawRom)
        {           
            RomData data = new RomData(rawRom);

            #region Move Mappings (TMs/HMs/Tutors)
            //Read the TM move mappings from the ROM
            data.TMMoves = ReadMoveMappings(data.Rom, data.Info.Offset("tmMoves"), data.Info.Num("tmMoves"));
            //Read the HM move mappings from the ROM
            data.HMMoves = ReadMoveMappings(data.Rom, data.Info.Offset("hmMoves"), data.Info.Num("hmMoves"));
            //Read the move tutor move mappings from the ROM
            data.tutorMoves = ReadMoveMappings(data.Rom, data.Info.Offset("moveTutorMoves"), data.Info.Num("moveTutorMoves"));
            #endregion

            // Read the pokemon base stats from the Rom
            data.Pokemon = ReadPokemonBaseStats(data.Rom, data.Info);
            data.PokemonLookup = new Dictionary<PokemonSpecies, PokemonBaseStats>();
            foreach (var pokemon in data.Pokemon)
                data.PokemonLookup.Add(pokemon.species, pokemon);
            // data.Starters = ReadStarters(rom);
            // Trainers and associated data
            data.ClassNames = ReadTrainerClassNames(data.Rom, data.Info);
            data.Trainers = ReadTrainers(data.Rom, data.Info, data.ClassNames);
            // Calculate the balance metrics from the loaded data
            data.TypeDefinitions = ReadTypeEffectivenessData(data.Rom, data.Info);
            data.Maps = new MapManager(data.Rom, data.Info);
            data.CalculateMetrics();
            return data;
        }
        // Read TM, HM, or Move tutor definitions from the rom (depending on args)
        private static Move[] ReadMoveMappings(Rom rom, int offset, int numToRead)
        {
            rom.Seek(offset);
            Move[] moves = new Move[numToRead];
            for (int i = 0; i < numToRead; i++, offset += 2)
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
        private static PokemonBaseStats[] ReadPokemonBaseStats(Rom rom, XmlManager data)
        {
            List<PokemonBaseStats> pokemon = new List<PokemonBaseStats>();
            int pkmnPtr = data.Offset("pokemonBaseStats");
            int pkmnSize = data.Size("pokemonBaseStats");
            int tmPtr = data.Offset("tmHmCompat");
            int tmHmSize = data.Size("tmHmCompat");
            int tutorPtr = data.Offset("moveTutorCompat");
            int tutorSize = data.Size("moveTutorCompat");
            int movePtr = data.Offset("movesets");
            int evolutionPtr = data.Offset("evolutions");
            int evolutionSize = data.Size("evolutions");
            for (int i = 0; i < data.Num("pokemonBaseStats"); i++)
            {
                if (i == (int)data.Attr("pokemonBaseStats", "skipAt")) // potentially skip empty slots
                {
                    int skipNum = (int)data.Attr("pokemonBaseStats", "skip");
                    i += skipNum;
                    movePtr += skipNum * 4; // (don't know why this is 4, cuz move segments are variable lengths possibly terminators?)
                }
                // Create Pokemon
                PokemonBaseStats pkmn = new PokemonBaseStats(rom, pkmnPtr + (i * pkmnSize), (PokemonSpecies)(i + 1));
                movePtr = ReadAttacks(rom, movePtr, out pkmn.moveSet);
                ReadTMHMCompat(rom, data, tmPtr + (i * tmHmSize), out pkmn.TMCompat, out pkmn.HMCompat);
                ReadTutorCompat(rom, data, tutorPtr + (i * tutorSize), out pkmn.moveTutorCompat);
                ReadEvolutions(rom, data, evolutionPtr + (i * evolutionSize), out pkmn.evolutions);
                pokemon.Add(pkmn);
            }
            return pokemon.ToArray();
        }
        // Read the attacks starting at offset (returns the index after completion)
        private static int ReadAttacks(Rom rom, int offset, out MoveSet moves)
        {
            moves = new MoveSet();
            byte curr = rom.ReadByte(offset);
            byte next = rom.ReadByte(offset + 1);
            while (curr != 255 || next != 255)
            {
                int lvl = next >> 1;
                Move move = (Move)((next % 2) * 256 + curr);
                moves.Add(move, lvl);
                offset += 2;
                curr = rom.ReadByte(offset);
                next = rom.ReadByte(offset + 1);
            }
            offset += 2;    //pass final FFFF
            return offset;
        }
        // Read the TMcompat and HM compat BitArrays starting at given offset
        private static void ReadTMHMCompat(Rom rom, XmlManager data, int offset, out BitArray tmCompat, out BitArray hmCompat)
        {
            int numTms = data.Num("tmMoves");
            int numHms = data.Num("hmMoves");
            tmCompat = new BitArray(numTms);
            hmCompat = new BitArray(numHms);
            byte[] tmHmChunk = rom.ReadBlock(offset, data.Size("tmHmCompat"));
            int mask = 0;
            for (int p = 0; p < numTms + numHms; ++p)
            {
                if (p % 8 == 0) mask = 1;
                BitArray set = p < numTms ? tmCompat : hmCompat;
                set.Set(p < numTms ? p : p - numTms, (tmHmChunk[p / 8] & mask) > 0);
                mask = mask << 1;
            }
        }
        // Read the move tutor compatibility BitArray at offset
        private static void ReadTutorCompat(Rom rom, XmlManager data, int offset, out BitArray tutCompat)
        {
            int numMoveTutors = data.Num("moveTutorMoves");
            int tutorCompatSize = data.Size("moveTutorCompat");
            tutCompat = new BitArray(numMoveTutors);
            byte[] tutChunk = rom.ReadBlock(offset, tutorCompatSize);
            int mask = 0;
            for (int p = 0; p < numMoveTutors; ++p)
            {
                if (p % 8 == 0) mask = 1;
                tutCompat.Set(p, (tutChunk[p / 8] & mask) > 0);
                mask = mask << 1;
            }
        }
        // Read evolutions
        private static void ReadEvolutions(Rom rom, XmlManager data, int offset, out Evolution[] evolutions)
        {
            int bytesPerEvolution = (int)data.Attr("evolutions", "sizePerEvolution");
            evolutions = new Evolution[(int)data.Attr("evolutions", "evolutionsPerPokemon")];          
            for(int i = 0; i < evolutions.Length; ++i, offset += bytesPerEvolution)
            {
                byte[] evolutionBlock = rom.ReadBlock(offset, bytesPerEvolution);
                evolutions[i] = new Evolution(evolutionBlock);
            }

        }
        #endregion

        // Read the starter pokemon (TODO)
        private static Pokemon[] ReadStarters(Rom rom, XmlManager data)
        {
            throw new System.NotImplementedException();
        }
        // Read the Trainer Class names
        private static string[] ReadTrainerClassNames(Rom rom, XmlManager data)
        {
            int addy = data.Offset("trainerClassNames");
            int numClasses = data.Num("trainerClassNames");
            int nameLength = (int)data.Attr("trainerClassNames", "length");

            string[] classNames = new string[numClasses];
            for(int i = 0; i < numClasses; ++i)
                classNames[i] = rom.ReadString(addy + (i * nameLength), nameLength);
            return classNames;
        }
        // Read the Trainers
        private static Trainer[] ReadTrainers(Rom rom, XmlManager data, string[] classNames)
        {
            List<Trainer> ret = new List<Trainer>();
            for (int i = 0; i < data.Num("trainerBattles"); ++i)
                ret.Add(new Trainer(rom, data.Offset("trainerBattles") + (i * data.Size("trainerBattles")), classNames));
            return ret.ToArray();
        }
        // Read Type Effectiveness data
        private static TypeEffectivenessChart ReadTypeEffectivenessData(Rom rom, XmlManager data)
        {
            TypeEffectivenessChart ret = new TypeEffectivenessChart();
            rom.Seek(data.Offset("typeEffectiveness"));
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
        private static void checkHash(byte[] rawRom, XmlManager data)
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
            if (hash != data.StringElt("hash"))
            {
                System.Windows.MessageBox.Show("The base ROM does not match the target ROM. This program was intended for!\n\nMD5: "
                                                + hash + "\nExpected: " + data.StringElt("hash"), "WARNING: ", System.Windows.MessageBoxButton.OK);
            }
        }
    }
}
