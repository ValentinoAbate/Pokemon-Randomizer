using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.Utilities;

namespace PokemonRandomizer.Backend.Writing
{
    public abstract class RomWriter
    {
        public abstract Rom Write(RomData data, Rom originalRom, RomMetadata metadata, XmlManager info);
    }
}
