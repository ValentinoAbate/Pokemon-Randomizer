using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;
using PokemonRandomizer.Backend.Scripting;

namespace PokemonRandomizer.Backend.Writing
{
    //This class takes a modified RomData object and converts it to a byte[]
    //to write to a file
    public static class RomWriter
    {
        public static byte[] Write(RomData data)
        {
            Rom rom = new Rom(data.Rom);
            var info = data.Info;
            var repoints = new RepointList();

            // Main Data

            // Write TM definitions
            WriteMoveMappings(rom, info.Offset("tmMoves"), data.TMMoves, info.HexAttr("tmMoves", "duplicateOffset"));
            // Write HM definitions
            WriteMoveMappings(rom, info.Offset("hmMoves"), data.HMMoves, info.HexAttr("hmMoves", "duplicateOffset"));
            // Write Move Tutor definitions
            WriteMoveMappings(rom, info.Offset("moveTutorMoves"), data.tutorMoves);
            // Write the move definitions
            WriteMoveData(data, rom, info, ref repoints);
            // Starter Writing currently only supported for RSE
            if(data.IsRubySapphireOrEmerald)
            {
                WriteStarters(data, rom, info);
            }
            // Catching tut currently only supported on BPEE
            if(data.Code == RomData.gameCodeEm)
            {
                WriteCatchingTutOpponent(data, rom, info);
            }
            WritePokemonBaseStats(data, rom, info, ref repoints);
            WriteTypeDefinitions(data, rom, ref repoints);
            WriteEncounters(data, rom, info);
            WriteTrainerBattles(data, rom, info);
            if(data.IsRubySapphireOrEmerald)
            {
                WriteMapData(data, rom, info);
            }

            // Hacks and tweaks

            // Write the pc potion item
            int? pcPotionOffset = info.FindOffset("pcPotion", rom);
            if (pcPotionOffset != null && data.Code == RomData.gameCodeEm)
            {
                rom.WriteUInt16((int)pcPotionOffset, (int)data.PcStartItem);
            }
            // Write the run indoors hack if applicable
            if (data.RunIndoors)
            {
                int? offset = info.FindOffset("runIndoors", rom);
                // If hack is supported
                if (offset != null)
                {
                    rom.WriteByte((int)offset, 0x00);
                }
            }
            // Make ??? a valid type for moves if applicable. Currently only supported for emerald
            if (data.UseUnknownTypeForMoves && data.Code == RomData.gameCodeEm)
            {
                // Hack the ??? type to be a valid type (Uses SP.ATK and SP.DEF)
                rom.WriteByte(0x069BCF, 0xD2);
            }
            // Apply hail weather hack if applicable. Currently only supported for emerald
            if (data.SnowyWeatherApplysHail && data.Code == RomData.gameCodeEm)
            {
                // Hail Weather Hack. Makes the weather types "steady snow" and "three snowflakes" cause hail in battle
                // Hack routine compiled from bluRose's ASM routine. Thanks blueRose (https://www.pokecommunity.com/member.php?u=471720)!
                // Emerald offsets from Panda Face (https://www.pokecommunity.com/member.php?u=660920)
                // Three snow flake spawning issue fix from ShinyDragonHunter (https://www.pokecommunity.com/member.php?u=241758)
                // Thread with all relevant posts: https://www.pokecommunity.com/showthread.php?t=351387&page=2
                var hailRoutine = new byte[]
                {
                    0x08, 0x4B, 0x19, 0x88, 0x80, 0x22, 0x10, 0x1C, 0x08, 0x40, 0x00, 0x28, 0x07, 0xD1, 0x1A, 0x80,
                    0x05, 0x49, 0x0D, 0x20, 0x08, 0x74, 0x53, 0x46, 0xCB, 0x75, 0x05, 0x48, 0x00, 0x47, 0x03, 0x48,
                    0x00, 0x47, 0x00, 0x00, 0xCC, 0x43, 0x02, 0x02, 0x74, 0x44, 0x02, 0x02, 0x4D, 0x2B, 0x04, 0x08,
                    0x43, 0x2B, 0x04, 0x08
                };
                int? hailHackroutineOffset = rom.WriteInFreeSpace(hailRoutine);
                if (hailHackroutineOffset != null)
                {
                    // Three snow flakes
                    rom.WritePointer(0x42AB8, (int)hailHackroutineOffset);
                    // Steady snow
                    rom.WritePointer(0x42AC4, (int)hailHackroutineOffset);
                    var hailMessageBlock = new byte[] { 0xF3, 0x00 };
                    //Three snow flakes
                    rom.WriteBlock(0x5CC922, hailMessageBlock);
                    // Steady snow
                    rom.WriteBlock(0x5CC928, hailMessageBlock);
                    // Fix Three snow flakes spawning issue
                    rom.WriteBlock(0xAD39E, new byte[] { 0x4B, 0xE0 });
                }
            }

            // Perform all of the repoint operations
            rom.RepointMany(repoints);
            return rom.File;
        }

