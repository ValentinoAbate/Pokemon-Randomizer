using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PokemonRandomizer.Backend.DataStructures
{
    using Utilities;
    /// <summary>
    /// Wraps a byte[] into a class designed to be read from
    /// </summary>
    public class Rom
    {
        /// <summary>
        /// Address rom is loaded into in GBA RAM
        /// </summary>
        private const int ramOffset = 0x08000000;
        public const int nullPointer = -ramOffset; // Due to how ReadPointer() Reads the 0x00000000 pointer
        public const int pointerSize = 4; //size of a pointer in bytes
        public const byte pointerPrefix = 0x08; //comes at the beginning of a pointer
        /// <summary>The byte that WriteInFreeSpace(byte[] data) considers free space </summary>
        public byte FreeSpaceByte { get; }
        /// <summary>The offset that WriteInFreeSpace(byte[] data) starts searching at </summary>
        public int SearchStartOffset { get; }
        public int InternalOffset { get; private set; }
        public Stack<int> SavedOffsets { get; } = new Stack<int>();
        public int Length { get => File.Length; }
        public byte[] File { get;}

        static Rom()
        {
            // Initialize the reverse-text table
            textToSymTable = new Dictionary<string, byte>();
            foreach(var kvp in symTable)
            {
                if (!textToSymTable.ContainsKey(kvp.Value))
                    textToSymTable.Add(kvp.Value, kvp.Key);
            }
        }

        /// <summary> Initilize a new Rom from raw data </summary>
        public Rom(byte[] rawRom, byte freeSpaceByte, int searchStartOffset)
        {
            FreeSpaceByte = freeSpaceByte;
            SearchStartOffset = searchStartOffset;
            File = new byte[rawRom.Length];
            Array.Copy(rawRom, File, rawRom.Length);
            InternalOffset = 0;           
        }
        /// <summary> Initilize a new Rom as a copy of an extant one </summary>
        public Rom(Rom toCopy)
        {
            FreeSpaceByte = toCopy.FreeSpaceByte;
            SearchStartOffset = toCopy.SearchStartOffset;
            File = new byte[toCopy.File.Length];
            Array.Copy(toCopy.File, File, toCopy.File.Length);
            InternalOffset = 0;  
        }
        /// <summary> Initilize an Empty Rom with a given length </summary>
        public Rom(int length, byte freeSpaceByte, int searchStartOffset = 0)
            : this(new byte[length], freeSpaceByte, searchStartOffset) { }
        /// <summary>Reads a byte from the internal offset</summary>
        public byte WriteByte(byte value) => File[InternalOffset++] = value;
        /// <summary>Reads a byte from the internal offset</summary>
        public byte WriteByte(int offset, byte value) => File[offset] = value;
        /// <summary>Read a block of bytes at the internal offset</summary>
        public byte[] ReadBlock(int length)
        {
            byte[] block = new byte[length];
            System.Array.Copy(File, InternalOffset, block, 0, length);
            InternalOffset += length;
            return block;
        }
        /// <summary>Read a block of bytes at the given offset</summary>
        public byte[] ReadBlock(int offset, int length)
        {
            byte[] block = new byte[length];
            System.Array.Copy(File, offset, block, 0, length);
            return block;
        }
        /// <summary>Write a block of bytes to the internal offset</summary>
        public void WriteBlock(byte[] data)
        {
            Array.Copy(data, 0, File, InternalOffset, data.Length);
            InternalOffset += data.Length;
        }
        /// <summary>Write a block of bytes to the given offset</summary>
        public void WriteBlock(int offset, byte[] data)
        {
            Array.Copy(data, 0, File, offset, data.Length);
        }
        /// <summary>Write a block of bytes to the given offset</summary>
        public void WriteBlock(int offset, byte[] data, int sourceInd, int length)
        {
            Array.Copy(data, sourceInd, File, offset, length);
        }

        #region Free Space and Hacking Utils
        /// <summary>Scans the File and returns all free space blocks above a certain size (in bytes)</summary> 
        public MemoryBlock[] ScanAllFreeSpace(int minSize = 10000, int? startAddy = null)
        {
            List<MemoryBlock> blocks = new List<MemoryBlock>();
            for (int offset = startAddy ?? SearchStartOffset; offset < File.Length; ++offset)
            {
                if (File[offset] != FreeSpaceByte || offset % 4 != 0)
                    continue;
                int start = offset;
                while (++offset < File.Length && File[offset] == FreeSpaceByte)
                    continue;
                if (offset - start >= minSize)
                    blocks.Add(new MemoryBlock(start, offset - start));
            }
            return blocks.ToArray();
        }
        /// <summary>Scans for the first open block of free space above a certain size (in bytes).
        /// Returns null if no big enough block is found</summary> 
        public MemoryBlock ScanForFreeSpace(int minSize = 10000, int? startAddy = null)
        {
            for (int offset = startAddy ?? SearchStartOffset; offset < File.Length; ++offset)
            {
                if (File[offset] != FreeSpaceByte || offset % 4 != 0)
                    continue;
                int start = offset;
                while (++offset < File.Length && File[offset] == FreeSpaceByte)
                    if (offset - start >= minSize)
                        return new MemoryBlock(start, offset - start);
            }
            return null;
        }
        /// <summary>Scans for the first open block of free space above a certain size (in bytes).
        /// Returns null if no big enough block is found, else returns the offset of the block</summary>
        private int? ScanForFreeSpaceOffset(byte freeSpace, int startAddy, int minSize = 10000)
        {
            for (int offset = startAddy; offset < File.Length; ++offset)
            {
                if (File[offset] != freeSpace || offset % 4 != 0)
                    continue;
                int start = offset;
                while (++offset < File.Length && File[offset] == freeSpace)
                    if (offset - start >= minSize)
                        return start;
            }
            return null;
        }
        /// <summary> <para>Write a chunk of data into the first availible block of free space. </para>
        /// <para>Uses the offset "SearchStartOffset" and the  byte "FreeSpaceByte" which are set in the Rom constructor</para> 
        /// Returns null if no big enough block is found, else returns the ROM offset of the block </summary>
        public int? WriteInFreeSpace(byte[] data, int? startOffset = null)
        {
            int? blockOffset = ScanForFreeSpaceOffset(FreeSpaceByte, startOffset ?? SearchStartOffset, data.Length);
            if (blockOffset == null)
                return null;
            System.Array.Copy(data, 0, File, (int)blockOffset, data.Length);
            return blockOffset;
        }
        /// <summary> Repoint all pointers to an offset to a target offset. 
        /// Argument given is assumed to be a 24-bit ROM address, and is converted to a 32-bit RAM address</summary>
        public void Repoint(int originalOffset, int newOffset)
        {
            //The offset with the ramOffset component
            int ptr = ramOffset + originalOffset;
            var pointerInstances = FindAll(BitConverter.GetBytes(ptr));
            foreach (var instance in pointerInstances)
                WritePointer(instance, newOffset);
        }
        /// <summary> 
        /// Preform many repoint operations in the same call, much more efficiently than calling repoint multiple times.
        /// Scans through the ROM and caches all pointer locations in a dictionary, then repoints.
        /// Argument given is assumed to be a List of Tuples of 24-bit ROM addresses, which are converted to 32-bit RAM addresses.
        /// The first int in the Tuple is the original pointer value, and the second int is new value
        /// </summary>
        public void RepointMany(List<System.Tuple<int, int>> repoints)
        {
            // A caching dictionary of pointer values to known locations
            var locations = new Dictionary<int, List<int>>();
            // Build caching dictionary
            for(int i = 0; i < Length - pointerSize + 1; i++)
            {
                if(File[i + pointerSize - 1] == pointerPrefix)
                {
                    int ptrValue = ReadPointer(i);
                    if (locations.ContainsKey(ptrValue))
                        locations[ptrValue].Add(i);
                    else
                        locations.Add(ptrValue, new List<int>() { i });
                }
            }
            // Preform repoints
            foreach(var repoint in repoints)
            {
                // If there were no pointers of the given value found, skip
                if (!locations.ContainsKey(repoint.Item1))
                    continue;
                foreach (var ptr in locations[repoint.Item1])
                    WritePointer(ptr, repoint.Item2);
            }
        }

        public int? WriteInFreeSpaceAndRepoint(byte[] data, int originalOffset, int? startOffset = null)
        {
            var newOffset = WriteInFreeSpace(data, startOffset);
            if (newOffset == null)
                return null;
            Repoint(originalOffset, (int)newOffset);
            return newOffset;
        }

        /// <summary> Set entire block to a given byte value at Internal offset</summary>
        public void SetBlock(int length, byte setTo)
        {
            WriteBlock(Enumerable.Repeat(setTo, length).ToArray());
        }
        /// <summary> Set entire block to a given byte value </summary>
        public void SetBlock(int offset, int length, byte setTo)
        {
            WriteBlock(offset, Enumerable.Repeat(setTo, length).ToArray());
        }
        /// <summary> Set entire block to the Free Space value </summary>
        public void WipeBlock(int length) => SetBlock(length, FreeSpaceByte);
        /// <summary> Set entire block to the Free Space value </summary>
        public void WipeBlock(int offset, int length) => SetBlock(offset, length, FreeSpaceByte);
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

        public bool IsValidOffset(int offset)
        {
            return offset >= 0 && offset < Length;
        }
        /// <summary>Sets the Rom's internal offset</summary>
        public void Seek(int offset)
        {
            InternalOffset = offset;
        }
        /// <summary>Returns the byte at the Rom's internal offset (without changing it)</summary>
        public byte Peek() => File[InternalOffset];
        /// <summary>Returns the byte at the Rom's internal offset + the given number ahead (without changing it)</summary>
        public byte Peek(int numAhead) => File[InternalOffset + numAhead];
        /// <summary>Increments the Rom's internal offset by a given number of bytes</summary>
        public void Skip(int numBytes = 1)
        {
            InternalOffset += numBytes;
        }
        /// <summary>Pushes the Rom's internal offset to the SavedOffsets Stack (to be loaded later)</summary>
        public void SaveOffset()
        {
            SavedOffsets.Push(InternalOffset);
        }
        /// <summary>Sets the Rom's internal offset from the SavedOffsets Stack</summary>
        public void LoadOffset()
        {
            if(SavedOffsets.Count > 0)
                InternalOffset = SavedOffsets.Pop();
        }
        /// <summary>Deletes the latest saved offset from the SavedOffsets Stack</summary>
        public void DumpOffset()
        {
            if (SavedOffsets.Count > 0)
                SavedOffsets.Pop();
        }
        /// <summary>Reads a byte from the internal offset</summary>
        public byte ReadByte() => File[InternalOffset++];
        /// <summary>Reads a byte from the given offset</summary>
        public byte ReadByte(int offset) => File[offset];

        #region Bitwise Reading/Writing

        /// <summary>Reads the bits of a byte in chunks of chunkSize from the internal offset</summary>
        public int[] ReadBits(int numBits = 8, int chunkSize = 1)
        {
            int numBytes = numBits % 8 == 0 ? numBits / 8 : (numBits / 8) + 1;
            InternalOffset += numBytes;
            return ReadBits(InternalOffset - numBytes, numBits, chunkSize);
        }
        /// <summary>Reads the bits of a byte in chunks of chunkSize from the given offset</summary>
        public int[] ReadBits(int offset, int numBits = 8, int chunkSize = 1)
        {
            // Create a mask for n bits, where n = chunkSize
            int mask = Utilities.MathUtils.IntPow(2, chunkSize) - 1;
            int numChunks = numBits / chunkSize;
            int chunksPerByte = 8 / chunkSize;
            int[] ret = new int[numChunks];
            for(int i = 0; i < numChunks; ++i)
            {
                byte src = File[offset + i / chunksPerByte];
                int shiftBy = (i % chunksPerByte) * chunkSize;
                ret[i] = (src & (mask << shiftBy)) >> shiftBy;
            }
            return ret;
        }
        /// <summary>Writes the input values to bits in chunks of chunkSize at the internal offset</summary>
        public void WriteBits(int chunkSize, params int[] chunkValues)
        {
            WriteBits(InternalOffset, chunkSize, chunkValues);
            int chunksPerByte = 8 / chunkSize;
            // Calculate number of bytes and increment offset by that number
            InternalOffset += (int)Math.Ceiling(chunkValues.Length / (float)chunksPerByte);
        }
        /// <summary>Writes the input values to bits in chunks of chunkSize at the given offset</summary>
        public void WriteBits(int offset, int chunkSize, params int[] chunkValues)
        {
            int chunksPerByte = 8 / chunkSize;
            byte[] byteValues = new byte[(int)Math.Ceiling(chunkValues.Length / (float)chunksPerByte)];
            int byteIndex = -1;
            for (int i = 0; i < chunkValues.Length; ++i)
            {
                int chunkIndex = i % chunksPerByte;
                if (chunkIndex == 0)
                    ++byteIndex;
                byteValues[byteIndex] += (byte)(chunkValues[i] * Utilities.MathUtils.IntPow(2, chunkIndex * chunkSize));
            }
            WriteBlock(offset, byteValues);
        }
        #endregion

        #region Pattern Searching
        /// <summary> Find all instances of a byte sequence in the ROM </summary>
        public List<int> FindAll(string hexString)
        {
            var pattern = Utilities.HexUtils.HexToBytes(hexString);
            return Search.Kmp.SearchAll(File, pattern);
        }
        /// <summary> Find all instances of a byte sequence in the ROM </summary>
        public List<int> FindAll(byte[] pattern)
        {
            return Search.Kmp.SearchAll(File, pattern);
        }
        /// <summary> Find the first instance of a byte sequence in the ROM </summary>
        public int FindFirst(string hexString)
        {
            var pattern = Utilities.HexUtils.HexToBytes(hexString);
            return Search.Kmp.Search(File, pattern);
        }
        /// <summary> Find all instances of a byte sequence in the ROM </summary>
        public int FindFirst(byte[] pattern)
        {
            return Search.Kmp.Search(File, pattern);
        }
        /// <summary> Find the index after a given pattern prefix. Throws exceptions if the prefix is not found or is a duplicate </summary>
        public int FindFromPrefix(string prefix) => FindFromPrefix(Utilities.HexUtils.HexToBytes(prefix));
        /// <summary> Find the index after a given pattern prefix. Throws exceptions if the prefix is not found or is a duplicate </summary>
        public int FindFromPrefix(byte[] prefix)
        {
            var prefixes = FindAll(prefix);
            // If no prefix was found, throw an exception
            if (prefixes.Count <= 0)
                throw new Exception("Error: no prefix found");
            // If more than 1 prefix was found, throw an exception
            if (prefixes.Count > 1)
                throw new Exception("Error: prefix is not unique");
            // return the location of the prefix + the length of the prefix
            return prefixes[0] + prefix.Length;
        }
        #endregion

        #region Number Reading and Writing (UInt16, 32, etc)
        private int ReadUInt(int numBytes)
        {
            int ret = ReadUInt(InternalOffset, numBytes);
            InternalOffset += numBytes;
            return ret;
        }
        /// <summary>Reads a UInt of specified number of bytes.
        /// The number of bits in the resulting int is numBytes * 8</summary>
        private int ReadUInt(int offset, int numBytes)
        {
            int ret = 0;
            for (int i = 0; i < numBytes; ++i)
                ret += (File[offset + i] << (i * 8));
            return ret;
        }       
        /// <summary>Reads a UInt32 (4 bytes) from the internal offset </summary>
        public int ReadUInt32() => ReadUInt(4);
        /// <summary>Reads a UInt32 (4 bytes) from the given offset </summary>
        public int ReadUInt32(int offset) => ReadUInt(offset, 4);
        /// <summary>Reads a UInt16 (2 bytes) from the internal offset </summary>
        public int ReadUInt16() => ReadUInt(2);
        /// <summary>Reads a UInt16 (2 bytes) from the Given offset </summary>
        public int ReadUInt16(int offset) => ReadUInt(offset, 2);
        /// <summary> Reads a pointer from the File at the given offset.
        /// A pointer on gen 3 ROMs is stored as a 32-bit number which points to a location in RAM where the game would be running
        /// <para> However, the actual address in the ROM is the first 24 bits, because the ROM is loaded into RAM at ramOffset </para>
        /// This method returns the 24-bit ROM address unless readRamAddy is set to true. To get the RAM adress, simply add ramOffset</summary>
        public int ReadPointer(int offset, bool readRamAddy = false)
        {
            return readRamAddy ? ReadUInt(offset, pointerSize) : ReadUInt(offset, pointerSize - 1);
        }
        /// <summary> Reads a pointer from the File at the internal offset.
        /// A pointer on gen 3 ROMs is stored as a 32-bit number which points to a location in RAM where the game would be running
        /// <para> However, the actual address in the ROM is the first 24 bits, because the ROM is loaded into RAM at ramOffset </para>
        /// This method returns the 24-bit ROM address unless readRamAddy is set to true. To get the RAM adress, simply add ramOffset</summary>
        public int ReadPointer(bool readRamAddy = false)
        {
            return readRamAddy ? ReadUInt(pointerSize) : ReadUInt(pointerSize) - ramOffset;
        }
        /// <summary>Writes a UInt of specified number of bytes to the internalOffset </summary>
        private void WriteUInt(int value, int numBytes)
        {
            WriteUInt(InternalOffset, value, numBytes);
            InternalOffset += numBytes;
        }
        /// <summary>Writes a UInt of specified number of bytes.
        /// The number of bits written is numBytes * 8</summary>
        private void WriteUInt(int offset, int value, int numBytes)
        {
            for (int i = 0; i < numBytes; i++)
            {
                File[i + offset] = unchecked((byte)(value & 0xff));
                value >>= 8;
            }
        }
        /// <summary>Writes a Unit32 (4 bytes) to the internal offset</summary>
        public void WriteUInt32(int value) => WriteUInt(value, 4);
        /// <summary>Writes a Unit32 (4 bytes) to the given offset</summary>
        public void WriteUInt32(int offset, int value) => WriteUInt(offset, value, 4);
        /// <summary>Writes a Unit16 (2 bytes) to the internal offset</summary>
        public void WriteUInt16(int value) => WriteUInt(value, 2);
        /// <summary>Writes a Unit16 (2 bytes) to the given</summary>
        public void WriteUInt16(int offset, int value) => WriteUInt(offset, value, 2);
        /// <summary>Writes a pointer to the File (a 32-bit number).
        /// If the number given is a 24-bit ROM address, it is converted to a 32-bit RAM adress by adding ramOffset </summary>
        public void WritePointer(int value, bool isRomAddy = true)
        {
            WriteUInt(isRomAddy ? ramOffset + value : value, pointerSize);
        }
        /// <summary>Writes a pointer to the File (a 32-bit number).
        /// If the number given is a 24-bit ROM address, it is converted to a 32-bit RAM adress by adding ramOffset </summary>
        public void WritePointer(int offset, int value, bool isRomAddy = true)
        {
            WriteUInt(offset, isRomAddy ? ramOffset + value : value, pointerSize);
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
            {0x2D,"&"},    {0x2E,"[ ]"},  {0x34,"[Lv]"}, {0x35,"="},    {0x36,";"},    {0x51,"¿"},
            {0x52,"¡"},    {0x53,"[PK]"}, {0x54,"[MN]"}, {0x55,"[PO]"}, {0x56,"[Ke]"}, {0x57,"[BL]"},
            {0x58,"[OC]"}, {0x59,"[K]"},  {0x5A,"Í"},    {0x5B,"%"},    {0x5C,"("},    {0x5D,")"},
            {0x68,"â"},    {0x6F,"í"},    {0x79,"[U]"},  {0x7A,"[D]"},  {0x7B,"[L]"},  {0x7C,"[R]"},
            {0xA1,"0"},    {0xA2,"1"},    {0xA3,"2"},    {0xA4,"3"},    {0xA5,"4"},    {0xA6,"5"},
            {0xA7,"6"},    {0xA8,"7"},    {0xA9,"8"},    {0xAA,"9"},    {0xAB,"!"},    {0xAC,"?"},
            {0xAD,"."},    {0xAE,"-"},    {0xAF,"·"},    {0xB0,"…"},    {0xB1,"“"},    {0xB2,"”"},
            {0xB3,"‘"},    {0xB4,"’"},    {0xB5,"?"},    {0xB6,"?"},    {0xB7,"$"},    {0xB8,"\""},
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
        private static readonly Dictionary<string, byte> textToSymTable; // Initialized in the static constructor
        private const byte textTerminatorSym = 0xFF;
        private const byte textVariableSym = 0xFD;
        private const char escapeChar = '\\';
        private const char groupChar = '[';
        private const char groupEndChar = ']';
        private const string textVariableStr = "\\v";
        private const string textUnknownStr = "\\x";
        /// <summary>Read a string of specified length from the File.</summary>
        public string ReadVariableLengthString(int maxLength = int.MaxValue)
        {
            var str = ReadString(InternalOffset, maxLength);
            InternalOffset += str.Length;
            return str;
        }

        /// <summary>Read a string of specified length from the File.</summary>
        public string ReadFixedLengthString(int length)
        {
            var str = ReadString(InternalOffset, length);
            InternalOffset += length;
            return str;
        }
        /// <summary>Read a string of specified length from the File.
        /// Use with 2 args to read a fixed length string</summary>
        public string ReadString(int offset, int maxLength = int.MaxValue)
        {
            string text = string.Empty;
            for (int i = 0; i < maxLength; i++)
            {
                byte code = File[offset + i];
                if (code == textTerminatorSym)
                {
                    break;
                }
                else if (symTable.ContainsKey(code))
                {
                    text += symTable[code];
                }
                else if (code == textVariableSym)
                {
                    int nextChar = File[offset + ++i] & 0xFF;
                    text += (textVariableStr + string.Format("%02X", nextChar));
                }
                else
                {
                    text += (textUnknownStr + string.Format("%02X", code));
                }

            }
            return text;
        }

        private byte[] TranslateString(string text)
        {
            // text is strictly longer than the tranlated byte list due to groups and escape chars
            var bytes = new List<byte>(text.Length);
            for(int i = 0; i < text.Length; ++i)
            {
                char currChar = text[i];

                // Parse symbol at current index
                string currSym;
                if(currChar == escapeChar)
                {
                    currSym = text.Substring(i++, 2);
                    if(currSym == textUnknownStr)
                    {
                        bytes.Add(byte.Parse(text.Substring(i, 2)));
                        i += 2;
                        continue;
                    }
                    else if(currSym == textVariableStr)
                    {
                        bytes.Add(textVariableSym);
                        bytes.Add(byte.Parse(text.Substring(i, 2)));
                        i += 2;
                        continue;
                    }
                }
                else if(currChar == groupChar)
                {
                    int groupLength = text.IndexOf(groupEndChar, i + 1) - i + 1;
                    currSym = text.Substring(i, groupLength);
                    i += (groupLength - 1); // group length counts the char at i
                }
                else
                {
                    currSym = currChar.ToString();
                }

                // Translate symbol to byte
                if(textToSymTable.ContainsKey(currSym))
                {
                    bytes.Add(textToSymTable[currSym]);
                }
                else
                {
                    Logger.main.Error("Unrecognized text symbol: " + currSym);
                }
            }
            return bytes.ToArray();
        }

        public void WriteFixedLengthString(string text, int length)
        {
            WriteFixedLengthString(InternalOffset, text, length);
            InternalOffset += length;
        }

        public void WriteFixedLengthString(int offset, string text, int length)
        {
            var translated = TranslateString(text);
            if(translated.Length >= length)
            {
                // Write as much of the translated string as we can
                Array.Copy(translated, 0, File, offset, length);
            }
            else
            {
                // Write the translated string
                WriteBlock(offset, translated);
                // Write the text terminator
                WriteByte(offset + translated.Length, textTerminatorSym);
                // Wipe the rest of the space
                int lengthUsed = translated.Length + 1;
                if(lengthUsed < length)
                {
                    SetBlock(offset + lengthUsed, length - lengthUsed, 0x00);
                }
            }
        }

        public void WriteVariableLengthString(string text)
        {
            var translated = TranslateString(text);
            WriteBlock(translated);
            WriteByte(textTerminatorSym);
        }

        public void WriteVariableLengthString(int offset, string text)
        {
            var translated = TranslateString(text);
            WriteBlock(offset, translated);
            WriteByte(offset + translated.Length, textTerminatorSym);
        }

        #endregion
    }
}
