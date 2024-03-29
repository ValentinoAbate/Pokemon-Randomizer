﻿using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Scripting;
using PokemonRandomizer.Backend.Utilities;
using PokemonRandomizer.Backend.GenIII.Constants.ElementNames;
using System;
using System.Collections.Generic;
using System.Linq;
using PokemonRandomizer.Backend.Scripting.GenIII;
using PokemonRandomizer.Backend.Utilities.Debug;

namespace PokemonRandomizer.Backend.Writing
{
    //This class takes a modified RomData object and converts it to a byte[]
    //to write to a file
    public class Gen3RomWriter : RomWriter
    {
        private readonly Dictionary<Item, Item> itemRemaps = new Dictionary<Item, Item>();
        private readonly HashSet<Item> failedItemRemaps = new HashSet<Item>();
        private readonly Gen3ScriptWriter scriptWriter;
        private readonly Gen3MapWriter mapWriter;
        private readonly Gen3PaletteWriter paletteWriter;
        public Gen3RomWriter()
        {
            scriptWriter = new Gen3ScriptWriter(RemapItem);
            mapWriter = new Gen3MapWriter(scriptWriter, RemapItem);
            paletteWriter = new Gen3PaletteWriter();
        }
        public override Rom Write(RomData data, Rom originalRom, RomMetadata metadata, XmlManager info, Settings settings)
        {
            Timer.main.Start();
            var rom = new Rom(originalRom);
            // Initialize repoint list
            var repoints = new RepointList();
            itemRemaps.Clear();
            failedItemRemaps.Clear();
            // Create new evolution stones
            CreateNewEvolutionStones(data.NewEvolutionStones, data.ItemData, rom, info);

            // Main Data

            // Write TM definitions
            WriteMoveMappings(rom, info, ElementNames.tmMoves, data.TMMoves, info.HexAttr(ElementNames.tmMoves, "duplicateOffset"));
            // Write HM definitions
            WriteMoveMappings(rom, info, ElementNames.hmMoves, data.HMMoves, info.HexAttr(ElementNames.hmMoves, "duplicateOffset"));
            // Write Move Tutor definitions
            WriteMoveMappings(rom, info, ElementNames.tutorMoves, data.tutorMoves);
            // Write the move definitions
            WriteMoveData(data.MoveData, rom, info, ref repoints);
            WritePokemonBaseStats(data, rom, info, ref repoints);
            WriteTypeDefinitions(data, rom, info, ref repoints);
            WriteEncounters(data, rom, info);
            WriteTrainerBattles(data, rom, info);
            mapWriter.WriteMapData(data, rom, info, metadata);
            WriteItemData(data.ItemData, rom, info);
            WritePickupData(data.PickupItems, rom, info, metadata);
            // Special Pokemon
            // Write the starter pokemon
            WriteStarters(data, rom, info);
            // Write the in-game trades
            WriteInGameTrades(data.Trades, rom, info);
            // Catching tut currently only supported on BPEE
            if (metadata.IsEmerald)
            {
                WriteCatchingTutOpponent(data, rom, info);
            }

            // Hacks and tweaks

            // Write the pc potion item
            int pcPotionOffset = info.FindOffset(ElementNames.pcPotion, rom);
            if (pcPotionOffset != Rom.nullPointer)
            { 
                rom.WriteUInt16(pcPotionOffset, (int)RemapItem(data.PcStartItem));
            }
            // Write the run indoors hack if applicable
            if (settings.RunIndoors)
            {
                int offset = info.FindOffset(ElementNames.runIndoors, rom);
                // If hack is supported
                if (offset != Rom.nullPointer)
                {
                    rom.WriteByte(offset, 0x00);
                }
            }
            // Make ??? a valid type for moves if applicable. Currently only supported for emerald
            if (settings.UseUnknownTypeForMoves && metadata.IsEmerald)
            {
                // Hack the ??? type to be a valid type (Uses SP.ATK and SP.DEF)
                rom.WriteByte(0x069BCF, 0xD2);
            }
            // Apply hail weather hack if applicable
            if (settings.HailHackSetting != Settings.HailHackOption.None)
            {
                ApplyHailHack(settings.HailHackSetting, rom, info);
            }
            // Apply evolve without national dex hack if supported
            // Right now, only supports level-up evolves (not evo stones)
            if(settings.EvolveWithoutNationalDex && metadata.IsFireRedOrLeafGreen)
            {
                int offset = info.FindOffset(ElementNames.evolveWithoutNatDex, rom);
                if(offset != Rom.nullPointer)
                {
                    // Get the data from the attribute
                    var byteData = info.ByteArrayAttr(ElementNames.evolveWithoutNatDex, "data");
                    rom.WriteBlock(offset, byteData);
                }
                offset = info.FindOffset(ElementNames.stoneEvolveWithoutNatDex, rom);
                if(offset != Rom.nullPointer)
                {
                    // Get the data from the attribute
                    var byteData = info.ByteArrayAttr(ElementNames.stoneEvolveWithoutNatDex, "data");
                    rom.WriteBlock(offset, byteData);
                }
            }

            // Perform all of the repoint operations
            rom.RepointMany(repoints);
            Timer.main.Stop();
            Timer.main.Log("ROM Writing");
            return rom;
        }