        private static void WriteMoveData(RomData data, Rom rom, XmlManager info, ref RepointList repoints)
        {;
            int moveCount = int.Parse(info.Attr("moveData", "num", info.Constants).Value);
            int dataPtrOffset = HexUtils.HexToInt(info.Attr("moveData", "ptr", info.Constants).Value);
            int dataOffset = rom.ReadPointer(dataPtrOffset);
            if (data.MoveData.Count == moveCount + 1) // original number of moves (move 0 is empty)
            {
                rom.Seek(dataOffset);
                foreach (var moveData in data.MoveData)
                    WriteMoveDataSingular(rom, moveData);
            }
            else // repoint necessary
            {
                int dataSize = info.Size("moveData");
                // Creat an empty rom block to write to
                Rom moveDataBlock = new Rom(dataSize * data.MoveData.Count, rom.FreeSpaceByte);
                foreach (var moveData in data.MoveData)
                    WriteMoveDataSingular(moveDataBlock, moveData);
                int? newOffset = rom.WriteInFreeSpace(moveDataBlock.File);
                if (newOffset != null)
                {
                    const int ppOffset = 4;
                    int newOffsetInt = (int)newOffset;
                    // Log repoint for main movedata
                    repoints.Add(dataOffset, newOffsetInt);
                    // Log repoint for PP data (original offset + 4)
                    repoints.Add(dataOffset + ppOffset, newOffsetInt + ppOffset);
                    // Wipe the old moveData location
                    rom.WipeBlock(dataOffset, dataSize * moveCount);
                }
            }
        }
        private static void WriteMoveDataSingular(Rom rom, MoveData data)
        {
            rom.WriteByte((byte)data.effect);
            rom.WriteByte(data.power);
            rom.WriteByte((byte)data.type);
            rom.WriteByte(data.accuracy);
            rom.WriteByte(data.pp);
            rom.WriteByte(data.effectChance);
            rom.WriteByte((byte)data.targets);
            rom.WriteByte(data.priority);
            byte[] flags = new byte[1];
            data.flags.CopyTo(flags, 0);
            rom.WriteByte(flags[0]);
            rom.SetBlock(3, 0x00); // three bytes of 0x00
        }

