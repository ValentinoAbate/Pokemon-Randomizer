using System;
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
            Rom file = new Rom(data.Rom);
            //Unlock National pokedex
            //if (data.NationalDexUnlocked)
            //{
            //    writeText(addy("e40004"), "[3172016732AC1F083229610825F00129E40825F30116CD40010003]");
            //    writeText(addy("1fa301"), "[0400e4]");
            //}
            WritePokemonBaseStats(data, file);
            WriteTypeDefinitions(data, file);
            WriteEncounters(data, file, data.Info);
            WriteTrainerBattles(data, file, data.Info);
            return file.File;
        }
        private static void WritePokemonBaseStats(RomData data, Rom file)
        {
            // do nothing right now
        }
        private static void WriteTypeDefinitions(RomData data, Rom file)
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
                    file.Repoint(data.Info.Offset("typeEffectiveness"), (int)newOffset);
            }
            else
            {
                file.WriteBlock(data.Info.Offset("typeEffectiveness"), typeData.ToArray());
            }
            #endregion
        }
        private static void WriteEncounters(RomData romData, Rom rom, XmlManager data)
        {
            #region Find and Seek encounter table
            var encounterPtrPrefix = data.Attr("wildPokemon", "ptrPrefix", data.Constants).Value;
            var prefixes = rom.FindAll(encounterPtrPrefix);
            // If no prefix was found, fall back on another method (no other method yet)
            if (prefixes.Count <= 0)
                throw new Exception("No wild pokemon ptr prefix found");
            // If more than 1 prefix was found, fall back on another method (potentially just choose the first)
            if (prefixes.Count > 1)
                throw new Exception("Wild pokemon ptr prefix is not unique");
            // Go to the location in the pointer after the prefix 
            // ptr is at the location + half the length of the hex string (-1 for the "0x" formatting)
            rom.Seek(rom.ReadPointer(prefixes[0] + (encounterPtrPrefix.Length / 2) - 1));
            #endregion

            var encounterIter = romData.Encounters.GetEnumerator();
            // Iterate until the ending marker (0xff, 0xff)
            while(rom.Peek() != 0xff || rom.Peek(1) != 0xff)
            {
                // skip bank, map, and padding
                rom.Skip(4);
                int grassPtr = rom.ReadPointer();
                int surfPtr = rom.ReadPointer();
                int rockSmashPtr = rom.ReadPointer();
                int fishPtr = rom.ReadPointer();
                // Save the internal offset before chasing pointers
                rom.Save();

                #region Load the actual Encounter sets for this area
                if (grassPtr > 0 && grassPtr < rom.Length)
                {
                    encounterIter.MoveNext();
                    WriteEncounterSet(encounterIter.Current, rom, grassPtr);
                }
                if (surfPtr > 0 && surfPtr < rom.Length)
                {
                    encounterIter.MoveNext();
                    WriteEncounterSet(encounterIter.Current, rom, surfPtr);
                }
                if (rockSmashPtr > 0 && rockSmashPtr < rom.Length)
                {
                    encounterIter.MoveNext();
                    WriteEncounterSet(encounterIter.Current, rom, rockSmashPtr);
                }
                if (fishPtr > 0 && fishPtr < rom.Length)
                {
                    encounterIter.MoveNext();
                    WriteEncounterSet(encounterIter.Current, rom, fishPtr);
                }
                #endregion

                // Load the saved offset to check the next header
                rom.Load();
            }
        }
        private static void WriteEncounterSet(EncounterSet set, Rom rom, int offset)
        {
            rom.Seek(offset);
            rom.WriteByte((byte)set.encounterRate);
            rom.Skip(3);
            rom.Seek(rom.ReadPointer());
            foreach (var encounter in set)
            {
                rom.WriteByte((byte)encounter.level);
                rom.WriteByte((byte)encounter.maxLevel);
                rom.WriteUInt16((int)encounter.pokemon);
            }
        }

        private static void WriteTrainerBattles(RomData romData, Rom rom, XmlManager data)
        {
            int numTrainers = data.Num("trainerBattles");
            rom.Seek(data.Offset("trainerBattles"));
            foreach(var trainer in romData.Trainers)
            {
                rom.Skip(40); // Just skip writing the trainers for now
                rom.Save();
                rom.Seek(trainer.pokemonOffset);
                #region Read pokemon from pokemonPtr
                foreach(var pokemon in trainer.pokemon)
                {
                    rom.WriteUInt16(pokemon.IVLevel);
                    rom.WriteUInt16(pokemon.level);
                    rom.WriteUInt16((int)pokemon.species);
                    if (trainer.dataType == TrainerPokemon.DataType.Basic)
                        rom.Skip(2); // Skip padding
                    else if (trainer.dataType == TrainerPokemon.DataType.HeldItem)
                        rom.WriteUInt16((int)pokemon.heldItem);
                    else if (trainer.dataType == TrainerPokemon.DataType.SpecialMoves)
                    {
                        foreach (var move in pokemon.moves)
                            rom.WriteUInt16((int)move);
                        rom.Skip(2);
                    }
                    else if (trainer.dataType == TrainerPokemon.DataType.SpecialMovesAndHeldItem)
                    {
                        rom.WriteUInt16((int)pokemon.heldItem);
                        foreach (var move in pokemon.moves)
                            rom.WriteUInt16((int)move);
                    }
                }
                #endregion
            }
        }
    }
}
