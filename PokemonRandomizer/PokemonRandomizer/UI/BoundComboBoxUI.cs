using System;
using System.Collections;
using System.Windows.Controls;
using System.Linq;

namespace PokemonRandomizer.UI
{
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

        public T BindVisibility<T>(T element, int index) where T : UIElement
        {
            ComboBox.SelectionChanged += (_, _2) => element.SetVisibility(ComboBox.SelectedIndex == index);
            element.SetVisibility(ComboBox.SelectedIndex == index);
            return element;
        }

        public T BindVisibility<T>(T element, params int[] indices) where T : UIElement
        {
            ComboBox.SelectionChanged += (_, _2) => element.SetVisibility(indices.Contains(ComboBox.SelectedIndex));
            element.SetVisibility(indices.Contains(ComboBox.SelectedIndex));
            return element;
        }
    }
}
