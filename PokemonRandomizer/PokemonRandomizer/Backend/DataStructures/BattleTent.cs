﻿using PokemonRandomizer.Backend.EnumTypes;
using System;
using System.Collections.Generic;
using PokemonRandomizer.Backend.DataStructures.Trainers;

namespace PokemonRandomizer.Backend.DataStructures
{
    public class BattleTent
    {
        public string Name { get; }
        public List<Item> Rewards { get; } = new List<Item>();
        public int OriginalRewardsCount { get; private set; }
        public List<FrontierTrainerPokemon> Pokemon { get; } = new List<FrontierTrainerPokemon>();
        public BattleTent(string name)
        {
            Name = name;
        }

        public void SetOriginalValues()
        {
            OriginalRewardsCount = Rewards.Count;
        }

        public override string ToString()
        {
            return $"Battle Tent: {Name}";
        }
    }
}
