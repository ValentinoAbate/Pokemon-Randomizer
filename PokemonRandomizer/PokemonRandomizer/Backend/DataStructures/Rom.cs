using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.Backend.DataStructures
{
    using Utilities.Debug;
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
        private int SearchStartOffset { get; set; }
        /// <summary>The byte that WriteInFreeSpace(byte[] data) can explicitly consider not to be free space</summary>
        private byte PaddingByte { get; }
        public int InternalOffset { get; private set; }
        public Stack<int> SavedOffsets { get; } = new Stack<int>();
        public int Length { get => File.Length; }
        public byte[] File { get; }

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
            PaddingByte = (byte)((FreeSpaceByte == 0xFF) ? 0x00 : 0xFF);
            File = new byte[rawRom.Length];
            Array.Copy(rawRom, File, rawRom.Length);
            InternalOffset = 0;           
        }
        /// <summary> Initilize a new Rom as a copy of an extant one </summary>
        public Rom(Rom toCopy)
        {
            FreeSpaceByte = toCopy.FreeSpaceByte;
            SearchStartOffset = toCopy.SearchStartOffset;
            PaddingByte = toCopy.PaddingByte;
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
        public void ReadBlock(ref byte[] destinationArray, int destinationIndex, int length)
        {
            Array.Copy(File, InternalOffset, destinationArray, destinationIndex, length);
            InternalOffset += length;
        }
        public void ReadBlock(int offset, ref byte[] destinationArray, int destinationIndex, int length)
        {
            Array.Copy(File, offset, destinationArray, destinationIndex, length);
        }
        /// <summary>Read a block of bytes at the internal offset</summary>
        public byte[] ReadBlock(int length)
        {
            byte[] block = new byte[length];
            Array.Copy(File, InternalOffset, block, 0, length);
            InternalOffset += length;
            return block;
        }
        /// <summary>Read a block of bytes at the given offset</summary>
        public byte[] ReadBlock(int offset, int length)
        {
            byte[] block = new byte[length];
            Array.Copy(File, offset, block, 0, length);
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
        /// <summary>Scans for the first open block of free space above a certain size (in bytes).
        /// Returns null if no big enough block is found, else returns the offset of the block
        /// If reserve is true and a valid block is found, the space will be reserved and will no longer be considered to be free</summary>
        public int? FindFreeSpaceOffset(int minSize, bool reserve = true)
        {
            var offset = FindFreeSpaceOffset(FreeSpaceByte, SearchStartOffset, minSize);
            if(reserve && offset.HasValue)
            {
                UpdateSearchStartOffset(offset.Value + minSize + 1);
            }
            return offset;
        }

        /// <summary>Scans for the first open block of free space above a certain size (in bytes).
        /// Returns null if no big enough block is found, else returns the offset of the block</summary>
        private int? FindFreeSpaceOffset(byte freeSpace, int startAddy, int minSize)
        {
            for (int offset = startAddy; offset < File.Length; ++offset)
            {
                if (File[offset] != freeSpace || offset % 4 != 0)
                    continue;
                int start = offset;
                while (++offset < File.Length && File[offset] == freeSpace)
                {
                    if (offset - start >= minSize)
                        return start;
                }
            }
            return null;
        }
        /// <summary> <para>Write a chunk of data into the first availible block of free space. </para>
        /// <para>Uses the offset "SearchStartOffset" and the  byte "FreeSpaceByte" which are set in the Rom constructor</para> 
        /// Returns null if no big enough block is found, else returns the ROM offset of the block </summary>
        public int? WriteInFreeSpace(byte[] data)
        {
            if(data.Length <=0)
                return null;
            // Padding calculations
            bool lastByteIsFreeSpace = data[data.Length - 1] == FreeSpaceByte;
            int dataLength = lastByteIsFreeSpace ? data.Length + 1 : data.Length;
            // Find free space block and reserve it (if availible)
            int? blockOffset = FindFreeSpaceOffset(dataLength);
            if (blockOffset == null)
                return null;
            // Write the data to the block
            Array.Copy(data, 0, File, blockOffset.Value, data.Length);
            // If padding was needed, add the padding
            if (lastByteIsFreeSpace)
            {
                WriteByte(blockOffset.Value + data.Length, PaddingByte);   
            }
            return blockOffset;
        }
        /// <summary> Repoint all pointers to an offset to a target offset. 
        /// Argument given is assumed to be a 24-bit ROM address, and is converted to a 32-bit RAM address</summary>
        [Obsolete("Performing an individual repoint is very inefficient. Please use the RepointMany function instead")]
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
        public void RepointMany(List<(int originalPtr, int newPtr)> repoints)
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
                    {
                        locations[ptrValue].Add(i);
                    }
                    else
                    {
                        locations.Add(ptrValue, new List<int>() { i });
                    }
                }
            }
            // Preform repoints
            foreach(var repoint in repoints)
            {
                // If there were no pointers of the given value found, skip
                if (!locations.ContainsKey(repoint.originalPtr))
                    continue;
                foreach (var ptr in locations[repoint.originalPtr])
                    WritePointer(ptr, repoint.newPtr);
            }
        }

        // Used to update the free space search start offset after writing to free space
        // This ensures that data written in free space doesn't get overwritten by new data
        private void UpdateSearchStartOffset(int newOffset)
        {
            if(newOffset > SearchStartOffset)
            {
                SearchStartOffset = newOffset;
            }
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
        /// <summary>Pushes the Rom's internal offset to the SavedOffsets Stack (to be loaded later) and then seeks the given offset</summary>
        public void SaveAndSeekOffset(int offset)
        {
            SavedOffsets.Push(InternalOffset);
            Seek(offset);
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
        public int ReadUInt24() => ReadUInt(3);
        /// <summary>Reads a UInt16 (2 bytes) from the Given offset </summary>
        public int ReadUInt24(int offset) => ReadUInt(offset, 3);
        /// <summary>Reads a UInt16 (2 bytes) from the internal offset </summary>
        public int ReadUInt16() => ReadUInt(2);
        /// <summary>Reads a UInt16 (2 bytes) from the Given offset </summary>
        public int ReadUInt16(int offset) => ReadUInt(offset, 2);
        /// <summary> Reads a pointer from the File at the given offset.
        /// A pointer on gen 3 ROMs is stored as a 32-bit number which points to a location in RAM where the game would be running
        /// <para> However, the actual address in the ROM is the first 24 bits, because the ROM is loaded into RAM at ramOffset </para>
        /// This method returns the 24-bit ROM address unless readRamAddy is set to true. To get the RAM adress, simply add ramOffset</summary>
        public int ReadPointer(int offset, bool readRamOffset = false)
        {
            return readRamOffset ? ReadUInt(offset, pointerSize) : ReadUInt(offset, pointerSize - 1);
        }
        /// <summary> Reads a pointer from the File at the internal offset.
        /// A pointer on gen 3 ROMs is stored as a 32-bit number which points to a location in RAM where the game would be running
        /// <para> However, the actual address in the ROM is the first 24 bits, because the ROM is loaded into RAM at ramOffset </para>
        /// This method returns the 24-bit ROM address unless readRamAddy is set to true. To get the RAM adress, simply add ramOffset</summary>
        public int ReadPointer(bool readRamOffset = false)
        {
            return readRamOffset ? ReadUInt(pointerSize) : ReadUInt(pointerSize) - ramOffset;
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
        public void WritePointer(int value, bool isRomOffset = true)
        {
            WriteUInt(isRomOffset ? ramOffset + value : value, pointerSize);
        }
        /// <summary>Writes a pointer to the File (a 32-bit number).
        /// If the number given is a 24-bit ROM address, it is converted to a 32-bit RAM adress by adding ramOffset </summary>
        public void WritePointer(int offset, int value, bool isRomOffset = true)
        {
            WriteUInt(offset, isRomOffset ? ramOffset + value : value, pointerSize);
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

        public byte[] TranslateString(string text, bool includeTerminator = false)
        {
            // text (+1 if adding a terminator) is strictly longer than the tranlated byte list due to groups and escape chars
            var bytes = new List<byte>(includeTerminator ? text.Length + 1 : text.Length);
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
            // Add a terminator if desired
            if (includeTerminator)
            {
                bytes.Add(textTerminatorSym);
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
                // Set the rest of the space to 0x00
                int lengthUsed = translated.Length + 1;
                if(lengthUsed < length)
                {
                    SetBlock(offset + lengthUsed, length - lengthUsed, 0x00);
                }
            }
        }

        public void WriteVariableLengthString(string text)
        {
            WriteBlock(TranslateString(text, true));
        }

        public void WriteVariableLengthString(int offset, string text)
        {
            WriteBlock(offset, TranslateString(text, true));
        }

        #endregion

        #region LZ77 Compression and Decompression

        // Compression Format Documentation (from https://www.akkit.org/info/gbatek.htm)
        // SWI 11h(GBA/NDS7/NDS9) - LZ77UnCompWram
        // SWI 12h(GBA/NDS7/NDS9) - LZ77UnCompVram(NDS: with Callback)
        // Expands LZ77-compressed data.The Wram function is faster, and writes in units of 8bits.For the Vram function the destination must be halfword aligned, data is written in units of 16bits.
        // If the size of the compressed data is not a multiple of 4, please adjust it as much as possible by padding with 0. Align the source address to a 4-Byte boundary.
        // r0 Source address, pointing to data as such:
        //  Data header (32bit)
        //    Bit 0-3   Reserved
        //    Bit 4-7   Compressed type(must be 1 for LZ77)
        //    Bit 8-31  Size of decompressed data
        //  Repeat below.Each Flag Byte followed by eight Blocks.
        //  Flag data (8bit)
        //    Bit 0-7   Type Flags for next 8 Blocks, MSB first
        //  Block Type 0 - Uncompressed - Copy 1 Byte from Source to Dest
        //    Bit 0-7   One data byte to be copied to dest
        //  Block Type 1 - Compressed - Copy N+3 Bytes from Dest-Disp-1 to Dest
        //    Bit 0-3   Disp MSBs
        //    Bit 4-7   Number of bytes to copy(minus 3)
        //    Bit 8-15  Disp LSBs
        // r1 Destination address
        // r2 Callback parameter(NDS SWI 12h only, see Callback notes below)
        // r3 Callback structure(NDS SWI 12h only, see Callback notes below)

        private const byte lzIdentifier = 0x10;
        private const int lzMinCompressedRunLength = 3;

        public byte[] ReadLZ77CompressedData(int offset)
        {
            SaveAndSeekOffset(offset);
            var data = ReadLZ77CompressedData();
            LoadOffset();
            return data;
        }

        public byte[] ReadLZ77CompressedData()
        {
            // Read header
            // First byte is the lz identifier, which is a magic number
            if (ReadByte() != lzIdentifier)
            {
                Logger.main.Error($"Attempting to read compressed data at {InternalOffset:x2}, but no lzIdentifier ({lzIdentifier:x2}) found");
                return Array.Empty<byte>();
            }
            // Next three bytes are the length of the data (in bytes) 
            int length = ReadUInt(3);
            var data = new byte[length];
            int dataIndex = 0;
            while(dataIndex < length)
            {
                // 1-byte section header. Tells you which of the next 8 tokens are compressed (by bitflag, msb is first token)
                byte sectionHeader = ReadByte();
                // All uncompressed, just copy the data
                if(sectionHeader == 0x00)
                {
                    int blockLength = Math.Min(8, length - dataIndex);
                    ReadBlock(ref data, dataIndex, blockLength);
                    dataIndex += blockLength;
                    continue;
                }
                // Some compressed tokens, iterate though tokens
                byte mask = 0b10000000;
                for(int tokenIndex = 0; tokenIndex < 8 && dataIndex < length; ++tokenIndex)
                {
                    if ((sectionHeader & mask) != 0) // Compressed Token
                    {
                        // Read Compressed Token
                        byte byte1 = ReadByte();
                        byte byte2 = ReadByte();
                        int runLength = (byte1 >> 4) + lzMinCompressedRunLength; // First 4 bits of byte one (+3)
                        int runOffset = (((byte1 & 0xF) << 8) | byte2) + 1; // Second 4 bits of byte 1 and byte two (+1)
                        // Uncompress compressed token into data
                        for(int runIndex = 0; runIndex < runLength; ++runIndex)
                        {
                            data[dataIndex] = data[dataIndex - runOffset];
                            ++dataIndex;
                        }
                        if (dataIndex == length)
                            return data;
                    }
                    else // Uncompressed token
                    {
                        data[dataIndex] = ReadByte();
                        ++dataIndex;
                    }
                    mask >>= 1;
                }
            }
            return data;
        }

        public void CompressToLZ77AndWriteData(int offset, byte[] data)
        {
            SaveAndSeekOffset(offset);
            CompressToLZ77AndWriteData(data);
            LoadOffset();
        }

        private const byte compressionHeaderMask = 0b10000000;

        public void CompressToLZ77AndWriteData(byte[] data)
        {
            // Write Header
            WriteByte(lzIdentifier);
            WriteUInt(data.Length, 3);

            // Write Data
            int dataIndex = 0;
            int tokenIndex = 0;
            byte header = 0x00;
            int headerOffset = InternalOffset;
            Skip(); // Skip the header byte. It will be filled in later when we know the compression status of upcoming tokens

            while (dataIndex < data.Length)
            {
                if (tokenIndex >= 8)
                {
                    WriteByte(headerOffset, header);
                    tokenIndex = 0;
                    header = 0x00;
                    headerOffset = InternalOffset;
                    Skip(); // Skip the header byte. It will be filled in later when we know the compression status of upcoming tokens
                }
                // Find the longest match
                (int runLength, int runOffset) = FindLongestMatch(data, dataIndex);
                // If compression token is lower than or equal to the exlusive minimum run length. Write uncompressed
                if(runLength < lzMinCompressedRunLength)
                {
                    WriteByte(data[dataIndex++]);
                }
                else
                {
                    // Translate to the values we actually write
                    int recordedOffset = Math.Min(runOffset - 1, 0xFFF);
                    int recordedLength = Math.Min(runLength - lzMinCompressedRunLength, 0xF);
                    // Write first byte (first 4 bits = length, next 4 bits = 4 msbs of offset)
                    WriteByte((byte)((recordedLength << 4) | (recordedOffset >> 8)));
                    // Write second byte (8 lsbs of offset)
                    WriteByte((byte)recordedOffset);
                    // Mark the token as compressed in the header
                    header |= (byte)(compressionHeaderMask >> tokenIndex);
                    dataIndex += runLength;
                }
                tokenIndex++;
            }
            // Write final header
            WriteByte(headerOffset, header);
        }

        private (int runLength, int runOffset) FindLongestMatch(byte[] data, int index)
        {
            int bestLength = 2;
            int bestOffset = -1;

            // Iterate though the even offsets (starting 2, because the minimum compressible match is 3)
            for (int runOffset = 2; index - runOffset >= 0; runOffset += 2)
            {
                int runLength = 0;
                // See how long the run at this offset is
                while (index + runLength < data.Length && runLength < 18 && data[index - runOffset + runLength] == data[index + runLength])
                {
                    ++runLength;
                }
                // If the run is longer than before, log the new bests
                if (runLength > bestLength)
                {
                    bestLength = runLength;
                    bestOffset = runOffset;
                }
            }

            return (bestLength, bestOffset);
        }

        #endregion

        #region BLZ (Bottom LZ) Compression and Decompression

        // All BLZ Compression and Decompression methods are
        // Based on BLZEncoder.java from Dabomstew's Universal Pokemon Randomizer
        // BLZEncoder.java is based on blz.c - Bottom LZ coding for Nintendo GBA/DS (Copyright (C) 2011 CUE)
        // Modified by Valentino Abate under the terms of the GPL

        private const int minBLZHeaderLength = 0x8;
        private const int maxBLZHeaderLength = 0xB;
        private const int maxBLZOutputLength = 0xFFFFFF;
        private const int BLZShift = 1;
        private const int BLZMask= 0x80;
        private const int BLZThreshold = 2;

        public byte[] ReadBLZCompressedData(int offset, int length)
        {
            // The difference between the length of the compressed portion of the data
            // And its length before compression
            int incLength = ReadUInt32(offset + length - 4);
            int headerLength = ReadByte(offset + length - 5);
            if(headerLength > maxBLZHeaderLength || headerLength < minBLZHeaderLength)
            {
                Logger.main.Error($"Error attempting to decompress BLZ data at {offset:x2}: bad header length");
                return Array.Empty<byte>();
            }
            // The length of the compressed portion of the data
            int compressedLength = ReadUInt24(offset + length - 8);
            // The length of the uncompressed portion of the data
            int uncompressedLength = length - compressedLength;
            // The length of the output data (the already uncompressedLength + the currently compressed length + the compression gain
            int outputLength = uncompressedLength + compressedLength + incLength;
            if(outputLength > maxBLZOutputLength)
            {
                Logger.main.Error($"Error attempting to decompress BLZ data at {offset:x2}: outLength ({outputLength} is longer than max length {maxBLZOutputLength})");
                return Array.Empty<byte>();
            }

            byte[] output = new byte[outputLength];
            // Copy uncompressed data to output file
            Array.Copy(File, offset, output, 0, uncompressedLength);
            // Prepare input data
            byte[] input = new byte[length - headerLength];
            Array.Copy(File, offset, input, 0 , input.Length);
            Array.Reverse(input, uncompressedLength, input.Length - uncompressedLength);


            // Iterate through input data
            uint mask = 0;
            int flags = 0;
            for (int outputIndex = uncompressedLength, inputIndex = uncompressedLength; outputIndex < output.Length && inputIndex < input.Length;)
            {
                if((mask >>= BLZShift) == 0)
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
                    int posOffset = (((byte1 << 8) | byte2) & 0x0FFF) + 3;
                    if (outputIndex + len > output.Length)
                    {
                        Logger.main.Warning($"BLZ Decompression warning: incorrect decoded length. Expected {output.Length}, got {outputIndex + len}");
                        len = Math.Max(0, output.Length - outputIndex);
                    }
                    while ((len--) > 0)
                    {
                        output[outputIndex] = output[outputIndex - posOffset];
                        ++outputIndex;
                    }
                }
            }
            Array.Reverse(output, uncompressedLength, output.Length - uncompressedLength);
            return output;
        }

        public byte[] ReadBLZCompressedData(int length)
        {
            SaveOffset();
            var data = ReadBLZCompressedData(InternalOffset, length);
            LoadOffset();
            return data;
        }

        #endregion
    }
}
