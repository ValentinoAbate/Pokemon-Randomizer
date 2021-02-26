namespace PokemonRandomizer.Backend.DataStructures.Scripts
{
    using EnumTypes;
    using Utilities;
    public class GivePokemonCommand : Command
    {
        public PokemonSpecies pokemon;
        public byte level;
        public Item heldItem;

        public override string ToString()
        {
            return "give " + pokemon.ToDisplayString() + " lvl " + level + " w/ " + heldItem.ToDisplayString();
        }
    }
}
