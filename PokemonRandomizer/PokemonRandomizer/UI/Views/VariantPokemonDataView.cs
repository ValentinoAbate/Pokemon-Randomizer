using PokemonRandomizer.UI.Models;
using System.Windows.Controls;
using System.Windows.Data;
using static PokemonRandomizer.UI.Models.VariantPokemonDataModel;
using static PokemonRandomizer.Backend.Randomization.PokemonVariantRandomizer;
using System.Collections.Generic;

namespace PokemonRandomizer.UI.Views
{
    public class VariantPokemonDataView : DataView<VariantPokemonDataModel>
    {
        public CompositeCollection StarterOptionDropdown => new CompositeCollection()
        {
            new ComboBoxItem() {Content="Random"},
            new ComboBoxItem() {Content="Balanced (Recommended)"},
            new ComboBoxItem() {Content="Custom"},
        };

        public VariantPokemonDataView(VariantPokemonDataModel model)
        {
            var stack = CreateMainStack();
            stack.Header("Variant Pokemon");
            stack.Description("This feature creates type variants of Pokemon, and edits their movesets, color palettes, base stats, and EV yields to fit their new type!", "This feature is designed to be configured at a 10-25% chance.");
            var generateVariant = stack.Add(new RandomChanceUI("Generate Variant Pokemon", model.CreateVariants, model.VariantChance));
            var optionTabs = generateVariant.BindEnabled(stack.Add(new TabControl() { MinHeight = 288, Margin = new System.Windows.Thickness(5) }));
            optionTabs.Add(TypeWeightTab(model));
            optionTabs.Add(BaseStatTab(model));
        }

        private TabItem TypeWeightTab(VariantPokemonDataModel model)
        {
            var stack = CreateStack();
            var typeWeightCb = stack.Add(new EnumComboBoxUI<TypeTransformationWeightOption>("Type Transformation Weights", StarterOptionDropdown, model.TypeTransformationOption));
            typeWeightCb.BindVisibility(stack.Add(new WeightedSetUI<TypeTransformation>("Single Type Transformation Weights", model.SingleTypeTransformationWeights, GetSingleTypeTransformationDropdown)), (int)TypeTransformationWeightOption.Custom);
            typeWeightCb.BindVisibility(stack.Add(new WeightedSetUI<TypeTransformation>("Dual Type Transformation Weights", model.DualTypeTransformationWeights, GetDualTypeTransformationDropdown)), (int)TypeTransformationWeightOption.Custom);
            return CreateTabItem("Type Transformation Options", stack);
        }

        private TabItem BaseStatTab(VariantPokemonDataModel model)
        {
            var stack = CreateStack();
            stack.Add(new BoundCheckBoxUI("Rebalance Attacking Stats", model.AdjustAttackStats));
            var bonusStatCb = stack.Add(new BoundCheckBoxUI("Bonus Stats", model.GiveBonusStats));
            bonusStatCb.BindVisibility(stack.Add(new BoundSliderUI("Bonus Stat Amount Average", model.BonusStatAmountMean, false, 0.5, 0, 50)));
            bonusStatCb.BindVisibility(stack.Add(new BoundSliderUI("Bonus Stat Amount Variance", model.BonusStatAmountStdDev, false, 0.5, 0, 25)));
            return CreateTabItem("Base Stat Options", stack);
        }

        private List<WeightedSetUI<TypeTransformation>.MenuBoxItem> GetSingleTypeTransformationDropdown() => new List<WeightedSetUI<TypeTransformation>.MenuBoxItem>
        {
            new WeightedSetUI<TypeTransformation>.MenuBoxItem { Item = TypeTransformation.SingleTypeReplacement, Header="Single Type Replacement"},
            new WeightedSetUI<TypeTransformation>.MenuBoxItem { Item = TypeTransformation.GainSecondaryType, Header="Gain Secondary Type"},
            new WeightedSetUI<TypeTransformation>.MenuBoxItem { Item = TypeTransformation.DoubleTypeReplacement, Header="Double Type Replacement"},
        };

        private List<WeightedSetUI<TypeTransformation>.MenuBoxItem> GetDualTypeTransformationDropdown() => new List<WeightedSetUI<TypeTransformation>.MenuBoxItem>
        {
            new WeightedSetUI<TypeTransformation>.MenuBoxItem { Item = TypeTransformation.SecondaryTypeReplacement, Header="Secondary Type Replacement"},
            new WeightedSetUI<TypeTransformation>.MenuBoxItem { Item = TypeTransformation.PrimaryTypeReplacement, Header="Primary Type Replacement"},
            new WeightedSetUI<TypeTransformation>.MenuBoxItem { Item = TypeTransformation.DoubleTypeReplacement, Header="Double Type Replacement"},
            new WeightedSetUI<TypeTransformation>.MenuBoxItem { Item = TypeTransformation.TypeLoss, Header="Become Single Typed", ToolTip="The dual-type pokemon will tranform into a single type pokemon of a new type"},
        };
    }
}
