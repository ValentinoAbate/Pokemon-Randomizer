using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.RomHandling.IndexTranslators;
using PokemonRandomizer.Backend.Utilities;
using System;

namespace PokemonRandomizer.Backend.RomHandling.Writing
{
    public class Gen4RomWriter : DSRomWriter
    {
        protected override IIndexTranslator IndexTranslator => Gen4IndexTranslator.Main;
        public override Rom Write(RomData data, Rom originalRom, RomMetadata metadata, XmlManager info, Settings settings)
        {
            throw new NotImplementedException("Gen IV Rom writing not supported");
        }
    }
}
