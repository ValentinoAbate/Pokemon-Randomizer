using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.DataStructures.DS;
using PokemonRandomizer.Backend.RomHandling.IndexTranslators;
using PokemonRandomizer.Backend.Utilities;

namespace PokemonRandomizer.Backend.RomHandling.Writing
{
    public class Gen4RomWriter : DSRomWriter
    {
        private const int headerSizeOffset = 0x84;
        protected override IIndexTranslator IndexTranslator => Gen4IndexTranslator.Main;

        public override Rom Write(RomData data, Rom originalRom, RomMetadata metadata, XmlManager info, Settings settings)
        {
            var rom = new Rom(originalRom.Length, 0xFF); // Set to all 0xFF?
            return rom;
        }
    }
}
