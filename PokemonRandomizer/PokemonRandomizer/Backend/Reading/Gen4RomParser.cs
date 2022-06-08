using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.GenIII.Constants.ElementNames;
using PokemonRandomizer.Backend.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.Backend.Reading
{
    public class Gen4RomParser : DSRomParser
    {
        public override RomData Parse(Rom rom, RomMetadata metadata, XmlManager info)
        {
            // Parse the NDS file structure
            var dsFileSystem = ParseNDSFile(rom, metadata, info);
            // Actually parse the ROM data
            RomData data = new RomData();


            throw new NotImplementedException("Gen IV Rom parsing not supported");
        }
    }
}
