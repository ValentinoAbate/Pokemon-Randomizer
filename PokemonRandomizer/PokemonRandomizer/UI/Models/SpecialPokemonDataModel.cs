namespace PokemonRandomizer.UI.Models
{
    public class SpecialPokemonDataModel : DataModel
    {
        public StartersDataModel StarterData { get; }
        public InGameTradesDataModel TradeData { get; }
        public GiftPokemonDataModel GiftData { get; }

        public SpecialPokemonDataModel(StartersDataModel starterData, InGameTradesDataModel tradesData, GiftPokemonDataModel giftData)
        {
            TradeData = tradesData;
            StarterData = starterData;
            GiftData = giftData;
        }
    }
}
