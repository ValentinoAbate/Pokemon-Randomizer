namespace PokemonRandomizer.UI.Models
{
    using Utilities;
    public class MiscDataModel : DataModel
    {
        public Box<bool> UpdateDOTMoves { get; set; } = new Box<bool>(false);
        public Box<bool> RunIndoors { get; set; } = new Box<bool>(true);
        public Box<bool> EvolveWithoutNationalDex { get; set; } = new Box<bool>(true);
        public Box<bool> CountRelicanthAsFossil { get; set; } = new Box<bool>(true);
        // RSE Only
        public Box<bool> RandomizeWallyAce { get; set; } = new Box<bool>(true);
        public Box<bool> EasyFirstRivalbattle { get; set; } = new Box<bool>(true);
    }
}
