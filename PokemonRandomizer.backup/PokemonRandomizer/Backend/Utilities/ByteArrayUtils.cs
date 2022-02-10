﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PokemonRandomizer.Backend.Utilities
{
    /// <summary>A class of byte[] extension methods to help process rom files</summary>
    public static class ByteArrayUtils
    {
        /// <summary>Read a block of bytes at the given offset</summary>
        public static byte[] ReadBlock(this byte[] rom, int offset, int length)
        {
            byte[] block = new byte[length];
            System.Array.ConstrainedCopy(rom, offset, block, 0, length);
            return block;
        }
        /// <summary>Write a block of bytes to the given offset</summary>
        public static void WriteBlock(this byte[] rom, int offset, byte[] data)
        {
            Array.ConstrainedCopy(data, 0, rom, offset, data.Length);
        }

        #region Free Space and Hacking Utils
        /// <summary>Scans the rom and returns all free space blocks above a certain size (in bytes)</summary> 
        public static MemoryBlock[] ScanAllFreeSpace(this byte[] rom, byte freeSpace, int startAddy, int minSize = 10000)
        {
            List<MemoryBlock> blocks = new List<MemoryBlock>();
            for (int offset = startAddy; offset < rom.Length; ++offset)
            {
                if (rom[offset] != freeSpace)
                    continue;
                int start = offset;
                while (++offset < rom.Length && rom[offset] == freeSpace)
                    continue;
                if (offset - start >= minSize)
                    blocks.Add(new MemoryBlock(start, offset - start));
            }
            return blocks.ToArray();
        }
        /// <summary>Scans for the first open block of free space above a certain size (in bytes).
        /// Returns null if no big enough block is found</summary> 
        public static MemoryBlock ScanForFreeSpace(this byte[] rom, byte freeSpace, int startAddy, int minSize = 10000)
        {
            for (int offset = startAddy; offset < rom.Length; ++offset)
            {
                if (rom[offset] != freeSpace)
                    continue;
                int start = offset;
                while (++offset < rom.Length && rom[offset] == freeSpace)
                    if (offset - start >= minSize)
                        return new MemoryBlock(start, offset - start);
            }
            return null;
        }
        /// <summary>Scans for the first open block of free space above a certain size (in bytes).
        /// Returns null if no big enough block is found, else returns the offset of the block</summary>
        private static int? ScanForFreeSpaceOffset(byte[] rom, byte freeSpace, int startAddy, int minSize = 10000)
        {
            for (int offset = startAddy; offset < rom.Length; ++offset)
            {
                if (rom[offset] != freeSpace)
                    continue;
                int start = offset;
                while(++offset < rom.Length && rom[offset] == freeSpace)
                    if (offset - start >= minSize)
                        return start;
            }
            return null;
        }
        /// <summary> Write a chunk of data into the first availible block of free space.
        /// Returns null if no big enough block is found, else returns the offset of the block </summary>
        public static int? WriteInFreeSpace(this byte[] rom, byte[] data, byte freeSpace, int startAddy)
        {
            int? blockOffset = ScanForFreeSpaceOffset(rom, freeSpace, startAddy, data.Length);
            if (blockOffset == null)
                return null;
            System.Array.ConstrainedCopy(data, 0, rom, (int)blockOffset, data.Length);
            return blockOffset;
        }
        /// <summary> Repoint all pointers to an offset to a target offset. 
        /// Argument given is assumed to by a 24-bit ROM address, and is converted to a 32-bit RAM address</summary>
        public static void Repoint(this byte[] rom, int originalOffset, int newOffset)
        {
            //The offset with the 0x08000000 component
            int ptr = 0x08000000 + originalOffset;
            for (int i = 0; i < rom.Length - 3; ++i)
            {
                if (ReadUInt(rom, i, 4) == ptr)
                    // Write a pointer (faster private version - same as WritePointer())
                    WriteUInt(rom, i, 0x08000000  + newOffset, 4);
            }
        }
        /// <summary> Set entire block to a given byte value </summary>
        public static void WipeBlock(this byte[] rom, int offset, int length, byte setTo)
        {
            Array.ConstrainedCopy(Enumerable.Repeat(setTo, length).ToArray(), 0, rom, offset, length);
        }
        ///.<summary>A simple class to hold an address in memory with a length</summary>
        public class MemoryBlock
        {
            public int offset;
            public int length;
            public MemoryBlock(int offset, int length)
            {
                this.offset = offset;
                this.length = length;
            }
            public override string ToString()
            {
                return offset.ToString("X") + " - " + (offset - 1 + length).ToString("X") + ": length " + length;
            }
        }
        #endregion

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

        #region Text Decoding Decoding and Translation
        // Constants
        private static readonly Dictionary<byte, string> symTable = new Dictionary<byte, string>
        {
            {0x00," "},    {0x01,"À"},    {0x02,"Á"},    {0x03,"Â"},    {0x04,"Ç"},    {0x05,"È"},
            {0x06,"É"},    {0x07,"Ê"},    {0x08,"Ë"},    {0x09,"Ì"},    {0x0B,"Î"},    {0x0C,"Ï"},
            {0x0D,"Ò"},    {0x0E,"Ó"},    {0x0F,"Ô"},    {0x10,"Æ"},    {0x11,"Ù"},    {0x12,"Ú"},
            {0x13,"Û"},    {0x14,"Ñ"},    {0x15,"ß"},    {0x16,"à"},    {0x17,"á"},    {0x19,"ç"},
            {0x1A,"è"},    {0x1B,"é"},    {0x1C,"ê"},    {0x1D,"ë"},    {0x1E,"ì"},    {0x20,"î"},
            {0x21,"ï"},    {0x22,"ò"},    {0x23,"ó"},    {0x24,"ô"},    {0x25,"æ"},    {0x26,"ù"},
            {0x27,"ú"},    {0x28,"û"},    {0x29,"ñ"},    {0x2A,"º"},    {0x2B,"ª"},    {0x2C,"·"},
            {0x2D,"&"},    {0x2E,""},     {0x34,"[Lv]"}, {0x35,"="},    {0x36,";"},    {0x51,"¿"},
            {0x52,"¡"},    {0x53,"[PK]"}, {0x54,"[MN]"}, {0x55,"[PO]"}, {0x56,"[Ke]"}, {0x57,"[BL]"},
            {0x58,"[OC]"}, {0x59,"[K]"},  {0x5A,"Í"},    {0x5B,"%"},    {0x5C,"("},    {0x5D,")"},
            {0x68,"â"},    {0x6F,"í"},    {0x79,"[U]"},  {0x7A,"[D]"},  {0x7B,"[L]"},  {0x7C,"[R]"},
            {0xA1,"0"},    {0xA2,"1"},    {0xA3,"2"},    {0xA4,"3"},    {0xA5,"4"},    {0xA6,"5"},
            {0xA7,"6"},    {0xA8,"7"},    {0xA9,"8"},    {0xAA,"9"},    {0xAB,"!"},    {0xAC,"?"},
            {0xAD,"."},    {0xAE,"-"},    {0xAF,"·"},    {0xB0,"…"},    {0xB1,"“"},    {0xB2,"”"},
            {0xB3,"‘"},    {0xB4,"’"},    {0xB5,"?"},    {0xB6,"?"},    {0xB7,"$"},    {0xB8,"\""},
            {0xB9,"[x]"},  {0xBA,"/"},    {0xBB,"A"},    {0xBC,"B"},    {0xBD,"C"},    {0xBE,"D"},
            {0xBF,"E"},    {0xC0,"F"},    {0xC1,"G"},    {0xC2,"H"},    {0xC3,"I"},    {0xC4,"J"},
            {0xC5,"K"},    {0xC6,"L"},    {0xC7,"M"},    {0xC8,"N"},    {0xC9,"O"},    {0xCA,"P"},
            {0xCB,"Q"},    {0xCC,"R"},    {0xCD,"S"},    {0xCE,"T"},    {0xCF,"U"},    {0xD0,"V"},
            {0xD1,"W"},    {0xD2,"X"},    {0xD3,"Y"},    {0xD4,"Z"},    {0xD5,"a"},    {0xD6,"b"},
            {0xD7,"c"},    {0xD8,"d"},    {0xD9,"e"},    {0xDA,"f"},    {0xDB,"g"},    {0xDC,"h"},
            {0xDD,"i"},    {0xDE,"j"},    {0xDF,"k"},    {0xE0,"l"},    {0xE1,"m"},    {0xE2,"n"},
            {0xE3,"o"},    {0xE4,"p"},    {0xE5,"q"},    {0xE6,"r"},    {0xE7,"s"},    {0xE8,"t"},
            {0xE9,"u"},    {0xEA,"v"},    {0xEB,"w"},    {0xEC,"x"},    {0xED,"y"},    {0xEE,"z"},
            {0xEF,"[>]"},  {0xF0,":"},    {0xF1,"Ä"},    {0xF2,"Ö"},    {0xF3,"Ü"},    {0xF4,"ä"},
            {0xF5,"ö"},    {0xF6,"ü"},    {0xF7,"[u]"},  {0xF8,"[d]"},  {0xF9,"[l]"},  {0xFA,"\\l"},
            {0xFB,"\\p"},  {0xFC,"\\c"},  {0xFE,"\n"}
        };
        private const byte textTerminator = 0xFF;
        private const byte textVariable   = 0xFD;
        /// <summary>Read a string of specified length from the rom.
        /// Use with 2 args to read a variable length string</summary>
        public static string ReadString(this byte[] rom, int offset, int maxLength = int.MaxValue)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < maxLength; i++)
            {
                byte code = rom[offset + i];
                if (symTable.ContainsKey(code))
                    sb.Append(symTable[code]);
                else if (code == textTerminator)
                    break;
                else if (code == textVariable)
                {
                    int nextChar = rom[offset + i + 1] & 0xFF;
                    sb.Append("\\v" + string.Format("%02X", nextChar));
                    i++;
                }
                else
                    sb.Append("\\x" + string.Format("%02X", code));
            }
            return sb.ToString();
        }

        //private byte[] translateString(String text)
        //{
        //    List<Byte> data = new ArrayList<Byte>();
        //    while (text.length() != 0)
        //    {
        //        int i = Math.max(0, 4 - text.length());
        //        if (text.charAt(0) == '\\' && text.charAt(1) == 'x')
        //        {
        //            data.add((byte)Integer.parseInt(text.substring(2, 4), 16));
        //            text = text.substring(4);
        //        }
        //        else if (text.charAt(0) == '\\' && text.charAt(1) == 'v')
        //        {
        //            data.add((byte)Gen3Constants.textVariable);
        //            data.add((byte)Integer.parseInt(text.substring(2, 4), 16));
        //            text = text.substring(4);
        //        }
        //        else
        //        {
        //            while (!(d.containsKey(text.substring(0, 4 - i)) || (i == 4)))
        //            {
        //                i++;
        //            }
        //            if (i == 4)
        //            {
        //                text = text.substring(1);
        //            }
        //            else
        //            {
        //                data.add(d.get(text.substring(0, 4 - i)));
        //                text = text.substring(4 - i);
        //            }
        //        }
        //    }
        //    byte[] ret = new byte[data.size()];
        //    for (int i = 0; i < ret.length; i++)
        //    {
        //        ret[i] = data.get(i);
        //    }
        //    return ret;
        //}

        //private String readFixedLengthString(int offset, int length)
        //{
        //    return readString(offset, length);
        //}

        //public String readVariableLengthString(int offset)
        //{
        //    return readString(offset, Integer.MAX_VALUE);
        //}

        //private void writeFixedLengthString(String str, int offset, int length)
        //{
        //    byte[] translated = translateString(str);
        //    int len = Math.min(translated.length, length);
        //    System.arraycopy(translated, 0, rom, offset, len);
        //    if (len < length)
        //    {
        //        rom[offset + len] = (byte)Gen3Constants.textTerminator;
        //        len++;
        //    }
        //    while (len < length)
        //    {
        //        rom[offset + len] = 0;
        //        len++;
        //    }
        //}

        //private void writeVariableLengthString(String str, int offset)
        //{
        //    byte[] translated = translateString(str);
        //    System.arraycopy(translated, 0, rom, offset, translated.length);
        //    rom[offset + translated.length] = (byte)0xFF;
        //}

        //private int lengthOfStringAt(int offset)
        //{
        //    int len = 0;
        //    while ((rom[offset + (len++)] & 0xFF) != 0xFF)
        //    {
        //    }
        //    return len - 1;
        //}
        #endregion
    }
}
