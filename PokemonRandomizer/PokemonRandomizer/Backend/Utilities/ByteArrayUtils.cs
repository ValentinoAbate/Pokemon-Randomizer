using System;

namespace PokemonRandomizer.Backend.Utilities
{
    /// <summary>A class of byte[] extension methods to help process rom files</summary>
    public static class ByteArrayUtils
    {
        /// <summary>Read a block of bytes at the given offset</summary>
        public static byte[] ReadBlock(this byte[] rom, int offset, int length)
        {
            byte[] block = new byte[length];
            Array.Copy(rom, offset, block, 0, length);
            return block;
        }
        /// <summary>Write a block of bytes to the given offset</summary>
        public static void WriteBlock(this byte[] rom, int offset, byte[] data)
        {
            Array.Copy(data, 0, rom, offset, data.Length);
        }

        #region Number Reading and Writing (UInt16, 32, etc)
        /// <summary>Reads a UInt of specified number of bytes.
        /// The number of bits in the resulting int is numBytes * 8</summary>
        private static int ReadUInt(byte[] rom, int offset, int numBytes)
        {
            int ret = 0;
            for (int i = 0; i < numBytes; ++i)
                ret += (rom[offset + i] << (i * 8));
            return ret;
        }
        /// <summary>Reads a Unit32 (4 bytes)</summary>
        public static int ReadUInt32(this byte[] rom, int offset)
        {
            return ReadUInt(rom, offset, 4);
        }
        /// <summary>Reads a Unit24 (4 bytes)</summary>
        public static int ReadUInt24(this byte[] rom, int offset)
        {
            return ReadUInt(rom, offset, 4);
        }
        /// <summary>Reads a Unit16 (2 bytes)</summary>
        public static int ReadUInt16(this byte[] rom, int offset)
        {
            return ReadUInt(rom, offset, 2);
        }
        /// <summary> Reads a pointer from the rom.
        /// A pointer on gen 3 ROMs is stored as a 32-bit number which points to a location in RAM where the game would be running
        /// <para> However, the actual address in the ROM is the first 24 bits, because the ROM is loaded into RAM at 0x08000000 </para>
        /// This method returns the 24-bit ROM address unless readRamAddy is set to true. To get the RAM adress, simply add 0x08000000</summary>
        public static int ReadPointer(this byte[] rom, int offset, bool readRamAddy = false)
        {
            return readRamAddy ? ReadUInt(rom, offset, 4) : ReadUInt(rom, offset, 3);
        }
        /// <summary>Writes a UInt of specified number of bytes.
        /// The number of bits written is numBytes * 8</summary>
        private static void WriteUInt(byte[] rom, int offset, int value, int numBytes)
        {
            for (int i = 0; i < numBytes; i++)
            {
                rom[i + offset] = unchecked((byte)(value & 0xff));
                value = value >> 8;
            }
        }
        /// <summary>Writes a Unit32 (4 bytes)</summary>
        public static void WriteUInt32(this byte[] rom, int offset, int value)
        {
            WriteUInt(rom, offset, value, 4);
        }
        /// <summary>Writes a Unit16 (2 bytes)</summary>
        public static void WriteUInt16(this byte[] rom, int offset, int value)
        {
            WriteUInt(rom, offset, value, 2);
        }
        /// <summary>Writes a pointer to the rom (a 32-bit number).
        /// If the number given is a 24-bit ROM address, it is converted to a 32-bit RAM adress by adding 0x08000000 </summary>
        public static void WritePointer(this byte[] rom, int offset, int value, bool isRomAddy = true)
        {
                WriteUInt(rom, offset, isRomAddy ? 0x08000000 + value : value, 4);
        }
        #endregion
    }
}
