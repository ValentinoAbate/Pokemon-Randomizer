using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Data;

namespace PokemonRandomizer.UI.Views
{
    using Models;
    using static Settings;
    using Backend.EnumTypes;
    public class StartersDataView : DataView<StartersDataModel>
    {
        public CompositeCollection StarterOptionDropdown => new CompositeCollection()
        {
            new ComboBoxItem() {Content="Unchanged", ToolTip="Leave the starter pokemon as they are in the base game" },
            new ComboBoxItem() {Content="Random", ToolTip="Randomly select three unevolved pokemon as starters" },
            new ComboBoxItem() {Content="Random Type Triangle", ToolTip="Randomly select three unevolved pokemon that form a triangle where each is strong against the next as starters"},
            new ComboBoxItem() {Content="Custom", ToolTip="Set 1 or more custom starters"},
        };
        private const string strongTriTooltip = "Only generate type triangles where each pokemon is super effective against AND resistant to the next (as opposed to just super effective against)";
        private const string safeMovesetsTooltip = "Ensure that starters will have attacking move(s) at level 5 that can hit all pokemon in the game" +
            "\nAny starter whose moveset is unsafe will be given Foresight or Odor Sleuth, and Tackle / Astonish if necessary" +
            "\nAny moves that would be skipped over at level 5 due to added moves will instead be learned at level 6";
        private const string banLegendariesTooltip = "Ban legendaries from being chosen as random starters. Legendaries specifically selected as custom starters will not be affected";
        public StartersDataView(StartersDataModel model, string[] pokemonNames, List<Pokemon> pokemon)
        {
            // Create stack and add content
            var stack = CreateMainStack();
            stack.Header("Randomization");

            // Randomization Strategy CB
            var optionCb = stack.Add(new EnumComboBoxUI<StarterPokemonOption>("Randomization Strategy", StarterOptionDropdown, model.StarterSetting) { ToolTip="The strategy used to select starter pokemon" });
            // Type Triangle UI
            optionCb.BindVisibility(stack.Add(new BoundCheckBoxUI("Force Strong Type Triangle", model.StrongStarterTypeTriangle, strongTriTooltip)), (int)StarterPokemonOption.RandomTypeTriangle);

            // Custom Starter UI
            var pokemonOptions = new List<string>(pokemonNames.Length + 1) { "Random" };
            pokemonOptions.AddRange(pokemonNames);
            BoundComboBoxUI CustomStarterCB(int index)
            {
                return new BoundComboBoxUI("", pokemonOptions, pokemon.IndexOf(model.CustomStarters[index]), i => model.CustomStarters[index] = pokemon[i]);
            }
            var customStarterStack = stack.Add(CreateHorizontalStack());
            customStarterStack.Add(new Label() { Content = "Custom Starters:", ToolTip="The custom starter pokemon to use (any pokemon set to \"Random\" will be a random unevolved pokemon)" }, CustomStarterCB(0), CustomStarterCB(1), CustomStarterCB(2));
            optionCb.BindVisibility(customStarterStack, (int)StarterPokemonOption.Custom);

            // Additional Settings
            stack.Add(optionCb.BindEnabled(new BoundCheckBoxUI("Ban Legendaries", model.BanLegendaries, banLegendariesTooltip), (int)StarterPokemonOption.Random, (int)StarterPokemonOption.RandomTypeTriangle, (int)StarterPokemonOption.Custom));
            stack.Header("Safety Checks");
            stack.Add(new BoundCheckBoxUI("Safe Starter Movesets", model.SafeStarterMovesets, safeMovesetsTooltip));
        }
    }
}
