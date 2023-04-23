using PokemonRandomizer.Backend.EnumTypes;
using System;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.DataStructures.Trainers
{
    public class FrontierBrainTrainerPokemon : TrainerPokemon, IHasTrainerPokemonNature, IHasFrontierTrainerEvs
    {
        public Nature Nature { get; set; }
        public IHasFrontierTrainerEvs.EvFlags Evs { get; set; }
    }
}
