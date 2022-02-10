using System.Windows.Controls;

namespace PokemonRandomizer.UI
{
    using Utilities;
    public class BoundTextBoxUI : StackPanel
    {
        public BoundTextBoxUI(string label, Box<string> value)
        {
            Margin = new System.Windows.Thickness(0, 2, 2, 2);
            Orientation = Orientation.Horizontal;
            if (!string.IsNullOrWhiteSpace(label))
                this.Add(new Label() { Content = label });
            var tb = new TextBox { Text = value, VerticalContentAlignment = System.Windows.VerticalAlignment.Center, MinWidth = 100 };
            tb.TextChanged += (_, _2) => value.Value = tb.Text;
            this.Add(tb);
        }
    }
}
