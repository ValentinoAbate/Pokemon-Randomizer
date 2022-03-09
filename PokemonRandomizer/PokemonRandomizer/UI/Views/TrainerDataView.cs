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
            new ComboBoxItem() {Content="Keep Ace", ToolTip="Recurring battles with a trainer will keep that trainers ace pokemon and evolve it if appropriate"},
            new ComboBoxItem() {Content="Keep Party", ToolTip="Recurring battles with a trainer will keep the pokemon from the previous battle and evolve them if appropriate. New pokemon will be added if the next battle has a bigger party." },
        };
        private static CompositeCollection BattleTypeStrategyDropdown => new CompositeCollection()
        {
            new ComboBoxItem() {Content="Random", ToolTip="Recurring battles with a trainer will have a random battle type"},
            new ComboBoxItem() {Content="Keep Same Type", ToolTip="Recurring battles with a trainer will keep the same battle type (if the last battle was a double battle, the next one will also be a double battle, etc)."},
        };

        public static IEnumerable<string> MetricTypes { get; } = PokemonSettingsUI.BasicPokemonMetricTypes.Concat(new List<string>()
        {
            PokemonMetric.typeTrainerParty,
            //PokemonMetric.typeTrainerClass,
        });

        private const string typeThemingTooltip = "Choose Pokemon based on their trainer's type theming. Most trainers will have their type theme determined by the types of their original party and their trainer class" +
            "\nGym Leaders, Gym Trainers, the Elite Four, and the Chamption will have the type theme of their original Gym / Elite Four position unless that theme is randomized by other settings" +
            "\nSpecial trainer classes like Rivals and Ace/Cool Trainers will have no type theme";

        private const string ignoreRestrictionsTooltip = "The chance that all restrictions (Evolution Restrictions, Legendary Ban, Type Theming, etc.) will be ignored for any given pokemon";

        public TrainerDataView(TrainerDataModel model)
        {
            var stack = CreateMainStack();
            // Pokemon Randomization
            stack.Header("Trainer Pokemon");
            var pokemonRand = stack.Add(new BoundCheckBoxUI(model.RandomizePokemon, "Randomize Pokemon"));
            var pokemonStack = stack.Add(pokemonRand.BindEnabled(CreateStack()));
            pokemonStack.Add(new BoundCheckBoxUI(model.TypeTheming, "Intelligent Type Theming") { ToolTip = typeThemingTooltip });
            var pokemonDetailsStack = pokemonStack.Add(new StackPanel() { Orientation = Orientation.Horizontal });
            pokemonDetailsStack.Add(new BoundCheckBoxUI(model.RestrictIllegalEvolutions, "Ban Illegal Evolutions"));
            pokemonDetailsStack.Add(new BoundCheckBoxUI(model.ForceHighestLegalEvolution, "Force Highest Legal Evolution"));
            pokemonStack.Add(new Label() { Content = "Ban Legendaries For: " });
            var banLegendariesStack = pokemonStack.Add(new StackPanel() { Orientation = Orientation.Horizontal });
            banLegendariesStack.Add(new BoundCheckBoxUI(model.BanLegendaries, "Normal Trainers"));
            banLegendariesStack.Add(new BoundCheckBoxUI(model.BanLegendariesMiniboss, "Minibosses") { ToolTip = "Minibosses include Ace/CoolTrainers, Team Admins, and Rivals" });
            banLegendariesStack.Add(new BoundCheckBoxUI(model.BanLegendariesBoss, "Bosses") { ToolTip = "Bosses include Team Leaders, Gym Leaders, the Elite Four, Champions, and Special bosses like Steven (Emerald) and Red (GS/HGSS)"});
            pokemonStack.Add(new BoundSliderUI("Ignore Restrictions Chance", model.PokemonNoise, true, 0.01, 0, 0.33) { ToolTip = ignoreRestrictionsTooltip }); ;
            pokemonStack.Add(new EnumComboBoxUI<PokemonPcgStrategy>("Recurring Trainer Pokemon Randomization Strategy", PokemonStrategyDropdown, model.PokemonStrategy));
            // Battle Type Randomization
            stack.Header("Battle Type");
            var battleTypeRand = stack.Add(new BoundCheckBoxUI(model.RandomizeBattleType, "Randomize Battle Type"));
            var typeStack = stack.Add(battleTypeRand.BindEnabled(CreateStack()));
            typeStack.Add(new BoundSliderUI("Double Battle Chance", model.DoubleBattleChance) { ToolTip = "The chance that the battle type will be a double battle when randomized" });
            typeStack.Add(new EnumComboBoxUI<BattleTypePcgStrategy>("Recurring Trainer Battle Type Strategy", BattleTypeStrategyDropdown, model.BattleTypeStrategy));
            // Difficulty
            stack.Header("Difficulty");
            stack.Add(new BoundSliderUI("Level Multiplier", model.LevelMult, true, 0.05, 0.5, 3));
            stack.Add(new BoundSliderUI("Minimum Trainer Pokemon IVs", model.MinIVs, false, 1, 0, 31) { ToolTip = "The minimum value for trainer pokemon IVs. Set to 31 for maximum difficulty!" });
            stack.Add(new BoundCheckBoxUI(model.SmartAI, "Smart Trainer AI") { ToolTip = "Use smart(er) trainer AI for all trainers, even ones who wouldn't normally have it" });
        }
    }
}
