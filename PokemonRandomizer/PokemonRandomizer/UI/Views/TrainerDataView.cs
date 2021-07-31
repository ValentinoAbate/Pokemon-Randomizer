using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.UI.Views
{
    using Models;
    using PokemonRandomizer.Backend.Utilities.Debug;
    using System.Windows.Controls;
    using System.Windows.Data;
    using static Settings;
    using static Settings.TrainerSettings;

    public class TrainerDataView : DataView<TrainerDataModel>
    {
        public static CompositeCollection PokemonStrategyDropdown { get; } = new CompositeCollection()
        {
            new ComboBoxItem() {Content="Random", ToolTip="Pokemon in recurring battles will be chosen completely randomly"},
            new ComboBoxItem() {Content="Keep Ace", ToolTip="Recurring battles with a trainer in this group will keep that trainers ace pokemon and evolve it if appropriate"},
            new ComboBoxItem() {Content="Keep Party", ToolTip="Recurring battles with a trainer in this group will keep the pokemon from the previous battle and evolve them if appropriate. New pokemon will be added if the next battle has a bigger party." },
        };
        public static CompositeCollection BattleTypeStrategyDropdown { get; } = new CompositeCollection()
        {
            new ComboBoxItem() {Content="Random", ToolTip="Recurring battles with a trainer in the group will have a random battle type"},
            new ComboBoxItem() {Content="Keep Same Type", ToolTip="Recurring battles with a trainer in this group will keep the same battle type (if the last battle was a double battle, the next one will also be a double battle, etc)."},
        };
        public TrainerDataView(TrainerDataModel model)
        {
            var mainStack = new StackPanel() { Orientation = Orientation.Vertical };
            Content = mainStack;
            var pokemonStack = new StackPanel { Orientation = Orientation.Vertical };
            pokemonStack.Add(new BoundComboBoxUI("Recurring Trainer Pokemon Randomization Strategy", PokemonStrategyDropdown, (int)model.PokemonStrategy, i => model.PokemonStrategy = (PokemonPcgStrategy)i));
            pokemonStack.Add(new PokemonSettingsUI(model.PokemonSettings, InitializeMetric));
            void OnRandomizePokemonChange(bool enabled)
            {
                model.RandomizePokemon = enabled;
                pokemonStack.IsEnabled = enabled;
            }
            mainStack.Add(new RandomChanceUI("Random Pokemon", model.RandomizePokemon, OnRandomizePokemonChange, model.PokemonRandChance, d => model.PokemonRandChance = d, Orientation.Horizontal));
            mainStack.Add(pokemonStack);
            OnRandomizePokemonChange(model.RandomizePokemon);
            mainStack.Add(new Separator());
            //var battleTypeStack = new StackPanel { Orientation = Orientation.Vertical };
            var battleTypeStratCb = new BoundComboBoxUI("Recurring Trainer Battle Type Randomization Strategy", BattleTypeStrategyDropdown, (int)model.BattleTypeStrategy, i => model.BattleTypeStrategy = (BattleTypePcgStrategy)i);
            //battleTypeStack.Add(new BoundComboBoxUI("Battle Type Randomization Strategy", BattleTypeStrategyDropdown, (int)model.BattleTypeStrategy, i => model.BattleTypeStrategy = (BattleTypePcgStrategy)i));
            mainStack.Add(new RandomChanceUI("Random Battle Type", model.RandomizeBattleType, b => model.RandomizeBattleType = b, model.BattleTypeRandChance, d => model.BattleTypeRandChance = d, Orientation.Horizontal, battleTypeStratCb));
        }

        private void InitializeMetric(MetricData metric)
        {
            Logger.main.Todo("Properly initialize trainer-specific metrics");
            return;
        }
    }
}
