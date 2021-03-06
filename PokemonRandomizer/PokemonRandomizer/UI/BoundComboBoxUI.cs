﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using PokemonRandomizer.Backend.Utilities;

namespace PokemonRandomizer.UI
{
    public class BoundComboBoxUI : StackPanel
    {
        public BoundComboBoxUI(string label, System.Collections.IEnumerable items, int currentIndex, Action<int> onIndexChange, Orientation orientation = Orientation.Horizontal) : base()
        {
            Orientation = orientation;
            if(!string.IsNullOrWhiteSpace(label))
            {
                Children.Add(new Label() { Content = label });
            }
            var cBox = new ComboBox() {  ItemsSource = items, SelectedIndex = currentIndex };
            Children.Add(cBox);
            cBox.SelectionChanged += (_, _2) => onIndexChange?.Invoke(cBox.SelectedIndex);
            Margin = new System.Windows.Thickness(2);
        }
    }
}
