using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer.Backend
{
    /// <summary>
    /// A class containing methods used to apply a .ips patch to a rom file
    /// Adapted from dabomstew's UP randomizer source https://github.com/Dabomstew/universal-pokemon-randomizer
    /// </summary>
    public class IpsPatcher
    {
        private const string signiture = "PATCH";
        private const int EOF = 0x454f46;
        private const int offsetBytes = 3; // Bytes in an offset (24 bits)
        private const int sizeBytes = 2; // Bytes in an offset (24 bits)

        public static void ApplyPatch(Rom rom, byte[] patch)
        {
            if (!IsValidPatch(patch))
                throw new Exception("Given patch does not have a valid signiture");

            int offset = 5; // Start after the signiture
            while (offset + 2 < patch.Length)
            {
                int writeOffset = patch.ReadUInt24(offset);
                if (writeOffset == EOF)
                    return; // Reached the end of the file (DONE)
                offset += offsetBytes;
                if (offset + 1 >= patch.Length)
                    throw new Exception("Entry wtih no size value");
                int size = patch.ReadUInt16(offset);
                offset += sizeBytes;
                if (size == 0) // Data is encoded with Run-Length-Encoding (RLE) (https://en.wikipedia.org/wiki/Run-length_encoding)
                {
                    if (offset + 1 >= patch.Length)
                        throw new Exception("No RLE size data in an RLE entry");
                    int rleSize = patch.ReadUInt16(offset);
                    if (writeOffset + rleSize > rom.Length)
                        throw new Exception("writeOffset is past the end of the ROM");
                    offset += sizeBytes;
                    if (offset >= patch.Length)
                        throw new Exception("abrupt ending to IPS file, entry cut off before RLE byte");
                    byte rleByte = patch[offset++];
                    for (int i = writeOffset; i < writeOffset + rleSize; i++)
                        rom.WriteByte(i, rleByte);
                }
                else // Data is a block of raw bytes
                {
                    if (offset + size > patch.Length)
                        throw new Exception("Block size too big");
                    if (writeOffset + size > rom.Length)
                        throw new Exception("writeOffset is past the end of the ROM");
                    rom.WriteBlock(writeOffset, patch, offset, size);
                    offset += size;
                }
            }
            throw new Exception("No EOF marker found in patch");
        }
        
        private static bool IsValidPatch(byte[] patch)
        {
            if (patch.Length < 8)
            {
                for (int i = 0; i < signiture.Length; ++i)
                    if (patch[i] != signiture[i])
                        return false;
                return true;
            }
            return false;
        }
    }
}
