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

        public Box<bool> PrioritizeVariants { get; set; } = new Box<bool>(false);
        public Box<bool> BanLegendaries { get; set; } = new Box<bool>(true);
        public Box<bool> BanIllegalEvolutions { get; set; } = new Box<bool>(true);
        public Box<DreamTeamBstTotalOption> UseTotalBST { get; set; } = new Box<DreamTeamBstTotalOption>(DreamTeamBstTotalOption.None);
        public Box<double> BstTotalUpperBound { get; set; } = new Box<double>(2200);
        public Box<double> BstTotalLowerBound { get; set; } = new Box<double>(2500);
        public Box<double> BstIndividualUpperBound { get; set; } = new Box<double>(475);
        public Box<double> BstIndividualLowerBound { get; set; } = new Box<double>(500);
        public Box<bool> UseTypeFilter { get; set; } = new Box<bool>(false);
        public Box<PokemonType> AllowedType1 { get; set; } = new Box<PokemonType>((PokemonType)(19));
        public Box<PokemonType> AllowedType2 { get; set; } = new Box<PokemonType>((PokemonType)(19));
        public Box<PokemonType> AllowedType3 { get; set; } = new Box<PokemonType>((PokemonType)(19));
    }
}
