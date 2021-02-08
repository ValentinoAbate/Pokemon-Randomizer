using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonRandomizer.Converters;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Data;

namespace PokemonRandomizer.UI
{
    public class PercentSliderUI : StackPanel
    {
        public PercentSliderUI(string name, double value, Action<double> onValueChange, double freq = 0.01, double min = 0, double max = 1) : base()
        {
            Orientation = Orientation.Horizontal;
            Children.Add(new Label() { Content = name, FontSize = 11 });
            var textBox = new TextBox()
            {
                Margin = new Thickness(3),
                Width = 45,
                Text = PercentConverter.ToPercentString(value)
            };
            var slider = new Slider()
            {
                Value = value,
                Margin = new Thickness(3,4,0,0),
                Minimum = min,
                Maximum = max,
                TickFrequency = freq,
                IsSnapToTickEnabled = true,
                Width = 120,
            };
            // Bind textbox and slider together
            Binding myBinding = new Binding
            {
                Source = slider,
                Path = new PropertyPath("Value"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Converter = new PercentConverter(),
            };
            BindingOperations.SetBinding(textBox, TextBox.TextProperty, myBinding);
            slider.ValueChanged += (_, args) => onValueChange?.Invoke(args.NewValue);
            Children.Add(slider);
            Children.Add(textBox);
        }
    }
}
