using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace PokemonRandomizer.UI
{
    public static class UIExtensions
    {
        private static Separator StdSeparator => new Separator() { Height = 1, UseLayoutRounding=true };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Add<T>(this Panel p, T element) where T : UIElement
        {
            p.Children.Add(element);
            return element;
        }
        public static void Add(this Panel p, params UIElement[] elements)
        {
            foreach(var element in elements)
            {
                p.Children.Add(element);
            }
        }
        public static void Header(this Panel p, string content)
        {
            p.Add(new Label { Content = content }, StdSeparator);
        }
        public static void Header(this Panel p, string content, string tooltip)
        {
            p.Add(new Label { Content = content, ToolTip = tooltip }, StdSeparator);
        }

        public static void Description(this Panel p, params string[] content)
        {
            foreach(var line in content)
            {
                p.Add(new Label() { Content = line });
            }
            p.Add(StdSeparator);
        }

        public static void Separator(this Panel p)
        {
            p.Add(StdSeparator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Add<T>(this ItemsControl control, T element) where T : UIElement
        {
            control.Items.Add(element);
            return element;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetVisibility(this UIElement element, bool visible)
        {
            element.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
