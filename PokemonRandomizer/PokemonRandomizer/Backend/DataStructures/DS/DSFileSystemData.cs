using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.DataStructures.DS
{
    public class DSFileSystemData
    {
        private readonly Dictionary<string, (int offset, int length)> fileData;

        public DSFileSystemData(int capacity)
        {
            fileData = new(capacity);
        }

        public void AddFile(string fullFilename, int offset, int length)
        {
            if(fileData.ContainsKey(fullFilename))
                return;
            fileData.Add(fullFilename, (offset, length));
        }

        public bool SeekFile(string fullFilename, Rom rom, out int fileLength)
        {
            if (fileData.ContainsKey(fullFilename))
            {
                var data = fileData[fullFilename];
                rom.Seek(data.offset);
                fileLength = data.length;
                return true;
            }
            fileLength = 0;
            return false;
        }

        public bool GetFile(string fullFilename, out int offset, out int fileLength)
        {
            if (fileData.ContainsKey(fullFilename))
            {
                var data = fileData[fullFilename];
                offset = data.offset;
                fileLength = data.length;
                return true;
            }
            offset = Rom.nullPointer;
            fileLength = 0;
            return false;
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
    }
}
