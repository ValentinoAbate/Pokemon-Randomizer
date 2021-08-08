using PokemonRandomizer.Converters;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace PokemonRandomizer.UI
{
    public class BoundSliderUI : StackPanel
    {
        public double Value
        {
            get => slider.Value;
            set => slider.Value = value;
        }
        public double Min
        {
            get => slider.Minimum;
            set => slider.Minimum = value;
        }
        public double Max
        {
            get => slider.Maximum;
            set => slider.Maximum = value;
        }

        private readonly Slider slider;
        public BoundSliderUI(string name, double value, Action<double> onValueChange, bool percent = true, double freq = 0.01, double min = 0, double max = 1) : base()
        {
            Orientation = Orientation.Horizontal;
            Children.Add(new Label() { Content = name });
            var textBox = new TextBox()
            {
                Margin = new Thickness(3),
                Width = 45,
                Text = PercentConverter.ToPercentString(value)
            };
            slider = new Slider()
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
            };
            if(percent)
            {
                myBinding.Converter = new PercentConverter();
            }
            BindingOperations.SetBinding(textBox, TextBox.TextProperty, myBinding);
            slider.ValueChanged += (_, args) => onValueChange?.Invoke(args.NewValue);
            Children.Add(slider);
            Children.Add(textBox);
        }
    }
}
