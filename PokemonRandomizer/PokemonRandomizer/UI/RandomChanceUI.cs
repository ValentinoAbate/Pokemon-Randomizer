using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PokemonRandomizer.UI
{
    public class RandomChanceUI : ContentControl
    {
        private const string chanceText = "Chance";

        public RandomChanceUI(string featureName, bool enabled, Action<bool> onEnabledChange, double chance, Action<double> onChanceChange, UIElement enableOnCheck = null, Orientation orientation = Orientation.Horizontal) : base()
        {
            var mainStack = new StackPanel() { Orientation = orientation };
            double marginH = orientation == Orientation.Horizontal ? 0 : 1.5;
            double marginW = orientation == Orientation.Vertical ? 0 : 1.5;
            var contentStack = new StackPanel() { Orientation = orientation, Margin = new Thickness(marginW, marginH, marginW, marginH), IsEnabled = enabled };
            // Set up checkbox
            void OnCheckBox(bool value)
            {
                onEnabledChange?.Invoke(value);
                contentStack.IsEnabled = value;
                if(enableOnCheck != null)
                {
                    enableOnCheck.IsEnabled = value;
                }
            }
            var checkBox = new BoundCheckBoxUI(enabled, OnCheckBox, featureName) { VerticalAlignment = VerticalAlignment.Center};
            // Add content to main stack
            mainStack.Add(checkBox);
            mainStack.Add(contentStack);

            // Add content to content stack
            contentStack.Add(new BoundSliderUI(chanceText, chance, onChanceChange));

            Content = mainStack;
            OnCheckBox(enabled);
        }
    }
}
