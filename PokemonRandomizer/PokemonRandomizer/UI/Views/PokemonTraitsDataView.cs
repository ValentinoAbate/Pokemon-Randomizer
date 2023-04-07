using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using PokemonRandomizer.UI.Utilities;

namespace PokemonRandomizer.UI.Views
{
    using Models;
    using PokemonRandomizer.Backend.DataStructures;
    using static PokemonRandomizer.Settings;
    using CatchRateOption = Settings.CatchRateOption;
    public class PokemonTraitsDataView : DataView<PokemonTraitsModel>
    {
        private const string fixImpossibleEvosTooltip = "Ensure all pokemon can evolve to all of their evolutions. See info file for detailed evolution info after randomization"+
            "\nAffected evolution types: " +
            "\nTrade -> Level Up or Trade" +
            "\nTrade w/ Item -> Varies (See \"Trade item evolution type\" setting)" +
            "\nBeauty -> Level Up or Beauty (Optional)" +
            "\nFriendship (Day) -> Sun Stone (FRLG Only)" +
            "\nFriendship (Night) -> Moon Stone (FRLG Only)";
        private const string dunsparcePlagueTooltip = "Add a chance that any given evolution line will be infected with the Dunsparce Plague" +
            "\nAny pokemon who's evolution line is infected will have a 50% chance of evolving into Dunsparce when evolving by basic level up" +
            "\nFor example, if the Treecko evolution line is infected, 50% of Treecko will evolve into Grovyle, and the other 50% will evolve into Dunsparce" +
            "\nIn this example, if a Treecko successfully evolves into Grovyle, the resulting Grovyle will always evolve into Sceptile (as the original Treecko was immune)" +
            "\nThe Dunsparce Plague also affects NPC trainers who keep and evolve their party over the course of the game";
        private const string dunsparcePlagueFriendshipTooltip = "If checked, pokemon whose evolution lines are infected by the Dunsparce Plague may also evolve into Dunsparce when evolving by basic friendship, depending on the time of day" +
            "\nPokemon that previously evolved properly by level up may still evolve into Dunsparce by friendship and vice-versa" +
            "\nFor example, if the Zubat line is infected and a given Zubat successfully evolves into Golbat, that Golbat may still evolve into Dunsparce instead of Crobat" +
            "\nAdditionally, if the Azurill line is infected and a given Azurill successfully evolves into Marill, that Marill may still evolve into Dunsparce instead of Azumarill";

        public PokemonTraitsDataView(PokemonTraitsModel model, RomMetadata metadata)
        {
            var tabs = CreateMainTabControl();
            tabs.Add(CreateEvolutionTab(model, metadata));
            tabs.Add(CreateLearnsetsTab(model));
            tabs.Add(CreateCatchRateTab(model));
            tabs.Add(CreateExpYieldTab(model));
        }

        private CompositeCollection TradeItemOptionDropdown => new CompositeCollection()
        {
            new ComboBoxItem() { Content="Level Up", ToolTip = "Pokemon that normally evolve by trading with an item will evolve by level-up. Slowpoke and Clamperl will evolve with Wurmple logic" },
            new ComboBoxItem() { Content="Use Item", ToolTip = "Pokemon that normally evolve by trading with an item will evolve when that item is used on them"},
        };

        private TabItem CreateEvolutionTab(PokemonTraitsModel model, RomMetadata metadata)
        {
            var stack = CreateStack();
            stack.Header(UISkin.Current.HacksAndTweaksHeader);
            var impossibleCb = stack.Add(new BoundCheckBoxUI("Fix Impossible Evolutions", model.FixImpossibleEvos, fixImpossibleEvosTooltip));
            impossibleCb.BindEnabled(stack.Add(new EnumComboBoxUI<TradeItemPokemonOption>("Trade item evolution type", TradeItemOptionDropdown, model.TradeItemEvoSetting) { ToolTip = "The evolution method used for pokemon that normally evolve by trading with an item" + TooltipConstants.checkDropdownTooltip}));
            impossibleCb.BindEnabled(stack.Add(new BoundCheckBoxUI("Fix Beauty-Based Evolutions", model.ConsiderEvolveByBeautyImpossible, "Allow pokemon that would normally only evolve with a high Beauty stat (Feebas) to evolve by level-up")));
            impossibleCb.BindEnabled(stack.Add(new BoundSliderUI("Fixed evolution level variance", model.ImpossibleEvoLevelStandardDev, false, 0.01, 0, 3) { ToolTip = "The amount a level-up level for a fixed evolution can vary by (on average)"}));
            var plagueCB = stack.Add(new RandomChanceUI("Dunsparce Plague", model.DunsparsePlague, model.DunsparsePlaugeChance) { ToolTip = dunsparcePlagueTooltip});
            if (!metadata.IsFireRedOrLeafGreen)
            {
                plagueCB.BindEnabled(stack.Add(new BoundCheckBoxUI("Apply Dunsparce Plague to Friendship Evolutions", model.DunsparsePlagueFriendship, dunsparcePlagueFriendshipTooltip)));
            }
            return CreateTabItem("Evolution", stack);
        }

        // Catch rate parameters
        private const string intelligentCatchRateTooltip = "Intelligently make some pokemon easier to catch so that they can be caught at the beginning of the game";

