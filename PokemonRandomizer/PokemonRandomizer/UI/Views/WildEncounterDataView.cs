using System.Windows.Controls;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Data;

namespace PokemonRandomizer.UI
{
    using static Backend.Randomization.WildEncounterRandomizer;
    public class WildEncounterDataView : DataView<WildEncounterDataModel>
    {
        public CompositeCollection StrategyDropdown => new CompositeCollection()
        {
            new ComboBoxItem() { Content="Unchanged" },
            new ComboBoxItem() { Content="Individual", ToolTip="Each encounter slot in each area becomes a new pokemon" },
            new ComboBoxItem() { Content="Area 1-1", ToolTip="All instances of a specific pokemon in each area become the same new pokemon (Recommended)" },
            new ComboBoxItem() { Content="Global 1-1", ToolTip="All instances of a specific pokemon in the wild become the same new pokemon" },
        };
        public static IEnumerable<string> MetricTypes { get; } = PokemonSettingsUI.BasicPokemonMetricTypes.Concat(new List<string>()
        {
            Settings.PokemonMetric.typeEncounterSet,
            Settings.PokemonMetric.typeEncounterBankType,
        });
        public WildEncounterDataView(WildEncounterDataModel model)
        {
            var stack = new StackPanel() { Orientation = Orientation.Vertical };
            Content = stack;
            stack.Add(new Label() { Content = "Wild Encounter Randomization" });
            stack.Add(new Separator());
            stack.Add(new EnumComboBoxUI<Strategy>("Randomization Strategy", StrategyDropdown, model.Strategy));
            stack.Add(new PokemonSettingsUI(model.PokemonSettings, MetricTypes, model.InitializeMetric));
        }
    }
}
