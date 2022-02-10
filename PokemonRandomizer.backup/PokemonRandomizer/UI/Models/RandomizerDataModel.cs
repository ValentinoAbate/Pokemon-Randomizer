namespace PokemonRandomizer.UI.Models
{
    using Utilities;
    public class RandomizerDataModel : DataModel
    {
        public Box<bool> UseSeed { get; set; } = new Box<bool>();
        public Box<string> Seed { get; set; } = new Box<string>("Enter a seed");
    }
}
