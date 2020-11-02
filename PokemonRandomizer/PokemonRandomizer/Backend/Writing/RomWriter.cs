using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.Utilities;

namespace PokemonRandomizer.Backend.Writing
{
    public abstract class RomWriter
    {
        public abstract byte[] Write(RomData data, XmlManager info);
    }
}
