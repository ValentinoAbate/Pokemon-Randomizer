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
        private readonly Dictionary<int, Arm9Overlay> arm9OverlaysByFileID;
        private readonly Arm9Overlay[] arm9Overlays;

        public DSFileSystemData(int fileCapacity, int arm9Capacity)
        {
            fileData = new(fileCapacity);
            arm9OverlaysByFileID = new(arm9Capacity);
            arm9Overlays = new Arm9Overlay[arm9Capacity];
        }

        public void AddFile(string fullFilename, int offset, int length)
        {
            if(fileData.ContainsKey(fullFilename))
                return;
            fileData.Add(fullFilename, (offset, length));
        }

        public void AddArm9Overlay(int index, int fileID, Arm9Overlay entry)
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

        public Rom GetArm9OverlayData(Rom rom, int overlayIndex, out int startOffset)
        {
            if(overlayIndex >= 0 && overlayIndex < arm9Overlays.Length)
            {
                return GetOverlayContents(rom, arm9Overlays[overlayIndex], out startOffset);
            }
            startOffset = 0;
            return new Rom(Array.Empty<byte>(), 0x00, 0x00);
        }

        public Rom GetArm9OverlayDataByFileID(Rom rom, int fileID, out int startOffset)
        {
            if (arm9OverlaysByFileID.ContainsKey(fileID))
            {
                return GetOverlayContents(rom, arm9OverlaysByFileID[fileID], out startOffset);
            }
            startOffset = 0;
            return new Rom(Array.Empty<byte>(), 0x00, 0x00);
        }

        private Rom GetOverlayContents(Rom rom, Arm9Overlay overlay, out int startOffset)
        {
            if (overlay.CompressionFlag <= 0)
            {
                startOffset = overlay.Start;
                return rom;
            }
            startOffset = 0;
            if (overlay.DecompressedData != null)
            {
                overlay.DecompressedData.Seek(0);
                return overlay.DecompressedData;
            }
            return overlay.DecompressedData = new Rom(rom.ReadBLZCompressedData(overlay.Start, overlay.Length), 0x00, 0x00);
        }
    }
}
