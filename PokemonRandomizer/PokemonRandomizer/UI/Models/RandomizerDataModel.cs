namespace PokemonRandomizer.UI.Models
{
    using Utilities;
    public class RandomizerDataModel : DataModel
    {
        public Box<bool> UseSeed = new Box<bool>();
        public Box<string> Seed = new Box<string>("Enter a seed");
    }
}
