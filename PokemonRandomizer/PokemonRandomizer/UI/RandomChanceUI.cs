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
        private const string randomizePrefix = "Randomize ";
        private const string chanceText = "Chance";

        public RandomChanceUI(string featureName, double chance, Action<double> onChanceChange, Orientation orientation, Control additionalContent = null) : this(featureName, chance > 0, null, chance, onChanceChange, orientation, additionalContent)
        {

        }

        public RandomChanceUI(string featureName, bool enabled, Action<bool> onEnabledChange, double chance, Action<double> onChanceChange, Orientation orientation, Control additionalContent = null) : base()
        {
            var border = new Border() { BorderThickness = new Thickness(0.5), Margin = new Thickness(2), BorderBrush = System.Windows.Media.Brushes.Black };
            var mainStack = new StackPanel() { Orientation = orientation, Margin = new Thickness(3) };
            border.Child = mainStack;
            var contentStack = new StackPanel() { Orientation = orientation, Margin = new Thickness(3), IsEnabled = enabled };
            // Set up checkbox
            void OnCheckBox(bool value)
            {
                onEnabledChange?.Invoke(value);
                contentStack.IsEnabled = value;
            }
            var checkBox = new BoundCheckBoxUI(OnCheckBox) { Content = randomizePrefix + featureName, VerticalAlignment = VerticalAlignment.Center, IsChecked = enabled };
            
            // Add content to main stack
            mainStack.Children.Add(checkBox);
            mainStack.Children.Add(new Separator());
            mainStack.Children.Add(contentStack);

            // Add content to content stack
            contentStack.Children.Add(new PercentSliderUI(chanceText, chance, onChanceChange));
            if(additionalContent != null)
                contentStack.Children.Add(additionalContent);

            Content = border;
        }
    }
}
