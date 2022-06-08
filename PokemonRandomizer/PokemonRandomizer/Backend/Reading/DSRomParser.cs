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
        protected DSFileSystemData ParseNDSFile(Rom rom, RomMetadata metadata, XmlManager info)
        {
            rom.Seek(fntOffsetOffset);
            int fntOffset = rom.ReadUInt32();
            int fntSize = rom.ReadUInt32();
            int fatOffset = rom.ReadUInt32();
            int fatSize = rom.ReadUInt32();

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

            var fileSystem = new DSFileSystemData(filenames.Count);

            // parse files
            foreach (int fileID in filenames.Keys)
            {
                string filename = filenames[fileID];
                int directory = fileDirectories[fileID];
                string dirPath = directoryPaths[directory + directoryOffset];
                string fullFilename = filename;
                if (!string.IsNullOrEmpty(dirPath))
                {
                    fullFilename = dirPath + pathSeparator + filename;
                }
                rom.Seek(fatOffset + (fileID * 8));
                int start = rom.ReadUInt32();
                int end = rom.ReadUInt32();
                fileSystem.AddFile(fullFilename, start, end - start);
            }

            return fileSystem;
        }
    }
}
