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

        public static bool TryGetBLZHeaderData(Rom rom, int offset, int length, out int headerLength, out int incLength, out int compressedLength, out int uncompressedLength, out int outputLength)
        {
            if (length < minBLZHeaderLength)
            {
                incLength = 0;
                headerLength = 0;
                compressedLength = 0;
                uncompressedLength = 0;
                outputLength = 0;
                return false;
            }
            // The difference between the length of the compressed portion of the data
            // And its length before compression
            incLength = rom.ReadUInt32(offset + length - 4);
            headerLength = rom.ReadByte(offset + length - 5);
            if (headerLength > maxBLZHeaderLength || headerLength < minBLZHeaderLength || length < headerLength)
            {
                compressedLength = 0;
                uncompressedLength = 0;
                outputLength = 0;
                return false;
            }
            // The length of the compressed portion of the data
            compressedLength = rom.ReadUInt24(offset + length - 8);
            // The length of the uncompressed portion of the data
            uncompressedLength = length - compressedLength;
            // The length of the output data (the already uncompressedLength + the currently compressed length + the compression gain
            outputLength = uncompressedLength + compressedLength + incLength;
            if (outputLength > maxBLZOutputLength)
            {
                return false;
            }
            return true;
        }

        public static byte[] Decompress(Rom rom, int offset, int length)
        {
            if (!TryGetBLZHeaderData(rom, offset, length, out int headerLength, out int inclength, out int compressedLenght, out int uncompressedLength, out int outputLength))
            {
                Logger.main.Error($"Error attempting to decompress BLZ data at {offset}: unable to parse header");
                return Array.Empty<byte>();
            }

            // Create output
            byte[] output = new byte[outputLength];
            // Copy uncompressed data to output file
            Array.Copy(rom.File, offset, output, 0, uncompressedLength);
            // Prepare input data
            byte[] input = new byte[length - headerLength];
            Array.Copy(rom.File, offset, input, 0, input.Length);
            Array.Reverse(input, uncompressedLength, input.Length - uncompressedLength);


            // Iterate through input data
            uint mask = 0;
            int flags = 0;
            for (int outputIndex = uncompressedLength, inputIndex = uncompressedLength; outputIndex < output.Length && inputIndex < input.Length;)
            {
                if ((mask >>= BLZShift) == 0)
                {
                    if (inputIndex + 1 >= input.Length)
                        break;
                    flags = input[inputIndex++];
                    mask = BLZMask;
                }
                if ((flags & mask) == 0)
                {
                    if (inputIndex + 1 >= input.Length)
                    {
                        break;
                    }
                    output[outputIndex++] = input[inputIndex++];
                }
                else if (inputIndex + 1 >= input.Length)
                {
                    break;
                }
                else
                {
                    byte byte1 = input[inputIndex++];
                    byte byte2 = input[inputIndex++];
                    // Length is the 4 Most significant bits of byte1
                    int len = (byte1 >> 4) + BLZThreshold + 1;
                    // Offset is the 4 Least significant bits of byte1 and byte2
                    int posOffset = ((byte1 << 8 | byte2) & 0x0FFF) + 3;
                    if (outputIndex + len > output.Length)
                    {
                        Logger.main.Warning($"BLZ Decompression warning: incorrect decoded length. Expected {output.Length}, got {outputIndex + len}");
                        len = Math.Max(0, output.Length - outputIndex);
                    }
                    while (len-- > 0)
                    {
                        output[outputIndex] = output[outputIndex - posOffset];
                        ++outputIndex;
                    }
                }
            }
            Array.Reverse(output, uncompressedLength, output.Length - uncompressedLength);
            return output;
        }
    }
}
