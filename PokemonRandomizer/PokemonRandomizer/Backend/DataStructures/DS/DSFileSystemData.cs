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
        private readonly Dictionary<int, Arm9Overlay> arm9Data;

        public DSFileSystemData(int fileCapacity, int arm9Capacity)
        {
            fileData = new(fileCapacity);
            arm9Data = new(arm9Capacity);
        }

        public void AddFile(string fullFilename, int offset, int length)
        {
            if(fileData.ContainsKey(fullFilename))
                return;
            fileData.Add(fullFilename, (offset, length));
        }

        public void AddArm9Overlay(int fileID, Arm9Overlay entry)
        {
            if (arm9Data.ContainsKey(fileID))
                return;
            arm9Data.Add(fileID, entry);
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
            if (string.IsNullOrEmpty(fullFilename) || !fileData.ContainsKey(fullFilename))
            {
                offset = Rom.nullPointer;
                fileLength = 0;
                return false;
            }
            var data = fileData[fullFilename];
            offset = data.offset;
            fileLength = data.length;
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
    }
}
