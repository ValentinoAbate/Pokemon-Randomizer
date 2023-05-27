﻿using PokemonRandomizer.Backend.EnumTypes;
using System;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.DataStructures.Trainers
{
    public class FrontierBrainTrainerPokemon : TrainerPokemon, IHasTrainerPokemonNature, IHasTrainerPokemonEvs
    {
        public override int MaxIV => PokemonBaseStats.maxIV;
        public Nature Nature { get; set; }
        public byte[] EVs { get; set; }
    }
}
