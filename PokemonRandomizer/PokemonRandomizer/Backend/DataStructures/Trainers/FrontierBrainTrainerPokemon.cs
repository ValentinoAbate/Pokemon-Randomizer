using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;
using System;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.DataStructures.Trainers
{
    public class FrontierBrainTrainerPokemon : TrainerPokemon, IHasTrainerPokemonNature, IHasTrainerPokemonEvs
    {
        public override int MaxIV => PokemonBaseStats.maxIV;
        public Nature Nature { get; set; }
        public byte[] EVs { get; set; }

        public override string ToString()
        {
            return $"{species.ToDisplayString()} ({Nature})";
        }

        public override TrainerPokemon Clone()
        {
            var other = new FrontierBrainTrainerPokemon();
            other.CopyBasicValuesFrom(this);
            other.Nature = Nature;
            other.EVs = new byte[EVs.Length];
            Array.Copy(EVs, other.EVs, EVs.Length);
            return other;
        }
    }
}
