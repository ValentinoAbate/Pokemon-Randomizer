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
        public static RomData Parse(byte[] rom)
        {           
            RomData data = new RomData(rom);

            #region Move Mappings (TMs/HMs/Tutors)
            //Read the TM move mappings from the ROM
            data.TMMoves = ReadMoveMappings(rom, data.Info.Addy("tmMoves"), data.Info.Num("tmMoves"));
            //Read the HM move mappings from the ROM
            data.HMMoves = ReadMoveMappings(rom, data.Info.Addy("hmMoves"), data.Info.Num("hmMoves"));
            //Read the move tutor move mappings from the ROM
            data.tutorMoves = ReadMoveMappings(rom, data.Info.Addy("moveTutorMoves"), data.Info.Num("moveTutorMoves"));
            #endregion

            // Read the pokemon base stats from the ROM
            data.Pokemon = ReadPokemonBaseStats(rom, data.Info);
            // data.Starters = ReadStarters(rom);
            data.Trainers = ReadTrainers(rom, data.Info);
            // Calculate the balance metrics from the loaded data
            data.TypeDefinitions = ReadTypeEffectivenessData(rom, data.Info);
            data.CalculateMetrics();
            return data;
        }
        // Read TM, HM, or Move tutor definitions from the rom (depending on args)
        private static Move[] ReadMoveMappings(byte[] rom, int ptr, int numToRead)
        {
            Move[] moves = new Move[numToRead];
            for (int i = 0; i < numToRead; i++, ptr += 2)
            {
                moves[i] = (Move)(rom[ptr] + rom[ptr + 1] * 256);
            }
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
        private static PokemonBaseStats[] ReadPokemonBaseStats(byte[] rom, XmlManager data)
        {
            List<PokemonBaseStats> pokemon = new List<PokemonBaseStats>();
            int pkmnPtr = data.Addy("pokemonBaseStats");
            int pkmnSize = data.Size("pokemonBaseStats");
            int tmPtr = data.Addy("tmHmCompat");
            int tmHmSize = data.Size("tmHmCompat");
            int tutorPtr = data.Addy("moveTutorCompat");
            int tutorSize = data.Size("moveTutorCompat");
            int movePtr = data.Addy("movesets");
            int evolutionPtr = data.Addy("evolutions");
            int evolutionSize = data.Size("evolutions");
            byte[] currBlock = new byte[data.Size("pokemonBaseStats")];
            for (int i = 0; i < data.Num("pokemonBaseStats"); i++)
            {
                if (i == (int)data.Attr("pokemonBaseStats", "skipAt")) // potentially skip empty slots
                {
                    int skipNum = (int)data.Attr("pokemonBaseStats", "skip");
                    i += skipNum;
                    movePtr += skipNum * 4; // (don't know why this is 4, cuz move segments are variable lengths possibly terminators?)
                }
                // Create Pokemon
                Array.ConstrainedCopy(rom, pkmnPtr + (i * pkmnSize), currBlock, 0, pkmnSize); 
                PokemonBaseStats pkmn = new PokemonBaseStats(currBlock, (PokemonSpecies)(i + 1));
                movePtr = ReadAttacks(rom, movePtr, out pkmn.moveSet);
                ReadTMHMCompat(rom, data, tmPtr + (i * tmHmSize), out pkmn.TMCompat, out pkmn.HMCompat);
                ReadTutorCompat(rom, data, tutorPtr + (i * tutorSize), out pkmn.moveTutorCompat);
                ReadEvolutions(rom, data, evolutionPtr + (i * evolutionSize), out pkmn.evolutions);
                pokemon.Add(pkmn);
            }
            return pokemon.ToArray();
        }
        // Read the attacks starting at atkPtr (returns the index after completion)
        private static int ReadAttacks(byte[] rom, int movePtr, out MoveSet moves)
        {
            moves = new MoveSet();
            while (rom[movePtr] != 255 || rom[movePtr + 1] != 255)
            {
                int lvl = rom[movePtr + 1] >> 1;
                Move move = (Move)((rom[movePtr + 1] % 2) * 256 + rom[movePtr]);
                moves.Add(move, lvl);
                movePtr += 2;
            }
            movePtr += 2;    //pass final FFFF
            return movePtr;
        }
        // Read the TMcompat and HM compat BitArrays starting at ptr
        private static void ReadTMHMCompat(byte[] rom, XmlManager data, int ptr, out BitArray tmCompat, out BitArray hmCompat)
        {
            int numTms = data.Num("tmMoves");
            int numHms = data.Num("hmMoves");
            tmCompat = new BitArray(numTms);
            hmCompat = new BitArray(numHms);
            byte[] tmHmChunk = new byte[data.Size("tmHmCompat")];
            Array.ConstrainedCopy(rom, ptr, tmHmChunk, 0, data.Size("tmHmCompat"));
            int mask = 0;
            for (int p = 0; p < numTms + numHms; ++p)
            {
                if (p % 8 == 0) mask = 1;
                BitArray set = p < numTms ? tmCompat : hmCompat;
                set.Set(p < numTms ? p : p - numTms, (tmHmChunk[p / 8] & mask) > 0);
                mask = mask << 1;
            }
        }
        // Read the move tutor compatibility BitArray at tutPtr
        private static void ReadTutorCompat(byte[] rom, XmlManager data, int tutPtr, out BitArray tutCompat)
        {
            int numMoveTutors = data.Num("moveTutorMoves");
            int tutorCompatSize = data.Size("moveTutorCompat");
            tutCompat = new BitArray(numMoveTutors);
            byte[] tutChunk = new byte[tutorCompatSize];
            Array.ConstrainedCopy(rom, tutPtr, tutChunk, 0, tutorCompatSize);
            int mask = 0;
            for (int p = 0; p < numMoveTutors; ++p)
            {
                if (p % 8 == 0) mask = 1;
                tutCompat.Set(p, (tutChunk[p / 8] & mask) > 0);
                mask = mask << 1;
            }
        }
        // Read all five evolutions
        private static void ReadEvolutions(byte[] rom, XmlManager data, int evolutionPtr , out Evolution[] evolutions)
        {
            int bytesPerEvolution = (int)data.Attr("evolutions", "sizePerEvolution");
            evolutions = new Evolution[(int)data.Attr("evolutions", "evolutionsPerPokemon")];
            byte[] evolutionBlock = new byte[bytesPerEvolution]; 
            for(int i = 0; i < evolutions.Length; ++i, evolutionPtr += bytesPerEvolution)
            {
                Array.ConstrainedCopy(rom, evolutionPtr, evolutionBlock, 0, bytesPerEvolution);
                evolutions[i] = new Evolution(evolutionBlock);
            }

        }
        #endregion

        // Read the starter pokemon (TODO)
        private static Pokemon[] ReadStarters(byte[] rom, XmlManager data)
        {
            throw new System.NotImplementedException();
        }
        // Read the Trainers
        private static Trainer[] ReadTrainers(byte[] rom, XmlManager data)
        {
            List<Trainer> ret = new List<Trainer>();
            for (int i = 0; i < data.Num("trainerBattles"); ++i)
                ret.Add(new Trainer(rom, data.Addy("trainerBattles") + (i * data.Size("trainerBattles"))));
            return ret.ToArray();
        }

        // Read Type Effectiveness data
        private static TypeEffectivenessChart ReadTypeEffectivenessData(byte[] rom, XmlManager data)
        {
            TypeEffectivenessChart ret = new TypeEffectivenessChart();
            int ptr = data.Addy("typeEffectiveness");
            bool ignoreAfterForesight = false;
            // Run until the end of structure sequence (0xff 0xff 0x00)
            while (rom[ptr] != 0xff)
            {
                // Skip the ignoreAfterForesight separator (0xfe 0xfe 0x00)
                if (rom[ptr] == 0xfe)
                {
                    ignoreAfterForesight = true;
                    ptr += 3;
                }
                PokemonType attackingType = (PokemonType)rom[ptr];
                PokemonType defendingType = (PokemonType)rom[ptr + 1];
                TypeEffectiveness ae = (TypeEffectiveness)rom[ptr + 2];
                ret.Add(attackingType, defendingType, ae, ignoreAfterForesight);
                ptr += 3;
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