        // Write TM, HM, or Move tutor definitions to the rom (depending on args)
        private static void WriteMoveMappings(Rom rom, int offset, Move[] moves, int? altOffset = null)
        {
            rom.Seek(offset);
            foreach (var move in moves)
                rom.WriteUInt16((int)move);
            // 0 check guards against alt offset not existing. Will make better in the future
            if (altOffset != null && (int)altOffset != 0)
            {
                rom.Seek((int)altOffset);
                foreach (var move in moves)
                    rom.WriteUInt16((int)move);
            }
        }
        private static void WriteStarters(RomData romData, Rom rom, XmlManager info)
        {
            // For some reason FRLG need duplicates of the starters stored
            bool duplicate = bool.Parse(info.Attr("starterPokemon", "duplicate").Value);
            int dupOffset = 0;
            if (duplicate)
                dupOffset = info.IntAttr("starterPokemon", "dupOffset");
            rom.Seek(info.Offset("starterPokemon"));
            rom.WriteUInt16((int)romData.Starters[0]);
            if (duplicate)
                rom.WriteUInt16(rom.InternalOffset + dupOffset, (int)romData.Starters[0]);
            rom.Skip(info.IntAttr("starterPokemon", "skip1"));
            rom.WriteUInt16((int)romData.Starters[1]);
            if (duplicate)
                rom.WriteUInt16(rom.InternalOffset + dupOffset, (int)romData.Starters[1]);
            rom.Skip(info.IntAttr("starterPokemon", "skip2"));
            rom.WriteUInt16((int)romData.Starters[2]);
            if (duplicate)
                rom.WriteUInt16(rom.InternalOffset + dupOffset, (int)romData.Starters[2]);
        }
        private static void WriteCatchingTutOpponent(RomData data, Rom rom, XmlManager info)
        {
            rom.Seek(info.Offset("catchingTutOpponent"));
            int pkmn = (int)data.CatchingTutPokemon;
            if (pkmn < 255)
            {
                rom.WriteByte((byte)pkmn);
                rom.WriteByte(Gen3Opcodes.setRegister | Gen3Opcodes.reg1);
                rom.WriteUInt16(0);
            }
            else
            {
                rom.WriteByte(0xFF);
                rom.WriteByte(Gen3Opcodes.setRegister | Gen3Opcodes.reg1);
                rom.WriteByte((byte)(pkmn - 0xFF));
                rom.WriteByte(Gen3Opcodes.addRegister | Gen3Opcodes.reg1);
            }
        }
        private static void WritePokemonBaseStats(RomData romData, Rom rom, XmlManager info, ref RepointList repoints)
        {
            int? offsetCheck = null;
            int pkmnPtr = info.Offset("pokemonBaseStats");
            int pkmnSize = info.Size("pokemonBaseStats");
            int tmPtr = info.Offset("tmHmCompat");
            int tmHmSize = info.Size("tmHmCompat");
            int tutorSize = info.Size("moveTutorCompat");
            // Need an extra +4 to skip the null pokemon. Determined from the move tutor move table's offset
            int tutorPtr = info.Offset("moveTutorMoves") + info.Num("moveTutorMoves") * info.Size("moveTutorMoves") + tutorSize;
            int originalMovePtr = info.Offset("movesets");
            int movePtr = 0;
            // Setup evolution offset
            const string evolutionsElt = "evolutions";
            offsetCheck = info.FindOffset(evolutionsElt, rom);
            if (offsetCheck == null)
                return;
            int evolutionSize = info.Size(evolutionsElt);
            // Add evolution size to skip the null pokemon
            int evolutionOffset = (int)offsetCheck + evolutionSize;
            int skipNum = (int)info.Attr("pokemonBaseStats", "skip");
            // Create an empty ROM block to write the learnsetData to
            Rom moveData = new Rom(romData.Pokemon.Sum((stats) => (stats.learnSet.Count * 2) + 2) + (skipNum * 4), rom.FreeSpaceByte);
            // If any of the movesets have changed we will have to perform repoint operations
            bool needToRelocateMoveData = romData.Pokemon.Any((stats) => stats.learnSet.Count != stats.learnSet.OriginalCount);
            // Find a block to write to in so we can log repoints if we need to
            int newMoveDataOffset = needToRelocateMoveData ? rom.ScanForFreeSpace(moveData.Length).offset : 0;
            // Main writing loop
            for (int i = 0; i < info.Num("pokemonBaseStats"); i++)
            {
                if (i == (int)info.Attr("pokemonBaseStats", "skipAt")) // potentially skip empty slots
                {
                    i += skipNum;
                    moveData.WriteBlock(movePtr, romData.SkippedLearnSetData);
                    movePtr += skipNum * 4; // (don't know why this is 4, cuz move segments are variable lengths possibly terminators?)
                }
                var stats = romData.PokemonLookup[(PokemonSpecies)(i + 1)];
                WriteBaseStatsSingle(stats, pkmnPtr + (i * pkmnSize), rom);
                // Log a repoint if necessary
                if (needToRelocateMoveData)
                    repoints.Add(stats.learnSet.OriginalOffset, newMoveDataOffset + movePtr);
                // Write moveset
                movePtr = WriteAttacks(moveData, stats.learnSet, movePtr);
                WriteTMHMCompat(stats, tmPtr + (i * tmHmSize), rom);
                WriteTutorCompat(stats, tutorPtr + (i * tutorSize), rom);
                WriteEvolutions(stats, evolutionOffset + (i * evolutionSize), rom);
            }
            // If we don't need to repoint move data, write it in it's original location
            if (!needToRelocateMoveData)
                rom.WriteBlock(originalMovePtr, moveData.File);
            else // else move and log repoint
            {
                rom.WriteBlock(newMoveDataOffset, moveData.File);
                rom.WipeBlock(originalMovePtr, moveData.File.Length);
                repoints.Add(originalMovePtr, (int)newMoveDataOffset);
            }
        }
        private static void WriteBaseStatsSingle(PokemonBaseStats pokemon, int offset, Rom rom)
        {
            rom.SaveOffset();
            rom.Seek(offset);
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
            rom.WriteByte(pokemon.baseFriendship);
            rom.WriteByte((byte)pokemon.growthType);
            rom.WriteByte((byte)pokemon.eggGroups[0]);
            rom.WriteByte((byte)pokemon.eggGroups[1]);
            rom.WriteByte((byte)pokemon.abilities[0]);
            rom.WriteByte((byte)pokemon.abilities[1]);
            rom.WriteByte(pokemon.safariZoneRunRate);
            rom.WriteByte((byte)(((byte)pokemon.searchColor << 1) + Convert.ToByte(pokemon.flip)));
            // Padding
            rom.SetBlock(2, 0x00);
            rom.LoadOffset();
        }

