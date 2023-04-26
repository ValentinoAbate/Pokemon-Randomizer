using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Scripting;
using PokemonRandomizer.Backend.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using PokemonRandomizer.Backend.Scripting.GenIII;
using PokemonRandomizer.Backend.Utilities.Debug;
using PokemonRandomizer.Backend.DataStructures.Scripts;
using PokemonRandomizer.Backend.Constants;
using PokemonRandomizer.Backend.RomHandling.IndexTranslators;
using PokemonRandomizer.Backend.DataStructures.Trainers;

namespace PokemonRandomizer.Backend.RomHandling.Writing
{
    //This class takes a modified RomData object and converts it to a byte[]
    //to write to a file
    public class Gen3RomWriter : RomWriter
    {
        protected override IIndexTranslator IndexTranslator => Gen3IndexTranslator.Main;

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
            // Attatch settings to script writer
            scriptWriter.Settings = settings;
            // Clear item remaps
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
            WriteMoveMappings(rom, info, ElementNames.tutorMoves, data.TutorMoves);
            // Write the move definitions
            WriteMoveData(data.MoveData, rom, info, ref repoints);
            WritePokemonBaseStats(data, rom, info);
            WriteTypeDefinitions(data.TypeDefinitions, rom, info, ref repoints);
            WriteEncounters(data, rom, info);
            WriteTrainerSprites(data, rom, info);
            WriteTrainerBattles(data, rom, info);
            WriteStevenAllyTrainerBattle(data, rom, info);
            mapWriter.WriteMapData(data, rom, info, metadata);
            WriteItemData(data.ItemData, rom, info);
            WritePickupData(data.PickupItems, rom, info, metadata);
            WriteSetBerryTreeScript(data.SetBerryTreeScript, rom, info, metadata);
            // Special Pokemon
            // Write the starter pokemon
            WriteStarters(data, rom, info);
            // Write the in-game trades
            WriteInGameTrades(data.Trades, rom, info);
            // Catching tut currently only supported on RSE
            if (settings.WriteCatchingTutPokemon && metadata.IsRubySapphireOrEmerald)
            {
                WriteCatchingTutOpponent(data, rom, info);
            }
            WriteBattleTents(rom, data.BattleTents, info);

            // Hacks and tweaks

