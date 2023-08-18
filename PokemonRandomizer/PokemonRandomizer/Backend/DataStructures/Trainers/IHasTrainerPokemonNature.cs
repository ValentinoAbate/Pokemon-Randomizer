using PokemonRandomizer.Backend.EnumTypes;

namespace PokemonRandomizer.Backend.DataStructures.Trainers
{
    public interface IHasTrainerPokemonNature : ITrainerPokemon
    {
        public Nature Nature { get; set; }
    }
}
