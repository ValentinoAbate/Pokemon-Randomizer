using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.Utilities;

namespace PokemonRandomizer.Backend.Reading
{
    public abstract class RomParser
    {
        public abstract RomData Parse(Rom rom, XmlManager info);
    }
}
