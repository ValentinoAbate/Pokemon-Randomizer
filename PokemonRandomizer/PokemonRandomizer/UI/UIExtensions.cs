using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PokemonRandomizer.UI
{
    public static class UIExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(this Panel p, UIElement element)
        {
            p.Children.Add(element);
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
