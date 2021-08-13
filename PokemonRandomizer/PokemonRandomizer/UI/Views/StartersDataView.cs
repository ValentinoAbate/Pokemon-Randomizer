using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Data;

namespace PokemonRandomizer.UI
{
    using static Settings;
    using Backend.EnumTypes;
    public class StartersDataView : DataView<StartersDataModel>
    {
        public CompositeCollection StarterOptionDropdown => new CompositeCollection()
        {
            new ComboBoxItem() {Content="Unchanged" },
            new ComboBoxItem() {Content="Random" },
            new ComboBoxItem() {Content="Random Type Triangle", ToolTip="Randomly select three starters where each is strong against the next"},
            new ComboBoxItem() {Content="Custom", ToolTip="Set 1 or more custom starters"},
        };
        private const string strongTriTooltip = "Only generate type triangles where each pokemon is super effective against AND resistant to the next (as opposed to just super effective against)";
        public StartersDataView(StartersDataModel model, string[] pokemonNames, List<Pokemon> pokemon)
        {
            // Create stack and add content
            var stack = new StackPanel() { Orientation = Orientation.Vertical };
            Content = stack;
            stack.Add(new Label() { Content = "Starter Randomization" });
            stack.Add(new Separator());

            // Randomization Strategy CB
            var optionCb = stack.Add(new EnumComboBoxUI<StarterPokemonOption>("Randomization Strategy", StarterOptionDropdown, model.StarterSetting));
            // Type Triangle UI
            optionCb.BindVisibility(stack.Add(new BoundCheckBoxUI(model.StrongStarterTypeTriangle, "Force Strong Type Triangle", strongTriTooltip)), (int)StarterPokemonOption.RandomTypeTriangle);

            // Custom Starter UI
            var pokemonOptions = new List<string>(pokemonNames.Length + 1) { "Random" };
            pokemonOptions.AddRange(pokemonNames);
            BoundComboBoxUI CustomStarterCB(int index)
            {
                return new BoundComboBoxUI("", pokemonOptions, pokemon.IndexOf(model.CustomStarters[index]), i => model.CustomStarters[index] = pokemon[i]);
            }
            var customStarterStack = stack.Add(new StackPanel() { Orientation = Orientation.Horizontal });
            customStarterStack.Add(new Label() { Content = "Custom Starters:" }, CustomStarterCB(0), CustomStarterCB(1), CustomStarterCB(2));
            optionCb.BindVisibility(customStarterStack, (int)StarterPokemonOption.Custom);

            // Additional Settings
            stack.Add(new BoundCheckBoxUI(model.BanLegendaries, "Ban Legendaries"));
            stack.Add(new BoundCheckBoxUI(model.SafeStarterMovesets, "Safe Starter Movesets"));
        }
    }
}
