namespace PokemonRandomizer.Backend.DataStructures.Scripts
{
    using EnumTypes;
    using Utilities;
    public class GivePokemonCommand : Command
    {
        public enum Type
        {
            Normal, // The give item event gives the item stored in item
            Variable, // The command gives an item stored in the variable (int)item
            Unknown // Unknown, happens in lilycove city once, can research more later
        }

        public Type type = Type.Normal;
        public Pokemon pokemon;
        public byte level;
        public Item heldItem;

        public override string ToString()
        {
            return $"Give {pokemon.ToDisplayString()} lvl {level} w/ {heldItem.ToDisplayString()}";
        }
    }
}
