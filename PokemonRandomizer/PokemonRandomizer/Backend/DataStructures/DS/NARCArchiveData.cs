using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonRandomizer.Backend.DataStructures.DS
{
    // Documentation from: https://www.romhacking.net/documents/469/
    // With modified formatting by me 
    //
    // Archive File Formats
    // ------------------------------------------------------
    // Nintendo Archive(ARC/NARC)
    // - uses Generic Header
    // - magic ID is #NARC (0x4E415243)
    // - contains 3 sub-sections
    // -------------------------------------------------------
    // #0 Section - Generic Header
    // -------------------------------------------------------
    // Offset | Length   | Name                               | Description
    // 0x0 	  | 0x4 	 | Magic ID                           | Identifies the file format.
    // 0x4 	  | 0x4 	 | Constant                           | Always (0xFFFE0001)
    // 0x8 	  | 0x4 	 | Section Size                       | Size of this section, including the header.
    // 0xC 	  | 0x2 	 | Header Size                        | Size of this header. (Should always equal 0x10)
    // 0xE 	  | 0x2 	 | Number of Sections                 | The number of sub-sections in this section. 
    // -------------------------------------------------------
    // #1 Section - File Allocation Table (BTAF)
    // -------------------------------------------------------
    // Offset | Length   | Name                               | Description
    // 0x0 	  | 0x4      | Magic ID 	                      | #BTAF (0x42544146)
    // 0x4 	  | 0x1      | Header Size                        |
    // 0x4 	  | 0x4      | Section Size                       | Size of this section, including the header.
    // 0x8 	  | 0x4      | Number of Files                    | The number of files in the archive.
    // DATA --------------------------------------------------| See below.
    // 0xC 	  | 0x4      | Start Offset                       | Is RELATIVE to the GMIF section start offset.
    // 0x10   | 0x4      | End Offset                         | Is RELATIVE to the GMIF section start offset.
    // -------------------------------------------------------
    // #2 Section - File Name Table (BTNF)
    // -------------------------------------------------------
    // Offset | Length   | Name                               | Description
    // 0x0 	  | 0x4      | Magic ID 	                      | #BTNF (0x42544E46)
    // 0x4 	  | 0x4      | Section Size                       | Size of this section, including the header.
    // DATA Directory Table ----------------------------------|
    //        | 0x4      | Directory Start Offset             | All other offsets are RELATIVE to the first directory start offset which is the root folder.
    //        | 0x2      | First File Position                |
    //        | 0x2      | Directory Count / Parent Directory | For the root folder it has the total number of directories in the archive, and the latter for everything else.
    // DATA Name Table ---------------------------------------| 
    //        | 0x1      | Size of Name                       | The first bit specifies whether it is a file or a folder.
    //        | 0xLENGTH | Name String                        |
    // EXTRA 	0x2 	 | Directory ID                       | The number reference of that directory.
    // -------------------------------------------------------
    // #3 Section - File Image (GMIF)
    // -------------------------------------------------------
    // Offset | Length   | Name                               | Description
    // 0x0    | 0x4 	 | Magic ID 	                      | #GMIF (0x474D4946)
    // 0x4 	  | 0x4 	 | Section Size                       | Size of this section, including the header.
    // 0x8 	  | 0x4 	 | Compression ID 	                  | #LZ77 (0x4C5A3737)

    // Code adapted from NARCArchive.java from UPR
    public class NARCArchiveData : DSFile
    {
        private const string fatbIdentifier = "FATB";
        private static readonly byte[] fatbIdHex = new byte[]{ 0x42,0x54,0x41,0x46 };
        private const string fimgIdentifier = "FIMG";
        private static readonly byte[] fimgIdHex = new byte[] { 0x47, 0x4D, 0x49, 0x46 };
        private const string fntbIdentifier = "FNTB";
        private static readonly byte[] fntbIdHex = new byte[] { 0x42, 0x54, 0x4E, 0x46 };
        private const int fntbFilenameDataStartOffset = 0x08;
        private const int sectionHeaderSize = 8;
        public int FileCount => files.Count;
        private readonly List<string> fileNames;
        private readonly List<(int offset, int length)> files;
        public int FileId { get; }
        public int OriginalOffset { get; }
        public NARCArchiveData(Rom rom, int offset, int length, int fileId)
        {
            FileId = fileId;
            OriginalOffset = offset;
            int fatbOffset = Rom.nullPointer;
            int fimgOffset = Rom.nullPointer;
            int fntbOffset = Rom.nullPointer;
            int fatbSize = 0;
            int fimgSize = 0;
            int fntbSize = 0;

            // Read the number of frames from generic header
            int sectionCount = rom.ReadUInt16(offset + headerNumberOfSectionsOffset);
            // Read frame data
            rom.Seek(offset + headerSize);
            for (int i = 0; i < sectionCount; ++i)
            {
                var identifierBytes = rom.ReadBlock(4);
                Array.Reverse(identifierBytes);
                string sectionIdentifier = Encoding.ASCII.GetString(identifierBytes);
                int sectionSize = rom.ReadUInt32() - sectionHeaderSize;
                if(sectionIdentifier == fatbIdentifier)
                {
                    fatbOffset = rom.InternalOffset;
                    fatbSize = sectionSize;
                }
                else if(sectionIdentifier == fimgIdentifier)
                {
                    fimgOffset = rom.InternalOffset;
                    fimgSize = sectionSize;
                }
                else if(sectionIdentifier == fntbIdentifier)
                {
                    fntbOffset = rom.InternalOffset;
                    fntbSize = sectionSize;
                }
                rom.Skip(sectionSize);
            }
            if(fatbOffset == Rom.nullPointer || fimgOffset == Rom.nullPointer || fntbOffset == Rom.nullPointer)
            {
                throw new ArgumentException($"No valid NARC file located at {offset:x2}. FATB, FIMG, or FNTB section not found");
            }

            // Read file offsets and sizes from FATB section. Actual contents are located in FIMG section. Offsets are relative to FIMG section start
            rom.Seek(fatbOffset);
            int fileCount = rom.ReadUInt32();
            files = new List<(int offset, int length)>(fileCount);
            for (int i = 0; i < fileCount; i++)
            {
                int startOffset = rom.ReadUInt32();
                int endOffset = rom.ReadUInt32();
                files.Add((fimgOffset + startOffset, endOffset - startOffset));
            }

            // Read the filenames from the FNTB frame, if they exist
            rom.Seek(fntbOffset);
            int fileNameIndicator = rom.ReadUInt32();
            // Filenames  (not 100% sure this is correct)
            if (fileNameIndicator == fntbFilenameDataStartOffset)
            {
                fileNames = new List<string>(fileCount);
                rom.Skip(4);
                for (int i = 0; i < fileCount; i++)
                {
                    byte nameLength = rom.ReadByte();
                    fileNames.Add(Encoding.ASCII.GetString(rom.ReadBlock(nameLength)));
                }
            }
            else
            {
                fileNames = new List<string>();
            }
        }

        public Rom WriteToFile(Rom originalRom, Rom overrideFile, int overrideFileId)
        {
            // TODO: something better than this lol
            var overrides = new Rom[overrideFileId + 1];
            overrides[overrideFileId] = overrideFile;
            return WriteToFile(originalRom, overrides);
        }

        public Rom WriteToFile(Rom originalRom, IReadOnlyList<Rom> fileOverrides)
        {
            int newFileCount = Math.Max(FileCount, fileOverrides.Count);
            // Calculate FATB size
            int fatbSize = sectionHeaderSize + 4 + (newFileCount * 8); // fileCount + fileHeader * file count
            int fntSize = sectionHeaderSize + fntbFilenameDataStartOffset;
            //foreach(var fileName in fileNames) // Don't actually know how to write filenames yet
            //{
            //    fntSize += (1 + fileName.Length);
            //}
            // Calculate FIMG size
            int fimgSize = sectionHeaderSize;
            for (int i = 0; i < newFileCount; i++)
            {
                if(i < fileOverrides.Count && fileOverrides[i] != null)
                {
                    fimgSize += fileOverrides[i].Length;
                }
                else if(i < files.Count)
                {
                    fimgSize += files[i].length;
                }
            }
            int totalSize = headerSize + fatbSize + fntSize + fimgSize;
            var file = new Rom(totalSize);

            // Wrtie Nitro header

            // Copy original header
            file.Copy(originalRom, OriginalOffset, headerSize);
            // Write total size to header
            file.WriteUInt32(headerSectionSizeOffset, totalSize);

            // Write FATB

            file.WriteBlock(fatbIdHex);
            file.WriteUInt32(fatbSize);
            file.WriteUInt32(newFileCount);
            int fatbDataOffset = file.InternalOffset;
            file.Skip(newFileCount * 8); // Write actual offsets later when writing FIMG

            // Write FNTB

            file.WriteBlock(fntbIdHex);
            file.WriteUInt32(fntSize);
            // Not sure how to write filenames yet, just write empty header
            file.WriteUInt32(0x04); 
            file.WriteUInt16(0x00);
            file.WriteUInt16(0x01);

            // Write FIMG
            file.WriteBlock(fimgIdHex);
            file.WriteUInt32(fimgSize);

            int fimgStartOffset = file.InternalOffset;
            // Write actual file information
            for (int i = 0; i < newFileCount; i++)
            {
                int startOffset = file.InternalOffset - fimgStartOffset;

                // Write file contents and record size
                int fileSize;
                if (i < fileOverrides.Count && fileOverrides[i] != null)
                {
                    var fileOverride = fileOverrides[i];
                    fileSize = fileOverride.Length;
                    file.Copy(fileOverride);
                }
                else if (i < files.Count)
                {
                    fileSize = files[i].length;
                    file.Copy(originalRom, files[i].offset, fileSize);
                }
                else
                {
                    continue;
                }
                
                // Write FATB entry
                file.SaveAndSeekOffset(fatbDataOffset + (i * 8));
                file.WriteUInt32(startOffset);
                file.WriteUInt32(startOffset + fileSize);
                file.LoadOffset();
            }
            return file;
        }

        public bool GetFile(int fileIndex, out int offset, out int length, out string name)
        {
            name = fileIndex < fileNames.Count ? fileNames[fileIndex] : string.Empty;
            if(fileIndex >= files.Count)
            {
                offset = Rom.nullPointer;
                length = 0;
                return false;
            }
            var fileData = files[fileIndex];
            offset = fileData.offset;
            length = fileData.length;
            return true;
        }

        public bool SeekFile(Rom rom, int fileIndex)
        {
            if(GetFile(fileIndex, out int offset, out _, out _))
            {
                rom.Seek(offset);
                return true;
            }
            return false;
        }
    }
}
