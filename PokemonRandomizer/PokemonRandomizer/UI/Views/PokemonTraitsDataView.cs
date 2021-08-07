using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PokemonRandomizer.UI
{
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
            //grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            var tabs = new TabControl();
            tabs.SetValue(Grid.RowProperty, 0);
            grid.Children.Add(tabs);

            tabs.Items.Add(CreateTypesTab(model));
            tabs.Items.Add(CreateEvolutionTab(model));
            tabs.Items.Add(CreateLearnsetsTab(model));
            tabs.Items.Add(CreateCatchRateTab(model));

            return grid;
        }

        private TabItem CreateTypesTab(PokemonTraitsModel model)
        {
            var tab = new TabItem() { Header = "Type" };
            var stack = new StackPanel() { Orientation = Orientation.Vertical };
            stack.Add(new Label() { Content = "Type Randomization" });
            stack.Add(new Separator());
            stack.Add(new RandomChanceUI("Single Type", model.RandomizeSingleType, b => model.RandomizeSingleType = b, model.SingleTypeRandChance, (d) => model.SingleTypeRandChance = d));
            stack.Add(new RandomChanceUI("Dual Type (Primary)", model.RandomizeDualTypePrimary, b => model.RandomizeDualTypePrimary = b, model.DualTypePrimaryRandChance, (d) => model.DualTypePrimaryRandChance = d));
            stack.Add(new RandomChanceUI("Dual Type (Secondary)", model.RandomizeDualTypeSecondary, b => model.RandomizeDualTypeSecondary = b, model.DualTypeSecondaryRandChance, (d) => model.DualTypeSecondaryRandChance = d));
            tab.Content = stack;
            return tab;
        }

        private TabItem CreateEvolutionTab(PokemonTraitsModel model)
        {
            var tab = new TabItem() { Header = "Evolution" };
            var stack = new StackPanel() { Orientation = Orientation.Vertical };
            stack.Add(new Label() { Content = UISkin.Current.HacksAndTweaksHeader });
            stack.Add(new Separator());
            var tradeEvoParams = new StackPanel() { Orientation = Orientation.Vertical };
            tradeEvoParams.SetVisibility(model.FixImpossibleEvos);
            tradeEvoParams.Add(new BoundComboBoxUI("Trade item evolution type", PokemonTraitsModel.CompatOptionDropdown, (int)model.TradeItemEvoSetting, (i) => model.TradeItemEvoSetting = (Settings.TradeItemPokemonOption)i));
            tradeEvoParams.Add(new BoundCheckBoxUI(model.ConsiderEvolveByBeautyImpossible, b => model.ConsiderEvolveByBeautyImpossible = b, "Fix Beauty-Based Evolutions"));
            tradeEvoParams.Add(new BoundSliderUI("Fixed evolution level variance", model.ImpossibleEvoLevelStandardDev, (d) => model.ImpossibleEvoLevelStandardDev = d, false, 0.01, 0, 3));
            
            void OnCheck(bool b)
            {
                model.FixImpossibleEvos = b;
                tradeEvoParams.SetVisibility(b);
            }
            var impossibleEvoCb = new BoundCheckBoxUI(model.FixImpossibleEvos, OnCheck, "Fix Trade Evolutions");
            stack.Add(impossibleEvoCb, tradeEvoParams);
            stack.Add(new RandomChanceUI("Dunsparse Plague", model.DunsparsePlague, (b) => model.DunsparsePlague = b, model.DunsparsePlaugeChance, (d) => model.DunsparsePlaugeChance = d));
            tab.Content = stack;
            return tab;
        }

        private TabItem CreateCatchRateTab(PokemonTraitsModel model)
        {
            var tab = new TabItem() { Header = "Catch Rate" };
            var stack = new StackPanel() { Orientation = Orientation.Vertical };
            stack.Add(new Label() { Content = "Catch Rate Randomization" });
            stack.Add(new Separator());
            var constantRateSlider = new BoundSliderUI("Constant Difficulty", model.CatchRateConstantDifficulty, (d) => model.CatchRateConstantDifficulty = d, false);
            constantRateSlider.SetVisibility(model.CatchRateSetting == CatchRateOption.Constant);
            void OnOptionChange(int index)
            {
                model.CatchRateSetting = (CatchRateOption)index;
                constantRateSlider.SetVisibility(model.CatchRateSetting == CatchRateOption.Constant);
            }
            stack.Add(new BoundComboBoxUI("Randomization Strategy", PokemonTraitsModel.CatchRateOptionDropdown, (int)model.CatchRateSetting, OnOptionChange));
            stack.Add(constantRateSlider);
            stack.Add(new BoundCheckBoxUI(model.KeepLegendaryCatchRates, (b) => model.KeepLegendaryCatchRates = b, "Keep Legendary Catch Rates"));
            tab.Content = stack;
            return tab;
        }

        private const string banSelfdestructTooltip = "Removes selfdestruct and explosion from all learnsets. Other settings that modify learnsets will not add selfdestruct or explosion. Useful for more forgiving Nuzlockes!";
        private const int maxAddMoves = 10;

        private TabItem CreateLearnsetsTab(PokemonTraitsModel model)
        {
            var tab = new TabItem() { Header = "Learnsets" };
            var stack = new StackPanel() { Orientation = Orientation.Vertical };
            stack.Add(new Label() { Content = "Bonus Moves" });
            stack.Add(new Separator());
            var bonusMovesStack = new StackPanel() { Orientation = Orientation.Vertical };
            bonusMovesStack.Add(new BoundSliderUI("Average number of moves to add", model.NumMovesMean, d => model.NumMovesMean = d, false, 0.5, 0, maxAddMoves));
            bonusMovesStack.Add(new BoundSliderUI("Number of moves variance", model.NumMovesStdDeviation, d => model.NumMovesStdDeviation = d, false, 0.5, 0, 5));
            bonusMovesStack.Add(new BoundSliderUI("Minimum number of moves to add", model.NumMovesMin, d => model.NumMovesMin = d, false, 1, 0, 5));
            bonusMovesStack.Add(new BoundCheckBoxUI(model.DisableAddingHmMoves, b => model.DisableAddingHmMoves = b, "Ban adding HM moves"));
            stack.Add(new RandomChanceUI("Bonus Moves", model.AddMoves, b => model.AddMoves = b, model.AddMovesChance, d => model.AddMovesChance = d, bonusMovesStack));
            stack.Add(bonusMovesStack);
            stack.Add(new Separator());
            stack.Add(new Label() { Content = UISkin.Current.HacksAndTweaksHeader });
            stack.Add(new Separator());
            stack.Add(new BoundCheckBoxUI(model.BanSelfdestruct, b => model.BanSelfdestruct = b, "Ban Selfdestruct", banSelfdestructTooltip));
            tab.Content = stack;
            return tab;
        }
    }
}
