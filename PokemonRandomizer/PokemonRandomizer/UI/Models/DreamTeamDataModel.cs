using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.UI.Utilities;
using static PokemonRandomizer.Settings;

namespace PokemonRandomizer.UI.Models
{
    public class DreamTeamDataModel : DataModel
    {
        public Box<DreamTeamSetting> DreamTeamOption { get; set; } = new Box<DreamTeamSetting>(DreamTeamSetting.None);

        public Pokemon[] CustomDreamTeam { get; set; } = new Pokemon[]
        {
            Pokemon.None,
            Pokemon.None,
            Pokemon.None,
            Pokemon.None,
            Pokemon.None,
            Pokemon.None,
        };

        public Box<bool> BanLegendaries { get; set; } = new Box<bool>(true);
        public Box<bool> BanIllegalEvolutions { get; set; } = new Box<bool>(true);
        public Box<DreamTeamBstTotalOption> UseTotalBST { get; set; } = new Box<DreamTeamBstTotalOption>(DreamTeamBstTotalOption.None);
        public Box<double> BstTotalUpperBound { get; set; } = new Box<double>(2200);
        public Box<double> BstTotalLowerBound { get; set; } = new Box<double>(2500);
        public Box<bool> UseTypeFilter { get; set; } = new Box<bool>(false);
        public PokemonType[] TypeFilter { get; set; } = new PokemonType[0];
    }
}
