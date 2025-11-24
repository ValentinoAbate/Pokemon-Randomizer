using PokemonRandomizer.Backend.Utilities;
using PokemonRandomizer.Backend.Utilities.Debug;
using System;

namespace PokemonRandomizer.Backend.Compression
{
    // All BLZ (Bottom LZ) Compression and Decompression methods are
    // Based on BLZCoder.java from Dabomstew's Universal Pokemon Randomizer
    // BLZEncoder.java is based on blz.c - Bottom LZ coding for Nintendo GBA/DS (Copyright (C) 2011 CUE)
    // Modified by Valentino Abate under the terms of the GPL

    // BLZ Compression Structure
    // Uncompressed Data Location: 0 -> uncompressedLength: Uncompressed data (raw)
    // 
    // Compressed Data Location: uncompressedLength -> (EOF - HeaderLength): Compressed Data (reversed)
    // Compression format:
    // BLZ compressed data is series of blocks of data where is block encodes bytes or sequences with flags
    // The first byte of the block is the flags (8 bitflags)
    // If the flag is 0, 1 byte of data is present in that position of the block
    // If the flag is 1, a run is encoded in that position of the block, with the run length and run offset encoded in 2 bytes
    // See Decompress function for more info
    //
    // Header location: uncompressedLength + CompressedLength -> EOF: Header
    // Header Format:
    // Last 4 bytes: Compression gain (uint32)
    // 5th from last byte: Header length (should be anywhere from 8 to 11 bytes)
    // 8th from last byte to 6th from last byte: Compressed Length (uint24)
    // The first 3 bytes are padding (0xFF) if needed to make sure the compressed length is 4-aligned
    public static class BLZ
    {
        private const int minBLZHeaderSize = 0x8;
        private const int maxBLZHeaderSize = 0xB;
        private const int maxBLZOutputLength = 0xFFFFFF;
        private const int BLZShift = 1;
        private const int BLZMask = 0x80;
        private const int BLZThreshold = 2;
        private const int minRunLength = 3;
        private const int maxRunOffset = 0x1002; // max value of a 12 bit number + 3
        private const int maxRunLength = 0x12; // max value of a 4 bit number + 3

