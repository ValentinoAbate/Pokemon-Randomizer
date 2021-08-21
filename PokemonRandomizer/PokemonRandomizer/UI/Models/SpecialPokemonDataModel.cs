namespace PokemonRandomizer.UI.Models
{
    public class SpecialPokemonDataModel : DataModel
    {
        public StartersDataModel StarterData { get; }
        public InGameTradesDataModel TradeData { get; }

        public SpecialPokemonDataModel(StartersDataModel starterData, InGameTradesDataModel tradesData)
        {
            TradeData = tradesData;
            StarterData = starterData;
        }
    }
}
