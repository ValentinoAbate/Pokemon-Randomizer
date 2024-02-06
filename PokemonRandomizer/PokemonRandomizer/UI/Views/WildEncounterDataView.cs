using System.Windows.Controls;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Data;

namespace PokemonRandomizer.UI.Views
{
    using Models;
    using static Backend.Randomization.WildEncounterRandomizer;
    public class WildEncounterDataView : DataView<WildEncounterDataModel>
    {
        public CompositeCollection StrategyDropdown => new CompositeCollection()
        {
            new ComboBoxItem() { Content="Unchanged", ToolTip="Wild pokemon will be left as they are in the base game"},
            new ComboBoxItem() { Content="Individual", ToolTip="Each encounter slot in each area becomes a new pokemon" },
            new ComboBoxItem() { Content="Area 1-1", ToolTip="All instances of a specific pokemon in each area become the same new pokemon (Recommended)" },
            new ComboBoxItem() { Content="Global 1-1", ToolTip="All instances of a specific pokemon in the wild become the same new pokemon" },
        };
        private const string matchAreaTypeTooltip = "Random wild pokemon will be more likely to match the types of pokemon that appear in the area they are chosen for" +
            "\nFor example, electric pokemon will be more likey to show up in the power plant";
        private const string matchEncounterTypeTooltip = "Random wild pokemon found by fishing, surfing, using Headbutt, or using Rock Smash will be more likely to match the type of pokemon that normally appear in those encounter slots" +
            "\nFor example, water pokemon will be more likely to be found when fishing";
        private const string matchIndividualTypeTooltip = "Random wild pokemon will be more likely to share a type with the pokemon they replace" +
            "\nFor example, a Wingull will be more likely to randomize to a Water or Flying pokemon";
        private const string matchPowerTooltip = "Random wild pokemon will be more likely to have a similar base stat total as the pokemon they replace";
        public WildEncounterDataView(WildEncounterDataModel model)
        {
            var stack = CreateMainStack();
            stack.Header("Wild Encounter Randomization");
            var strategyDropdown = stack.Add(new EnumComboBoxUI<Strategy>("Randomization Strategy", StrategyDropdown, model.Strategy));
            var settingsStack = strategyDropdown.BindEnabled(stack.Add(CreateStack()), 1, 2, 3);
            settingsStack.Add(new BoundCheckBoxUI("Area Type Weighting", model.MatchAreaType, matchAreaTypeTooltip));
            settingsStack.Add(new BoundCheckBoxUI("Encounter Type Type Weighting", model.MatchEncounterType, matchEncounterTypeTooltip));
            settingsStack.Add(new BoundCheckBoxUI("Individual Type Weighting", model.MatchIndividualType, matchIndividualTypeTooltip));
            settingsStack.Add(new BoundCheckBoxUI("Power Weighting", model.MatchPower, matchPowerTooltip));
            settingsStack.Add(new PokemonSettingsUI(model.PokemonSettings));
        }
    }
}
