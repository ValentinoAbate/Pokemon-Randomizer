using PokemonRandomizer.UI.Utilities;
using static PokemonRandomizer.Settings;

namespace PokemonRandomizer.UI.Models
{
    public class StaticPokemonDataModel : DataModel
    {
        public Box<double> StaticEncounterRandChance { get; set; } = new Box<double>(1);
        public Box<bool> RandomizeStaticEncounters { get; set; } = new Box<bool>();
        public PokemonSettings StaticEncountersSettings { get; set; } = new PokemonSettings()
        {
            BanLegendaries = true,
        };
        public Box<LegendaryRandSetting> LegendarySetting { get; set; } = new Box<LegendaryRandSetting>(LegendaryRandSetting.RandomizeEnsureLegendary);

        public Box<bool> PreventDuplicates { get; set; } = new Box<bool>(true);
        public Box<bool> Remap { get; set; } = new Box<bool>(true);
    }
}
