﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PokemonEmeraldRandomizer.Backend
{
    //This class takes a modified RomData object and converts it to a byte[]
    //to write to a file
    public static class RomWriter
    {
        public static byte[] Write(RomData data)
        {
            byte[] file = data.Rom.Clone() as byte[];
            //Unlock National pokedex
            //if (data.NationalDexUnlocked)
            //{
            //    writeText(addy("e40004"), "[3172016732AC1F083229610825F00129E40825F30116CD40010003]");
            //    writeText(addy("1fa301"), "[0400e4]");
            //}
            WritePokemonBaseStats(data, file);
            WriteTypeDefinitions(data, file);
            return file;
        }
        private static void WritePokemonBaseStats(RomData data, byte[] file)
        {
            // MESSY SLOPPY AND UNFINISHED
            #region Convert to byte[]
            List<byte> statData = new List<byte>();
            List<byte> moveData = new List<byte>();
            List<byte> atkData = new List<byte>();
            List<byte> TMHMData = new List<byte>();
            List<byte> tutorData = new List<byte>();
            foreach (var baseStats in data.Pokemon)
            {
                statData.AddRange(baseStats.ToByteArray());
            }
            #endregion
            int pkmnSize = data.Info.Size("pokemonBaseStats");
            byte[] b = new byte[700];
            Array.ConstrainedCopy(data.Rom, data.Info.Addy("pokemonBaseStats") + pkmnSize * 251, b, 0, 700);
            statData.InsertRange(pkmnSize * 251, b);
            file.WriteBlock(data.Info.Addy("pokemonBaseStats"), statData.ToArray());
        }
        private static void WriteTypeDefinitions(RomData data, byte[] file)
        {
            #region Convert TypeChart to byte[]
            List<byte> typeData = new List<byte>();
            foreach (var typePair in data.TypeDefinitions.Keys)
            {
                typeData.Add((byte)typePair.attackingType);
                typeData.Add((byte)typePair.defendingType);
                typeData.Add((byte)data.TypeDefinitions.GetEffectiveness(typePair));
            }
            typeData.AddRange(TypeEffectivenessChart.separatorSequence);
            foreach (var typePair in data.TypeDefinitions.KeysIgnoreAfterForesight)
            {
                typeData.Add((byte)typePair.attackingType);
                typeData.Add((byte)typePair.defendingType);
                typeData.Add((byte)data.TypeDefinitions.GetEffectiveness(typePair));
            }
            typeData.AddRange(TypeEffectivenessChart.endSequence);
            #endregion

            #region Write to File
            //Move and Repoint if necessary
            if (data.TypeDefinitions.Count > data.TypeDefinitions.InitCount)
            {
                int? newOffset = file.WriteInFreeSpace(typeData.ToArray());
                if (newOffset != null)
                    file.Repoint(data.Info.Addy("typeEffectiveness"), (int)newOffset);
            }
            else
            {
                file.WriteBlock(data.Info.Addy("typeEffectiveness"), typeData.ToArray());
            }
            #endregion
        }
    }
}