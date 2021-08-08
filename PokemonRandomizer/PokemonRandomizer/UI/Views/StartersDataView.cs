using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Data;

namespace PokemonRandomizer.UI
{
    public class StartersDataView : DataView<StartersDataModel>
    {
        public CompositeCollection StarterOptionDropdown => new CompositeCollection()
        {
            new ComboBoxItem() {Content="Unchanged" },
            new ComboBoxItem() {Content="Random" },
            new ComboBoxItem() {Content="Random Type Triangle", ToolTip="Randomly select three starters where each is strong against the next"},
            new ComboBoxItem() {Content="Custom", ToolTip="Set 1 or more custom starters"},
        };
        public StartersDataView(StartersDataModel model, string[] pokemonNames)
        {
            var legendCb = new BoundCheckBoxUI(model.BanLegendaries, (b) => model.BanLegendaries = b, "Ban Legendaries");
            var safeMovesetsCb = new BoundCheckBoxUI(model.SafeStarterMovesets, (b) => model.SafeStarterMovesets = b, "Safe Starter Movesets");
            var strongTriangleCb = new BoundCheckBoxUI(model.StrongStarterTypeTriangle, (b) => model.StrongStarterTypeTriangle = b, "Force Strong Type Triangle");

            // Custom Starter UI
            var pokemonOptions = new List<string>(pokemonNames.Length + 1) { "Random" };
            pokemonOptions.AddRange(pokemonNames);
            BoundComboBoxUI CustomStarterCB(int index)
            {
                return new BoundComboBoxUI("", pokemonOptions, pokemonOptions.IndexOf(model.CustomStarters[index]), (i) => model.CustomStarters[index] = pokemonOptions[i]);
            }
            var customStarterStack = new StackPanel() { Orientation = Orientation.Horizontal };
            customStarterStack.Add(new Label() { Content = "Custom Starters:" }, CustomStarterCB(0), CustomStarterCB(1), CustomStarterCB(2));

            void OnMainOptionChange(int option)
            {
                model.StarterSetting = (Settings.StarterPokemonOption)option;
                strongTriangleCb.SetVisibility(option == (int)Settings.StarterPokemonOption.RandomTypeTriangle);
                customStarterStack.SetVisibility(option == (int)Settings.StarterPokemonOption.Custom);
            }

            OnMainOptionChange((int)model.StarterSetting);
            var mainOptionBox = new BoundComboBoxUI("Randomization Strategy", StarterOptionDropdown, (int)model.StarterSetting, OnMainOptionChange);
            // Create stack and add content
            var stack = new StackPanel() { Orientation = Orientation.Vertical };
            Content = stack;
            stack.Add(new Label() { Content = "Starter Randomization" });
            stack.Add(new Separator());
            stack.Add(mainOptionBox, customStarterStack, strongTriangleCb, legendCb, safeMovesetsCb);
        }
    }
}
