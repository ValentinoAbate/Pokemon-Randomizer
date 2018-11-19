using System;
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
        //The MD5 Hash of the ROM this code was designed for
        private const string targetROMHash = "7b058a7aea5bfbb352026727ebd87e17";
        //Parse the ROM bytes into a ROMData object
        public static ROMData Parse(byte[] rawROM)
        {

            //checkHash(rawROM); TURNED FOR DEBUG
            ROMData data = new ROMData(rawROM);
            data.Pokemon = ReadPokemonBaseStats(rawROM);
            //data.Starters = ReadStarters(rawROM);
            //data.Trainers = ReadTrainers(rawROM);
            return data;
        }
        //Read the Pokemon base stat definitions from the ROM
        private static List<PokemonBaseStats> ReadPokemonBaseStats(byte[] rawROM)
        {
            List<PokemonBaseStats> pokemon = new List<PokemonBaseStats>();
            int pkmnPtr = AddressUtils.pokemonBaseStatsAddy;
            int TMPtr = AddressUtils.TMCompatAddy;
            int tutorPtr = AddressUtils.tutorCompatAddy;
            int movePtr = AddressUtils.movesetAddy;
            byte[] curr_block = new byte[28];
            for (int i = 0; i < 411; i++, pkmnPtr += 28)//, TMptr += 8, tutorPtr += 4)
            {
                if (i >= 251 && i < 276) //skip 25 empty slots
                {    
                    i += 25;
                    pkmnPtr += (25 * 28);
                    movePtr += 100; //4 * 25
                }
                //family = false;
                //preevo = false;
                //Create Pokemon
                Array.ConstrainedCopy(rawROM, pkmnPtr, curr_block, 0, 28); 
                PokemonBaseStats pkmn = new PokemonBaseStats(curr_block, (PokemonSpecies)(i + 1));
                //Read Attacks
                movePtr = ReadAttacks(rawROM, movePtr, out pkmn.moveSet);
                pokemon.Add(pkmn);
            }
            return pokemon;
        }
        //Read the attacks starting at atkPtr (returns the index after completion)
        private static int ReadAttacks(byte[] rawROM, int movePtr, out MoveSet moves)
        {
            moves = new MoveSet();
            while (rawROM[movePtr] != 255 || rawROM[movePtr + 1] != 255)
            {
                int lvl = rawROM[movePtr + 1] >> 1;
                Move move = (Move)((rawROM[movePtr + 1] % 2) * 256 + rawROM[movePtr]);
                moves.Add(move, lvl);
                movePtr += 2;
            }
            movePtr += 2;    //pass final FFFF
            return movePtr;
        }
        private static Pokemon[] ReadStarters(byte[] rawROM)
        {
            throw new System.NotImplementedException();
        }
        private static List<Trainer> ReadTrainers(byte[] rawROM)
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