        private CompositeCollection CatchRateOptionDropdown => new CompositeCollection()
        {
            new ComboBoxItem() { Content="Unchanged", ToolTip="Leave catch rates the same as they are in the base game"},
            new ComboBoxItem() { Content="Random", ToolTip="All pokemon will have a completely random catch rate"},
            new ComboBoxItem() { Content="Constant", ToolTip = "Set all pokemon to the same catch rate based on a difficulty value (0-1)"},
            new ComboBoxItem() { Content="Intelligent (Easy)", ToolTip = intelligentCatchRateTooltip + " (Easy mode)"},
            new ComboBoxItem() { Content="Intelligent (Normal)", ToolTip = intelligentCatchRateTooltip},
            new ComboBoxItem() { Content="Intelligent (Hard)", ToolTip = intelligentCatchRateTooltip + " (Hard mode)"},
            new ComboBoxItem() { Content="All Easiest", ToolTip = "All pokemon are as easy to catch as possible"},
        };

        private TabItem CreateCatchRateTab(PokemonTraitsModel model)
        {
            var stack = CreateStack();
            stack.Header("Catch Rate Randomization");
            var optionCb = stack.Add(new EnumComboBoxUI<CatchRateOption>("Randomization Strategy", CatchRateOptionDropdown, model.CatchRateSetting) { ToolTip = "The strategy to use when modifying catch rates" + TooltipConstants.checkDropdownTooltip});
            optionCb.BindVisibility(stack.Add(new BoundSliderUI("Constant Difficulty", model.CatchRateConstantDifficulty, false) { ToolTip = "The constant diffuculty to set all pokemon's catch rate to. Lower is easier and higher is harder"}), (int)CatchRateOption.Constant);
            stack.Add(new BoundCheckBoxUI("Keep Legendary Catch Rates", model.KeepLegendaryCatchRates, "Keeps catch rates for legendary pokemon the same as they are in the base game"));
            stack.Header("Egg Hatch Rate Modifications");
            stack.Add(new BoundCheckBoxUI("Fast Egg Hatching", model.FastHatching, "All pokemon eggs hatch in the minimum possible egg cycles"));
            return CreateTabItem("Catch / Hatch Rate", stack);
        }

        private TabItem CreateExpYieldTab(PokemonTraitsModel model)
        {
            var stack = CreateStack();
            stack.Description("Modify the base EXP given by pokemon. Setting this to 0% will make every pokemon give only 1 EXP!");
            stack.Add(new BoundSliderUI("Base Exp Yield Modifier", model.BaseExpYieldMultiplier, true, 0.1, 0, 2));
            stack.Description("Set the base EVs for every pokemon to 0. This will disable EV gain through normal means!");
            stack.Add(new BoundCheckBoxUI("Set Base EV Yield to 0", model.ZeroBaseEVs));
            return CreateTabItem("Exp / EV Yields", stack);
        }

        private const string banSelfdestructTooltip = "Removes selfdestruct and explosion from all learnsets. Other settings that modify learnsets will not add selfdestruct or explosion. Useful for more forgiving Nuzlockes!";
        private const int maxAddMoves = 10;

        private List<WeightedSetUI<AddMoveSource>.MenuBoxItem> GetAddMoveWeightDropdown() => new List<WeightedSetUI<AddMoveSource>.MenuBoxItem>
        {
            new WeightedSetUI<AddMoveSource>.MenuBoxItem { Item = AddMoveSource.Random, Header="Random", ToolTip="The chance that a given bonus move will be a completely random move"},
            new WeightedSetUI<AddMoveSource>.MenuBoxItem { Item = AddMoveSource.EggMoves, Header="Egg Moves", ToolTip="The chance that a bonus move will be one of the pokemon's egg moves",},
        };

        private const string bonusMoveWeightsTooltip = "The chances that determine which bonus move source is chosen for each bonus move granted";

        private TabItem CreateLearnsetsTab(PokemonTraitsModel model)
        {
            var stack = CreateStack();
            stack.Header("Bonus Moves");
            var bonusMovesStack = CreateStack();
            bonusMovesStack.Add(new WeightedSetUI<AddMoveSource>("Bonus Move Source", model.AddMoveSourceWeights, GetAddMoveWeightDropdown, bonusMoveWeightsTooltip, 100));
            bonusMovesStack.Add(new BoundSliderUI("Average number of moves to add", model.NumMovesMean, false, 0.5, 0, maxAddMoves) { ToolTip = "The average amount of bonus moves given to each evolution line. On average, each evolution line will be given the average amount +/- variance" });
            bonusMovesStack.Add(new BoundSliderUI("Number of moves variance", model.NumMovesStdDeviation, false, 0.5, 0, 5) { ToolTip = "The variance in the amount of bonus moves given to each evolution line. On average, each evolution line will be given the average amount +/- variance" });
            //bonusMovesStack.Add(new BoundSliderUI("Minimum number of moves to add", model.NumMovesMin, false, 1, 0, 5));
            bonusMovesStack.Add(new BoundCheckBoxUI("Ban adding HM moves", model.DisableAddingHmMoves, "Prevents HM moves from being added as bonus moves"));
            stack.Add(new RandomChanceUI("Bonus Moves", model.AddMoves, model.AddMovesChance, bonusMovesStack) { ToolTip = "Add bonus moves to pokemon learnsets. The percentage is the chance that any given evolution line will recieve bonus moves. Variant pokemon will not recieve bonus moves" });
            stack.Add(bonusMovesStack);
            stack.Separator();
            stack.Header(UISkin.Current.HacksAndTweaksHeader);
            stack.Add(new BoundCheckBoxUI("Ban Selfdestruct", model.BanSelfdestruct, banSelfdestructTooltip));
            return CreateTabItem("Learnsets", stack); ;
        }
    }
}
