namespace PokemonRandomizer.Backend.DataStructures.Scripts
{
    using EnumTypes;
    using Utilities;
    public class GiveEggCommand : Command
    {
        public Pokemon pokemon;
        public override string ToString()
        {
            return "give " + pokemon.ToDisplayString() + " egg";
        }
    }
}
