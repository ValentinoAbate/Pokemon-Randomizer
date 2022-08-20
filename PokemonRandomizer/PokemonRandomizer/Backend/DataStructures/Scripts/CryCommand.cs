using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;

namespace PokemonRandomizer.Backend.DataStructures.Scripts
{
    public class CryCommand : Command, IHasCommandInputType
    {
        public CommandInputType InputType { get; set; }
        public Pokemon Pokemon { get; set; }
        public int effect;

        public override string ToString()
        {
            return $"Play {Pokemon.ToDisplayString()}'s cry (effect {effect:x2})";
        }
    }
}
