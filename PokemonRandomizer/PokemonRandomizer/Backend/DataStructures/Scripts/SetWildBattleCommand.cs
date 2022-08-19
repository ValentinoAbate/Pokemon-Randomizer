using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;
using System;
namespace PokemonRandomizer.Backend.DataStructures.Scripts
{
    public class SetWildBattleCommand : Command
    {
        public Pokemon Pokemon { get; set; }
        public byte Level { get; set; }
        public Item HeldItem { get; set; }

        public override string ToString()
        {
            return $"Set wild battle to {Pokemon.ToDisplayString()} lvl {Level} w/ {HeldItem.ToDisplayString()}";
        }
    }
}
