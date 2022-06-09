using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.DataStructures.DS
{
    public class NARCArchiveData
    {
        private const string fatbIdentifier = "FATB";
        private const string fimgIdentifier = "FIMG";
        private const string fntbIdentifier = "FNTB";
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
        }
    }
}
