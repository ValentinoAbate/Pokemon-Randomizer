using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.DataStructures.Trainers;
using PokemonRandomizer.UI.Models;
using System.Collections.Generic;
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
            new ComboBoxItem() { Content="Off" },
            new ComboBoxItem() { Content="On (Intelligent)", ToolTip = intelligentTypeThemeTooltip },
            new ComboBoxItem() { Content="On (Random)", ToolTip = randomTypeThemeTooltip },
        };
        private static List<TrainerOrgTypeTheme> TypeThemeOptions => new List<TrainerOrgTypeTheme>()
        {
            TrainerOrgTypeTheme.Off,
            TrainerOrgTypeTheme.On,
            TrainerOrgTypeTheme.Random
        };
        private static CompositeCollection ChampionTypeThemeDropdown => new CompositeCollection()
        {
            new ComboBoxItem() { Content="Same as Elite Four", ToolTip="Use the same setting as the Elite Four" },
            new ComboBoxItem() { Content="Off" },
            new ComboBoxItem() { Content="On (Intelligent)", ToolTip = intelligentTypeThemeTooltip },
            new ComboBoxItem() { Content="On (Random)", ToolTip = randomTypeThemeTooltip },
        };
        private static List<TrainerOrgTypeTheme> VillainousTypeThemeOptions => new List<TrainerOrgTypeTheme>()
        {
            TrainerOrgTypeTheme.Default,
            TrainerOrgTypeTheme.Off,
            TrainerOrgTypeTheme.Random
        };
        private static CompositeCollection VillainousTypeThemeDropdown => new CompositeCollection()
        {
            new ComboBoxItem() { Content="Same as Normal Trainers" },
            new ComboBoxItem() { Content="Off" },
            new ComboBoxItem() { Content="On (Random)", ToolTip = randomTypeThemeTooltip },
        };

        private static CompositeCollection DuplicatePreventionDropdown => new CompositeCollection()
        {
            new ComboBoxItem() { Content="None" },
            new ComboBoxItem() { Content="Prevent Duplicates (Randomized Only)", ToolTip = "Prevent duplicate type themes within randomized gyms and elite four members.\nFor example, if gym type themes are unrandomized, elite four members will be allowed to be the same type as a gym, but not the same as each other" },
            new ComboBoxItem() { Content="Prevent Duplicates (Randomized and Unrandomized)", ToolTip =  "Prevent duplicate type themes within all gyms and elite four members (randomized or not).\nFor example, if gym type themes are unrandomized, elite four members will be not be allowed to be the same type as a gym or each other"},
        };

        private static CompositeCollection ThemePriorityDropdown => new CompositeCollection()
        {
            new ComboBoxItem() { Content="Gym Leaders", ToolTip="If a trainer is a gym leader and a team leader (Giovanni, etc.), they will use their gym's theme" },
            new ComboBoxItem() { Content="Villanous Team Leaders", ToolTip="If a trainer is a gym leader and a team leader (Giovanni, etc.), they will use their team's theme" },
        };

        private const string teamSubtypesTooltip = "When randomizing Team Type Themes, keep the Secondary Type Theme(s). The Secondary Type Themes are:" +
            "\nTeam Rocket: NRM, FTG, PSY, GRD | Team Aqua: PSN, DRK | Team Magma: PSN, DRK";
        private const string gruntThemeTooltip = "If checked, the team's theme will apply to grunts. If unchecked, team grunts will use default type theming";
        public TrainerOrganizationDataView(TrainerOrganizationDataModel model, RomMetadata metadata)
        {
            var stack = CreateMainStack();
            stack.Header("Trainer Organization Theming");
            stack.Description("These settings control how type theming is applied to different trainer organizations such as Gyms, the Elite Four, and Villainous Teams like Team Rocket!", "These settings only apply if the \"Randomize Pokemon\" setting in the \"Trainers\" tab is enabled.");
            stack.Header("Gyms and Elite Four");
            stack.Add(new EnumComboBoxUI<TrainerOrgTypeTheme>("Gym Type Theming", TypeThemeDropdown, model.GymTypeTheming, TypeThemeOptions));
            stack.Add(new EnumComboBoxUI<TrainerOrgTypeTheme>("Elite Four Type Theming", TypeThemeDropdown, model.EliteFourTheming, TypeThemeOptions));
            if (metadata.IsFireRedOrLeafGreen)
            {
                stack.Add(new Label() { Content = "Note: Champion type theming is disabled in FRLG because the champion is a Rival" });
            }
            else
            {
                stack.Add(new EnumComboBoxUI<TrainerOrgTypeTheme>("Champion Type Theming", ChampionTypeThemeDropdown, model.ChampionTheming));
            }
            stack.Add(new EnumComboBoxUI<GymEliteFourPreventDupesSetting>("Gym and Elite Four Duplicate Theme Prevention", DuplicatePreventionDropdown, model.GymAndEliteDupePrevention));
            stack.Header("Villainous Teams");
            stack.Add(new EnumComboBoxUI<TrainerOrgTypeTheme>("Type Theming", VillainousTypeThemeDropdown, model.TeamTypeTheming, VillainousTypeThemeOptions));
            stack.Add(new BoundCheckBoxUI(model.GruntTheming, "Apply Team Theme To Grunts") { ToolTip = gruntThemeTooltip });
            stack.Add(new BoundCheckBoxUI(model.KeepTeamSubtypes, "Keep Team Subtypes") { ToolTip = teamSubtypesTooltip });
            stack.Add(new EnumComboBoxUI<Trainer.Category>("Priority Theme Category", ThemePriorityDropdown, model.PriorityCategory, new List<Trainer.Category>() { Trainer.Category.GymLeader, Trainer.Category.TeamLeader } ));
            //stack.Header("Miscellaneous Organizations", "Miscellanous Organizations include: The Winstrates, Nugget Bridge, The Fighting Dojo, The Soda Pop House");
            //stack.Add(new EnumComboBoxUI<TrainerOrgTypeTheme>("Type Theming", TypeThemeDropdown, model.SmallOrgTypeTheming));
        }
    }
}
