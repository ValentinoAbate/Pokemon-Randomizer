namespace PokemonRandomizer.UI.Models
{
    public class SpecialPokemonDataModel : DataModel
    {
        public StartersDataModel StarterData { get; set; }
        public InGameTradesDataModel TradeData { get; set; }
        public GiftPokemonDataModel GiftData { get; set; }

        public SpecialPokemonDataModel()
        {

        }

        public SpecialPokemonDataModel(StartersDataModel starterData, InGameTradesDataModel tradesData, GiftPokemonDataModel giftData)
        {
            TradeData = tradesData;
            StarterData = starterData;
            GiftData = giftData;
        }
    }
}
