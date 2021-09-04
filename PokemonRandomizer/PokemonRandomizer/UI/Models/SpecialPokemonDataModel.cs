namespace PokemonRandomizer.UI.Models
{
    public class SpecialPokemonDataModel : DataModel
    {
        public StartersDataModel StarterData { get; set; }
        public InGameTradesDataModel TradeData { get; set; }
        public GiftPokemonDataModel GiftData { get; set; }

        public DreamTeamDataModel DreamTeamData { get; set; }

        public SpecialPokemonDataModel()
        {

        }

        public SpecialPokemonDataModel(StartersDataModel starterData, InGameTradesDataModel tradesData, GiftPokemonDataModel giftData, DreamTeamDataModel dreamTeamData)
        {
            TradeData = tradesData;
            StarterData = starterData;
            GiftData = giftData;
            DreamTeamData = dreamTeamData;
        }
    }
}
