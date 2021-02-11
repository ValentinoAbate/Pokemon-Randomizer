using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PokemonRandomizer.UI
{
    public class BoundCheckBoxUI : CheckBox
    {
        public BoundCheckBoxUI(bool isChecked, Action<bool> onEnabledChange) : base()
        {
            IsChecked = isChecked;
            Checked += (_, _2) => onEnabledChange?.Invoke(true);
            Unchecked += (_, _2) => onEnabledChange?.Invoke(false);
        }

        public BoundCheckBoxUI(bool isChecked, Action<bool> onEnabledChange, string label) : this(isChecked, onEnabledChange)
        {
            Content = label;
        }

        public BoundCheckBoxUI(bool isChecked, Action<bool> onEnabledChange, string label, string tooltip) : this(isChecked, onEnabledChange, label)
        {
            ToolTip = tooltip;
        }
    }
}
