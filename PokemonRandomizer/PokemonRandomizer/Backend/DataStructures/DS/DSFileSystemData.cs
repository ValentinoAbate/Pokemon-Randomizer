﻿using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonRandomizer.Backend.DataStructures.DS
{
    public class DSFileSystemData
    {
        private const int fntOffsetOffset = 0x40;
        private const char pathSeparator = '/';
        private const int directoryOffset = 0xF000;
        private const int fileHeaderSize = 8;
        private const int arm9OverlayHeaderOffset = 0x50;
        private const int arm9OverlayDataSize = 32;
        private const int arm9OffsetOffset = 0x20;
        private const int arm9SizeOffset = 0x2C;
        private const uint nitroFooterSigniture = 0xDEC00621;
        private const int nitroFooterSize = 12;

        private readonly Dictionary<string, (int offset, int length)> fileData;
        private readonly Dictionary<int, Arm9Overlay> arm9OverlaysByFileID;
        private readonly Arm9Overlay[] arm9Overlays;
        private readonly Rom decompressedArm9Data;
        private readonly int arm9Offset;
        private readonly int arm9Size;
        private readonly byte[] arm9Footer;

        public DSFileSystemData(Rom rom)
        {
            rom.Seek(fntOffsetOffset);
            int fntOffset = rom.ReadUInt32();
            rom.Skip(4); //int fntSize = rom.ReadUInt32();
            int fatOffset = rom.ReadUInt32();
            rom.Skip(4); //int fatSize = rom.ReadUInt32();

            // Read fnt table data
            rom.Seek(fntOffset + 0x6);
            int dircount = rom.ReadUInt16();
            rom.Seek(fntOffset);
            int[] subTableOffsets = new int[dircount];
            int[] firstFileIDs = new int[dircount];
            int[] parentDirIDs = new int[dircount];
            for (int i = 0; i < dircount; ++i)
            {
                subTableOffsets[i] = rom.ReadUInt32() + fntOffset;
                firstFileIDs[i] = rom.ReadUInt16();
                parentDirIDs[i] = rom.ReadUInt16();
            }

            // Get directory and file names
            var directoryNames = new string[dircount];
            var filenames = new Dictionary<int, string>();
            var fileDirectories = new Dictionary<int, int>();
            for (int i = 0; i < dircount; i++)
            {
                int firstFileID = firstFileIDs[i];
                // read subtable
                rom.Seek(subTableOffsets[i]);
                while (true)
                {
                    byte control = rom.ReadByte();
                    if (control == 0x00)
                    {
                        break;
                    }
                    int nameLength = control & 0x7F;
                    string name = Encoding.ASCII.GetString(rom.ReadBlock(nameLength));
                    if ((control & 0x80) > 0x00)
                    {
                        // sub-directory
                        int subDirectoryID = rom.ReadUInt16();
                        directoryNames[subDirectoryID - directoryOffset] = name;
                    }
                    else
                    {
                        int fileID = firstFileID++;
                        filenames.Add(fileID, name);
                        fileDirectories.Add(fileID, i);
                    }
                }
            }

            var directoryPaths = new Dictionary<int, string>();

            // Calculate full path names
            for (int i = 1; i < dircount; i++)
            {
                string dirname = directoryNames[i];
                if (dirname != null)
                {
                    string fullDirName = string.Empty;
                    int curDir = i;
                    while (!string.IsNullOrEmpty(dirname))
                    {
                        if (!string.IsNullOrEmpty(fullDirName))
                        {
                            fullDirName = pathSeparator + fullDirName;
                        }
                        fullDirName = dirname + fullDirName;
                        int parentDir = parentDirIDs[curDir];
                        if (parentDir >= 0xF001 && parentDir <= 0xFFFF)
                        {
                            curDir = parentDir - directoryOffset;
                            dirname = directoryNames[curDir];
                        }
                        else
                        {
                            break;
                        }
                    }
                    directoryPaths.Add(i + directoryOffset, fullDirName);
                }
                else
                {
                    directoryPaths.Add(i + directoryOffset, string.Empty);
                }
            }

            // arm9 overlays
            rom.Seek(arm9OverlayHeaderOffset);
            int arm9OverlayTableOffset = rom.ReadUInt32();
            int arm9OverlayTablesize = rom.ReadUInt32();
            int arm9OverlayCount = arm9OverlayTablesize / arm9OverlayDataSize;

            // Intialize data
            fileData = new(filenames.Count);
            arm9OverlaysByFileID = new(arm9OverlayCount);
            arm9Overlays = new Arm9Overlay[arm9OverlayCount];

            // parse files
            foreach (var kvp in filenames)
            {
                int fileID = kvp.Key;
                string filename = filenames[fileID];
                int directory = fileDirectories[fileID];
                string dirPath = directoryPaths[directory + directoryOffset];
                string fullFilename = filename;
                if (!string.IsNullOrEmpty(dirPath))
                {
                    fullFilename = dirPath + pathSeparator + filename;
                }
                rom.Seek(fatOffset + (fileID * fileHeaderSize));
                int start = rom.ReadUInt32();
                int end = rom.ReadUInt32();
                AddFile(fullFilename, start, end - start);
            }

            // parse overlays
            rom.Seek(arm9OverlayTableOffset);
            for (int i = 0; i < arm9OverlayCount; i++)
            {
                var overlay = new Arm9Overlay { ID = i };
                // Read entry data
                rom.Skip(4);
                overlay.RamAddress = rom.ReadUInt32();
                overlay.RamSize = rom.ReadUInt32();
                overlay.BssSize = rom.ReadUInt32();
                overlay.StaticStart = rom.ReadUInt32();
                overlay.StaticEnd = rom.ReadUInt32();
                overlay.FileID = rom.ReadUInt32();
                overlay.CompressedSize = rom.ReadUInt24();
                overlay.CompressionFlag = rom.ReadByte();
                // Read start and end
                rom.SaveAndSeekOffset(fatOffset + (overlay.FileID * fileHeaderSize));
                overlay.Start = rom.ReadUInt32();
                overlay.End = rom.ReadUInt32();
                rom.LoadOffset();
                AddArm9Overlay(i, overlay.FileID, overlay);
            }

            // Parse Arm9 Data
            arm9Offset = rom.ReadUInt32(arm9OffsetOffset);
            arm9Size = rom.ReadUInt32(arm9SizeOffset);
            // Footer
            rom.Seek(arm9Offset + arm9Size);
            uint nitroCode = rom.ReadUInt32Full();
            if(nitroCode == nitroFooterSigniture)
            {
                arm9Footer = rom.ReadBlock(arm9Offset + arm9Size, nitroFooterSize);
            }
            else
            {
                arm9Footer = Array.Empty<byte>();
            }
            // Read compressed data if compressed
            if(rom.TryGetBLZHeaderData(arm9Offset, arm9Size, out _, out int incLength, out _, out _, out _ ) && incLength > 0)
            {
                decompressedArm9Data = new Rom(rom.ReadBLZCompressedData(arm9Offset, arm9Size));
            }
        }

        private void AddFile(string fullFilename, int offset, int length)
        {
            if(fileData.ContainsKey(fullFilename))
                return;
            fileData.Add(fullFilename, (offset, length));
        }

        private void AddArm9Overlay(int index, int fileID, Arm9Overlay entry)
        {
            if (!arm9OverlaysByFileID.ContainsKey(fileID))
            {
                arm9OverlaysByFileID.Add(fileID, entry);
            }
            if(index >= 0 && index < arm9Overlays.Length)
            {
                arm9Overlays[index] = entry;
            }
        }

        // File Data

        public bool SeekFile(string fullFilename, Rom rom, out int fileLength)
        {
            if (fileData.ContainsKey(fullFilename))
            {
                var (offset, length) = fileData[fullFilename];
                rom.Seek(offset);
                fileLength = length;
                return true;
            }
            fileLength = 0;
            return false;
        }

        public bool GetFile(string fullFilename, out int offset, out int fileLength)
        {
            if (string.IsNullOrEmpty(fullFilename) || !fileData.ContainsKey(fullFilename))
            {
                offset = Rom.nullPointer;
                fileLength = 0;
                return false;
            }
            (offset, fileLength) = fileData[fullFilename];
            return true;

        }

        public bool GetNarcFile(Rom rom, string fullFilename, out NARCArchiveData narc)
        {
            if(!GetFile(fullFilename, out int offset, out int length))
            {
                narc = null;
                return false;
            }
            narc = new NARCArchiveData(rom, offset, length);
            return true;
        }

        // Arm9 Overlay Data

        public Rom GetArm9OverlayData(Rom rom, int overlayIndex, out int startOffset, out int length)
        {
            if(overlayIndex >= 0 && overlayIndex < arm9Overlays.Length)
            {
                return GetOverlayContents(rom, arm9Overlays[overlayIndex], out startOffset, out length);
            }
            startOffset = 0;
            length = 0;
            return new Rom(Array.Empty<byte>());
        }

        public Rom GetArm9OverlayDataByFileID(Rom rom, int fileID, out int startOffset, out int length)
        {
            if (arm9OverlaysByFileID.ContainsKey(fileID))
            {
                return GetOverlayContents(rom, arm9OverlaysByFileID[fileID], out startOffset, out length);
            }
            startOffset = 0;
            length = 0;
            return new Rom(Array.Empty<byte>());
        }

        private Rom GetOverlayContents(Rom rom, Arm9Overlay overlay, out int startOffset, out int length)
        {
            if (overlay.CompressionFlag <= 0)
            {
                startOffset = overlay.Start;
                length = overlay.Length;
                return rom;
            }
            startOffset = 0;
            if(overlay.DecompressedData == null)
            {
                overlay.DecompressedData = new Rom(rom.ReadBLZCompressedData(overlay.Start, overlay.Length));
            }
            length = overlay.DecompressedData.Length;
            overlay.DecompressedData.Seek(0);
            return overlay.DecompressedData;
        }

        // Arm 9 Data

        public Rom GetArm9Data(Rom rom, out int startOffset, out int length)
        {
            if(decompressedArm9Data != null)
            {
                startOffset = 0;
                length = decompressedArm9Data.Length;
                return decompressedArm9Data;
            }
            startOffset = arm9Offset;
            length = arm9Size;
            return rom;
        }
    }
}
