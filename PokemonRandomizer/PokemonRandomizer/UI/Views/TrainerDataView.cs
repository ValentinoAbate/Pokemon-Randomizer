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

    public class TrainerDataView : DataView<TrainerDataModel>
    {
        private static CompositeCollection PokemonStrategyDropdown => new CompositeCollection()
        {
            new ComboBoxItem() {Content="Random", ToolTip="Pokemon in recurring battles will be chosen completely randomly"},
            new ComboBoxItem() {Content="Keep Ace", ToolTip="Recurring battles with a trainer in this group will keep that trainers ace pokemon and evolve it if appropriate"},
            new ComboBoxItem() {Content="Keep Party", ToolTip="Recurring battles with a trainer in this group will keep the pokemon from the previous battle and evolve them if appropriate. New pokemon will be added if the next battle has a bigger party." },
        };
        private static CompositeCollection BattleTypeStrategyDropdown => new CompositeCollection()
        {
            new ComboBoxItem() {Content="Random", ToolTip="Recurring battles with a trainer in the group will have a random battle type"},
            new ComboBoxItem() {Content="Keep Same Type", ToolTip="Recurring battles with a trainer in this group will keep the same battle type (if the last battle was a double battle, the next one will also be a double battle, etc)."},
        };

        public static IEnumerable<string> MetricTypes { get; } = PokemonSettingsUI.BasicPokemonMetricTypes.Concat(new List<string>()
        {
            PokemonMetric.typeTrainerParty,
            //PokemonMetric.typeTrainerClass,
        });

        public TrainerDataView(TrainerDataModel model)
        {
            var stack = CreateMainStack();
            // Pokemon Randomization
            stack.Header("Trainer Pokemon");
            var pokemonRand = stack.Add(new RandomChanceUI("Randomize Pokemon", model.RandomizePokemon, model.PokemonRandChance));
            var pokemonStack = stack.Add(pokemonRand.BindEnabled(CreateStack()));
            pokemonStack.Add(new PokemonSettingsUI(model.PokemonSettings, MetricTypes, model.InitializeMetric));
            pokemonStack.Add(new EnumComboBoxUI<PokemonPcgStrategy>("Recurring Trainer Pokemon Randomization Strategy", PokemonStrategyDropdown, model.PokemonStrategy));
            // Battle Type Randomization
            stack.Header("Battle Type");
            var battleTypeRand = stack.Add(new RandomChanceUI("Randomize Battle Type", model.RandomizeBattleType, model.BattleTypeRandChance));
            var typeStack = stack.Add(battleTypeRand.BindEnabled(CreateStack()));
            typeStack.Add(new BoundSliderUI("Double Battle Chance", model.DoubleBattleChance) { ToolTip = "The chance that the battle type will be a double battle when randomized" });
            typeStack.Add(new EnumComboBoxUI<BattleTypePcgStrategy>("Recurring Trainer Battle Type Strategy", BattleTypeStrategyDropdown, model.BattleTypeStrategy));

        }
    }
}
