using System.Windows.Controls;

namespace PokemonRandomizer.UI
{
    public class TypedComboBoxItem<T> : ComboBoxItem
    {
        public T Item { get; set; }
    }
}
