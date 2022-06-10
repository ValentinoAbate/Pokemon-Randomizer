using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonRandomizer.Backend.DataStructures.DS
{
    // see https://www.romhacking.net/documents/[469]nds_formats.htm#NARC
    // Code adapted from NARCArchive.java from UPR
    public class NARCArchiveData
    {
        private const string fatbIdentifier = "FATB";
        private const string fimgIdentifier = "FIMG";
        private const string fntbIdentifier = "FNTB";
        public int FileCount => files.Count;
        private readonly List<string> fileNames;
        private readonly List<(int offset, int length)> files;
        public NARCArchiveData(Rom rom, int offset, int length)
        {
            int fatbFrameOffset = Rom.nullPointer;
            int fimgFrameOffset = Rom.nullPointer;
            int fntbFrameOffset = Rom.nullPointer;
            int fatbFrameSize = 0;
            int fimgFrameSize = 0;
            int fntbFrameSize = 0;

            rom.Seek(offset + 0x0E);
            // Read the number of frames
            int frameCount = rom.ReadUInt16();
            // Read frame data
            rom.Seek(offset + 0x10);
            for (int i = 0; i < frameCount; ++i)
            {
                var identifierBytes = rom.ReadBlock(4);
                Array.Reverse(identifierBytes);
                string frameIdentifier = Encoding.ASCII.GetString(identifierBytes);
                int frameLength = rom.ReadUInt32() - 8;
                if(frameIdentifier == fatbIdentifier)
                {
                    fatbFrameOffset = rom.InternalOffset;
                    fatbFrameSize = frameLength;
                }
                else if(frameIdentifier == fimgIdentifier)
                {
                    fimgFrameOffset = rom.InternalOffset;
                    fimgFrameSize = frameLength;
                }
                else if(frameIdentifier == fntbIdentifier)
                {
                    fntbFrameOffset = rom.InternalOffset;
                    fntbFrameSize = frameLength;
                }
                rom.Skip(frameLength);
            }
            if(fatbFrameOffset == Rom.nullPointer || fimgFrameOffset == Rom.nullPointer || fntbFrameOffset == Rom.nullPointer)
            {
                throw new ArgumentException($"No valid NARC file located at {offset:x2}. FATB, FIMG, or FNTB frame not found");
            }

            // Read file offsets and sizes from FATB fram. Actual contents are located in FIMG frame
            rom.Seek(fatbFrameOffset);
            int fileCount = rom.ReadUInt32();
            files = new List<(int offset, int length)>(fileCount);
            for (int i = 0; i < fileCount; i++)
            {
                int startOffset = rom.ReadUInt32();
                int endOffset = rom.ReadUInt32();
                files.Add((fimgFrameOffset + startOffset, endOffset - startOffset));
            }

            // Read the filenames from the FNTB frame, if they exist
            rom.Seek(fntbFrameOffset);
            int fileNameIndicator = rom.ReadUInt32();
            // Filenames 
            if (fileNameIndicator == 8)
            {
                fileNames = new List<string>(fileCount);
                rom.Skip(4);
                for (int i = 0; i < fileCount; i++)
                {
                    byte nameLength = rom.ReadByte();
                    fileNames.Add(Encoding.ASCII.GetString(rom.ReadBlock(nameLength)));
                }
            }
            else
            {
                fileNames = new List<string>();
            }
        }

        public bool GetFile(int fileIndex, out int offset, out int length, out string name)
        {
            name = fileIndex < fileNames.Count ? fileNames[fileIndex] : string.Empty;
            if(fileIndex >= files.Count)
            {
                offset = Rom.nullPointer;
                length = 0;
                return false;
            }
            var fileData = files[fileIndex];
            offset = fileData.offset;
            length = fileData.length;
            return true;
        }
    }
}
