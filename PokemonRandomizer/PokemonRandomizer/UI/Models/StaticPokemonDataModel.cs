using PokemonRandomizer.UI.Utilities;
using static PokemonRandomizer.Settings;

namespace PokemonRandomizer.UI.Models
{
    public class StaticPokemonDataModel : DataModel
    {
        public Box<double> StaticRandChance { get; set; } = new Box<double>(1);
        public Box<bool> RandomizeStatics { get; set; } = new Box<bool>();
        public PokemonSettings Settings { get; set; } = new PokemonSettings()
        {
            BanLegendaries = true,
        };
        public Box<LegendaryRandSetting> LegendarySetting { get; set; } = new Box<LegendaryRandSetting>(LegendaryRandSetting.RandomizeEnsureLegendary);

        public Box<bool> PreventDupes { get; set; } = new Box<bool>(true);
        public Box<bool> Remap { get; set; } = new Box<bool>(true);
    }
}
