namespace PokemonRandomizer.Backend.DataStructures.Scripts
{
    public class GivePokedexCommand : Command
    {
        public enum PokedexType
        {
            Regional,
            National,
        }
        public PokedexType Type { get; set; }

        public override string ToString()
        {
            return $"give {Type} pokedex";
        }
    }
}
