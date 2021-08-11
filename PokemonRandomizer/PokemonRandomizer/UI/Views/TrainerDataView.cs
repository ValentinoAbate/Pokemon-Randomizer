using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.UI.Views
{
    using Models;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using static Settings;
    using static Settings.TrainerSettings;

    public class TrainerDataView : GroupDataView<TrainerDataModel>
    {
        public CompositeCollection PokemonStrategyDropdown => new CompositeCollection()
        {
            new ComboBoxItem() {Content="Random", ToolTip="Pokemon in recurring battles will be chosen completely randomly"},
            new ComboBoxItem() {Content="Keep Ace", ToolTip="Recurring battles with a trainer in this group will keep that trainers ace pokemon and evolve it if appropriate"},
            new ComboBoxItem() {Content="Keep Party", ToolTip="Recurring battles with a trainer in this group will keep the pokemon from the previous battle and evolve them if appropriate. New pokemon will be added if the next battle has a bigger party." },
        };
        public CompositeCollection BattleTypeStrategyDropdown => new CompositeCollection()
        {
            new ComboBoxItem() {Content="Random", ToolTip="Recurring battles with a trainer in the group will have a random battle type"},
            new ComboBoxItem() {Content="Keep Same Type", ToolTip="Recurring battles with a trainer in this group will keep the same battle type (if the last battle was a double battle, the next one will also be a double battle, etc)."},
        };

        public static IEnumerable<string> MetricTypes { get; } = PokemonSettingsUI.BasicPokemonMetricTypes.Concat(new List<string>()
        {
            PokemonMetric.typeTrainerParty,
            //PokemonMetric.typeTrainerClass,
        });

        public override Panel CreateModelView(TrainerDataModel model)
        {
            var grid = new Grid();
            //grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
 
            var tabs = new TabControl();
            tabs.SetValue(Grid.RowProperty, 0);
            grid.Children.Add(tabs);

            tabs.Items.Add(CreatePokemonTab(model));
            tabs.Items.Add(CreateBattleTypeTab(model));
            return grid;
        }

        private TabItem CreatePokemonTab(TrainerDataModel model)
        {
            var tab = new TabItem() { Header = "Pokemon" };
            var stack = new StackPanel() { Orientation = Orientation.Vertical };
            var pokemonStack = new StackPanel() { Orientation = Orientation.Vertical };
            pokemonStack.Add(new EnumComboBoxUI<PokemonPcgStrategy>("Recurring Trainer Pokemon Randomization Strategy", PokemonStrategyDropdown, model.PokemonStrategy));
            pokemonStack.Add(new PokemonSettingsUI(model.PokemonSettings, MetricTypes, model.InitializeMetric));
            stack.Add(new RandomChanceUI("Random Pokemon", model.RandomizePokemon, model.PokemonRandChance, pokemonStack));
            stack.Add(pokemonStack);
            tab.Content = stack;
            return tab;
        }

        private TabItem CreateBattleTypeTab(TrainerDataModel model)
        {
            var tab = new TabItem() { Header = "Battle Type" };
            var stack = new StackPanel() { Orientation = Orientation.Vertical };
            var typeStack = new StackPanel { Orientation = Orientation.Vertical };

            typeStack.Add(new EnumComboBoxUI<BattleTypePcgStrategy>("Recurring Trainer Battle Type Strategy", BattleTypeStrategyDropdown, model.BattleTypeStrategy));
            typeStack.Add(new BoundSliderUI("Double Battle Chance", model.DoubleBattleChance) { ToolTip = "The chance that the battle type will be a double battle when randomized" });
            stack.Add(new RandomChanceUI("Random Battle Type", model.RandomizeBattleType, model.BattleTypeRandChance, typeStack));
            stack.Add(typeStack);
            tab.Content = stack;
            return tab;
        }

        public override TrainerDataModel CloneModel(TrainerDataModel model)
        {
            throw new NotImplementedException();
        }
    }
}
