﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace PokemonEmeraldRandomizer.Backend
{
    public static class ROMParser
    {
        #region Symbolic Constants
        //The MD5 Hash of the ROM this code was designed for
        private const string targetROMHash = "7b058a7aea5bfbb352026727ebd87e17";
        //Number of bytes in the pokemon base stats data structure
        private const int pkmnBaseStatBytes = 28;
        //Number of bytes in a pokemon's TM/HM compat chunk
        private const int TMHMCompatBytes = 8;
        //Number of bytes in a pokemon's tutor compat chunk
        private const int tutorCompatBytes = 4;
        #endregion

        //Parse the ROM bytes into a ROMData object
        public static ROMData Parse(byte[] rom)
        {

            //checkHash(rom); TURNED OFF FOR DEBUG
            ROMData data = new ROMData(rom);

            #region Move Mappings (TMs/HMs/Tutors)
            //Read the TM move mappings from the ROM
            data.TMMoves = ReadMoveMappings(rom, AddyUtils.TMMovesAddy, ROMData.numTMs);
            //Read the HM move mappings from the ROM
            data.HMMoves = ReadMoveMappings(rom, AddyUtils.HHMovesAddy, ROMData.numHMs);
            //Read the move tutor move mappings from the ROM
            data.tutorMoves = ReadMoveMappings(rom, AddyUtils.moveTutorMovesAddy, ROMData.numMoveTutors);
            #endregion

            //Read the pokemon base stats from the ROM
            data.Pokemon = ReadPokemonBaseStats(rom);
            //data.Starters = ReadStarters(rom);
            //data.Trainers = ReadTrainers(rom);
            return data;
        }
        //Read TM, HM, or Move tutor definitions from the rom (depending on args)
        private static Move[] ReadMoveMappings(byte[] rom, int ptr, int numToRead)
        {
            Move[] moves = new Move[numToRead];
            for (int i = 0; i < numToRead; i++, ptr += 2)
            {
                moves[i] = (Move)(rom[ptr] + rom[ptr + 1] * 256);
            }
            return moves;
        }

        #region Read Pokemon Base Stats
        //Read the Pokemon base stat definitions from the ROM
        private static PokemonBaseStats[] ReadPokemonBaseStats(byte[] rom)
        {
            List<PokemonBaseStats> pokemon = new List<PokemonBaseStats>();
            int pkmnPtr = AddyUtils.pokemonBaseStatsAddy;
            int TMPtr = AddyUtils.TMCompatAddy;
            int tutorPtr = AddyUtils.tutorCompatAddy;
            int movePtr = AddyUtils.movesetAddy;
            byte[] curr_block = new byte[pkmnBaseStatBytes];
            for (int i = 0; i < 411; i++, pkmnPtr += pkmnBaseStatBytes, TMPtr += TMHMCompatBytes, tutorPtr += tutorCompatBytes)
            {
                if (i >= 251 && i < 276) //skip 25 empty slots
                {    
                    i += 25;
                    pkmnPtr += 700; //pkmnBaseStatBytes * 25;
                    movePtr += 100; //4 * 25 (don't know why this is 4, cuz move segments are variable lengths);
                    TMPtr += 200; //TMHMCompatBytes * 25
                    tutorPtr += 100; //tutorCompatBytes * 25
                }
                //family = false;
                //preevo = false;
                //Create Pokemon
                Array.ConstrainedCopy(rom, pkmnPtr, curr_block, 0, pkmnBaseStatBytes); 
                PokemonBaseStats pkmn = new PokemonBaseStats(curr_block, (PokemonSpecies)(i + 1));
                movePtr = ReadAttacks(rom, movePtr, out pkmn.moveSet);
                ReadTMHMCompat(rom, TMPtr, out pkmn.TMCompat, out pkmn.HMCompat);
                ReadTutorCompat(rom, tutorPtr, out pkmn.moveTutorCompat);
                pokemon.Add(pkmn);
            }
            return pokemon.ToArray();
        }
        //Read the attacks starting at atkPtr (returns the index after completion)
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
        //Read the TMcompat and HM compat BitArrays starting at ptr
        private static void ReadTMHMCompat(byte[] rom, int ptr, out BitArray TMCompat, out BitArray HMCompat)
        {
            TMCompat = new BitArray(ROMData.numTMs);
            HMCompat = new BitArray(ROMData.numHMs);
            byte[] tm_hm_chunk = new byte[TMHMCompatBytes];
            Array.ConstrainedCopy(rom, ptr, tm_hm_chunk, 0, TMHMCompatBytes);
            int mask = 0;
            for (int p = 0; p < ROMData.numTMHMs; ++p)
            {
                if (p % 8 == 0) mask = 1;
                BitArray set = p < ROMData.numTMs ? TMCompat : HMCompat;
                set.Set(p < ROMData.numTMs ? p : p - ROMData.numTMs, (tm_hm_chunk[p / 8] & mask) > 0);
                mask = mask << 1;
            }
        }
        //Read the move tutor compatibility BitArray at tutPtr
        private static void ReadTutorCompat(byte[] rom, int tutPtr, out BitArray tutCompat)
        {
            tutCompat = new BitArray(ROMData.numMoveTutors);
            byte[] tut_chunk = new byte[tutorCompatBytes];
            Array.ConstrainedCopy(rom, tutPtr, tut_chunk, 0, tutorCompatBytes);
            int mask = 0;
            for (int p = 0; p < ROMData.numMoveTutors; ++p)
            {
                if (p % 8 == 0) mask = 1;
                tutCompat.Set(p, (tut_chunk[p / 8] & mask) > 0);
                mask = mask << 1;
            }
        }
        #endregion

        //Read the starter pokemon (TODO)
        private static Pokemon[] ReadStarters(byte[] rom)
        {
            throw new System.NotImplementedException();
        }
        //Read the Trainers (TODO)
        private static List<Trainer> ReadTrainers(byte[] rom)
        {
            throw new System.NotImplementedException();
        }
        //Checks the hash of the rom to see if its the right version
        private static void checkHash(byte[] rawRom)
        {
            MD5 mD5 = MD5.Create();
            byte[] data = mD5.ComputeHash(rawRom); 
            // Create a new Stringbuilder to collect the byte and create a string.
            StringBuilder sBuilder = new StringBuilder();
            // Loop through each byte of the hashed data and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            string hash = sBuilder.ToString();
            if (hash != targetROMHash)
            {
                System.Windows.MessageBox.Show("The base ROM does not match the target ROM. This program was intended for!\n\nMD5: "
                                                + hash + "\nExpected: " + targetROMHash, "WARNING: ", System.Windows.MessageBoxButton.OK);
            }
        }
    }
}
