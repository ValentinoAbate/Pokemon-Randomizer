using PokemonRandomizer.UI.Utilities;
using static PokemonRandomizer.Settings;

namespace PokemonRandomizer.UI.Models
{
    public class TrainerOrganizationDataModel : DataModel
    {
        public Box<TrainerOrgTypeTheme> GymTypeTheming { get; set; } = new Box<TrainerOrgTypeTheme>(TrainerOrgTypeTheme.Default);
        public Box<TrainerOrgTypeTheme> EliteFourTheming { get; set; } = new Box<TrainerOrgTypeTheme>(TrainerOrgTypeTheme.Default);
        public Box<TrainerOrgTypeTheme> ChampionTheming { get; set; } = new Box<TrainerOrgTypeTheme>(TrainerOrgTypeTheme.Default);
        public Box<TrainerOrgTypeTheme> TeamTypeTheming { get; set; } = new Box<TrainerOrgTypeTheme>(TrainerOrgTypeTheme.Default);
        public Box<bool> GruntTheming { get; set; } = new Box<bool>(true);
        public Box<bool> KeepTeamSubtypes { get; set; } = new Box<bool>(true);
        public Box<TrainerOrgTypeTheme> SmallOrgTypeTheming { get; set; } = new Box<TrainerOrgTypeTheme>(TrainerOrgTypeTheme.Default);
    }
}
