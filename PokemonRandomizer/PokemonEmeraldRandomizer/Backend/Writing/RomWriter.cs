using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;

namespace PokemonRandomizer.Backend.Writing
{
    //This class takes a modified RomData object and converts it to a byte[]
    //to write to a file
    public static class RomWriter
    {
        public static byte[] Write(RomData data)
        {
            Rom file = new Rom(data.Rom);
            // Write TM definitions
            WriteMoveMappings(file, data.Info.Offset("tmMoves"), data.TMMoves);
            // Write HM definitions
            WriteMoveMappings(file, data.Info.Offset("hmMoves"), data.HMMoves);
            // Write Move Tutor definitions
            WriteMoveMappings(file, data.Info.Offset("moveTutorMoves"), data.tutorMoves);
            // Write the pc potion item
            data.Rom.WriteUInt16(data.Info.Offset("pcPotion"), (int)data.PcStartItem);
            WriteStarters(data, file, data.Info);
            WritePokemonBaseStats(data, file, data.Info);
            WriteTypeDefinitions(data, file);
            WriteEncounters(data, file, data.Info);
            WriteTrainerBattles(data, file, data.Info);
            return file.File;
        }
        // Write TM, HM, or Move tutor definitions to the rom (depending on args)
        private static void WriteMoveMappings(Rom rom, int offset, Move[] moves)
        {
            rom.Seek(offset);
            foreach(var move in moves)
                rom.WriteUInt16((int)move);
        }
        private static void WriteStarters(RomData romData, Rom rom, XmlManager data)
        {
            // For some reason FRLG need duplicates of the starters stored
            bool duplicate = bool.Parse(data.Attr("starterPokemon", "duplicate").Value);
            int dupOffset = 0;
            if(duplicate)
                dupOffset = data.IntAttr("starterPokemon", "dupOffset");
            rom.Seek(data.Offset("starterPokemon"));
            rom.WriteUInt16((int)romData.Starters[0]);
            if(duplicate)
                rom.WriteUInt16(rom.InternalOffset + dupOffset, (int)romData.Starters[0]);
            rom.Skip(data.IntAttr("starterPokemon", "skip1"));
            rom.WriteUInt16((int)romData.Starters[1]);
            if (duplicate)
                rom.WriteUInt16(rom.InternalOffset + dupOffset, (int)romData.Starters[1]);
            rom.Skip(data.IntAttr("starterPokemon", "skip2"));
            rom.WriteUInt16((int)romData.Starters[2]);
            if (duplicate)
                rom.WriteUInt16(rom.InternalOffset + dupOffset, (int)romData.Starters[2]);
        }
        private static void WritePokemonBaseStats(RomData romData, Rom rom, XmlManager data)
        {
            int pkmnPtr = data.Offset("pokemonBaseStats");
            int pkmnSize = data.Size("pokemonBaseStats");
            int tmPtr = data.Offset("tmHmCompat");
            int tmHmSize = data.Size("tmHmCompat");
            int tutorPtr = data.Offset("moveTutorCompat");
            int tutorSize = data.Size("moveTutorCompat");
            int movePtr = data.Offset("movesets");
            int evolutionPtr = data.Offset("evolutions");
            int evolutionSize = data.Size("evolutions");
            rom.Seek(pkmnPtr);
            for (int i = 0; i < data.Num("pokemonBaseStats"); i++)
            {
                if (i == (int)data.Attr("pokemonBaseStats", "skipAt")) // potentially skip empty slots
                {
                    int skipNum = (int)data.Attr("pokemonBaseStats", "skip");
                    i += skipNum;
                    movePtr += skipNum * 4; // (don't know why this is 4, cuz move segments are variable lengths possibly terminators?)
                    rom.Skip(pkmnSize * skipNum);
                }
                WriteBaseStatsSingle(romData.PokemonLookup[(PokemonSpecies)(i + 1)], rom);
                //movePtr = ReadAttacks(rom, movePtr, out pkmn.learnSet);
                //ReadTMHMCompat(rom, data, tmPtr + (i * tmHmSize), out pkmn.TMCompat, out pkmn.HMCompat);
                //ReadTutorCompat(rom, data, tutorPtr + (i * tutorSize), out pkmn.moveTutorCompat);
                rom.SaveOffset();
                WriteEvolutions(romData.PokemonLookup[(PokemonSpecies)(i + 1)], evolutionPtr + (i * evolutionSize), romData, rom, data);
                rom.LoadOffset();
            }
        }
        private static void WriteBaseStatsSingle(PokemonBaseStats pokemon, Rom rom)
        {
            // fill in stats (hp/at/df/sp/sa/sd)
            rom.WriteBlock(pokemon.stats);
            // convert types to bytes and write
            rom.WriteBlock(Array.ConvertAll(pokemon.types, (t) => (byte)t));
            rom.WriteByte(pokemon.catchRate);
            rom.WriteByte(pokemon.baseExpYield);
            // Next two bytes bits 0-11 are ev Yields, in chunks of 2
            rom.WriteBits(2, pokemon.evYields);
            rom.WriteUInt16((int)pokemon.heldItems[0]);
            rom.WriteUInt16((int)pokemon.heldItems[1]);
            rom.WriteByte(pokemon.genderRatio);
            rom.WriteByte(pokemon.eggCycles);
            rom.WriteByte((byte)pokemon.growthType);
            rom.WriteByte((byte)pokemon.eggGroups[0]);
            rom.WriteByte((byte)pokemon.eggGroups[1]);
            rom.WriteByte((byte)pokemon.abilities[0]);
            rom.WriteByte((byte)pokemon.abilities[1]);
            rom.WriteByte(pokemon.safariZoneRunRate);
            rom.WriteByte((byte)(((byte)pokemon.searchColor << 1) + Convert.ToByte(pokemon.flip)));
            // Padding
            rom.SetBlock(2, 0x00);
        }
        private static void WriteEvolutions(PokemonBaseStats pokemon, int offset, RomData romData, Rom rom, XmlManager data)
        {
            rom.Seek(offset);
            foreach(var evo in pokemon.evolvesTo)
            {
                rom.WriteUInt16((int)evo.Type);
                rom.WriteUInt16(evo.parameter);
                rom.WriteUInt16((int)evo.Pokemon);
                rom.Skip(2);
            }
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
                rom.SaveOffset();

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
                rom.LoadOffset();
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
                rom.WriteByte((byte)trainer.dataType);
                rom.WriteByte((byte)trainer.trainerClass);
                //// Write Gender (byte 2 bit 0)
                //gender = (Gender)((rom.Peek() & 0x80) >> 7);
                //// Write music track index (byte 2 bits 1-7)
                //musicIndex = (byte)(rom.WriteByte() & 0x7F);
                rom.Skip(1);
                // Write sprite index (byte 3)
                rom.WriteByte(trainer.spriteIndex);
                //// Write name (I think bytes 4 - 15?)
                //name = rom.WriteFixedLengthString(12);
                rom.Skip(12);
                // Write items (bytes 16-23)
                for (int i = 0; i < 4; ++i)
                    rom.WriteUInt16((int)trainer.useItems[i]);
                // Write double battle (byte 24)
                rom.WriteByte(Convert.ToByte(trainer.isDoubleBattle));
                // What is in bytes 25-27?
                rom.Skip(3);
                // Write AI flags
                byte[] aiBytes = new byte[4];
                trainer.AIFlags.CopyTo(aiBytes, 0);
                rom.WriteBlock(aiBytes);
                // Write pokemon num
                rom.WriteByte((byte)trainer.pokemon.Length);
                // What is in bytes 33-35?
                rom.Skip(3);
                // Bytes 36-39 (end of data)
                rom.WritePointer(trainer.pokemonOffset);

                rom.SaveOffset();
                rom.Seek(trainer.pokemonOffset);
                #region Write pokemon from pokemonPtr
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
                rom.LoadOffset();
            }
        }
    }
}
