using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.DataStructures.DS;
using PokemonRandomizer.Backend.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.Reading
{
    public abstract class DSRomParser : RomParser
    {
        private const int fntOffsetOffset = 0x40;
        protected DSRom ParseNDSFile(Rom rom, RomMetadata metadata, XmlManager info)
        {
            rom.Seek(fntOffsetOffset);
            int fntOffset = rom.ReadUInt32();
            int fntSize = rom.ReadUInt32();
            int fatOffset = rom.ReadUInt32();
            int fatSize = rom.ReadUInt32();

            // Read FAT table data
            rom.Seek(fatOffset);
            var fat = rom.ReadBlock(fatSize);
            return null;
        }
    }
}
