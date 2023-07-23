using PokemonRandomizer.UI.Models;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Data;
using static PokemonRandomizer.UI.Models.BattleFrontierDataModel;

namespace PokemonRandomizer.UI.Views
{
    public class BattleFrontierDataView : DataView<BattleFrontierDataModel>
    {
        public const string powerScaledTooltip = "Pokemon and move choices will be scaled by the original pokemon's power and moveset";
        public const string levelTooltip = "Pokemon and move choices will be calculated as if the pokemon were level ";
        public const string level100Tooltip = levelTooltip + "100";
        public const string level50Tooltip = levelTooltip + "50";
        private static CompositeCollection PowerScalingDropdown => new()
        {
            new ComboBoxItem() { Content = "Power Scaled", ToolTip = powerScaledTooltip},
            new ComboBoxItem() { Content = "Level 100", ToolTip = level100Tooltip },
            new ComboBoxItem() { Content = "Level 50", ToolTip = level50Tooltip },
        };
        private const string brainLegendaryBanTooltip = "Prevent legendary pokemon from being randomly chosen as Frontier Brain pokemon" +
            "\nLegendaries can still be chosen as Frontier Brain pokemon when the unrandomized pokemon is legendary and \"Ensure Randomized Legendaries are Legendary\" is selected";


        public BattleFrontierDataView(BattleFrontierDataModel model)
        {
            var stack = CreateMainStack();
            stack.Header("Battle Frontier Pokemon Randomization");
            var pokemonRand = stack.Add(new RandomChanceUI("Randomize Battle Frontier Pokemon", model.RandomizePokemon, model.PokemonRandChance));
            var pokemonRandStack = stack.Add(pokemonRand.BindEnabled(CreateStack()));
            pokemonRandStack.Add(new EnumComboBoxUI<FrontierPokemonRandStrategy>("Power Scaling", PowerScalingDropdown, model.PokemonRandStrategy));
            pokemonRandStack.Add(new SpecialMoveSettingsUI(model.SpecialMoveSettings));
            pokemonRandStack.Add(new BoundCheckBoxUI("Ban Legendaries", model.BanLegendaries));

            stack.Header("Frontier Brain Pokemon Randomization");
            var brainPokemonRand = stack.Add(new RandomChanceUI("Randomize Frontier Brain Pokemon", model.RandomizeBrainPokemon, model.BrainPokemonRandChance));
            var brainPokemonStack = stack.Add(brainPokemonRand.BindEnabled(CreateStack()));
            brainPokemonStack.Add(new SpecialMoveSettingsUI(model.BrainSpecialMoveSettings));
            brainPokemonStack.Add(new BoundCheckBoxUI("Ban Legendaries", model.BanBrainLegendaries, brainLegendaryBanTooltip));
            brainPokemonStack.Add(new BoundCheckBoxUI("Ensure Randomized Legendaries are Legendary", model.KeepBrainLegendaries, "Ensure that legendary Frontier Brain pokemon always randomize to other legendaries"));
        }
    }
}