        public static bool TryGetBLZHeaderData(byte[] data, int offset, int length, out int headerLength, out int compressionGain, out int compressedLength, out int uncompressedLength, out int outputLength)
        {
            if (length < minBLZHeaderSize)
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
            compressionGain = data.ReadUInt32(headerEndOffset - 4);
            headerLength = data.ReadByte(headerEndOffset - 5);
            if (headerLength > maxBLZHeaderSize || headerLength < minBLZHeaderSize || length < headerLength)
            {
                compressedLength = 0;
                uncompressedLength = 0;
                outputLength = 0;
                return false;
            }
            // The length of the compressed portion of the data
            compressedLength = data.ReadUInt24(headerEndOffset - 8);
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

        public static byte[] Decompress(byte[] data, int offset, int length, out int uncompressedLength)
        {
            if (!TryGetBLZHeaderData(data, offset, length, out int headerLength, out _, out _, out uncompressedLength, out int outputLength))
            {
                Logger.main.Error($"Error attempting to decompress BLZ data at {offset}: unable to parse header");
                return Array.Empty<byte>();
            }

            // Create output
            byte[] output = new byte[outputLength];
            // Copy uncompressed data to output file
            Array.Copy(data, offset, output, 0, uncompressedLength);
            // Prepare input data
            byte[] compressed = new byte[length - (headerLength + uncompressedLength)];
            Array.Copy(data, offset + uncompressedLength, compressed, 0, compressed.Length);
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

        public static byte[] Compress(byte[] data, int leaveUncompressedSize, int offset = 0, int length = -1)
        {
            // If length is not defined, read until end of data
            if (length < 0)
            {
                length = data.Length - offset;
            }

            // Prepare buffer
            var buffer = new byte[length + ((length + 7) / 8) + 11];

            // Prepare input (the data to compress)
            var input = data.ReadBlock(offset + leaveUncompressedSize, length - leaveUncompressedSize);
            Array.Reverse(input);

            int bufferInd = 0;
            int inputIndex = 0;
            int flagsInd = 0;
            uint mask = 0;
            while (inputIndex < input.Length)
            {
                if ((mask >>= BLZShift) == 0)
                {
                    flagsInd = bufferInd++;
                    buffer[flagsInd] = 0; // no flags yet
                    mask = BLZMask;
                }
                else // Shift to next flag
                {
                    buffer[flagsInd] <<= 1;
                }
                if (TryFindSequence(input, inputIndex, out int runOffset, out int runLength))
                {
                    inputIndex += runLength;
                    buffer[flagsInd] |= 1; // Set sequence flag
                    int encodedLength = runLength - minRunLength;
                    int encodedOffset = runOffset - minRunLength;
                    buffer[bufferInd++] = (byte)((encodedLength << 4) | (encodedOffset >> 8)); // 4 bits of length, then 4 MSBs of offset
                    buffer[bufferInd++] = (byte)(encodedOffset & 0xFF); // 8 LSBs of offset
                }
                else
                {
                    buffer[bufferInd++] = input[inputIndex++]; // Write raw input
                }
            }
            // Move final flags to appropriate positions
            while (mask > 1)
            {
                mask >>= BLZShift;
                buffer[flagsInd] <<= 1;
            }

            int compressedSize = bufferInd;

            // Calculate header size
            int headerIndex = leaveUncompressedSize + compressedSize;
            // Header padding (if needed)
            int headerAlign = (4 - (headerIndex & 0x0003)) & 0x0003;
            int headerSize = minBLZHeaderSize + headerAlign;

            // Write final output
            var finalOutput = new byte[leaveUncompressedSize + compressedSize + headerSize];
            
            // Copy uncompressed data to final output
            Array.Copy(data, finalOutput, leaveUncompressedSize);
            
            // Reverse and then copy compressed data to final output
            Array.Reverse(buffer, 0, compressedSize);
            Array.Copy(buffer, 0, finalOutput, leaveUncompressedSize, compressedSize);
            
            // Write Header padding (if needed)
            for (int i = 0; i < headerAlign; ++i)
            {
                finalOutput[headerIndex++] = 0xFF;
            }
            // Write header
            finalOutput.WriteUInt24(headerIndex, compressedSize + headerSize);
            headerIndex += 3;
            finalOutput[headerIndex++] = (byte)headerSize;
            int compressionGain = length - (compressedSize + leaveUncompressedSize + headerSize);
            finalOutput.WriteUInt32(headerIndex, compressionGain);
            return finalOutput;
        }

        private static bool TryFindSequence(byte[] input, int inputIndex, out int runOffset, out int runLength)
        {
            runOffset = 0;
            runLength = BLZThreshold; // Ignore any runs of 2 or less bytes (they take up more space when compressed)
            // Maximum run offest is the maximum storable offset, or the inputIndex, whichever is smaller
            int maxOffset = Math.Min(maxRunOffset, inputIndex);
            for (int offset = 3; offset <= maxOffset; offset++)
            {
                // Search for sequences at this offset
                int length = 0;
                while(length < maxRunLength && length < offset)
                {
                    int sequenceIndex = inputIndex + length;
                    if (sequenceIndex >= input.Length)
                    {
                        break;
                    }
                    if (input[sequenceIndex] != input[sequenceIndex - offset])
                    {
                        break;
                    }
                    ++length;
                }
                // Prefer closer pos if same length
                if (length <= runLength)
                {
                    continue;
                }
                runOffset = offset;
                runLength = length;
                // Maximum compressable sequence length, always take
                if (runLength == maxRunLength)
                {
                    return true;
                }
            }
            return runLength > BLZThreshold;
        }
    }
}
