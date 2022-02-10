using PokemonRandomizer.UI.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Controls;

namespace PokemonRandomizer.UI
{
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
