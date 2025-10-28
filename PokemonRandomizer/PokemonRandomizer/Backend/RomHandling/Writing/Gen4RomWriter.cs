using PokemonRandomizer.Backend.Constants;
using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.DataStructures.DS;
using PokemonRandomizer.Backend.RomHandling.IndexTranslators;
using PokemonRandomizer.Backend.Utilities;
using System;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.RomHandling.Writing
{
    public class Gen4RomWriter : DSRomWriter
    {
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
            var rom = new Rom(originalRom.Length, 0xFF); // Set to all 0xFF?

            // Get Header Size
            int headerSize = originalRom.ReadUInt32(headerSizeOffset);
            // Copy original header data
            rom.WriteBlock(0, originalRom.ReadBlock(0, headerSize));

            // Write Arm9 data at the first aligned location after the header
            WriteArm9(rom, Align(headerSize), dsFileSystem, data, originalRom, metadata, info, settings, out int arm9OverlayTableOffset);

            // Write Arm7 data at the first aligned location after the arm9 overlay table (arm9 table will be written later)
            WriteArm7(rom, Align(arm9OverlayTableOffset + dsFileSystem.Arm9OverlayTableSize), originalRom, out int arm7EndOffset);

            // Copy banner to new rom at the 1st aligned location after arm7 data
            WriteBanner(rom, Align(arm7EndOffset), originalRom, out int bannerEndOffset);

            // Copy filename table (fnt) to new rom at the 1st aligned location after banner
            WriteFilenameTable(rom, Align(bannerEndOffset), originalRom, out int filenameTableEndOffset);

            // File allocation table offset
            int fatOffset = Align(filenameTableEndOffset);

            // Write file allocation table and file data
            WriteFiles(rom, fatOffset, arm9OverlayTableOffset, dsFileSystem, originalRom, out int fileEndOffset);

            WriteApplicationEnd(rom, fileEndOffset, out int applicationEndOffset);

            WriteDeviceCapacity(rom, applicationEndOffset);

            // Write Header CRC Checksum
            rom.WriteUInt16(DSFileSystemData.crcOffset, (int)CRC16.Calculate(rom.ReadBlock(0, DSFileSystemData.crcOffset)));

            return rom;
        }

        private void WriteArm9(Rom rom, int offset, DSFileSystemData dsFileSystem, RomData data, Rom originalRom, RomMetadata metadata, XmlManager info, Settings settings, out int arm9EndOffset)
        {
            arm9EndOffset = offset;
            // Create new arm9 data
            var originalArm9Data = dsFileSystem.GetArm9Data(originalRom, out int arm9Start, out int originalArm9Size);
            var arm9Data = new Rom(originalArm9Data.ReadBlock(arm9Start, originalArm9Size));

            // Write RomData to new arm9 data
            WriteTmMoves(arm9Data, data, info);

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
                rom.WriteBlock(dsFileSystem.Arm9Footer);
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
                arm9.WriteUInt16(MoveToInternalIndex(data.GetTmMove(i)));
            }
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
        private void WriteFiles(Rom rom, int fatOffset, int arm9OverlayTableOffset, DSFileSystemData dsFileSystem, Rom originalRom, out int fileDataEndOffset)
        {
            int dataOffset = fatOffset + dsFileSystem.FATSize;
            for (int i = 0; i < dsFileSystem.FileCount; ++i)
            {
                dataOffset = Align(dataOffset);
                int fileSize;
                if (dsFileSystem.TryGetArm9OverlayByFileID(i, out Arm9Overlay overlay)) 
                {
                    // TODO: Custom Data
                    // TODO: Recompress data?
                    var data = dsFileSystem.GetOverlayContents(originalRom, overlay, out int fileStart, out fileSize).ReadBlock(fileStart, fileSize);
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
                else if(dsFileSystem.GetFile(i, out int fileOffset, out fileSize))
                {
                    // TODO: custom data handling
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
