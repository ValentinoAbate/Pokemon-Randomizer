using System;
using System.Collections;
using System.Windows.Controls;

namespace PokemonRandomizer.UI
{
    public class BoundComboBoxUI : StackPanel
    {
        public ComboBox ComboBox { get; private set; }
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
    }
}
