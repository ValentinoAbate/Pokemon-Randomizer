using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.UI.Models;
using System.Windows.Controls;
using System.Windows.Data;
using static PokemonRandomizer.Settings;


namespace PokemonRandomizer.UI.Views
{
    public class TrainerOrganizationDataView : DataView<TrainerOrganizationDataModel>
    {
        private const string intelligentTypeThemeTooltip = "Use each trainer/organization's original type theme. See the tooltip of the \"Intelligent Type Theming\" setting in the Trainers tab for more details";
        private const string randomTypeThemeTooltip = "Choose a random type theme";
        private const string randomPreventDupesTypeThemeTooltip = "Choose a random type theme, preventing duplicate type-themes across all organizations with the setting (until all possible type themes have been used).";
        private static CompositeCollection TypeThemeDropdown => new CompositeCollection()
        {
            new ComboBoxItem() { Content="Default", ToolTip="Use the same setting as the \"Intelligent Type Theming\" setting in the Trainers tab" },
            new ComboBoxItem() { Content="Off" },
            new ComboBoxItem() { Content="Intelligent", ToolTip = intelligentTypeThemeTooltip },
            new ComboBoxItem() { Content="Random", ToolTip = randomTypeThemeTooltip },
            new ComboBoxItem() { Content="Random (Prevent Duplicates)", ToolTip = randomPreventDupesTypeThemeTooltip },
        };
        private static CompositeCollection ChampionTypeThemeDropdown => new CompositeCollection()
        {
            new ComboBoxItem() { Content="Same as Elite Four", ToolTip="Use the same setting as the Elite Four" },
            new ComboBoxItem() { Content="Off" },
            new ComboBoxItem() { Content="Intelligent", ToolTip = intelligentTypeThemeTooltip },
            new ComboBoxItem() { Content="Random" },
            new ComboBoxItem() { Content="Random (Prevent Duplicates)", ToolTip = randomPreventDupesTypeThemeTooltip },
        };

        private const string teamSubtypesTooltip = "When randomizing Team Type Themes, only randomize the primary Type Theme(s). The Type Themes are:" +
            "\nTeam Rocket: PSN (Primary), NRM | Team Aqua: WAT (Primary), PSN, DRK | Team Magma: GRD (Primary), FIR (Primary), PSN, DRK";
        private const string gruntThemeTooltip = "If checked, the team's theme will apply to grunts. If unchecked, team grunts will use default type theming";
        public TrainerOrganizationDataView(TrainerOrganizationDataModel model, RomMetadata metadata)
        {
            var stack = CreateMainStack();
            stack.Header("Gyms");
            stack.Add(new EnumComboBoxUI<TrainerOrgTypeTheme>("Type Theming", TypeThemeDropdown, model.GymTypeTheming));
            stack.Header("Elite Four + Champion");
            stack.Add(new EnumComboBoxUI<TrainerOrgTypeTheme>("Elite Four Type Theming", TypeThemeDropdown, model.EliteFourTheming));
            if (metadata.IsFireRedOrLeafGreen)
            {
                stack.Add(new Label() { Content = "Note: Champion type theming is disabled in FRLG because the champion is a Rival" });
            }
            else
            {
                stack.Add(new EnumComboBoxUI<TrainerOrgTypeTheme>("Champion Type Theming", ChampionTypeThemeDropdown, model.ChampionTheming));
            }
            stack.Header("Villainous Teams");
            stack.Add(new EnumComboBoxUI<TrainerOrgTypeTheme>("Type Theming", TypeThemeDropdown, model.TeamTypeTheming));
            stack.Add(new BoundCheckBoxUI(model.GruntTheming, "Apply Team Theme To Grunts") { ToolTip = gruntThemeTooltip });
            stack.Add(new BoundCheckBoxUI(model.KeepTeamSubtypes, "Keep Team Subtypes") { ToolTip = teamSubtypesTooltip });
            //stack.Header("Miscellaneous Organizations", "Miscellanous Organizations include: The Winstrates, Nugget Bridge, The Fighting Dojo, The Soda Pop House");
            //stack.Add(new EnumComboBoxUI<TrainerOrgTypeTheme>("Type Theming", TypeThemeDropdown, model.SmallOrgTypeTheming));
        }
    }
}
