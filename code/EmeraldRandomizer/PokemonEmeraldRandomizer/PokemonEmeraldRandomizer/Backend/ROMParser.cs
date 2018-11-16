using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace PokemoneEmeraldRandomizer.Backend
{
    public static class ROMParser
    {
        //The MD5 Hash of the ROM this code was designed for
        private const string targetROMHash = "7b058a7aea5bfbb352026727ebd87e17";

        public static ROMData Parse(byte[] rawROM)
        {

            checkHash(rawROM);
            ROMData data = new ROMData(rawROM);
            //data.Starters = ReadStarters(rawROM);
            //data.Pokemon = ReadPokemon(rawROM);
            //data.Trainers = ReadTrainers(rawROM);
            return data;
        }
        private static TrainerPokemon[] ReadStarters(byte[] rawROM)
        {
            throw new System.NotImplementedException();
        }
        private static List<Pokemon> ReadPokemon(byte[] rawROM)
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
