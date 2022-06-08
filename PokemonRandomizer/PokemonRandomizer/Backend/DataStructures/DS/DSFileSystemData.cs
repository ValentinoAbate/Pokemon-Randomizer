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
    }
}
