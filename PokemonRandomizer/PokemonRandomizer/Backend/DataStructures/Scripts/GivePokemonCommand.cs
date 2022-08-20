namespace PokemonRandomizer.Backend.DataStructures.Scripts
{
    using EnumTypes;
    using Utilities;
    public class GivePokemonCommand : Command, IHasCommandInputType
    {
        public Pokemon pokemon;
        public byte level;
        public Item heldItem;

        public CommandInputType InputType { get; set; }

        public override string ToString()
        {
            return $"Give {pokemon.ToDisplayString()} lvl {level} w/ {heldItem.ToDisplayString()}";
        }
    }
}