        // Write the attacks starting at offset (returns the index after completion)
        private static int WriteAttacks(Rom rom, LearnSet moves, int offset)
        {
            foreach (var move in moves)
            {
                // Write the first byte of the move
                rom.WriteByte(offset, (byte)move.move);
                // if the move number is over 255, the last bit of the learn level byte is set to 1
                if ((int)move.move > 255)
                    rom.WriteByte(offset + 1, (byte)(move.learnLvl * 2 + 1));
                else
                    rom.WriteByte(offset + 1, (byte)(move.learnLvl * 2));
                offset += 2;
            }
            rom.WriteByte(offset, 0xff);
            rom.WriteByte(offset + 1, 0xff);
            offset += 2;    //pass final FFFF
            return offset;
        }
        private static void WriteTMHMCompat(PokemonBaseStats pokemon, int offset, Rom rom)
        {
            bool[] tmValues = new bool[pokemon.TMCompat.Count];
            bool[] hmValues = new bool[pokemon.HMCompat.Count];
            pokemon.TMCompat.CopyTo(tmValues, 0);
            pokemon.HMCompat.CopyTo(hmValues, 0);
            rom.WriteBits(offset, 1, tmValues.Concat(hmValues).Select((b) => b ? 1 : 0).ToArray());
        }
        private static void WriteTutorCompat(PokemonBaseStats pokemon, int offset, Rom rom)
        {
            bool[] mtValues = new bool[pokemon.moveTutorCompat.Count];
            pokemon.moveTutorCompat.CopyTo(mtValues, 0);
            rom.WriteBits(offset, 1, mtValues.Select((b) => b ? 1 : 0).ToArray());
        }
        private static void WriteEvolutions(PokemonBaseStats pokemon, int offset, Rom rom)
        {
            rom.Seek(offset);
            foreach (var evo in pokemon.evolvesTo)
            {
                rom.WriteUInt16((int)evo.Type);
                rom.WriteUInt16(evo.parameter);
                rom.WriteUInt16((int)evo.Pokemon);
                rom.Skip(2);
            }
        }
        private static void WriteTypeDefinitions(RomData data, Rom rom, ref RepointList repoints)
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
                int oldOffset = data.Info.Offset("typeEffectiveness");
                int? newOffset = rom.WriteInFreeSpace(typeData.ToArray());
                if(newOffset != null)
                    repoints.Add(oldOffset, (int)newOffset);                   
            }
            else
            {
                rom.WriteBlock(data.Info.Offset("typeEffectiveness"), typeData.ToArray());
            }
            #endregion
        }
        private static void WriteEncounters(RomData romData, Rom rom, XmlManager info)
        {
            #region Find and Seek encounter table
            var encounterPtrPrefix = info.Attr("wildPokemon", "ptrPrefix", info.Constants).Value;
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
            while (rom.Peek() != 0xff || rom.Peek(1) != 0xff)
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
        /// <summary>
        /// Write the trainer battles to the output file. Doesn't write all data (currently unfinished)
        /// </summary>
        private static void WriteTrainerBattles(RomData romData, Rom rom, XmlManager info)
        {
            //int numTrainers = info.Num("trainerBattles"); // Needed later for determining if expansion is necessary
            rom.Seek(info.Offset("trainerBattles"));
            foreach (var trainer in romData.Trainers)
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
                foreach (var pokemon in trainer.pokemon)
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
        private static void WriteMapData(RomData romData, Rom rom, XmlManager info)
        {
            romData.Maps.Write(rom, info);
        }
        private class RepointList : List<Tuple<int, int>>
        {
            public void Add(int oldOffset, int newOffset)
            {
                Add(new Tuple<int, int>(oldOffset, newOffset));
            }
        }
    }
}
