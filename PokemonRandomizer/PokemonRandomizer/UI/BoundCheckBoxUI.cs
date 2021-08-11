using System;
using System.Windows;
using System.Windows.Controls;

namespace PokemonRandomizer.UI
{
    using Utilities;
    public class BoundCheckBoxUI : CheckBox
    {
        public BoundCheckBoxUI(Box<bool> box, UIElement enableOnChecked = null) : this(box, b => box.Value = b, enableOnChecked) { }
        public BoundCheckBoxUI(Box<bool> box, string label, UIElement enableOnChecked = null) : this(box, b => box.Value = b, label, enableOnChecked) { }
        public BoundCheckBoxUI(Box<bool> box, string label, string tooltip, UIElement enableOnChecked = null) : this(box, b => box.Value = b, label, tooltip, enableOnChecked) { }
        public BoundCheckBoxUI(bool isChecked, Action<bool> onEnabledChange, UIElement enableOnChecked = null) : base()
        {
            Margin = new Thickness(5, 2, 2, 2);
            IsChecked = isChecked;
            if(enableOnChecked != null)
            {
                enableOnChecked.IsEnabled = isChecked;
            }
            void OnChecked(object _, RoutedEventArgs _2)
            {
                onEnabledChange?.Invoke(true);
                if(enableOnChecked != null)
                    enableOnChecked.IsEnabled = true;
            }
            void OnUnchecked(object _, RoutedEventArgs _2)
            {
                onEnabledChange?.Invoke(false);
                if (enableOnChecked != null)
                    enableOnChecked.IsEnabled = false;
            }
            Checked += OnChecked;
            Unchecked += OnUnchecked;
        }

        public BoundCheckBoxUI(bool isChecked, Action<bool> onEnabledChange, string label, UIElement enableOnChecked = null) : this(isChecked, onEnabledChange, enableOnChecked)
        {
            Content = label;
        }

        public BoundCheckBoxUI(bool isChecked, Action<bool> onEnabledChange, string label, string tooltip, UIElement enableOnChecked = null) : this(isChecked, onEnabledChange, label, enableOnChecked)
        {
            ToolTip = tooltip;
        }

        public void BindVisibility(UIElement bindElement)
        {
            Checked += (_, _2) => bindElement.SetVisibility(true);
            Unchecked += (_, _2) => bindElement.SetVisibility(false);
            bindElement.SetVisibility(IsChecked ?? false);
        }
    }
}
