using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.UI.Views
{
    using Models;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using static Settings.TrainerSettings;

    public class TrainerDataView : DataView<TrainerDataModel>
    {
        private static CompositeCollection PokemonStrategyDropdown => new CompositeCollection()
        {
            new ComboBoxItem() {Content="None", ToolTip="Pokemon in recurring battles will be chosen completely randomly"},
            new ComboBoxItem() {Content="Keep Ace", ToolTip="Recurring battles with a trainer will keep that trainer's ace pokemon and evolve it if appropriate"},
            new ComboBoxItem() {Content="Keep Party", ToolTip="Recurring battles with a trainer will keep the pokemon from the previous battle and evolve them if appropriate. New pokemon will be added if the next battle has a bigger party" },
        };
        private static CompositeCollection BattleTypeStrategyDropdown => new CompositeCollection()
        {
            new ComboBoxItem() {Content="None", ToolTip="Recurring battles with a trainer will have a random battle type"},
            new ComboBoxItem() {Content="Keep Same Type", ToolTip="Recurring battles with a trainer will keep the same battle type (if the last battle was a double battle, the next one will also be a double battle, etc)"},
        };

        private static CompositeCollection MetricDataDropdown => new CompositeCollection()
        {
            new ComboBoxItem() {Content="Individual Pokemon Type", ToolTip="Each pokemon will be more likely to randomize to a pokemon that has at least one of its original types"},
            new ComboBoxItem() {Content="Party Type", ToolTip="Each pokemon will be be more likely to randomize to a pokemon whose type appears in the trainer's original party, weighted by the number of times that type appears"},
        };

        private static CompositeCollection DuplicateReductionOptionDropdown => new()
        {
            new ComboBoxItem() { Content = "None", ToolTip = "Trainers will have the normal chance of having duplicate pokemon" },
            new ComboBoxItem() { Content = "Weak", ToolTip = "Trainers will be 75% as likely to have duplicate pokemon" },
            new ComboBoxItem() { Content = "Moderate", ToolTip = "Trainers will be 50% as likely to have duplicate pokemon" },
            new ComboBoxItem() { Content = "Strong", ToolTip = "Trainers will be 25% as likely to have duplicate pokemon" },
            new ComboBoxItem() { Content = "Strict", ToolTip = "Trainers will not have duplicate pokemon except when restrictions are ignored" },
        };

        private const string typeThemingTooltip = "Choose Pokemon based on their trainer's type theming. Most trainers will have their type theme determined by the types of their original party and their trainer class" +
            "\nGym Leaders, Gym Trainers, the Elite Four, and the Champion will have the type theme of their original Gym / Elite Four position unless that theme is randomized by other settings" +
            "\nSpecial trainer classes like Rivals and Ace/Cool Trainers will have no type theme";

        private const string ignoreRestrictionsTooltip = "The chance that all restrictions (Evolution Restrictions, Legendary Ban, Type Theming, etc.) will be ignored for any given pokemon";
        private const string normalTrainerTooltip = "Normal trainers include all trainers that are not considered bosses or minibosses";
        private const string minibossTooltip = "Minibosses include Ace/CoolTrainers, Team Admins, and Rivals";
        private const string bossTooltip = "Bosses include Team Leaders, Gym Leaders, the Elite Four, Champions, and Special bosses like Steven (Emerald) and Red (GS/HGSS)";
        private const string alwaysGenMovesetsTooltip = "Specifically chooses movesets for all trainer pokemon instead of using the default move generation" +
            "\nThe moves chosen are based on the moves the pokemon or a pre-evolution can know at the level it appears at (no TMs, etc.)" +
            "\nThis setting can help pokemon that evolve from stones have usable movesets, and makes movesets less predictable";
        private const string randomizePokemonTooltip = "Randomize the pokemon in each trainer's party" +
            "\nTrainers in side content like the battle frontier are controlled by separate seetings when applicable (see the \"Postgame and Side Content\" tab if available)";
        private const string duplicatePokemonReductionTooltip = "Reduce the chance that randomly generated trainers have more than one of the same pokemon";
        private const string bonusPokemonTooltip = "The number of bonus pokemon to add to each trainer in the selected categories" +
            "\nA trainer will never have more than six pokemon" +
            "\nBonus pokemon will be the same level as and hold the same item as the last pokemon in the trainer's party";
        private const string recurringTrainerTooltip = "The special pokemon choice logic to apply to consecutive battles against the same trainer, including rival battles";
        private const string recurringTrainerBattleTypeTooltip = "The special battle type choice logic to apply to consecutive battles against the same trainer, including rival battles";
        private const string battleTypeRandTooltip = "Randomize what type of battle each trainer is (single, double, etc.)" +
            "\nYou can use this feature in combination with the \"Double Battle Chance\" feature to set all battles to a specific type" +
            "\nWARNING: Entering into a double battle created by this feature with only one non-fainted pokemon in your party can cause anomalous behavior (but no crashes or softlocks)";
        private const string levelMultiplierTooltip = "The level multiplier to apply to all trainer pokemon" +
            "\nUse this feature in combination with the \"Base Exp Yield Modifier\" to create very difficult ROMs!";

        public TrainerDataView(TrainerDataModel model)
        {
            var stack = CreateMainStack();
            // Pokemon Randomization
            stack.Header("Trainer Pokemon");
            var pokemonRand = stack.Add(new BoundCheckBoxUI("Randomize Pokemon", model.RandomizePokemon, randomizePokemonTooltip));
            var pokemonStack = stack.Add(pokemonRand.BindEnabled(CreateStack()));
            var typeBox = pokemonStack.Add(new BoundCheckBoxUI("Type Theming (Intelligent)", model.TypeTheming, typeThemingTooltip));
            pokemonStack.Add(typeBox.BindEnabled(new EnumComboBoxUI<TrainerTypeDataSource>("Normal Trainer Type Data Source", MetricDataDropdown, model.TypeDataSource) { ToolTip = "The type data source intelligent type theming will use for trainers that don't have special logic" }));
            pokemonStack.Add(new EnumComboBoxUI<TrainerDataModel.DuplicateReductionOption>("Duplicate Pokemon Reduction", DuplicateReductionOptionDropdown, model.DuplicateReduction) { ToolTip = duplicatePokemonReductionTooltip });
            var pokemonDetailsStack = pokemonStack.Add(CreateHorizontalStack());
            pokemonDetailsStack.Add(new BoundCheckBoxUI("Ban Illegal Evolutions", model.RestrictIllegalEvolutions, PokemonSettingsUI.legalEvoTooltip));
            pokemonDetailsStack.Add(new BoundCheckBoxUI("Force Highest Legal Evolution", model.ForceHighestLegalEvolution, PokemonSettingsUI.highestLegalEvoTooltip));

            // Legendary banning
            var banLegendariesStack = pokemonStack.Add(CreateHorizontalStack());
            banLegendariesStack.Add(new Label() { Content = "Ban Legendaries For: ", ToolTip = PokemonSettingsUI.banLegendariesTooltip });
            banLegendariesStack.Add(new BoundCheckBoxUI("Normal Trainers", model.BanLegendaries, normalTrainerTooltip) { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 2, 2, 2) });
            banLegendariesStack.Add(new BoundCheckBoxUI("Minibosses", model.BanLegendariesMiniboss, minibossTooltip) { VerticalAlignment = VerticalAlignment.Center });
            banLegendariesStack.Add(new BoundCheckBoxUI("Bosses", model.BanLegendariesBoss, bossTooltip) { VerticalAlignment = VerticalAlignment.Center });
            pokemonStack.Add(new BoundSliderUI("Ignore Restrictions Chance", model.PokemonNoise, true, 0.001, 0, 0.02) { ToolTip = ignoreRestrictionsTooltip });
            // Bonus pokemon
            var bonusPokemonStack = pokemonStack.Add(CreateHorizontalStack());
            bonusPokemonStack.Add(new BoundSliderUI("Bonus Pokemon", model.NumBonusPokemon, false, 1, 0, 6) { ToolTip = bonusPokemonTooltip });
            bonusPokemonStack.Add(new Label() { Content = "Add Bonus Pokemon To: " });
            bonusPokemonStack.Add(new BoundCheckBoxUI("Normal Trainers", model.BonusPokemon, normalTrainerTooltip) { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0,2,2,2) });
            bonusPokemonStack.Add(new BoundCheckBoxUI("Minibosses", model.BonusPokemonMiniboss, minibossTooltip) { VerticalAlignment = VerticalAlignment.Center });
            bonusPokemonStack.Add(new BoundCheckBoxUI("Bosses", model.BonusPokemonBoss, bossTooltip) { VerticalAlignment = VerticalAlignment.Center });
            pokemonStack.Add(new BoundCheckBoxUI("Always Generate Movesets", model.ForceCustomMoves) { ToolTip = alwaysGenMovesetsTooltip });
            pokemonStack.Add(new EnumComboBoxUI<PokemonPcgStrategy>("Recurring Trainer Pokemon Randomization Strategy", PokemonStrategyDropdown, model.PokemonStrategy) { ToolTip = recurringTrainerTooltip });
            // Battle Type Randomization
            stack.Header("Battle Type");
            var battleTypeRand = stack.Add(new BoundCheckBoxUI("Randomize Battle Type", model.RandomizeBattleType, battleTypeRandTooltip));
            var typeStack = stack.Add(battleTypeRand.BindEnabled(CreateStack()));
            typeStack.Add(new BoundSliderUI("Double Battle Chance", model.DoubleBattleChance) { ToolTip = "The chance that the battle type will be a double battle when randomized\nBattles with only one pokemon will never be double battles" });
            typeStack.Add(new EnumComboBoxUI<BattleTypePcgStrategy>("Recurring Trainer Battle Type Strategy", BattleTypeStrategyDropdown, model.BattleTypeStrategy) { ToolTip = recurringTrainerBattleTypeTooltip });
            // Difficulty
            stack.Header("Difficulty");
            stack.Add(new BoundSliderUI("Level Multiplier", model.LevelMult, true, 0.05, 0.5, 3) { ToolTip = levelMultiplierTooltip });
            stack.Add(new BoundSliderUI("Minimum Trainer Pokemon IVs", model.MinIVs, false, 1, 0, 31) { ToolTip = "The minimum value for trainer pokemon IVs. Set to 31 for maximum difficulty!" });
            stack.Add(new BoundCheckBoxUI("Smart Trainer AI", model.SmartAI) { ToolTip = "Use smart(er) trainer AI for all trainers, even ones who wouldn't normally have it" });
        }
    }
}
