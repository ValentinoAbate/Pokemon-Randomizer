using System;
using System.Collections;
using System.Windows.Controls;

namespace PokemonRandomizer.UI
{
    using System.Collections.Generic;
    using System.Windows;
    using Utilities;
    public class BoundComboBoxUI : StackPanel
    {
        public ComboBox ComboBox { get; private set; }

        public BoundComboBoxUI(string label, IEnumerable items, Box<int> index, Orientation orientation = Orientation.Horizontal)
            : this(label, items, index, i => index.Value = i, orientation) { }
        public BoundComboBoxUI(string label, IEnumerable items, int currentIndex, Action<int> onIndexChange, Orientation orientation = Orientation.Horizontal) : base()
        {
            Orientation = orientation;
            if(!string.IsNullOrWhiteSpace(label))
            {
                Children.Add(new Label() { Content = label });
            }
            ComboBox = new ComboBox() {  ItemsSource = items, SelectedIndex = currentIndex };
            Children.Add(ComboBox);
            ComboBox.SelectionChanged += (_, _2) => onIndexChange?.Invoke(ComboBox.SelectedIndex);
            Margin = new System.Windows.Thickness(0,2,2,2);
        }

        public void BindVisibility(UIElement element, int index)
        {
            ComboBox.SelectionChanged += (_, _2) => element.SetVisibility(ComboBox.SelectedIndex == index);
            element.SetVisibility(ComboBox.SelectedIndex == index);
        }
    }

    public class EnumComboBoxUI<T> : BoundComboBoxUI where T : Enum
    {
        public EnumComboBoxUI(string label, IEnumerable items, Box<T> index, Orientation orientation = Orientation.Horizontal) 
            : base(label, items, Convert.ToInt32(index.Value), i => index.Value = (T)Enum.Parse(typeof(T), i.ToString()), orientation)
        {
        }

        public EnumComboBoxUI(string label, IEnumerable displayOptions, Box<T> current, List<T> options, Orientation orientation = Orientation.Horizontal)
            : base(label, displayOptions, options.IndexOf(current), i => current.Value = options[i], orientation)
        {
        }
    }
}
