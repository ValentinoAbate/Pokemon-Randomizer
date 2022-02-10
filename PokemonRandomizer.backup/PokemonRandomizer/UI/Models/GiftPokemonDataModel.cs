using static PokemonRandomizer.Settings;

namespace PokemonRandomizer.UI.Models
{
    using Utilities;
    public class GiftPokemonDataModel : DataModel
    {
        public Box<double> GiftPokemonRandChance { get; set; } = new Box<double>(1);
        public Box<bool> RandomizeGiftPokemon { get; set; } = new Box<bool>();
        public PokemonSettings GiftSpeciesSettings { get; set; } = new PokemonSettings()
        {
            BanLegendaries = true,
        };
        public Box<bool> EnsureFossilRevivesAreFossilPokemon { get; set; } = new Box<bool>(true);
        public Box<bool> EnsureGiftEggsAreBabyPokemon { get; set; } = new Box<bool>(true);
    }
}
