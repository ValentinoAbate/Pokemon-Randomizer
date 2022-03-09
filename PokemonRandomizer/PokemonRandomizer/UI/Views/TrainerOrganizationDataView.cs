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
        private static CompositeCollection TypeThemeDropdown => new CompositeCollection()
        {
            new ComboBoxItem() { Content="Default", ToolTip="Use the same setting as the \"Intelligent Type Theming\" setting in the Trainers tab" },
            new ComboBoxItem() { Content="Off" },
            new ComboBoxItem() { Content="Intelligent", ToolTip = intelligentTypeThemeTooltip },
            new ComboBoxItem() { Content="Random", ToolTip = randomTypeThemeTooltip },
        };
        private static CompositeCollection ChampionTypeThemeDropdown => new CompositeCollection()
        {
            new ComboBoxItem() { Content="Same as Elite Four", ToolTip="Use the same setting as the Elite Four" },
            new ComboBoxItem() { Content="Off" },
            new ComboBoxItem() { Content="Intelligent", ToolTip = intelligentTypeThemeTooltip },
            new ComboBoxItem() { Content="Random", ToolTip = randomTypeThemeTooltip },
        };

        private const string teamSubtypesTooltip = "When randomizing Team Type Themes, only randomize the primary Type Theme(s). The Type Themes are:" +
            "\nTeam Rocket: PSN (Primary), NRM | Team Aqua: WAT (Primary), PSN, DRK | Team Magma: GRD (Primary), FIR (Primary), PSN, DRK";
        private const string gruntThemeTooltip = "If checked, the team's theme will apply to grunts. If unchecked, team grunts will use default type theming";
        private const string preventDupesTooltip = "When checked, randomly chosen Gym or Elite Four member type themes will not be duplicates of any unrandomized or other randomized type themes";
        public TrainerOrganizationDataView(TrainerOrganizationDataModel model, RomMetadata metadata)
        {
            var stack = CreateMainStack();
            stack.Header("Trainer Organization Theming");
            stack.Description("These settings control how type theming is applied to different trainer organizations such as Gyms, the Elite Four, and Villainous Teams like Team Rocket!", "These settings only apply if the \"Randomize Pokemon\" setting in the \"Trainers\" tab is enabled.");
            stack.Header("Gyms and Elite Four");
            stack.Add(new EnumComboBoxUI<TrainerOrgTypeTheme>("Gym Type Theming", TypeThemeDropdown, model.GymTypeTheming));
            stack.Add(new EnumComboBoxUI<TrainerOrgTypeTheme>("Elite Four Type Theming", TypeThemeDropdown, model.EliteFourTheming));
            if (metadata.IsFireRedOrLeafGreen)
            {
                stack.Add(new Label() { Content = "Note: Champion type theming is disabled in FRLG because the champion is a Rival" });
            }
            else
            {
                stack.Add(new EnumComboBoxUI<TrainerOrgTypeTheme>("Champion Type Theming", ChampionTypeThemeDropdown, model.ChampionTheming));
            }
            stack.Add(new BoundCheckBoxUI(model.NoDupeGymAndEliteFourTypes, "Prevent Duplicate Gym and Elite Four Themes") { ToolTip = preventDupesTooltip });
            stack.Header("Villainous Teams");
            stack.Add(new EnumComboBoxUI<TrainerOrgTypeTheme>("Type Theming", TypeThemeDropdown, model.TeamTypeTheming));
            stack.Add(new BoundCheckBoxUI(model.GruntTheming, "Apply Team Theme To Grunts") { ToolTip = gruntThemeTooltip });
            stack.Add(new BoundCheckBoxUI(model.KeepTeamSubtypes, "Keep Team Subtypes") { ToolTip = teamSubtypesTooltip });
            //stack.Header("Miscellaneous Organizations", "Miscellanous Organizations include: The Winstrates, Nugget Bridge, The Fighting Dojo, The Soda Pop House");
            //stack.Add(new EnumComboBoxUI<TrainerOrgTypeTheme>("Type Theming", TypeThemeDropdown, model.SmallOrgTypeTheming));
        }
    }
}
