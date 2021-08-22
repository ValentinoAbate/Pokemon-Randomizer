using System.Windows;
using System.Windows.Controls;

namespace PokemonRandomizer.UI.Views
{
    using Models;
    public abstract class DataView<T> : ContentControl where T : DataModel
    {
        protected StackPanel CreateStack() => new StackPanel() { Orientation = Orientation.Vertical };

        protected TabItem CreateTabItem(string header, UIElement content) => new TabItem() { Header = header, Content = content };

        protected TabControl CreateMainTabControl()
        {
            var tabs = new TabControl();
            Content = tabs;
            return tabs;
        }
    }
}
