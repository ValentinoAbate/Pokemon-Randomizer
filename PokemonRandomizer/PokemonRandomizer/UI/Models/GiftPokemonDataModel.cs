using static PokemonRandomizer.Settings;

namespace PokemonRandomizer.UI.Models
{
    using Utilities;
    public class GiftPokemonDataModel : DataModel
    {
        public Box<double> GiftPokemonRandChance { get; } = new Box<double>(1);
        public Box<bool> RandomizeGiftPokemon { get; } = new Box<bool>();
        public PokemonSettings GiftSpeciesSettings { get; } = new PokemonSettings()
        {
            BanLegendaries = true,
        };
        public Box<bool> EnsureFossilRevivesAreFossilPokemon { get; } = new Box<bool>(true);
        public Box<bool> EnsureGiftEggsAreBabyPokemon { get; } = new Box<bool>(true);
    }
}
