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

        public RandomChanceUI(string featureName, double chance, Action<double> onChanceChange, Orientation orientation, Control additionalContent = null) : this(featureName, chance > 0, null, chance, onChanceChange, orientation, additionalContent)
        {

        }

        public RandomChanceUI(string featureName, bool enabled, Action<bool> onEnabledChange, double chance, Action<double> onChanceChange, Orientation orientation, Control additionalContent = null) : base()
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
            }
            var checkBox = new BoundCheckBoxUI(OnCheckBox) { Content = featureName, VerticalAlignment = VerticalAlignment.Center, IsChecked = enabled };
            
            // Add content to main stack
            mainStack.Children.Add(checkBox);
            mainStack.Children.Add(new Separator());
            mainStack.Children.Add(contentStack);

            // Add content to content stack
            contentStack.Children.Add(new BoundSliderUI(chanceText, chance, onChanceChange));
            if(additionalContent != null)
                contentStack.Children.Add(additionalContent);

            Content = mainStack;
        }
    }
}
