using PokemonRandomizer.UI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.UI.Models
{
    public class TrainerOrganizationDataModel : DataModel
    {
        public enum TypeTheme
        {
            Default,
            Off,
            On,
            Random,
            RandomNoDupes,
        }

        public Box<TypeTheme> GymTypeTheming { get; set; } = new Box<TypeTheme>(TypeTheme.Default);
        public Box<bool> GymTrainerTheming { get; set; } = new Box<bool>(true);
        public Box<TypeTheme> EliteFourTheming { get; set; } = new Box<TypeTheme>(TypeTheme.Default);
        public Box<TypeTheme> ChampionTheming { get; set; } = new Box<TypeTheme>(TypeTheme.Default);
        public Box<TypeTheme> TeamTypeTheming { get; set; } = new Box<TypeTheme>(TypeTheme.Default);
        public Box<bool> GruntTheming { get; set; } = new Box<bool>(true);
        public Box<bool> KeepTeamSubtypes { get; set; } = new Box<bool>(true);
        public Box<TypeTheme> SmallOrgTypeTheming { get; set; } = new Box<TypeTheme>(TypeTheme.Default);
    }
}