        private Item RemapItem(Item item)
        {
            return itemRemaps.ContainsKey(item) ? itemRemaps[item] : item;
        }
        private void CreateNewEvolutionStones(IEnumerable<Item> newEvolutionStones, List<ItemData> itemData, Rom rom, XmlManager info)
        {
            // Item remapping (for generating new evolution stones)
            var blankItemsWithEffects = new Queue<int>();
            for (int i = (int)Item.Potion; i < (int)Item.Sitrus_Berry; ++i)
            {
                if (itemData[i].IsUnused)
                {
                    blankItemsWithEffects.Enqueue(i);
                }
            }
            // New evolution stones
            itemRemaps.Clear();
            failedItemRemaps.Clear();
            int itemEffectTableOffset = info.FindOffset(ElementNames.itemEffectsTable, rom);
            int stoneEffectOffset = info.FindOffset(ElementNames.stoneEffect, rom);
            if (itemEffectTableOffset != Rom.nullPointer && stoneEffectOffset != Rom.nullPointer)
            {
                // Get the moonstone data
                var moonStone = itemData[(int)Item.Moon_Stone];
                foreach (var item in newEvolutionStones)
                {
                    if (blankItemsWithEffects.Count <= 0)
                    {
                        failedItemRemaps.Add(item);
                        continue;
                    }
                    int newIndex = blankItemsWithEffects.Dequeue();
                    var newItemData = itemData[newIndex];
                    // Copy the old item data to the new item data
                    var oldItemData = itemData[(int)item];
                    oldItemData.CopyTo(newItemData);
                    // Set the old item's name to something different so we can recognize it
                    oldItemData.Name += "?";
                    // Copy the moonstone type and field effect offset
                    newItemData.type = moonStone.type;
                    newItemData.fieldEffectOffset = moonStone.fieldEffectOffset;
                    // Set the proper index
                    newItemData.Item = (Item)newIndex;
                    // Add the item to the remap
                    itemRemaps.Add(item, newItemData.Item);
                    // Link the evolution stone script
                    int tableIndex = (newItemData.Item - Item.Potion) * Rom.pointerSize;
                    rom.WritePointer(itemEffectTableOffset + tableIndex, stoneEffectOffset);
                }
            }
            else
            {
                foreach (var item in newEvolutionStones)
                {
                    failedItemRemaps.Add(item);
                }
            }
        }

