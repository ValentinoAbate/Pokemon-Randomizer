﻿using System;
using PokemonRandomizer.Backend.EnumTypes;

namespace PokemonRandomizer.Backend.DataStructures.Trainers
{
    public class StevenAllyTrainerPokemon : TrainerPokemon, IHasTrainerPokemonNature, IHasTrainerPokemonEvs
    {
        public override int MaxIV => PokemonBaseStats.maxIV;
        public Nature Nature { get; set; }
        public byte[] EVs { get; set; }

        public override TrainerPokemon Clone()
        {
            var other = new StevenAllyTrainerPokemon();
            other.CopyBasicValuesFrom(this);
            other.Nature = Nature;
            other.EVs = new byte[EVs.Length];
            Array.Copy(EVs, other.EVs, EVs.Length);
            return other;
        }
    }
}
