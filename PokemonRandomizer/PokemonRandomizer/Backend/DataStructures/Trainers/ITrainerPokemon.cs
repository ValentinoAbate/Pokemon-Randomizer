using PokemonRandomizer.Backend.EnumTypes;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.DataStructures.Trainers
{
    public interface ITrainerPokemon
    {
        public bool HasSpecialMoves { get; }
        public IReadOnlyList<Move> Moves { get; }
        public Pokemon Species { get; } 
    }
}