        private void ApplyHailHack(Settings.HailHackOption option, Rom rom, XmlManager info)
        {
            if (!info.HasElement(ElementNames.hailHack))
            {
                Logger.main.Unsupported(ElementNames.hailHack);
                return;
            }
            // Hail Weather Hack. Makes the weather types "steady snow" and "three snowflakes" cause hail in battle
            // Hack routine compiled from bluRose's ASM routine. Thanks blueRose (https://www.pokecommunity.com/member.php?u=471720)!
            // Emerald offsets from Panda Face (https://www.pokecommunity.com/member.php?u=660920)
            // Three snow flake spawning issue fix from ShinyDragonHunter (https://www.pokecommunity.com/member.php?u=241758)
            // Thread with all relevant posts: https://www.pokecommunity.com/showthread.php?t=351387&page=2
            var hailRoutine = info.SafeArrayAttr(ElementNames.hailHack, "routine", info.ByteArrayAttr);
            if (hailRoutine.Length == 0)
            {
                Logger.main.Unsupported(ElementNames.hailHack + " (no routine found)");
                return;
            }
            int? hailHackroutineOffset = rom.WriteInFreeSpace(hailRoutine);
            if (hailHackroutineOffset != null)
            {
                var hailMessageBlock = new byte[] { 0xF3, 0x00 };
                if (option.HasFlag(Settings.HailHackOption.Snow))
                {
                    // Add battle weather routine
                    rom.WritePointer(info.HexAttr(ElementNames.hailHack, "snowRoutineOffset"), (int)hailHackroutineOffset);
                    // Fix message
                    rom.WriteBlock(info.HexAttr(ElementNames.hailHack, "snowMessageOffset"), hailMessageBlock);
                    // Fix Three snow flakes spawning issue
                    rom.WriteBlock(info.HexAttr(ElementNames.hailHack, "snowFixOffset"), new byte[] { 0x4B, 0xE0 });
                    if(info.HasElementWithAttr(ElementNames.hailHack, "snowPostBattleFixOffset")) 
                    {
                        rom.WriteBlock(info.HexAttr(ElementNames.hailHack, "snowPostBattleFixOffset"), new byte[] { 0x04, 0x1C, 0xE1, 0xF7, 0x32, 0xF8, 0x0D, 0x4A, 0xD9, 0x21, 0xC9, 0x00, 0x88, 0x5C });
                    }
                }
                if (option.HasFlag(Settings.HailHackOption.SteadySnow))
                {
                    // Add battle weather routine
                    rom.WritePointer(info.HexAttr(ElementNames.hailHack, "steadySnowRoutineOffset"), (int)hailHackroutineOffset);
                    // Fix message
                    rom.WriteBlock(info.HexAttr(ElementNames.hailHack, "steadySnowMessageOffset"), hailMessageBlock);
                }
            }
            else
            {
                Logger.main.Error("Failed to write hail hack routine in free space. Hail hack will not be applied");
            }
        }

