using PokemonRandomizer.Backend.EnumTypes;
using System;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.DataStructures
{
    public class BattleTent
    {
        public string Name { get; }
        public List<Item> Rewards { get; } = new List<Item>();
        public int OriginalRewardsCount { get; private set; }
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
