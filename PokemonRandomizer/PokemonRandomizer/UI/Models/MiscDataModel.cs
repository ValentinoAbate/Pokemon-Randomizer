namespace PokemonRandomizer.UI.Models
{
    using Utilities;
    public class MiscDataModel : DataModel
    {
        public Box<bool> RunIndoors { get; set; } = new Box<bool>(true);
        public Box<bool> EvolveWithoutNationalDex { get; set; } = new Box<bool>(true);
        public Box<bool> CountRelicanthAsFossil { get; set; } = new Box<bool>(true);
    }
}
