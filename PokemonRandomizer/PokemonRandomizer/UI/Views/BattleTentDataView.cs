using PokemonRandomizer.UI.Models;
using System;
using System.Windows.Controls;
using System.Windows.Data;
using static PokemonRandomizer.UI.Models.BattleFrontierDataModel;

namespace PokemonRandomizer.UI.Views
{
    public class BattleTentDataView : DataView<BattleTentDataModel>
    {
        private const string level30Tooltip = BattleFrontierDataView.levelTooltip + "30";
        private const string level30RentalTooltip = level30Tooltip + " (rental pokemon in the Slateport Battle Tent are level 30)";
        private static CompositeCollection RentalScalingDropdown => new()
        {
            new ComboBoxItem() { Content = "Power Scaled", ToolTip = BattleFrontierDataView.powerScaledTooltip },
            new ComboBoxItem() { Content = "Level 100", ToolTip = BattleFrontierDataView.level100Tooltip },
            new ComboBoxItem() { Content = "Level 50", ToolTip = BattleFrontierDataView.level50Tooltip },
            new ComboBoxItem() { Content = "Level 30", ToolTip = level30RentalTooltip },
        };

        private static CompositeCollection PowerScalingDropDown => new()
        {
            new ComboBoxItem() { Content = "Power Scaled", ToolTip = BattleFrontierDataView.powerScaledTooltip },
            new ComboBoxItem() { Content = "Level 100", ToolTip = BattleFrontierDataView.level100Tooltip },
            new ComboBoxItem() { Content = "Level 50", ToolTip = BattleFrontierDataView.level50Tooltip },
            new ComboBoxItem() { Content = "Level 30", ToolTip = level30Tooltip },
        };
        public BattleTentDataView(BattleTentDataModel model)
        {
            var stack = CreateMainStack();
            stack.Header("Battle Tent Pokemon Randomization (Fallarbor and Verdanturf Battle Tents)");
            var pokemonRand = stack.Add(new RandomChanceUI("Randomize Battle Tent Pokemon", model.RandomizePokemon, model.PokemonRandChance));
            var pokemonRandStack = stack.Add(pokemonRand.BindEnabled(CreateStack()));
            pokemonRandStack.Add(new EnumComboBoxUI<FrontierPokemonRandStrategy>("Power Scaling", PowerScalingDropDown, model.PokemonRandStrategy));
            pokemonRandStack.Add(new SpecialMoveSettingsUI(model.SpecialMoveSettings));
            pokemonRandStack.Add(new BoundCheckBoxUI("Ban Legendaries", model.BanLegendaries));

            stack.Header("Rental Pokemon Randomization (Slateport Battle Tent)");
            var rentalPokemonRand = stack.Add(new RandomChanceUI("Randomize Rental Pokemon", model.RandomizeRentalPokemon, model.RentalPokemonRandChance));
            var rentalPokemonStack = stack.Add(rentalPokemonRand.BindEnabled(CreateStack()));
            rentalPokemonStack.Add(new EnumComboBoxUI<FrontierPokemonRandStrategy>("Power Scaling", RentalScalingDropdown, model.RentalPokemonRandStrategy));
            rentalPokemonStack.Add(new SpecialMoveSettingsUI(model.RentalSpecialMoveSettings));
            rentalPokemonStack.Add(new BoundCheckBoxUI("Ban Legendaries", model.BanRentalLegendaries));

            stack.Header("Battle Tent Prize Randomization");
            stack.Add(new BoundCheckBoxUI("Randomize Battle Tent Prizes", model.RandomizePrizes));
        }
    }
}
