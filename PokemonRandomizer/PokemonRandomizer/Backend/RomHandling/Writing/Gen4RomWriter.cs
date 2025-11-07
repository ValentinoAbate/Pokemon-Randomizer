using PokemonRandomizer.Backend.Constants;
using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.DataStructures.DS;
using PokemonRandomizer.Backend.DataStructures.Trainers;
using PokemonRandomizer.Backend.RomHandling.IndexTranslators;
using PokemonRandomizer.Backend.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.RomHandling.Writing
{
    public class Gen4RomWriter : DSRomWriter
    {
        private const string doubleBattleFixFailureMessage = "Double battle settings were enabled but double battle fix failed. Please retry without battle type randomization";
        private const int headerSizeOffset = 0x84;
        private const int alignment = 0b0001_1111_1111;
        private const int applicationEndAlignment = 0b0011;
        private static int Align(int offset) => Align(offset, alignment);
        private static int Align(int offset, int alignment) => (offset + alignment) & ~alignment;
        protected override IIndexTranslator IndexTranslator => Gen4IndexTranslator.Main;

        public override Rom Write(RomData data, Rom originalRom, RomMetadata metadata, XmlManager info, Settings settings)
        {
            // Parse the original NDS file structure
            var dsFileSystem = new DSFileSystemData(originalRom);
            
            // Write override data to files (by file ID)
            var fileOverrides = new Dictionary<int, Rom>();

            WriteMoveTutorMoves(data, originalRom, dsFileSystem, info, fileOverrides);

            WriteMoves(data, originalRom, dsFileSystem, info, fileOverrides);
            WriteStarters(data, originalRom, dsFileSystem, metadata, info, fileOverrides);
            WriteTrainers(data, originalRom, dsFileSystem, metadata, info, fileOverrides);
            WriteTypeEffectivenessData(data, originalRom, dsFileSystem, info, fileOverrides);

            // Do actual writing to output rom
            var rom = new Rom(originalRom.Length, 0xFF); // Set to all 0xFF?

            // Get Header Size
            int headerSize = originalRom.ReadUInt32(headerSizeOffset);
            // Copy original header data
            rom.WriteBlock(0, originalRom.ReadBlock(0, headerSize));

            // Write Arm9 data at the first aligned location after the header
            WriteArm9(rom, Align(headerSize), dsFileSystem, data, originalRom, metadata, info, fileOverrides, settings, out int arm9OverlayTableOffset);

            // Write Arm7 data at the first aligned location after the arm9 overlay table (arm9 table will be written later)
            WriteArm7(rom, Align(arm9OverlayTableOffset + dsFileSystem.Arm9OverlayTableSize), originalRom, out int arm7EndOffset);

            // Copy banner to new rom at the 1st aligned location after arm7 data
            WriteBanner(rom, Align(arm7EndOffset), originalRom, out int bannerEndOffset);

            // Copy filename table (fnt) to new rom at the 1st aligned location after banner
            WriteFilenameTable(rom, Align(bannerEndOffset), originalRom, out int filenameTableEndOffset);

            // File allocation table offset
            int fatOffset = Align(filenameTableEndOffset);

            // Write file allocation table and file data
            WriteFiles(fileOverrides, rom, fatOffset, arm9OverlayTableOffset, dsFileSystem, originalRom, out int fileEndOffset);

            WriteApplicationEnd(rom, fileEndOffset, out int applicationEndOffset);

            WriteDeviceCapacity(rom, applicationEndOffset);

            // Write Header CRC Checksum
            rom.WriteUInt16(DSFileSystemData.crcOffset, (int)CRC16.Calculate(rom.ReadBlock(0, DSFileSystemData.crcOffset)));

            return rom;
        }

        private void WriteMoveTutorMoves(RomData data, Rom originalRom, DSFileSystemData dsFileSystem, XmlManager info, Dictionary<int, Rom> fileOverrides)
        {
            if(!TryGetOverrideOverlay(ElementNames.tutorMoves, originalRom, dsFileSystem, info, fileOverrides, out Rom overlay))
            {
                return;
            }
            int skip = Math.Max(0, info.Size(ElementNames.tutorMoves) - 2);
            foreach(var move in data.TutorMoves)
            {
                overlay.WriteUInt16(MoveToInternalIndex(move));
                overlay.Skip(skip);
            }
        }

        private void WriteMoves(RomData data, Rom originalRom, DSFileSystemData dsFileSystem, XmlManager info, Dictionary<int, Rom> fileOverrides)
        {
            if (!dsFileSystem.GetNarcFile(originalRom, info.Path(ElementNames.moveData), out var moveDataNarc))
            {
                return;
            }
            int numMoves = data.MoveData.Count;
            var moveFileOverrides = new List<Rom>(numMoves);
            byte[] flagsBuffer = new byte[1];
            for (int i = 0; i < numMoves; i++)
            {
                var move = data.MoveData[i];

                // Trainer Data
                var moveData = new Rom(16, 0x00);

                // Copy original data if available
                if (moveDataNarc.GetFile(i, out int originalMoveDataOffset, out int originalMoveDataLength, out _))
                {
                    moveData.Copy(originalRom, originalMoveDataOffset, originalMoveDataLength);
                    moveData.Seek(0);
                }

                moveData.WriteUInt16(MoveEffectToInternalIndex(move.effect));
                moveData.WriteByte((byte)move.MoveCategory);
                moveData.WriteByte(move.power);
                moveData.WriteByte((byte)move.type);
                moveData.WriteByte(move.accuracy);
                moveData.WriteByte(move.pp);
                moveData.WriteByte(move.effectChance);
                moveData.WriteUInt16(MoveTargetsToInternalIndex(move.targets));
                moveData.WriteByte(move.priority);
                // Write flags
                move.flags.CopyTo(flagsBuffer, 0);
                moveData.WriteByte(flagsBuffer[0]);
                moveData.Skip(4); // Skip contest data for now

                moveFileOverrides.Add(moveData);
            }
            fileOverrides.Add(moveDataNarc.FileId, moveDataNarc.WriteToFile(originalRom, moveFileOverrides));
        }

        private void WriteStarters(RomData data, Rom originalRom, DSFileSystemData dsFileSystem, RomMetadata metadata, XmlManager info, Dictionary<int, Rom> fileOverrides)
        {
            if (!info.HasElement(ElementNames.starterPokemon))
            {
                return;
            }
            if (metadata.IsHGSS)
            {
                //TODO
                return;
            }
            int overlayId = info.Overlay(ElementNames.starterPokemon);
            int offset = info.Offset(ElementNames.starterPokemon);
            var overlay = GetOverrideOverlay(overlayId, originalRom, dsFileSystem, fileOverrides);
            overlay.Seek(offset);
            foreach (var starter in data.Starters)
            {
                overlay.WriteUInt16(PokemonToInternalIndex(starter));
                overlay.Skip(2);
            }
            // TODO: Fix rival scripts
            // TODO: Fix starter picking screen
        }

        private void WriteTrainers(RomData data, Rom originalRom, DSFileSystemData dsFileSystem, RomMetadata metadata, XmlManager info, Dictionary<int, Rom> fileOverrides)
        {
            if (!dsFileSystem.GetNarcFile(originalRom, info.Path(ElementNames.trainerBattles), out var trainerNarc))
            {
                return;
            }
            if (!dsFileSystem.GetNarcFile(originalRom, info.Path(ElementNames.GenIV.trainerPokemon), out var trainerPokemonNarc))
            {
                return;
            }
            int numTrainers = data.Trainers.Count;
            var trainerFileOverrides = new List<Rom>(numTrainers);
            var trainerPokemonOverrides = new List<Rom>(numTrainers);
            bool isPlatinumOrHGSS = metadata.IsPlatinum || metadata.IsHGSS;
            for (int i = 0; i < numTrainers; i++)
            {
                var trainer = data.Trainers[i];

                // Trainer Data
                var trainerData = new Rom(20, 0x00);

                // Copy original data if available
                if (trainerNarc.GetFile(i, out int originalTrainerDataOffset, out int originalTrainerDataLength, out _))
                {
                    trainerData.Copy(originalRom, originalTrainerDataOffset, originalTrainerDataLength);
                    trainerData.Seek(0);
                }

                // Write custom data
                trainerData.WriteByte((byte)trainer.DataType);
                trainerData.WriteByte(trainer.trainerClass);
                trainerData.Skip();
                trainerData.WriteByte(trainer.Pokemon.Count);
                for (int itemInd = 0; itemInd < 4; ++itemInd)
                {
                    trainerData.WriteUInt16(ItemToInternalIndex(trainer.useItems[itemInd]));
                }
                byte[] aiBytes = new byte[4];
                trainer.AIFlags.CopyTo(aiBytes, 0);
                trainerData.WriteBlock(aiBytes);
                trainerData.WriteUInt32((byte)(trainer.IsDoubleBattle ? (trainer.OriginallyDoubleBattle ? 0x02 : 0x03) : 0x00));
                trainerFileOverrides.Add(trainerData);

                // Trainer Pokemon

                int pokemonSize = trainer.DataType switch 
                { 
                    TrainerPokemon.DataType.Basic => 6,
                    TrainerPokemon.DataType.HeldItem => 8,
                    TrainerPokemon.DataType.SpecialMoves => 14,
                    TrainerPokemon.DataType.SpecialMovesAndHeldItem => 16,
                    _ => throw new ArgumentException($"Improper pokemon data type detected: {trainer.ToString()} ({trainer.DataType.ToDisplayString()})")
                };
                if (isPlatinumOrHGSS)
                {
                    pokemonSize += 2;
                }

                var pokemonData = new Rom(pokemonSize * trainer.Pokemon.Count, 0x00);
                foreach(var pokemon in trainer.Pokemon)
                {
                    pokemonData.WriteByte(pokemon.IVLevel);
                    pokemonData.Skip(); // TODO: Gender/ability index on HGSS, 2nd byte of IVLevel for plat (see volkner's Electivire weirdness)
                    pokemonData.WriteUInt16(pokemon.level);
                    pokemonData.WriteUInt16(PokemonToInternalIndex(pokemon.species));
                    // TODO: form specifier
                    if (pokemon.dataType is TrainerPokemon.DataType.HeldItem or TrainerPokemon.DataType.SpecialMovesAndHeldItem)
                    {
                        pokemonData.WriteUInt16(ItemToInternalIndex(pokemon.heldItem));
                    }
                    if (pokemon.dataType is TrainerPokemon.DataType.SpecialMoves or TrainerPokemon.DataType.SpecialMovesAndHeldItem)
                    {
                        foreach(var move in pokemon.moves)
                        {
                            pokemonData.WriteUInt16(MoveToInternalIndex(move));
                        }
                    }
                    if (isPlatinumOrHGSS)
                    {
                        pokemonData.Skip(2); // Capsule seal info: rom.ReadUint16() (contains info on the ball seal - not present in DP)
                    }
                }
                trainerPokemonOverrides.Add(pokemonData);
            }
            fileOverrides.Add(trainerNarc.FileId, trainerNarc.WriteToFile(originalRom, trainerFileOverrides));
            fileOverrides.Add(trainerPokemonNarc.FileId, trainerPokemonNarc.WriteToFile(originalRom, trainerPokemonOverrides));
        }

        private void WriteTypeEffectivenessData(RomData data, Rom originalRom, DSFileSystemData dsFileSystem, XmlManager info, Dictionary<int, Rom> fileOverrides)
        {
            if (!info.HasElement(ElementNames.typeEffectiveness))
            {
                return;
            }
            int overlayId = info.Overlay(ElementNames.typeEffectiveness);
            int offset = info.Offset(ElementNames.typeEffectiveness);
            var overlay = GetOverrideOverlay(overlayId, originalRom, dsFileSystem, fileOverrides);
            var typeData = TypeEffectivenessChartToByteArray(data.TypeDefinitions);
            overlay.WriteBlock(offset, typeData);
        }

        private bool TryGetOverrideOverlay(string elementName, Rom originalRom, DSFileSystemData dsFileSystem, XmlManager info, Dictionary<int, Rom> fileOverrides, out Rom overlay)
        {
            if (!info.HasElement(elementName))
            {
                overlay = null;
                return false;
            }
            int overlayId = info.Overlay(elementName);
            overlay = GetOverrideOverlay(overlayId, originalRom, dsFileSystem, fileOverrides);
            return info.FindAndSeekOffset(elementName, overlay);
        }

        private Rom GetOverrideOverlay(int overlayId, Rom originalRom, DSFileSystemData dsFileSystem, Dictionary<int, Rom> fileOverrides)
        {
            if (!fileOverrides.TryGetValue(overlayId, out var overlay))
            {
                var originalOverlay = dsFileSystem.GetArm9OverlayData(originalRom, overlayId, out int overlayStart, out int overlaySize);
                overlay = new Rom(originalOverlay.ReadBlock(overlayStart, overlaySize));
                fileOverrides.Add(overlayId, overlay);
            }
            return overlay;
        }

        private void WriteArm9(Rom rom, int offset, DSFileSystemData dsFileSystem, RomData data, Rom originalRom, RomMetadata metadata, XmlManager info, Dictionary<int, Rom> fileOverrides, Settings settings, out int arm9EndOffset)
        {
            arm9EndOffset = offset;
            // Create new arm9 data
            var originalArm9Data = dsFileSystem.GetArm9Data(originalRom, out int arm9Start, out int originalArm9Size);
            var arm9Data = new Rom(originalArm9Data.ReadBlock(arm9Start, originalArm9Size));

            // Write RomData to new arm9 data
            WriteTmMoves(arm9Data, data, info);
            FixDoubleBattles(arm9Data, originalRom, dsFileSystem, info, fileOverrides, settings);

            // Write arm9 data
            int arm9Size;
            if (dsFileSystem.Arm9Compressed)
            {
                // TODO
                // compress arm9 data
                // write new compression header data if needed
                // Size is compressed size
                throw new System.NotImplementedException();
            }
            else
            {
                arm9Size = arm9Data.Length;
                rom.WriteBlock(offset, arm9Data.File);
                arm9EndOffset += arm9Size;
            }

            // Write arm9 footer (if necessary)
            if(dsFileSystem.Arm9Footer.Length > 0)
            {
                rom.WriteBlock(arm9EndOffset, dsFileSystem.Arm9Footer);
                arm9EndOffset += dsFileSystem.Arm9Footer.Length;
            }

            // Write new arm9 offset and size to header
            rom.WriteUInt32(DSFileSystemData.arm9OffsetOffset, offset);
            rom.WriteUInt32(DSFileSystemData.arm9SizeOffset, arm9Size);
        }

        private void WriteTmMoves(Rom arm9, RomData data, XmlManager info)
        {
            if (!info.FindAndSeekOffset(ElementNames.tmMoves, arm9))
            {
                return;
            }
            int numTms = info.Num(ElementNames.tmMoves);
            int numHms = info.Num(ElementNames.hmMoves);
            for (int i = 0; i < numTms; ++i)
            {
                arm9.WriteUInt16(MoveToInternalIndex(data.GetTmMove(i)));
            }
            for (int i = 0; i < numHms; ++i)
            {
                arm9.WriteUInt16(MoveToInternalIndex(data.GetHmMove(i)));
            }
        }

        // Code ported from universal pokemon randomizer ZX under the terms of the GPL-3
        // https://github.com/Ajarmar/universal-pokemon-randomizer-zx
        // Original code by Ajarmar and tom-overton!
        //
        // Explanation, modified from comment in original code:
        // In Gen 4, the game prioritizes showing the special double battle intro over almost any
        // other kind of intro. Since the trainer music is tied to the intro, this results in the
        // vast majority of "special" trainers losing their intro and music in double battle mode.
        // To fix this, the below code patches the executable to skip the case for the special
        // double battle intro (by changing a beq to an unconditional branch); this slightly breaks
        // battles that are double battles in the original game, but the trade-off is worth it.
        //
        // Then, also patch various subroutines that control the "Trainer Eye" event and text boxes
        // related to this in order to make double battles work on all trainers
        private void FixDoubleBattles(Rom arm9, Rom originalRom, DSFileSystemData dsFileSystem, XmlManager info, Dictionary<int, Rom> fileOverrides, Settings settings)
        {
            // Return if fix not needed (don't apply to avoid side-effects)
            if (!settings.RandomizeTrainerBattleType || settings.DoubleBattleChance <= 0)
                return;
            if (!info.FindAndSeekOffset(ElementNames.doubleBattleFix, arm9))
            {
                throw new NotSupportedException(doubleBattleFixFailureMessage);
            }
            arm9.WriteByte(0xE0);

            // After getting the double battle flag, return immediately instead of
            // converting it to a 1 for non-zero values/0 for zero
            arm9.Seek(arm9.FindFromPrefix(info.Attr(ElementNames.doubleBattleFix, "flagReturnPrefix")));
            arm9.WriteUInt16(0xBD08);

            // Instead of doing "double trainer walk" for nonzero values, do it only for
            // value == 2
            arm9.Seek(arm9.FindFromPrefix(info.Attr(ElementNames.doubleBattleFix, "walkingPrefix1")));
            arm9.WriteByte(0x2); // cmp r0, #0x2
            arm9.Skip(2);
            arm9.WriteByte(0xD0); // beq DOUBLE_TRAINER_WALK

            // Instead of checking if the value was exactly 1 after checking that it was
            // nonzero, check that it's 2 again lol
            arm9.Seek(arm9.FindFromPrefix(info.Attr(ElementNames.doubleBattleFix, "walkingPrefix2")));
            arm9.WriteByte(0x2);

            // Once again, compare a value to 2 instead of just checking that it's nonzero
            arm9.Seek(arm9.FindFromPrefix(info.Attr(ElementNames.doubleBattleFix, "textBoxPrefix")));
            arm9.WriteUInt16(0x46C0);
            arm9.WriteUInt16(0x2802);
            arm9.Skip();
            arm9.WriteByte(0xD0);

            // This NARC has some data that controls how text boxes are handled at the end of a trainer battle.
            if (!dsFileSystem.GetNarcFile(originalRom, info.Path(ElementNames.doubleBattleFix), out var battleSkillSubSeqNarc))
            {
                throw new NotSupportedException(doubleBattleFixFailureMessage);
            }
            int trainerEndFileId = info.IntAttr(ElementNames.doubleBattleFix, "trainerEndFileId");
            battleSkillSubSeqNarc.GetFile(trainerEndFileId, out int trainerEndFileOffset, out int trainerEndFileLength, out _);
            // Copy Original File
            var trainerEndOverrideFile = new Rom(originalRom.ReadBlock(trainerEndFileOffset, trainerEndFileLength));
            // Changing this byte from 4 -> 0 makes it check if the "double battle" flag is
            // exactly 2 instead of checking "flag & 2", which makes the single trainer double battles use the
            // single battle handling (since we set their flag to 3 instead of 2)
            trainerEndOverrideFile.Seek(info.HexAttr(ElementNames.doubleBattleFix, "trainerEndFileEndTextBoxOffset"));
            trainerEndOverrideFile.WriteByte(0x00);
            // Write file override
            fileOverrides.Add(battleSkillSubSeqNarc.FileId, battleSkillSubSeqNarc.WriteToFile(originalRom, trainerEndOverrideFile, trainerEndFileId));
        }

        // For now, this just exactly copies the Arm7 data and overlay data (will modify if Arm7 data needs to be modified)
        private void WriteArm7(Rom rom, int offset, Rom originalRom, out int arm7EndOffset)
        {
            // Arm7 data
            int originalArm7Offset = originalRom.ReadUInt32(DSFileSystemData.arm7OffsetOffset);
            int arm7Size = originalRom.ReadUInt32(DSFileSystemData.arm7SizeOffset);

            rom.WriteBlock(offset, originalRom.ReadBlock(originalArm7Offset, arm7Size));

            // Arm7 overlay data
            int originalArm7OverlayOffset = originalRom.ReadUInt32(DSFileSystemData.arm7OverlayOffsetOffset);
            int arm7OverlaySize = originalRom.ReadUInt32(DSFileSystemData.arm7SizeOffset);
            int arm7OverlayOffset = offset + arm7Size;

            rom.WriteBlock(arm7OverlayOffset, originalRom.ReadBlock(originalArm7OverlayOffset, arm7OverlaySize));
            
            arm7EndOffset = arm7OverlayOffset + arm7OverlaySize;

            // Write Arm7 and Arm7 overlay offset to header
            // No need to write sizes because we are not modifying data
            rom.WriteUInt32(DSFileSystemData.arm7OffsetOffset, offset);
            rom.WriteUInt32(DSFileSystemData.arm7OverlayOffsetOffset, arm7OverlayOffset);
        }

        private void WriteBanner(Rom rom, int offset, Rom originalRom, out int bannerEndOffset)
        {
            int originalBannerOffset = originalRom.ReadUInt32(DSFileSystemData.bannerOffsetOffset);
            rom.WriteBlock(offset, originalRom.ReadBlock(originalBannerOffset, DSFileSystemData.bannerSize));
            bannerEndOffset = offset + DSFileSystemData.bannerSize;

            // Write new banner offset to header
            rom.WriteUInt32(DSFileSystemData.bannerOffsetOffset, offset);
        }

        // Writes Filename table (fnt). Doesn't support adding new files for now (just copies from base from)
        private void WriteFilenameTable(Rom rom, int offset, Rom originalRom, out int filenameTableEndOffset)
        {
            int originalFntOffset = originalRom.ReadUInt32(DSFileSystemData.fntOffsetOffset);
            int fntSize = originalRom.ReadUInt32(DSFileSystemData.fntOffsetOffset);

            rom.WriteBlock(offset, originalRom.ReadBlock(originalFntOffset, fntSize));
            filenameTableEndOffset = offset + fntSize;

            rom.WriteUInt32(DSFileSystemData.fntOffsetOffset, offset);
        }

        // Write file allocation table (FAT) and files (including arm9 overlays)
        private void WriteFiles(Dictionary<int, Rom> fileOverrides, Rom rom, int fatOffset, int arm9OverlayTableOffset, DSFileSystemData dsFileSystem, Rom originalRom, out int fileDataEndOffset)
        {
            int dataOffset = fatOffset + dsFileSystem.FATSize;
            for (int i = 0; i < dsFileSystem.FileCount; ++i)
            {
                dataOffset = Align(dataOffset);
                int fileSize;
                if (dsFileSystem.TryGetArm9OverlayByFileID(i, out Arm9Overlay overlay)) 
                {  
                    byte[] data;
                    if(fileOverrides.TryGetValue(i, out var overrideData))
                    {
                        data = overrideData.File;
                        fileSize = data.Length;
                    }
                    else
                    {
                        data = dsFileSystem.GetOverlayContents(originalRom, overlay, out int fileStart, out fileSize).ReadBlock(fileStart, fileSize);
                    }
                    // TODO: Recompress data?
                    rom.WriteBlock(dataOffset, data);
                    // Write Header
                    rom.Seek(arm9OverlayTableOffset + (overlay.ID * DSFileSystemData.arm9OverlayHeaderSize));
                    rom.WriteUInt32(overlay.ID);
                    rom.WriteUInt32(overlay.RamAddress);
                    rom.WriteUInt32(fileSize);
                    rom.WriteUInt32(overlay.BssSize);
                    rom.WriteUInt32(overlay.StaticStart);
                    rom.WriteUInt32(overlay.StaticEnd);
                    rom.WriteUInt32(overlay.FileID);
                    rom.WriteUInt24(0); // Compressed size if compressed
                    rom.WriteByte(0); // Compression flag if compressed
                }
                else if(fileOverrides.TryGetValue(i, out var fileOverride))
                {
                    rom.WriteBlock(dataOffset, fileOverride.File);
                    fileSize = fileOverride.Length;
                }
                else if(dsFileSystem.GetFile(i, out int fileOffset, out fileSize))
                {
                    rom.WriteBlock(dataOffset, originalRom.ReadBlock(fileOffset, fileSize));
                }
                // Write FAT entry
                rom.Seek(fatOffset + (i * DSFileSystemData.fileHeaderSize));
                rom.WriteUInt32(dataOffset);
                rom.WriteUInt32(dataOffset + fileSize);
                dataOffset += fileSize;
            }
            fileDataEndOffset = dataOffset;

            // Write FAT offset to header (FAT size is not written because adding additional files is not supported)
            rom.WriteUInt32(DSFileSystemData.fatOffsetOffset, fatOffset);
            
            // Write arm9 overlay table offset to header
            rom.WriteUInt32(DSFileSystemData.arm9OverlayHeaderTableOffsetOffset, arm9OverlayTableOffset);
        }

        private void WriteApplicationEnd(Rom rom, int fileEndOffset, out int applicationEndOffset)
        {
            applicationEndOffset = Align(fileEndOffset, applicationEndAlignment);
            // Mark end of file if needed
            if(applicationEndOffset != fileEndOffset)
            {
                rom.WriteByte(applicationEndOffset - 1, 0x00);
            }
            rom.WriteUInt32(DSFileSystemData.applicationEndOffsetOffset, applicationEndOffset);
        }

        private void WriteDeviceCapacity(Rom rom, int romSize)
        {
            // Device capacity = 128kb * (2^x) where x is the byte at 0x14
            // x should be the smallest value such that device capacity >= romSize
            const int kb128 = 131072;
            for (byte x = 0; x <= byte.MaxValue; x++)
            {
                if((kb128 * Math.Pow(2, x)) >= romSize)
                {
                    rom.WriteByte(DSFileSystemData.deviceCapacityOffset, x);
                    return;
                }
            }
        }
    }
}