        private void WriteMoveData(List<MoveData> data, Rom rom, XmlManager info, ref RepointList repoints)
        {
            int dataOffset = info.FindOffset(ElementNames.moveData, rom);
            // No data offset, move data writing unsupported
            if (dataOffset == Rom.nullPointer)
                return;
            int moveCount = info.Num(ElementNames.moveData);
            if (data.Count == moveCount + 1) // original number of moves (move 0 is empty)
            {
                rom.Seek(dataOffset);
                foreach (var moveData in data)
                    WriteMoveDataSingular(rom, moveData);
            }
            else // repoint necessary
            {
                int dataSize = info.Size(ElementNames.moveData);
                // Creat an empty rom block to write to
                Rom moveDataBlock = new Rom(dataSize * data.Count, rom.FreeSpaceByte);
                foreach (var moveData in data)
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
        private void WriteMoveDataSingular(Rom rom, MoveData data)
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
        private void WriteMoveMappings(Rom rom, XmlManager info, string elementName, Move[] moves, int? altOffset = null)
        {
            if (!info.FindAndSeekOffset(elementName, rom))
                return;
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
        private void WriteStarters(RomData romData, Rom rom, XmlManager info)
        {
            if (!info.FindAndSeekOffset(ElementNames.starterPokemon, rom))
                return;
            rom.WriteUInt16((int)romData.Starters[0]);
            rom.Skip(info.IntAttr(ElementNames.starterPokemon, "skip1"));
            rom.WriteUInt16((int)romData.Starters[1]);
            rom.Skip(info.IntAttr(ElementNames.starterPokemon, "skip2"));
            rom.WriteUInt16((int)romData.Starters[2]);
        }
        private void WriteCatchingTutOpponent(RomData data, Rom rom, XmlManager info)
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
        private void WriteInGameTrades(List<InGameTrade> trades, Rom rom, XmlManager info)
        {
            if (!info.FindAndSeekOffset(ElementNames.trades, rom))
                return; // TODO: Log
            foreach (var t in trades)
            {
                rom.WriteFixedLengthString(t.pokemonName, InGameTrade.pokemonNameLength);
                rom.WriteUInt16((int)t.pokemonRecieved);
                rom.WriteBlock(t.IVs);
                rom.WriteUInt32(t.abilityNum);
                rom.WriteUInt32(t.trainerID);
                rom.WriteBlock(t.contestStats);
                rom.Skip(3);
                rom.WriteUInt32(t.personality);
                rom.WriteUInt16((int)RemapItem(t.heldItem));
                rom.WriteByte(t.mailNum);
                rom.WriteFixedLengthString(t.trainerName, InGameTrade.trainerNameLength);
                rom.WriteByte(t.trainerGender);
                rom.WriteByte(t.sheen);
                rom.WriteUInt32((int)t.pokemonWanted);
            }
        }
        private void WritePokemonBaseStats(RomData romData, Rom rom, XmlManager info, ref RepointList repoints)
        {
            #region Setup Offsets

            // Setup pokemon offset
            int pkmnOffset = info.FindOffset(ElementNames.pokemonBaseStats, rom);
            if (pkmnOffset == Rom.nullPointer)
                return;
            int pkmnSize = info.Size(ElementNames.pokemonBaseStats);
            // Setup evolution offset
            int evolutionOffset = info.FindOffset(ElementNames.evolutions, rom);
            if (evolutionOffset == Rom.nullPointer)
                return;
            int evolutionSize = info.Size(ElementNames.evolutions);
            // Add evolution size to skip the null pokemon
            evolutionOffset += evolutionSize;
            // setup TmHmCompat offset
            int tmHmCompatOffset = info.FindOffset(ElementNames.tmHmCompat, rom);
            if (tmHmCompatOffset == Rom.nullPointer)
                return;
            int tmHmSize = info.Size(ElementNames.tmHmCompat);
            // Skip over the null pokemon
            tmHmCompatOffset += tmHmSize;
            // Setup Move Tutor Compat offset
            int tutorCompatOffset = info.FindOffset(ElementNames.tutorMoves, rom);
            if (tutorCompatOffset == Rom.nullPointer)
                return;
            int tutorSize = info.Size(ElementNames.tutorCompat);
            // Skip over the tutor move definitions to get to the compatibilities, and +tutorSize to skip the null pkmn
            tutorCompatOffset += (info.Num(ElementNames.tutorMoves) * info.Size(ElementNames.tutorMoves)) + tutorSize;
            // Setup moveset offset
            int originalMovesetOffset = info.FindOffset(ElementNames.movesets, rom);
            if (originalMovesetOffset == Rom.nullPointer)
                return;
            // Setup palette offsets
            int pokemonPaletteSize = info.Size(ElementNames.pokemonPalettes);
            int normalPaletteOffset = info.FindOffset(ElementNames.pokemonPalettes, rom) + pokemonPaletteSize;
            int shinyPaletteOffset = info.FindOffset(ElementNames.pokemonPalettesShiny, rom) + pokemonPaletteSize;

            #endregion

            int movesetIndex = 0;
            int skipNum = (int)info.Attr(ElementNames.pokemonBaseStats, "skip");
            // Create an empty ROM block to write the learnsetData to
            Rom moveData = new Rom(romData.Pokemon.Sum((stats) => (stats.learnSet.Count * 2) + 2) + (skipNum * 4), rom.FreeSpaceByte);
            // If any of the movesets have changed we will have to perform repoint operations
            bool needToRelocateMoveData = romData.Pokemon.Any((stats) => stats.learnSet.Count != stats.learnSet.OriginalCount);
            // Find a block to write to in so we can log repoints if we need to
            int? newMoveDataOffset = needToRelocateMoveData ? rom.FindFreeSpaceOffset(moveData.Length) : null;
            bool ableToRelocateMoveData = newMoveDataOffset != null;
            int skipAt = (int)info.Attr(ElementNames.pokemonBaseStats, "skipAt");
            // Main writing loop
            for (int i = 0; i < info.Num(ElementNames.pokemonBaseStats); i++)
            {
                if (i == skipAt) // potentially skip empty slots
                {
                    i += skipNum;
                    moveData.WriteBlock(movesetIndex, romData.SkippedLearnSetData);
                    movesetIndex += skipNum * 4; // (don't know why this is 4, cuz move segments are variable lengths possibly terminators?)
                }
                var stats = romData.GetBaseStats((Pokemon)(i + 1));
                WriteBaseStatsSingle(stats, pkmnOffset + (i * pkmnSize), rom);
                // Log a repoint if necessary
                if (needToRelocateMoveData)
                {
                    if (ableToRelocateMoveData)
                    {
                        repoints.Add(stats.learnSet.OriginalOffset, (int)newMoveDataOffset + movesetIndex);
                        // Write moveset
                        movesetIndex = WriteAttacks(moveData, stats.learnSet, movesetIndex);
                    }
                }
                else
                {
                    // Write moveset
                    movesetIndex = WriteAttacks(moveData, stats.learnSet, movesetIndex);
                }
                WriteTMHMCompat(stats, tmHmCompatOffset + (i * tmHmSize), rom);
                WriteTutorCompat(stats, tutorCompatOffset + (i * tutorSize), rom);
                WriteEvolutions(stats, evolutionOffset + (i * evolutionSize), rom);
                WritePokemonPalettes(stats, normalPaletteOffset + (i * pokemonPaletteSize), shinyPaletteOffset + (i * pokemonPaletteSize), rom);
            }
            // If we don't need to repoint move data, write it in it's original location
            if (!needToRelocateMoveData)
            {
                rom.WriteBlock(originalMovesetOffset, moveData.File);
            }
            else if(ableToRelocateMoveData) // else move and log repoint
            {
                rom.WriteBlock((int)newMoveDataOffset, moveData.File);
                repoints.Add(originalMovesetOffset, (int)newMoveDataOffset);
            }
            else
            {
                Logger.main.Error("Moveset data relocation is needed, but couldn't find enough Free Space to write Moveset data. Moveset data will not be written");
            }
        }
        private void WriteBaseStatsSingle(PokemonBaseStats pokemon, int offset, Rom rom)
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
            rom.WriteUInt16((int)RemapItem(pokemon.heldItems[0]));
            rom.WriteUInt16((int)RemapItem(pokemon.heldItems[1]));
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
        private int WriteAttacks(Rom rom, LearnSet moves, int offset)
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
        private void WriteTMHMCompat(PokemonBaseStats pokemon, int offset, Rom rom)
        {
            bool[] tmValues = new bool[pokemon.TMCompat.Count];
            bool[] hmValues = new bool[pokemon.HMCompat.Count];
            pokemon.TMCompat.CopyTo(tmValues, 0);
            pokemon.HMCompat.CopyTo(hmValues, 0);
            rom.WriteBits(offset, 1, tmValues.Concat(hmValues).Select((b) => b ? 1 : 0).ToArray());
        }
        private void WriteTutorCompat(PokemonBaseStats pokemon, int offset, Rom rom)
        {
            bool[] mtValues = new bool[pokemon.moveTutorCompat.Count];
            pokemon.moveTutorCompat.CopyTo(mtValues, 0);
            rom.WriteBits(offset, 1, mtValues.Select((b) => b ? 1 : 0).ToArray());
        }
        private void WriteEvolutions(PokemonBaseStats pokemon, int offset, Rom rom)
        {
            rom.Seek(offset);
            foreach (var evo in pokemon.evolvesTo)
            {
                if(evo.Type == EvolutionType.UseItem)
                {
                    var item = (Item)evo.parameter;
                    if(failedItemRemaps.Contains(item))
                    {
                        rom.WriteUInt16((int)EvolutionType.LevelUp);
                        rom.WriteUInt16(32);
                    }
                    else
                    {
                        rom.WriteUInt16((int)evo.Type);
                        rom.WriteUInt16((int)RemapItem(item));
                    }
                }
                else
                {
                    rom.WriteUInt16((int)evo.Type);
                    rom.WriteUInt16(evo.parameter);
                }
                rom.WriteUInt16((int)evo.Pokemon);
                rom.Skip(2);
            }
        }
        private void WritePokemonPalettes(PokemonBaseStats pokemon, int normalOffeset, int shinyOffset, Rom rom)
        {
            rom.SaveAndSeekOffset(normalOffeset);
            paletteWriter.WriteCompressed(rom.ReadPointer(), pokemon.palette, rom);
            rom.WriteUInt16(pokemon.paletteIndex);
            rom.Seek(shinyOffset);
            paletteWriter.WriteCompressed(rom.ReadPointer(), pokemon.shinyPalette, rom);
            rom.WriteUInt16(pokemon.shinyPaletteIndex);
            rom.LoadOffset();
        }
        private void WriteTypeDefinitions(RomData data, Rom rom, XmlManager info, ref RepointList repoints)
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
                int oldOffset = info.Offset("typeEffectiveness");
                int? newOffset = rom.WriteInFreeSpace(typeData.ToArray());
                if(newOffset != null)
                    repoints.Add(oldOffset, (int)newOffset);                   
            }
            else
            {
                rom.WriteBlock(info.Offset("typeEffectiveness"), typeData.ToArray());
            }
            #endregion
        }
        private void WriteEncounters(RomData romData, Rom rom, XmlManager info)
        {
            if (!info.FindAndSeekOffset("wildPokemon", rom))
                return;
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
        private void WriteEncounterSet(EncounterSet set, Rom rom, int offset)
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
        private void WriteTrainerBattles(RomData romData, Rom rom, XmlManager info)
        {
            // If fail, reading trainer battles is not supported for this ROM
            if (!info.FindAndSeekOffset(ElementNames.trainerBattles, rom))
                return;
            //int numTrainers = info.Num("trainerBattles"); // Needed later for determining if expansion is necessary
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
                // Write name (I think bytes 4 - 15?)
                rom.WriteFixedLengthString(trainer.name, Trainer.nameLength);
                // Write items (bytes 16-23)
                for (int i = 0; i < 4; ++i)
                    rom.WriteUInt16((int)RemapItem(trainer.useItems[i]));
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
                        rom.WriteUInt16((int)RemapItem(pokemon.heldItem));
                    else if (trainer.dataType == TrainerPokemon.DataType.SpecialMoves)
                    {
                        foreach (var move in pokemon.moves)
                            rom.WriteUInt16((int)move);
                        rom.Skip(2);
                    }
                    else if (trainer.dataType == TrainerPokemon.DataType.SpecialMovesAndHeldItem)
                    {
                        rom.WriteUInt16((int)RemapItem(pokemon.heldItem));
                        foreach (var move in pokemon.moves)
                            rom.WriteUInt16((int)move);
                    }
                }
                #endregion
                rom.LoadOffset();
            }
        }

