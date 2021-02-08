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
        public BoundCheckBoxUI(Action<bool> onEnabledChange) : base()
        {
            Checked += (_, _2) => onEnabledChange?.Invoke(true);
            Unchecked += (_, _2) => onEnabledChange?.Invoke(false);
        }
    }
}
