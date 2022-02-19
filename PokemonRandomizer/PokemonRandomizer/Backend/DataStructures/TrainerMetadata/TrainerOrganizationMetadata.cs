using PokemonRandomizer.Backend.EnumTypes;

namespace PokemonRandomizer.Backend.DataStructures.TrainerMetadata
{
    public abstract class TrainerOrganizationMetadata
    {
        public bool Untyped => Types.Length <= 0;
        public abstract PokemonType[] Types { get; protected set; }
    }
}
