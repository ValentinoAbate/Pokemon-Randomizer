using PokemonRandomizer.Backend.Constants;
using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.DataStructures.DS;
using PokemonRandomizer.Backend.EnumTypes;
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
        private static int Align(int offset) => (offset + alignment) & ~alignment;
        protected override IIndexTranslator IndexTranslator => Gen4IndexTranslator.Main;

        public override Rom Write(RomData data, Rom originalRom, RomMetadata metadata, XmlManager info, Settings settings)
        {
            // Parse the original NDS file structure
            var dsFileSystem = new DSFileSystemData(originalRom);
            var rom = new Rom(originalRom.Length, 0xFF); // Set to all 0xFF?

            // Get Header Size
            int headerSize = originalRom.ReadUInt32(headerSizeOffset);
            // Copy original header data
            rom.Seek(0);
            rom.WriteBlock(originalRom.ReadBlock(0, headerSize));

            // Write Arm9 data at the first aligned location after the header
            WriteArm9(rom, Align(headerSize), dsFileSystem, data, originalRom, metadata, info, settings, out int arm9OverlayTableOffset);

            // Write Arm7 data at the first aligned location after the arm9 overlay table
            WriteArm7(rom, Align(arm9OverlayTableOffset + dsFileSystem.Arm9OverlayTableSize), originalRom, out int arm7EndOffset);

            return rom;
        }

        private void WriteArm9(Rom rom, int offset, DSFileSystemData dsFileSystem, RomData data, Rom originalRom, RomMetadata metadata, XmlManager info, Settings settings, out int arm9EndOffset)
        {
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
            }

            // Write arm9 footer (if necessary)
            if(dsFileSystem.Arm9Footer.Length > 0)
            {
                rom.WriteBlock(dsFileSystem.Arm9Footer);
            }

            // Record end offset
            arm9EndOffset = rom.InternalOffset;

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
            int arm7OverlayOffset = rom.InternalOffset;

            rom.WriteBlock(arm7OverlayOffset, originalRom.ReadBlock(originalArm7OverlayOffset, arm7OverlaySize));
            
            arm7EndOffset = rom.InternalOffset;

            // Write Arm7 and Arm7 overlay offset to header
            // No need to write sizes because we are not modifying data
            rom.WriteUInt32(DSFileSystemData.arm7OffsetOffset, offset);
            rom.WriteUInt32(DSFileSystemData.arm7OverlayOffsetOffset, arm7OverlayOffset);
        }
        }
    }
}
