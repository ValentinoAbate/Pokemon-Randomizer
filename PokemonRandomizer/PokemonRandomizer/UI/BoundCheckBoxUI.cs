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
            Margin = new Thickness(5, 0, 0, 0);
            IsChecked = isChecked;
            VerticalContentAlignment = VerticalAlignment.Center;
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
            Content = new Label() { Content = label, Margin = new Thickness(-5, 0, 0, 0), VerticalContentAlignment = VerticalAlignment.Center };
        }

        public BoundCheckBoxUI(bool isChecked, Action<bool> onEnabledChange, string label, string tooltip, UIElement enableOnChecked = null) : this(isChecked, onEnabledChange, label, enableOnChecked)
        {
            ToolTip = tooltip;
        }

        public T BindVisibility<T>(T bindElement) where T : UIElement
        {
            Checked += (_, _2) => bindElement.SetVisibility(true);
            Unchecked += (_, _2) => bindElement.SetVisibility(false);
            bindElement.SetVisibility(IsChecked ?? false);
            return bindElement;
        }

        public T BindEnabled<T>(T bindElement) where T : UIElement
        {
            Checked += (_, _2) => bindElement.IsEnabled = true;
            Unchecked += (_, _2) => bindElement.IsEnabled = false;
            bindElement.IsEnabled = IsChecked ?? false;
            return bindElement;
        }
    }
}
