using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.DataStructures.Trainers;
using PokemonRandomizer.UI.Utilities;
using static PokemonRandomizer.Settings;

namespace PokemonRandomizer.UI.Models
{
    public class TrainerOrganizationDataModel : DataModel
    {
        public Box<TrainerOrgTypeTheme> GymTypeTheming { get; set; } = new Box<TrainerOrgTypeTheme>(TrainerOrgTypeTheme.Default);
        public Box<TrainerOrgTypeTheme> EliteFourTheming { get; set; } = new Box<TrainerOrgTypeTheme>(TrainerOrgTypeTheme.Default);
        public Box<TrainerOrgTypeTheme> ChampionTheming { get; set; } = new Box<TrainerOrgTypeTheme>(TrainerOrgTypeTheme.Default);
        public Box<GymEliteFourPreventDupesSetting> GymAndEliteDupePrevention { get; set; } = new Box<GymEliteFourPreventDupesSetting>(GymEliteFourPreventDupesSetting.RandomizedOnly);
        public Box<TrainerOrgTypeTheme> TeamTypeTheming { get; set; } = new Box<TrainerOrgTypeTheme>(TrainerOrgTypeTheme.Default);
        public Box<bool> GruntTheming { get; set; } = new Box<bool>(true);
        public Box<bool> KeepTeamSubtypes { get; set; } = new Box<bool>(true);
        public Box<Trainer.Category> PriorityCategory { get; set; } = new Box<Trainer.Category>(Trainer.Category.GymLeader);
        public Box<TrainerOrgTypeTheme> SmallOrgTypeTheming { get; set; } = new Box<TrainerOrgTypeTheme>(TrainerOrgTypeTheme.Default);
    }
}
