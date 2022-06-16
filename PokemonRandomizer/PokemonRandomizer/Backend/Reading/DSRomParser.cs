using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.DataStructures.DS;
using PokemonRandomizer.Backend.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.Reading
{
    public abstract class DSRomParser : RomParser
    {
        private const int fntOffsetOffset = 0x40;
        private const char pathSeparator = '/';
        private const int directoryOffset = 0xF000;
        private const int fileHeaderSize = 8;
        private const int arm9HeaderOffset = 0x50;
        private const int arm9OverlayDataSize = 32;
        protected DSFileSystemData ParseNDSFileSystemData(Rom rom)
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
            rom.Seek(arm9HeaderOffset);
            int arm9OverlayTableOffset = rom.ReadUInt32();
            int arm9OverlayTablesize = rom.ReadUInt32();
            int arm9OverlayCount = arm9OverlayTablesize / arm9OverlayDataSize;

            var fileSystem = new DSFileSystemData(filenames.Count, arm9OverlayCount);

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
                fileSystem.AddFile(fullFilename, start, end - start);
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
                fileSystem.AddArm9Overlay(i, overlay.FileID, overlay);
            }

            return fileSystem;
        }
    }
}
