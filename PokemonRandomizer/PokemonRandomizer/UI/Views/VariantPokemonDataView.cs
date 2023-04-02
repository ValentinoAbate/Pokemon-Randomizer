using PokemonRandomizer.UI.Models;
using System.Windows.Controls;
using System.Windows.Data;
using static PokemonRandomizer.UI.Models.VariantPokemonDataModel;
using static PokemonRandomizer.Backend.Randomization.PokemonVariantRandomizer;
using PokemonRandomizer.UI.Utilities;
using System.Collections.Generic;

namespace PokemonRandomizer.UI.Views
{
    public class VariantPokemonDataView : DataView<VariantPokemonDataModel>
    {
        private const string rebalanceAttackingStatsTooltip = "Rebalance the attacking stats of Variant pokemon to match their variant type(s)" +
            "\nFor example, a psychic type Machamp will become a special attacker instead of a physical attacker" +
            "\nAdditionally, a fighting/psychic Machamp will become a split attacker to support both its STAB types" +
            "\nAttack stat rebalancing will never result in a pokemon's base stats being lowered, but may result in the lower of its two attacking stats increasing if the pokemon becomes a split attacker";

        private const string typeWeightDropdownTooltip = "The chances that determine which initial type transformation is chosen for the first pokemon in a Variant evolution line" +
            "\nThe type transformation applied to pokemon that evolve from the first pokemon are then calculated based on the initial type transformation and the type changes the pokemon would normally undergo when evolving" +
            "\nSee the \"Custom\" dropdown option for a list of all initial type transformations" +
            TooltipConstants.checkDropdownTooltip;
        private const string singleTypeWeightTooltip = "The chances that determine which initial type transformation is chosen for a given single-typed pokemon";
        private const string dualTypeWeightTooltip = "The chances that determine which initial type transformation is chosen for a given dual-typed pokemon";

        public CompositeCollection TypeTransformationOptionDropdown => new CompositeCollection()
        {
            new ComboBoxItem() {Content="Random", ToolTip = "Randomly choose an initial type transformation for each evolution line (Not Recommended)"},
            new ComboBoxItem() {Content="Balanced (Recommended)", ToolTip = "Choose an initial type transformation based on balanced chances that make \"Double Type Replacement\" and \"Become Single Type\" transformations rarer (Recommended)"},
            new ComboBoxItem() {Content="Custom", ToolTip = "Choose custom chances that determine which initial type transformation will be chosen"},
        };

        public VariantPokemonDataView(VariantPokemonDataModel model)
        {
            var stack = CreateMainStack();
            stack.Header("Variant Pokemon");
            stack.Description("This feature creates type variants of Pokemon, and edits their movesets, color palettes, base stats, and EV yields to fit their new type!", "This feature is designed to be configured at a 10-25% chance.");
            var generateVariant = stack.Add(new RandomChanceUI("Generate Variant Pokemon", model.CreateVariants, model.VariantChance) { ToolTip = "Generate Variant Pokemon. The percentage is the chance that any given evolution line will become variants"});
            var optionTabs = generateVariant.BindEnabled(stack.Add(new TabControl() { MinHeight = 288, Margin = new System.Windows.Thickness(5) }));
            optionTabs.Add(TypeWeightTab(model));
            optionTabs.Add(BaseStatTab(model));
        }

        private TabItem TypeWeightTab(VariantPokemonDataModel model)
        {
            var stack = CreateStack();
            var typeWeightCb = stack.Add(new EnumComboBoxUI<TypeTransformationWeightOption>("Type Transformation Chances", TypeTransformationOptionDropdown, model.TypeTransformationOption) { ToolTip = typeWeightDropdownTooltip });
            typeWeightCb.BindVisibility(stack.Add(new WeightedSetUI<TypeTransformation>("Single Type Transformation Chances", model.SingleTypeTransformationWeights, GetSingleTypeTransformationDropdown, singleTypeWeightTooltip)), (int)TypeTransformationWeightOption.Custom);
            typeWeightCb.BindVisibility(stack.Add(new WeightedSetUI<TypeTransformation>("Dual Type Transformation Chances", model.DualTypeTransformationWeights, GetDualTypeTransformationDropdown, dualTypeWeightTooltip)), (int)TypeTransformationWeightOption.Custom);
            return CreateTabItem("Type Transformation Options", stack);
        }

        private TabItem BaseStatTab(VariantPokemonDataModel model)
        {
            var stack = CreateStack();
            stack.Add(new BoundCheckBoxUI("Rebalance Attacking Stats", model.AdjustAttackStats, rebalanceAttackingStatsTooltip));
            var bonusStatCb = stack.Add(new BoundCheckBoxUI("Bonus Stats", model.GiveBonusStats) { ToolTip = "Give variant Pokemon Evolution Lines a small buff to their base stats"});
            bonusStatCb.BindVisibility(stack.Add(new BoundSliderUI("Bonus Stat Amount Average", model.BonusStatAmountMean, false, 0.5, 0, 50) { ToolTip = "The average amount of base stats granted to each evolution line. Full calculation: Average +/- Variance" }));
            bonusStatCb.BindVisibility(stack.Add(new BoundSliderUI("Bonus Stat Amount Variance", model.BonusStatAmountStdDev, false, 0.5, 0, 25) { ToolTip = "The amount of variance in the amount of base stats granted to each evolution line. Full calculation: Average +/- Variance"}));
            return CreateTabItem("Base Stat Options", stack);
        }

        private List<WeightedSetUI<TypeTransformation>.MenuBoxItem> GetSingleTypeTransformationDropdown() => new List<WeightedSetUI<TypeTransformation>.MenuBoxItem>
        {
            new WeightedSetUI<TypeTransformation>.MenuBoxItem { Item = TypeTransformation.SingleTypeReplacement, Header="Single Type Replacement", ToolTip = "The chance that a single-typed pokemon will become a different single type"},
            new WeightedSetUI<TypeTransformation>.MenuBoxItem { Item = TypeTransformation.GainSecondaryType, Header="Gain Secondary Type", ToolTip = "The chance that a single-typed pokemon will gain a new secondary type"},
            new WeightedSetUI<TypeTransformation>.MenuBoxItem { Item = TypeTransformation.DoubleTypeReplacement, Header="Double Type Replacement", ToolTip="The chance that a single-typed pokemon will become a dual-typed pokemon of two new types"},
        };

        private List<WeightedSetUI<TypeTransformation>.MenuBoxItem> GetDualTypeTransformationDropdown() => new List<WeightedSetUI<TypeTransformation>.MenuBoxItem>
        {
            new WeightedSetUI<TypeTransformation>.MenuBoxItem { Item = TypeTransformation.SecondaryTypeReplacement, Header="Secondary Type Replacement", ToolTip = "The chance that a dual-typed pokemon's secondary type will become a different type"},
            new WeightedSetUI<TypeTransformation>.MenuBoxItem { Item = TypeTransformation.PrimaryTypeReplacement, Header="Primary Type Replacement", ToolTip = "The chance that a dual-typed pokemon's primary type will become a different type"},
            new WeightedSetUI<TypeTransformation>.MenuBoxItem { Item = TypeTransformation.DoubleTypeReplacement, Header="Double Type Replacement", ToolTip = "The chance that a dual-typed pokemon's primary and secondary types will become different types"},
            new WeightedSetUI<TypeTransformation>.MenuBoxItem { Item = TypeTransformation.TypeLoss, Header="Become Single Typed", ToolTip="The chance that a dual-typed pokemon will become a single-typed pokemon of a new type"},
        };
    }
}