            // Write the pc potion item
            int pcPotionOffset = info.FindOffset(ElementNames.pcPotion, rom);
            if (pcPotionOffset != Rom.nullPointer)
            {
                if(settings.MysteryGiftItemAcquisitionSetting == Settings.MysteryGiftItemSetting.StartingItem)
                {
                    var pcItemArray = new Rom(new byte[(data.MysteryGiftEventItems.Count + 2) * 4], rom.FreeSpaceByte, 0);
                    pcItemArray.WriteUInt16((int)RemapItem(data.PcStartItem));
                    pcItemArray.WriteUInt16(1);
                    foreach(var item in data.MysteryGiftEventItems)
                    {
                        pcItemArray.WriteUInt16(ItemToInternalIndex(RemapItem(item.Item)));
                        pcItemArray.WriteUInt16(1);
                    }
                    pcItemArray.WriteRepeating(0x00, 4);
                    int? newPCItemArrayOffset = rom.WriteInFreeSpace(pcItemArray.File);
                    if(newPCItemArrayOffset.HasValue && newPCItemArrayOffset != Rom.nullPointer && info.HasPointer(ElementNames.pcPotion))
                    {
                        rom.WritePointer(info.Pointer(ElementNames.pcPotion), newPCItemArrayOffset.Value);
                    }
                    else
                    {
                        Logger.main.Error($"Expanded PC item table could not be written to free space. Mystery Gift Event Items will not start in the PC");
                        rom.WriteUInt16(pcPotionOffset, (int)RemapItem(data.PcStartItem));
                    }
                }
                else // Normal PC Potion Setting
                {
                    rom.WriteUInt16(pcPotionOffset, (int)RemapItem(data.PcStartItem));
                }
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
            // Apply sun weather fix for FRLG
            if (settings.WeatherSetting != Settings.WeatherOption.Unchanged && metadata.IsFireRedOrLeafGreen)
            {
                ApplySunHack(rom, info, metadata);
            }
            // Apply evolve without national dex hack if supported
            if (settings.EvolveWithoutNationalDex && metadata.IsFireRedOrLeafGreen)
            {
                int offset = info.FindOffset(ElementNames.GenIII.evolveWithoutNatDex, rom);
                if (offset != Rom.nullPointer)
                {
                    // Get the data from the attribute
                    var byteData = info.ByteArrayAttr(ElementNames.GenIII.evolveWithoutNatDex, "data");
                    rom.WriteBlock(offset, byteData);
                }
                offset = info.FindOffset(ElementNames.GenIII.stoneEvolveWithoutNatDex, rom);
                if (offset != Rom.nullPointer)
                {
                    // Get the data from the attribute
                    var byteData = info.ByteArrayAttr(ElementNames.GenIII.stoneEvolveWithoutNatDex, "data");
                    rom.WriteBlock(offset, byteData);
                }
            }
            if (settings.DeoxysMewObeyFix && (metadata.IsEmerald || metadata.IsFireRedOrLeafGreen))
            {
                ApplyDeoxysMewObeyFix(rom, info);
            }
            // Apply forecast hack if necessary
            if (data.GetBaseStats(Pokemon.CASTFORM).IsVariant)
            {
                ApplyDualTypeForecastHack(rom, info);
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
            var blankItemsWithEffects = new Queue<int>(Item.Sitrus_Berry - Item.Potion);
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
            int stoneEffectOffset = rom.ReadPointer(itemEffectTableOffset + ItemEffectTableOffset(Item.Moon_Stone));
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
                    rom.WritePointer(itemEffectTableOffset + ItemEffectTableOffset(newItemData.Item), stoneEffectOffset);
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

        private static int ItemEffectTableOffset(Item item) => (item - Item.Potion) * Rom.pointerSize;

        private void ApplyHailHack(Settings.HailHackOption option, Rom rom, XmlManager info)
        {
            if (!info.HasElement(ElementNames.GenIII.hailHack))
            {
                Logger.main.Unsupported(ElementNames.GenIII.hailHack);
                return;
            }
            // Hail Weather Hack. Makes the weather types "steady snow" and "three snowflakes" cause hail in battle
            // Hack routine for FR 1.0 and Emerald compiled from bluRose's ASM routine. Thanks blueRose (https://www.pokecommunity.com/member.php?u=471720)!\
            // Hack routine for other versions (R/S and FRLG 1.1 modified from the original routine by me)
            // Emerald offsets from Panda Face (https://www.pokecommunity.com/member.php?u=660920)
            // Three snow flake spawning issue fix from ShinyDragonHunter (https://www.pokecommunity.com/member.php?u=241758)
            // Thread with all relevant posts: https://www.pokecommunity.com/showthread.php?t=351387&page=2
            var hailRoutine = info.ByteArrayAttr(ElementNames.GenIII.hailHack, "routine");
            if (hailRoutine.Length == 0)
            {
                Logger.main.Unsupported(ElementNames.GenIII.hailHack + " (no routine found)");
                return;
            }
            int? hailHackroutineOffset = rom.WriteInFreeSpace(hailRoutine);
            if (hailHackroutineOffset != null)
            {
                var hailMessageBlock = new byte[] { 0xF3, 0x00 };
                if (option.HasFlag(Settings.HailHackOption.Snow))
                {
                    // Add battle weather routine
                    rom.WritePointer(info.HexAttr(ElementNames.GenIII.hailHack, "snowRoutineOffset"), (int)hailHackroutineOffset);
                    // Fix message
                    rom.WriteBlock(info.HexAttr(ElementNames.GenIII.hailHack, "snowMessageOffset"), hailMessageBlock);
                    // Fix Three snow flakes spawning issue
                    rom.WriteBlock(info.HexAttr(ElementNames.GenIII.hailHack, "snowFixOffset"), new byte[] { 0x4B, 0xE0 });
                    // Fix Three snow flakes post-battle x position issue
                    if (info.HasElementWithAttr(ElementNames.GenIII.hailHack, "snowPostBattleFixOffset") && info.HasElementWithAttr(ElementNames.GenIII.hailHack, "snowPostBattleAsm"))
                    {
                        rom.WriteBlock(info.HexAttr(ElementNames.GenIII.hailHack, "snowPostBattleFixOffset"), info.ByteArrayAttr(ElementNames.GenIII.hailHack, "snowPostBattleAsm"));
                    }
                }
                if (option.HasFlag(Settings.HailHackOption.FallingAsh))
                {
                    // Add battle weather routine
                    rom.WritePointer(info.HexAttr(ElementNames.GenIII.hailHack, "steadySnowRoutineOffset"), (int)hailHackroutineOffset);
                    // Fix message
                    rom.WriteBlock(info.HexAttr(ElementNames.GenIII.hailHack, "steadySnowMessageOffset"), hailMessageBlock);
                }
            }
            else
            {
                Logger.main.Error("Failed to write hail hack routine in free space. Hail hack will not be applied");
            }
        }

        private void ApplySunHack(Rom rom, XmlManager info, RomMetadata metadata)
        {
            // binary for sunfixFR1 compiled from MrPkmn's sun fix asm. Thanks MrPkmn!
            // post: (https://www.pokecommunity.com/showpost.php?p=8823257&postcount=5)
            // profile: (https://www.pokecommunity.com/member.php?u=86277)
            // sun fixes for other versions (LG 1.0 and FRLG 1.1 were compiled from asm written by me, modified from the original by MrPkmn)
            byte[] asm;
            if (metadata.IsFireRed)
            {
                asm = metadata.Version == 0 ? Resources.Patches.Patches.sunfixFR1 : Resources.Patches.Patches.sunfixFR1_1;
            }
            else if (metadata.IsLeafGreen)
            {
                asm = metadata.Version == 0 ? Resources.Patches.Patches.sunfixLG1 : Resources.Patches.Patches.sunfixLG1_1;
            }
            else
            {
                Logger.main.Error("Attempting Sun Fix on a non-FRLG rom. this should not happen");
                return;
            }
            int routineOffset = rom.WriteInFreeSpace(asm) ?? Rom.nullPointer;
            if(routineOffset != Rom.nullPointer)
            {
                rom.Seek(info.HexAttr(ElementNames.GenIII.sunHack, "routineOffset"));
                rom.WriteBlock(new byte[] { 0x01, 0x48, 0x00, 0x47, 0xC0, 0x46 });
                rom.WritePointer(routineOffset + 1);
                rom.Seek(info.HexAttr(ElementNames.GenIII.sunHack, "secondOffset"));
                rom.WriteBlock(new byte[] { 0x20, 0x22, 0x02, 0x70 });
            }
            else
            {
                Logger.main.Error("Failed to write sun fix routine in free space. Sunny overworld weather will cause black screens after battle");
            }
        }

        private void ApplyDeoxysMewObeyFix(Rom rom, XmlManager info)
        {
            int deoxysCheckOffset = info.FindOffset(ElementNames.GenIII.deoxysMewObeyFix, rom);
            if(deoxysCheckOffset == Rom.nullPointer)
            {
                Logger.main.Error("Attempting to apply deoxys/mew obey fix, but could not find the proper offset. Fix will not be applied");
                return;
            }
            rom.Seek(deoxysCheckOffset);
            // Use the glitch pokemon index (0x00) instead of deoxys
            rom.WriteByte(0x00);
            rom.WriteByte(Gen3Opcodes.setRegister | Gen3Opcodes.reg1);
            // 2nd repetition is esxsentially nop to clear the commands that use to be here
            rom.WriteByte(0x00);
            rom.WriteByte(Gen3Opcodes.setRegister | Gen3Opcodes.reg1);
            // Skip to the mew check
            rom.Seek(deoxysCheckOffset + info.HexAttr(ElementNames.GenIII.deoxysMewObeyFix, "mewOffset"));
            // Change mew check to glitch pokemon check
            int mewCheck = ((Gen3Opcodes.cmpRegister | Gen3Opcodes.reg0) << 8) | (PokemonToInternalIndex(Pokemon.MEW));
            if (rom.ReadUInt16(rom.InternalOffset) != mewCheck)
            {
                Logger.main.Error("Attempting to apply deoxys/mew obey fix, mew check was not found where it was expected. Only deoxys will be fixed");
                return;
            }
            int glitchPokemonCheck = ((Gen3Opcodes.cmpRegister | Gen3Opcodes.reg0) << 8) | (0x00);
            rom.WriteUInt16(glitchPokemonCheck);
        }

        private void ApplyDualTypeForecastHack(Rom rom, XmlManager info)
        {
            int forecastFormChangeOffset = info.FindOffset(ElementNames.forecastRoutine, rom);
            // See forecast form change routine
            if (forecastFormChangeOffset == Rom.nullPointer)
            {
                Logger.main.Unsupported(ElementNames.forecastRoutine);
                return;
            }
            // Prevent the initial type change from setting the secondary type
            rom.WriteByte(forecastFormChangeOffset + 104, 0x15);
            // Prevent the normal type change from setting the secondary type
            rom.WriteByte(forecastFormChangeOffset + 210, 0x1A);
            // Prevent the sun type change from setting the secondary type
            rom.WriteByte(forecastFormChangeOffset + 254, 0x10);
            // Prevent the rain type change from setting the secondary type
            rom.WriteByte(forecastFormChangeOffset + 298, 0x10);
            // Prevent the hail type change from setting the secondary type
            rom.WriteByte(forecastFormChangeOffset + 342, 0x10);

            // Set all secondary type checks to check for normal (essentially null them out)
            rom.WriteByte(forecastFormChangeOffset + 246, 0x00); // sun
            rom.WriteByte(forecastFormChangeOffset + 290, 0x00); // rain
            rom.WriteByte(forecastFormChangeOffset + 334, 0x00); // snow
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
        private void WritePokemonBaseStats(RomData romData, Rom rom, XmlManager info)
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
            // setup TmHmCompat offset
            int tmHmCompatOffset = info.FindOffset(ElementNames.tmHmCompat, rom);
            if (tmHmCompatOffset == Rom.nullPointer)
                return;
            int tmHmSize = info.Size(ElementNames.tmHmCompat);
            // Setup Move Tutor Compat offset
            int tutorCompatOffset = info.FindOffset(ElementNames.tutorCompat, rom);
            int tutorSize = 0;
            if (tutorCompatOffset != Rom.nullPointer)
            {
                tutorSize = info.Size(ElementNames.tutorCompat);
            }
            // Setup moveset offset
            int movesetTableOffset = info.FindOffset(ElementNames.movesets, rom);
            if (movesetTableOffset == Rom.nullPointer)
                return;
            // Setup palette offsets
            int pokemonPaletteSize = info.Size(ElementNames.pokemonPalettes);
            int normalPaletteOffset = info.FindOffset(ElementNames.pokemonPalettes, rom);
            int shinyPaletteOffset = info.FindOffset(ElementNames.pokemonPalettesShiny, rom);

            #endregion
            int skipNum = info.IntAttr(ElementNames.pokemonBaseStats, "skip");
            int skipAt = info.IntAttr(ElementNames.pokemonBaseStats, "skipAt");
            // Main writing loop
            for (int i = 1; i <= info.Num(ElementNames.pokemonBaseStats); i++)
            {
                if (i == skipAt) // potentially skip empty slots
                {
                    i += skipNum;
                }
                var stats = romData.GetBaseStats((Pokemon)i);
                WriteBaseStatsSingle(stats, pkmnOffset + i * pkmnSize, rom);
                int learnsetPointerOffset = movesetTableOffset + (i * Rom.pointerSize);
                if (stats.learnSet.Count == stats.learnSet.OriginalCount)
                {
                    WriteAttacks(rom, stats.learnSet, rom.ReadPointer(learnsetPointerOffset));
                }
                else // Relocate Moveset
                {
                    int? newLearnsetOffset = rom.FindFreeSpaceOffset((stats.learnSet.Count * 2) + 2);
                    if (newLearnsetOffset.HasValue)
                    {
                        rom.WritePointer(learnsetPointerOffset, newLearnsetOffset.Value);
                        WriteAttacks(rom, stats.learnSet, newLearnsetOffset.Value);
                    }
                    else
                    {
                        Logger.main.Error($"Failed to write learnset for {stats.Name} in free space. Learnset data for {stats.Name} will not be written");
                    }
                }
                WriteTMHMCompat(stats, tmHmCompatOffset + i * tmHmSize, rom);
                WriteTutorCompat(stats, tutorCompatOffset + i * tutorSize, rom);
                WriteEvolutions(stats, evolutionOffset + i * evolutionSize, rom);
                WritePokemonPalettes(stats, normalPaletteOffset + i * pokemonPaletteSize, shinyPaletteOffset + i * pokemonPaletteSize, rom);
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
        private void WriteAttacks(Rom rom, LearnSet moves, int offset)
        {
            rom.SaveAndSeekOffset(offset);
            foreach (var move in moves)
            {
                // Write the first byte of the move
                rom.WriteByte((byte)move.move);
                // if the move number is over 255, the last bit of the learn level byte is set to 1
                if ((int)move.move > 255)
                {
                    rom.WriteByte((byte)(move.learnLvl * 2 + 1));
                }
                else
                {
                    rom.WriteByte((byte)(move.learnLvl * 2));
                }
            }
            rom.WriteRepeating(0xff, 2); // Terminator (0xFFFF)
            rom.LoadOffset();
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
            if (pokemon.moveTutorCompat.Count <= 0)
                return;
            bool[] mtValues = new bool[pokemon.moveTutorCompat.Count];
            pokemon.moveTutorCompat.CopyTo(mtValues, 0);
            rom.WriteBits(offset, 1, mtValues.Select((b) => b ? 1 : 0).ToArray());
        }
        private void WriteEvolutions(PokemonBaseStats pokemon, int offset, Rom rom)
        {
            rom.Seek(offset);
            foreach (var evo in pokemon.evolvesTo)
            {
                if (evo.Type == EvolutionType.UseItem)
                {
                    var item = evo.ItemParamater;
                    if (failedItemRemaps.Contains(item))
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
                    if (evo.EvolvesWithItem)
                    {
                        rom.WriteUInt16(ItemToInternalIndex(evo.ItemParamater));
                    }
                    else // Don't need to consider Pokemon or Move arguments in Gen III
                    {
                        rom.WriteUInt16(evo.IntParameter);
                    }
                }
                rom.WriteUInt16((int)evo.Pokemon);
                rom.Skip(2);
            }
        }
        private void WritePokemonPalettes(PokemonBaseStats pokemon, int normalOffeset, int shinyOffset, Rom rom)
        {
            rom.SaveAndSeekOffset(normalOffeset);
            // Later do for any palette that would exeed the max size after compression
            if(pokemon.species == Pokemon.CASTFORM && pokemon.IsVariant)
            {
                int newCastformPalOffset = rom.FindFreeSpaceOffset(156) ?? Rom.nullPointer;
                if(newCastformPalOffset != Rom.nullPointer)
                {
                    paletteWriter.WriteCompressed(newCastformPalOffset, pokemon.palette, rom);
                    rom.WritePointer(newCastformPalOffset);
                }
                else
                {
                    Logger.main.Error("Failed to repoint castform palette (not enough free space), castform palette will not be written");
                    rom.Skip(4);
                }
            }
            else
            {
                paletteWriter.WriteCompressed(rom.ReadPointer(), pokemon.palette, rom);
            }
            rom.WriteUInt16(pokemon.paletteIndex);
            rom.Seek(shinyOffset);
            paletteWriter.WriteCompressed(rom.ReadPointer(), pokemon.shinyPalette, rom);
            rom.WriteUInt16(pokemon.shinyPaletteIndex);
            rom.LoadOffset();
        }
        private void WriteTypeDefinitions(TypeEffectivenessChart typeDefinitions, Rom rom, XmlManager info, ref RepointList repoints)
        {
            int typeDefinitionOffset = info.FindOffset(ElementNames.typeEffectiveness, rom);
            if (typeDefinitionOffset == Rom.nullPointer || typeDefinitions.Count <= 0)
            {
                return;
            }
            #region Convert TypeChart to byte[]
            var typeData = new List<byte>(typeDefinitions.Count * 3 + TypeEffectivenessChart.separatorSequence.Length + TypeEffectivenessChart.endSequence.Length);
            foreach (var (typePair, effectiveness) in typeDefinitions.TypeRelations)
            {
                typeData.Add((byte)typePair.attackingType);
                typeData.Add((byte)typePair.defendingType);
                typeData.Add((byte)effectiveness);
            }
            typeData.AddRange(TypeEffectivenessChart.separatorSequence);
            foreach (var (typePair, effectiveness) in typeDefinitions.IgnoreAfterForesight)
            {
                typeData.Add((byte)typePair.attackingType);
                typeData.Add((byte)typePair.defendingType);
                typeData.Add((byte)effectiveness);
            }
            typeData.AddRange(TypeEffectivenessChart.endSequence);
            #endregion

            #region Write to File
            //Move and Repoint if necessary
            if (typeDefinitions.Count > typeDefinitions.InitCount)
            {
                int? newOffset = rom.WriteInFreeSpace(typeData.ToArray());
                if (newOffset.HasValue)
                {
                    repoints.Add(typeDefinitionOffset, newOffset.Value);
                }
            }
            else
            {
                rom.WriteBlock(typeDefinitionOffset, typeData.ToArray());
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

        private void WriteTrainerSprites(RomData romData, Rom rom, XmlManager info)
        {
            // Write front palettes
            if (!info.FindAndSeekOffset(ElementNames.trainerPalettesFront, rom))
                return;
            foreach(var sprite in romData.TrainerSprites)
            {
                paletteWriter.WriteCompressed(rom.ReadPointer(), sprite.FrontPalette, rom);
                rom.Skip(4);
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
                // Check for trainer pokemon repoint, and try to apply it if necessary
                if (trainer.PokemonData.NeedsRepoint)
                {
                    int? newPokemonOffset = rom.FindFreeSpaceOffset(PokemonDataSize(trainer.PokemonData));
                    if (!newPokemonOffset.HasValue)
                    {
                        Logger.main.Error($"Trainer {trainer.Name} needs to have it's pokemon repointed, but there is not enough free space. This trainer will not have any data written");
                        return;
                    }
                    trainer.pokemonOffset = newPokemonOffset.Value;
                }

                // Write trainer data

                rom.WriteByte((byte)trainer.DataType);
                rom.WriteByte((byte)trainer.trainerClass);
                //// Write Gender (byte 2 bit 0)
                //gender = (Gender)((rom.Peek() & 0x80) >> 7);
                //// Write music track index (byte 2 bits 1-7)
                //musicIndex = (byte)(rom.WriteByte() & 0x7F);
                rom.Skip(1);
                // Write sprite index (byte 3)
                rom.WriteByte(trainer.spriteIndex);
                // Write name (I think bytes 4 - 15?)
                rom.WriteFixedLengthString(trainer.Name, Trainer.nameLength);
                // Write items (bytes 16-23)
                for (int i = 0; i < 4; ++i)
                    rom.WriteUInt16((int)RemapItem(trainer.useItems[i]));
                // Write double battle (byte 24)
                rom.WriteByte(Convert.ToByte(trainer.IsDoubleBattle));
                // What is in bytes 25-27?
                rom.Skip(3);
                // Write AI flags
                byte[] aiBytes = new byte[4];
                trainer.AIFlags.CopyTo(aiBytes, 0);
                rom.WriteBlock(aiBytes);
                // Write pokemon num
                rom.WriteByte((byte)trainer.Pokemon.Count);
                // What is in bytes 33-35?
                rom.Skip(3);
                // Bytes 36-39 (end of data)
                rom.WritePointer(trainer.pokemonOffset);

                // Write pokemon data

                rom.SaveOffset();
                rom.Seek(trainer.pokemonOffset);
                foreach (var pokemon in trainer.Pokemon)
                {
                    rom.WriteUInt16(pokemon.IVLevel);
                    rom.WriteUInt16(pokemon.level);
                    rom.WriteUInt16((int)pokemon.species);
                    if (trainer.DataType == TrainerPokemon.DataType.Basic)
                        rom.Skip(2); // Skip padding
                    else if (trainer.DataType == TrainerPokemon.DataType.HeldItem)
                        rom.WriteUInt16((int)RemapItem(pokemon.heldItem));
                    else if (trainer.DataType == TrainerPokemon.DataType.SpecialMoves)
                    {
                        foreach (var move in pokemon.moves)
                            rom.WriteUInt16((int)move);
                        rom.Skip(2);
                    }
                    else if (trainer.DataType == TrainerPokemon.DataType.SpecialMovesAndHeldItem)
                    {
                        rom.WriteUInt16((int)RemapItem(pokemon.heldItem));
                        foreach (var move in pokemon.moves)
                            rom.WriteUInt16((int)move);
                    }
                }
                rom.LoadOffset();
            }
        }

        private void WriteStevenAllyTrainerBattle(RomData data, Rom rom, XmlManager info)
        {
            if (!info.FindAndSeekOffset(ElementNames.GenIII.stevenAllyBattle, rom))
            {
                return;
            }
            if (!data.SpecialTrainers.ContainsKey(StevenAllyTrainer.specialTrainerKey))
            {
                return;
            }
            var battle = data.SpecialTrainers[StevenAllyTrainer.specialTrainerKey][0];
            foreach(var pokemon in battle.Pokemon)
            {
                if(pokemon is not StevenAllyTrainerPokemon stevenPokemon)
                {
                    Logger.main.Error("Steven Ally Battle with non Steven Trainer Pokemon detected. Steven Ally Battle will not be written properly");
                    return;
                }
                rom.WriteUInt16(PokemonToInternalIndex(stevenPokemon.species));
                rom.WriteByte((byte)stevenPokemon.IVLevel);
                rom.WriteByte((byte)stevenPokemon.level);
                rom.WriteByte((byte)stevenPokemon.Nature);
                rom.WriteBlock(stevenPokemon.EVs);
                rom.Skip();
                for (int moveInd = 0; moveInd < TrainerPokemon.numMoves; ++moveInd)
                {
                    rom.WriteUInt16(MoveToInternalIndex(pokemon.moves[moveInd]));
                }
            }
        }

        private int PokemonDataSize(Trainer.TrainerPokemonData data)
        {
            int perPokemon = data.DataType switch
            {
                TrainerPokemon.DataType.Basic or TrainerPokemon.DataType.HeldItem => 8,
                TrainerPokemon.DataType.SpecialMoves or TrainerPokemon.DataType.SpecialMovesAndHeldItem => 16,
                _ => throw new NotImplementedException("Unknown trainer pokemon type! Unable to get size"),
            };
            return perPokemon * data.Pokemon.Count;
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
                if (item.Description.Length == item.OriginalDescription.Length)
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

        private void WriteSetBerryTreeScript(Script script, Rom rom, XmlManager info, RomMetadata metadata)
        {
            if (script == null)
                return;
            int offset = info.FindOffset(ElementNames.setBerryTreeScript, rom);
            if (offset == Rom.nullPointer)
                return;
            scriptWriter.Write(script, rom, offset, metadata);
        }

        // Battle frontier and minigames
        private void WriteBattleTents(Rom rom, List<BattleTent> battleTents, XmlManager info)
        {
            foreach (var battleTent in battleTents)
            {
                WriteBattleTentPokemon(rom, battleTent, info);
                WriteBattleTentRewards(rom, battleTent, info);
            }
        }

        private void WriteBattleTentPokemon(Rom rom, BattleTent battleTent, XmlManager info)
        {
            var pokemonElement = info.Attr(battleTent.Name, "pokemonElement");
            if (!info.FindAndSeekOffset(pokemonElement, rom))
                return;
            int num = info.Num(pokemonElement);
            for (int i = 0; i < num; ++i)
            {
                WriteFrontierTrainerPokemon(rom, battleTent.Pokemon[i]);
            }
        }

        private void WriteBattleTentRewards(Rom rom, BattleTent battleTent, XmlManager info)
        {
            int rewardsPointerOffset = info.Pointer(info.Attr(battleTent.Name, "rewardsElement"));
            if (battleTent.Rewards.Count <= battleTent.OriginalRewardsCount)
            {
                // Write over original data
                rom.Seek(rom.ReadPointer(rewardsPointerOffset));
            }
            else // Needs expansion and repoint
            {
                int? newOffset = rom.FindFreeSpaceOffset((battleTent.Rewards.Count + 1) * 2, true);
                if (!newOffset.HasValue)
                {
                    Logger.main.Error("Unable to write expanded battle tent rewards in free space. Battle tent rewards will not be written");
                    return;
                }
                // Repoint and seek new offset
                rom.WritePointer(rewardsPointerOffset, newOffset.Value);
                rom.Seek(newOffset.Value);
            }
            // Actually write items
            foreach (var item in battleTent.Rewards)
            {
                rom.WriteUInt16(ItemToInternalIndex(RemapItem(item)));
            }
            rom.WriteRepeating(0x00, 2); // Array terminator
        }

        private void WriteFrontierTrainerPokemon(Rom rom, FrontierTrainerPokemon pokemon)
        {
            rom.WriteUInt16(PokemonToInternalIndex(pokemon.species));
            for (int i = 0; i < TrainerPokemon.numMoves; ++i)
            {
                rom.WriteUInt16(MoveToInternalIndex(pokemon.moves[i]));
            }
            rom.WriteByte((byte)pokemon.HeldItemIndex);
            rom.WriteByte((byte)pokemon.Evs);
            rom.WriteUInt32((int)pokemon.Nature);
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
