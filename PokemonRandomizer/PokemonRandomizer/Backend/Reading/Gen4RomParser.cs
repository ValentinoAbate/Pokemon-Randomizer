using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.DataStructures.DS;
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
            var dsFileSystem = ParseNDSFileSystemData(rom);
            var fileData = dsFileSystem.GetFile("poketool/personal/pl_personal.narc", out int baseStatsOffset, out int baseStatsLength);
            var pokemonNARC = new NARCArchiveData(rom, baseStatsOffset, baseStatsLength);
            // Actually parse the ROM data
            RomData data = new RomData();


            throw new NotImplementedException("Gen IV Rom parsing not supported");
        }
    }
}
