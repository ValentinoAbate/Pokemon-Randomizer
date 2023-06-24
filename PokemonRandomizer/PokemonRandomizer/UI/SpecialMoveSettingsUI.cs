using PokemonRandomizer.UI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using static PokemonRandomizer.Backend.DataStructures.ItemData;
using static PokemonRandomizer.Settings;

namespace PokemonRandomizer.UI
{
    public class SpecialMoveSettingsUI : ContentControl
    {
        public class SpecialMoveSettingsWrapper
        {
            public Box<SpecialMoveSettings.UsageOption> Usage { get; set; }
            public Box<SpecialMoveSettings.Sources> Sources { get; set; }

            public SpecialMoveSettings Settings => new SpecialMoveSettings() { Usage = Usage, AllowedSources = Sources };

            public SpecialMoveSettingsWrapper() : this(SpecialMoveSettings.UsageOption.None, SpecialMoveSettings.Sources.None) { }
            public SpecialMoveSettingsWrapper(SpecialMoveSettings.UsageOption usage, SpecialMoveSettings.Sources sources)
            {
                Usage = new Box<SpecialMoveSettings.UsageOption>(usage);
                Sources = new Box<SpecialMoveSettings.Sources>(sources);
            }
        }

        private const string dynamicUsageTooltip = "Pokemon will be allowed to use the same number of special moves from each source as they can in the base game" +
            "\nFor example, a pokemon that had 1 TM move and one egg move in the base game will be allowed to use up to one TM move and up to one egg move (if TM and egg moves are selected sources)";
        private static CompositeCollection UsageDropdown => new()
        {
            new ComboBoxItem() { Content = "None", ToolTip = "Pokemon will not be allowed to use moves other than those they can learn by level-up" },
            new ComboBoxItem() { Content = "Constant", ToolTip = "Pokemon will be allowed to use a fixed amount of special moves (1-4)" },
            new ComboBoxItem() { Content = "Dynamic", ToolTip =  dynamicUsageTooltip},
            new ComboBoxItem() { Content = "Unlimited", ToolTip =  "Pokemon will be allowed to use unlimited special moves"},
        };

        private static List<BoundFlagsEnumListBoxUI<SpecialMoveSettings.Sources>.MenuBoxItem> GetSourceDropdown() => new()
        {
            new () { Item = SpecialMoveSettings.Sources.TM, Content="TMs", ToolTip="Moves that can be learned via TMs"},
            new () { Item = SpecialMoveSettings.Sources.HM, Content="HMs", ToolTip="Moves that can be learned via HMs"},
            new () { Item = SpecialMoveSettings.Sources.Tutor, Content="Move Tutors", ToolTip="Moves that can be learned via move tutors (such as the battle frontier tutors in Emerald)"},
            new () { Item = SpecialMoveSettings.Sources.Egg, Content="Egg Moves", ToolTip="Moves that can be learned as egg moves"},
            new () { Item = SpecialMoveSettings.Sources.Special, Content="Special", ToolTip="Moves that can be learned in-game under special circumstances (such as Volt Tackle)"},
        };

        private static SpecialMoveSettings.Sources SourceOrEquals(SpecialMoveSettings.Sources s1, SpecialMoveSettings.Sources s2)
        {
            return s1 |= s2;
        }
        public SpecialMoveSettingsUI(SpecialMoveSettingsWrapper settings)
        {
            var stack = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };
            stack.Add(new EnumComboBoxUI<SpecialMoveSettings.UsageOption>("Special Move Usage", UsageDropdown, settings.Usage));
            var sources = stack.Add(new BoundFlagsEnumListBoxUI<SpecialMoveSettings.Sources>("Allowed Sources", settings.Sources, GetSourceDropdown, SourceOrEquals));
            // List box formatting
            sources.ListBox.Width = 275;
            sources.ListBox.Padding = new System.Windows.Thickness(5, 0, 5, 0);
            sources.ListBox.SetValue(
                ScrollViewer.HorizontalScrollBarVisibilityProperty,
                ScrollBarVisibility.Disabled);
            Content = stack;
        }
    }
}
