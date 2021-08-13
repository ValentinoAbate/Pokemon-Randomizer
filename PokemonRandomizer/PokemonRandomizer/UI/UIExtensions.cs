using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace PokemonRandomizer.UI
{
    public static class UIExtensions
    {
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetVisibility(this UIElement element, bool visible)
        {
            element.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
