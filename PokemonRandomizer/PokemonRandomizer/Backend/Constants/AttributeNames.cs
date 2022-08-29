namespace PokemonRandomizer.Backend.Constants
{
    /// <summary>
    /// Contains symbolic constants for referencing the names of attributes in a ROM info file.
    /// </summary>
    public abstract class AttributeNames
    {
        // Common
        public const string elementNames = "elementNames";
        public const string names = "names";
        public const string name = "name";
        public const string className = "className";
        public const string types = "types";
        // Pokemon Attributes
        public const string evolutionsPerPokemon = "evolutionsPerPokemon";
        public const string eggMovePokemonSigniture = "pokemonSigniture";
        // Map attribures
        public const string banks = "banks";
        public const string maps = "maps";
    }
}
