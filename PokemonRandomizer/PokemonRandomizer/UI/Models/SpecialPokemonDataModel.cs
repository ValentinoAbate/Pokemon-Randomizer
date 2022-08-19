namespace PokemonRandomizer.UI.Models
{
    public class SpecialPokemonDataModel : DataModel
    {
        public StartersDataModel StarterData { get; set; } = new StartersDataModel();
        public InGameTradesDataModel TradeData { get; set; } = new InGameTradesDataModel();
        public GiftPokemonDataModel GiftData { get; set; } = new GiftPokemonDataModel();
        public StaticPokemonDataModel StaticPokemonData { get; set; } = new StaticPokemonDataModel();
        public DreamTeamDataModel DreamTeamData { get; set; } = new DreamTeamDataModel();
    }
}
