using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace PokemonRandomizer.UI
{
    public class StartersDataView : DataView<StartersDataModel>
    {
        public StartersDataView(StartersDataModel model, string[] pokemonNames)
        {
            var legendCb = new BoundCheckBoxUI((b) => model.BanLegendaries = b) { Content = "Ban Legendaries", IsChecked = model.BanLegendaries };
            var safeMovesetsCb = new BoundCheckBoxUI((b) => model.SafeStarterMovesets = b) { Content = "Safe Starter Movesets", IsChecked = model.SafeStarterMovesets };
            var strongTriangleCb = new BoundCheckBoxUI((b) => model.StrongStarterTypeTriangle = b) { Content = "Force Strong Type Triangle", IsChecked = model.StrongStarterTypeTriangle };

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
            var mainOptionBox = new BoundComboBoxUI("Starter Randomization", StartersDataModel.StarterOptionDropdown, (int)model.StarterSetting, OnMainOptionChange);
            // Create stack and add content
            var contentStack = new StackPanel() { Orientation = Orientation.Vertical };
            Content = contentStack;
            contentStack.Add(mainOptionBox, customStarterStack, strongTriangleCb, legendCb, safeMovesetsCb);
        }
    }
}