        private static readonly string[] tmDescriptionRemovals = new string[]
        {
            " the enemy", " a little", " slightly", " about"
        };

        private static readonly (string, string)[] tmDescriptionReplacements = new (string, string)[]
        {
            ("critical- hit", "crit"), ("critical -hit", "crit"), ("critical-hit", "crit"), ("functions", "works"),
            ("POKéMON’s", "user’s"), ("leaves the user immobile the next turn.", "the user must recharge next turn."),
            ("always inflict", "inflict"), ("eliminates", "removes"), ("causes fainting", "one-hit KOs"),
            ("Flies up on the first turn", "Flies up"), ("A corkscrewing attack", "An attack"),
            ("inflicts more damage on", "does more damage to"), ("Frightens with", "Makes"), ("switch out", "switch"),
            ("horrible screech", "screech"), ("strikes", "hits"), ("rainbow-colored", "rainbow"),
            ("Liquifies the user’s body", "Liquifies the body"), ("shares them equally", "splits them"),
            ("Covers the user in mud", "Sprays mud"), ("A 1st-turn\" 1st-strike move that causes flinching", "A 1st-turn\" 1st-strike move that flinches"),
            ("in the foe’s face", "at the foe"), ("that is thrown", "thrown"), ("May confuse the foe", "May confuse"),
            ("Ensnares", "Snares"), ("Senses the foe’s action", "Senses action"), ("Wraps and squeezes", "Squeezes"),
            ("Strikes the foe with", "Strikes with"), ("strike the foe", "strike"), ("Envelops", "Covers"), ("stop it from", "prevent"),
            ("over 2 to 6 turns", "2 to 6 times"), ("if the TRAINER is disliked", "with lower friendship"),
            ("Torments the foe and stops", "Torments the foe to stop")
        };

