using System;
using System.Collections;

namespace PokemonRandomizer.Backend.DataStructures.Trainers
{
    public interface IHasTrainerAI
    {
        public BitArray AIFlags { get; set; }
    }
}
