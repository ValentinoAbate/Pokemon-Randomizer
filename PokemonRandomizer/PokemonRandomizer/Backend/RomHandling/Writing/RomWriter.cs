using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;

namespace PokemonRandomizer.Backend.RomHandling.Writing
{
    public abstract class RomWriter
    {
        public abstract Rom Write(RomData data, Rom originalRom, RomMetadata metadata, XmlManager info, Settings settings);

        protected abstract int ItemToInternalIndex(Item item);
    }
}