        private void WriteItemData(IEnumerable<ItemData> itemData, Rom rom, XmlManager info)
        {
            if (!info.FindAndSeekOffset(ElementNames.itemData, rom))
                return;
            int tmDescriptionLineLength = info.IntAttr(ElementNames.itemData, "tmDescriptionLineLength");
            int tmDescriptionMaxLines = info.IntAttr(ElementNames.itemData, "tmDescriptionMaxLines");
            foreach (var item in itemData)
            {
                rom.WriteFixedLengthString(item.Name, ItemData.nameLength);
                rom.WriteUInt16((int)item.Item);
                rom.WriteUInt16(item.Price);
                rom.WriteByte(item.holdEffect);
                rom.WriteByte(item.param);
                if (item.ReformatDescription)
                {
                    if (item.IsTM())
                    {
                        item.Description = TextUtils.ReformatAndAbbreviate(item.Description, '\n',
                            tmDescriptionLineLength, tmDescriptionMaxLines, tmDescriptionRemovals, tmDescriptionReplacements);
                    }
                }
                if(item.Description.Length == item.OriginalDescription.Length)
                {
                    rom.WritePointer(item.descriptionOffset);
                    // Description may have changed even if length didn't
                    rom.WriteVariableLengthString(item.descriptionOffset, item.Description);
                }
                else
                {
                    // Description has changed
                    rom.WritePointer(rom.WriteInFreeSpace(rom.TranslateString(item.Description, true)) ?? item.descriptionOffset);
                }
                rom.WriteByte(item.keyItemValue);
                rom.WriteByte(Convert.ToByte(item.RegisterableKeyItem));
                rom.WriteByte(item.pocket);
                rom.WriteByte(item.type);
                rom.WritePointer(item.fieldEffectOffset);
                rom.WriteUInt32(item.battleUsage);
                rom.WritePointer(item.battleEffectOffset);
                rom.WriteUInt32(item.extraData);
            }
            // If we have the offset for the item sprites, write the item sprite data
            if (!info.FindAndSeekOffset(ElementNames.itemSprites, rom))
                return;
            foreach (var item in itemData)
            {
                rom.WritePointer(item.spriteOffset);
                rom.WritePointer(item.paletteOffset);
            }
        }

        private void WritePickupData(PickupData data, Rom rom, XmlManager info, RomMetadata metadata)
        { 
            if (!info.FindAndSeekOffset(ElementNames.pickupItems, rom))
                return;
            int numItems = info.Num(ElementNames.pickupItems);
            if (metadata.IsEmerald) // Use the two item tables
            {
                for (int i = 0; i < numItems; i++)
                {
                    rom.WriteUInt16((int)RemapItem(data.Items[i]));
                }
                if (!info.FindAndSeekOffset(ElementNames.pickupRareItems, rom))
                    return;
                numItems = info.Num(ElementNames.pickupRareItems);
                for (int i = 0; i < numItems; i++)
                {
                    rom.WriteUInt16((int)RemapItem(data.RareItems[i]));
                }
            }
            else // Is RS or FRLG, use items + chances
            {
                for (int i = 0; i < numItems; i++)
                {
                    var item = data.ItemChances[i];
                    rom.WriteUInt16((int)RemapItem(item.item));
                    rom.WriteUInt16(item.chance);
                }
            }
        }

        public class RepointList : List<(int, int)>
        {
            public void Add(int oldOffset, int newOffset)
            {
                Add((oldOffset, newOffset));
            }
        }
    }
}
