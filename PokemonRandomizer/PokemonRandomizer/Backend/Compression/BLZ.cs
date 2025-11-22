using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.Utilities.Debug;
using System;

namespace PokemonRandomizer.Backend.Compression
{
    // All BLZ (Bottom LZ) Compression and Decompression methods are
    // Based on BLZCoder.java from Dabomstew's Universal Pokemon Randomizer
    // BLZEncoder.java is based on blz.c - Bottom LZ coding for Nintendo GBA/DS (Copyright (C) 2011 CUE)
    // Modified by Valentino Abate under the terms of the GPL
    public static class BLZ
    {
        private const int minBLZHeaderLength = 0x8;
        private const int maxBLZHeaderLength = 0xB;
        private const int maxBLZOutputLength = 0xFFFFFF;
        private const int BLZShift = 1;
        private const int BLZMask = 0x80;
        private const int BLZThreshold = 2;

        public static bool TryGetBLZHeaderData(Rom rom, int offset, int length, out int headerLength, out int compressionGain, out int compressedLength, out int uncompressedLength, out int outputLength)
        {
            if (length < minBLZHeaderLength)
            {
                compressionGain = 0;
                headerLength = 0;
                compressedLength = 0;
                uncompressedLength = 0;
                outputLength = 0;
                return false;
            }
            int headerEndOffset = offset + length;
            // The difference between the length of the compressed portion of the data
            // And its length before compression
            compressionGain = rom.ReadUInt32(headerEndOffset - 4);
            headerLength = rom.ReadByte(headerEndOffset - 5);
            if (headerLength > maxBLZHeaderLength || headerLength < minBLZHeaderLength || length < headerLength)
            {
                compressedLength = 0;
                uncompressedLength = 0;
                outputLength = 0;
                return false;
            }
            // The length of the compressed portion of the data
            compressedLength = rom.ReadUInt24(headerEndOffset - 8);
            // The length of the uncompressed portion of the data
            uncompressedLength = length - compressedLength;
            // The length of the output data (the already uncompressed length + the currently compressed length + the compression gain
            outputLength = uncompressedLength + compressedLength + compressionGain;
            if (outputLength > maxBLZOutputLength)
            {
                return false;
            }
            return true;
        }

        public static byte[] Decompress(Rom rom, int offset, int length)
        {
            if (!TryGetBLZHeaderData(rom, offset, length, out int headerLength, out _, out _, out int uncompressedLength, out int outputLength))
            {
                Logger.main.Error($"Error attempting to decompress BLZ data at {offset}: unable to parse header");
                return Array.Empty<byte>();
            }

            // Create output
            byte[] output = new byte[outputLength];
            // Copy uncompressed data to output file
            Array.Copy(rom.File, offset, output, 0, uncompressedLength);
            // Prepare input data
            byte[] compressed = new byte[length - (headerLength + uncompressedLength)];
            Array.Copy(rom.File, offset + uncompressedLength, compressed, 0, compressed.Length);
            Array.Reverse(compressed);

            // Process compressed data
            uint mask = 0;
            int flags = 0;
            for (int outputInd = uncompressedLength, compressedInd = 0; outputInd < output.Length && compressedInd < compressed.Length;)
            {
                if ((mask >>= BLZShift) == 0)
                {
                    if (compressedInd + 1 >= compressed.Length)
                        break;
                    flags = compressed[compressedInd++];
                    mask = BLZMask;
                }
                if (compressedInd + 1 >= compressed.Length)
                {
                    break;
                }
                // If the flag is 0, read one byte from the input
                if ((flags & mask) == 0)
                {
                    output[outputInd++] = compressed[compressedInd++];
                    continue;
                }
                // Flag is 1, indicating a compressed run in the input
                // A compressed run is a repeating sequence of bytes
                // A compressed run is encoded as follows:
                // 4 Most significant bits of byte1 is the modified length of the sequence (in bytes)
                // To get the actual length of the sequence, add 3 (the minimum compressable sequence length)
                // The 4 least significant bits of byte1 combined with byte2 is the modified offset of the sequence to repeat
                // To get the actual offset of the sequence, add 3 (the minimum compressable sequence length)
                // The +3 additions are done to maximize the value of the bits
                // Look back offset bytes in the output to find the sequence and then write it to the output
                byte byte1 = compressed[compressedInd++];
                byte byte2 = compressed[compressedInd++];
                // Length is the 4 Most significant bits of byte1
                int len = (byte1 >> 4) + BLZThreshold + 1;
                // Offset is the 4 Least significant bits of byte1 and byte2
                int posOffset = ((byte1 << 8 | byte2) & 0x0FFF) + 3;
                if (outputInd + len > output.Length)
                {
                    Logger.main.Warning($"BLZ Decompression warning: incorrect decoded length. Expected {output.Length}, got {outputInd + len}");
                    len = Math.Max(0, output.Length - outputInd);
                }
                while (len-- > 0)
                {
                    output[outputInd] = output[outputInd - posOffset];
                    ++outputInd;
                }
            }
            Array.Reverse(output, uncompressedLength, output.Length - uncompressedLength);
            return output;
        }
    }
}
