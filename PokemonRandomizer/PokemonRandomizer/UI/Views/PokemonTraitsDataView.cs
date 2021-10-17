using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace PokemonRandomizer.UI.Views
{
    using Models;
    using static PokemonRandomizer.Settings;
    using CatchRateOption = Settings.CatchRateOption;
    public class PokemonTraitsDataView : GroupDataView<PokemonTraitsModel>
    {
        public override PokemonTraitsModel CloneModel(PokemonTraitsModel model)
        {
            throw new NotImplementedException();
        }

        public override Panel CreateModelView(PokemonTraitsModel model)
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            var tabs = new TabControl();
            tabs.SetValue(Grid.RowProperty, 0);
            grid.Children.Add(tabs);

            //tabs.Items.Add(CreateTypesTab(model));
            tabs.Items.Add(CreateEvolutionTab(model));
            tabs.Items.Add(CreateLearnsetsTab(model));
            tabs.Items.Add(CreateCatchRateTab(model));

            return grid;
        }

        private TabItem CreateTypesTab(PokemonTraitsModel model)
        {
            var stack = CreateStack();
            stack.Header("Type Randomization");
            stack.Add(new RandomChanceUI("Single Type", model.RandomizeSingleType, model.SingleTypeRandChance));
            stack.Add(new RandomChanceUI("Dual Type (Primary)", model.RandomizeDualTypePrimary, model.DualTypePrimaryRandChance));
            stack.Add(new RandomChanceUI("Dual Type (Secondary)", model.RandomizeDualTypeSecondary, model.DualTypeSecondaryRandChance));
            return CreateTabItem("Type", stack);
        }

        private CompositeCollection CompatOptionDropdown => new CompositeCollection()
        {
            new ComboBoxItem() { Content="Level Up", ToolTip = "Pokemon that normally evolve by trading with an item will evolve by level-up. Slowpoke and Clamperl will evolve with wurmple logic" },
            new ComboBoxItem() { Content="Use Item", ToolTip = "Pokemon that normally evolve by trading with an item will evolve when that item is used on them"},
        };

        private TabItem CreateEvolutionTab(PokemonTraitsModel model)
        {
            var stack = CreateStack();
            stack.Header(UISkin.Current.HacksAndTweaksHeader);
            var impossibleCb = stack.Add(new BoundCheckBoxUI(model.FixImpossibleEvos, "Fix Trade Evolutions"));
            impossibleCb.BindVisibility(stack.Add(new EnumComboBoxUI<TradeItemPokemonOption>("Trade item evolution type", CompatOptionDropdown, model.TradeItemEvoSetting)));
            stack.Add(new BoundCheckBoxUI(model.ConsiderEvolveByBeautyImpossible, "Fix Beauty-Based Evolutions"));
            stack.Add(new BoundSliderUI("Fixed evolution level variance", model.ImpossibleEvoLevelStandardDev, false, 0.01, 0, 3));
            stack.Add(new RandomChanceUI("Dunsparse Plague", model.DunsparsePlague, model.DunsparsePlaugeChance));
            return CreateTabItem("Evolution", stack);
        }

        // Catch rate parameters
        private const string intelligentCatchRateTooltip = "Intelligently make some pokemon easier to catch because they can be found at the beginning of the game";

        private CompositeCollection CatchRateOptionDropdown => new CompositeCollection()
        {
            new ComboBoxItem() { Content="Unchanged"},
            new ComboBoxItem() { Content="Random"},
            new ComboBoxItem() { Content="Constant", ToolTip = "Set all pokemon to the same catch difficulty"},
            new ComboBoxItem() { Content="Intelligent (Easy)", ToolTip = intelligentCatchRateTooltip + " (Easy)"},
            new ComboBoxItem() { Content="Intelligent (Normal)", ToolTip = intelligentCatchRateTooltip},
            new ComboBoxItem() { Content="Intelligent (Hard)", ToolTip = intelligentCatchRateTooltip + " (Hard)"},
            new ComboBoxItem() { Content="All Easiest", ToolTip = "All pokemon are as easy to catch as possible"},
        };

        private TabItem CreateCatchRateTab(PokemonTraitsModel model)
        {
            var stack = CreateStack();
            stack.Header("Catch Rate Randomization");
            var optionCb = stack.Add(new EnumComboBoxUI<CatchRateOption>("Randomization Strategy", CatchRateOptionDropdown, model.CatchRateSetting));
            optionCb.BindVisibility(stack.Add(new BoundSliderUI("Constant Difficulty", model.CatchRateConstantDifficulty, false)), (int)CatchRateOption.Constant);
            stack.Add(new BoundCheckBoxUI(model.KeepLegendaryCatchRates, "Keep Legendary Catch Rates"));
            return CreateTabItem("Catch Rate", stack);
        }

        private const string banSelfdestructTooltip = "Removes selfdestruct and explosion from all learnsets. Other settings that modify learnsets will not add selfdestruct or explosion. Useful for more forgiving Nuzlockes!";
        private const int maxAddMoves = 10;

        private List<WeightedSetUI<AddMoveSource>.MenuBoxItem> GetAddMoveWeightDropdown() => new List<WeightedSetUI<AddMoveSource>.MenuBoxItem>
        {
            new WeightedSetUI<AddMoveSource>.MenuBoxItem { Item = AddMoveSource.Random, Header="Random"},
            new WeightedSetUI<AddMoveSource>.MenuBoxItem { Item = AddMoveSource.EggMoves, Header="Egg Moves"},
        };

        private TabItem CreateLearnsetsTab(PokemonTraitsModel model)
        {
            var stack = CreateStack();
            stack.Header("Bonus Moves");
            var bonusMovesStack = CreateStack();
            bonusMovesStack.Add(new WeightedSetUI<AddMoveSource>("Bonus Move Source", model.AddMoveSourceWeights, GetAddMoveWeightDropdown, 100));
            bonusMovesStack.Add(new BoundSliderUI("Average number of moves to add", model.NumMovesMean, false, 0.5, 0, maxAddMoves));
            bonusMovesStack.Add(new BoundSliderUI("Number of moves variance", model.NumMovesStdDeviation, false, 0.5, 0, 5));
            //bonusMovesStack.Add(new BoundSliderUI("Minimum number of moves to add", model.NumMovesMin, false, 1, 0, 5));
            bonusMovesStack.Add(new BoundCheckBoxUI(model.DisableAddingHmMoves, "Ban adding HM moves"));
            stack.Add(new RandomChanceUI("Bonus Moves", model.AddMoves, model.AddMovesChance, bonusMovesStack));
            stack.Add(bonusMovesStack);
            stack.Add(new Separator());
            stack.Header(UISkin.Current.HacksAndTweaksHeader);
            stack.Add(new BoundCheckBoxUI(model.BanSelfdestruct, "Ban Selfdestruct", banSelfdestructTooltip));
            return CreateTabItem("Learnsets", stack); ;
        }
    }
}
