using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace PokemonRandomizer.UI
{
    using Backend.Randomization;
    using System.Windows;

    public class WeightedSetUI<T> : Border
    {
        private static int FindIndex(T item, IReadOnlyList<MenuBoxItem> choices)
        {
            for(int i = 0; i < choices.Count; ++i)
            {
                var choice = choices[i];
                if (choice.Item.Equals(item))
                    return i;
            }
            return -1;
        }

        private float CbWidth { get; }

        private readonly List<WeightUI> weights = new List<WeightUI>();
        private readonly StackPanel mainStack;
        private readonly WeightedSet<T> set;
        private readonly IReadOnlyList<MenuBoxItem> referenceList;
        private readonly Button addButton;
        private readonly ContextMenu contextMenu;


        public WeightedSetUI(string name, WeightedSet<T> set, Func<IReadOnlyList<MenuBoxItem>> getChoiceList, float cbWidth = 150)
        {
            CbWidth = cbWidth;
            this.set = set;
            referenceList = getChoiceList();
            mainStack = new StackPanel() { Orientation = Orientation.Vertical };
            var controlStack = new StackPanel { Orientation = Orientation.Horizontal };
            controlStack.Add(new Label() { Content = name });
            addButton = controlStack.Add(new Button() { Content = "Add Weight", Height = 17, Width = 100, Margin = new Thickness(0, 0, 0, 1), FontSize = 11 });
            addButton.Click += AddButtonClick;
            contextMenu = new ContextMenu();
            foreach(var item in referenceList)
            {
                contextMenu.Items.Add(item);
                item.Click += (_, _2) => AddMenuClick(item);
                if (set.Contains(item.Item))
                {
                    item.Visibility = Visibility.Collapsed;
                }
            }
            addButton.ContextMenu = contextMenu;
            mainStack.Add(controlStack);
            mainStack.Separator();
            foreach(var item in set)
            {
                Add(item.Key, item.Value);
            }
            Child = mainStack;
        }

        private void AddButtonClick(object sender, RoutedEventArgs e)
        {
            addButton.ContextMenu.IsEnabled = true;
            addButton.ContextMenu.PlacementTarget = addButton;
            addButton.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            addButton.ContextMenu.IsOpen = true;
        }

        private void AddMenuClick(MenuBoxItem item)
        {
            item.Visibility = Visibility.Collapsed;
            Add(item.Item, 0);
            OnValueChanged();
        }

        private void Add(T item, float weight)
        {
            var newWeight = new WeightUI(item, weight, this, weights.Count == 0, referenceList);
            mainStack.Add(newWeight);
            weights.Add(newWeight);
            // Enable the remove button for the default weight if there is more than one active
            if (weights.Count > 1)
            {
                weights[0].RemoveButton.Visibility = Visibility.Visible;
            }
            // If we've added every option, disable the add button
            if (weights.Count >= referenceList.Count)
            {
                addButton.IsEnabled = false;
            }
        }

        private bool changingValues = false;
        private void OnValueChanged(WeightUI changed = null)
        {
            if (changingValues || weights.Count <= 0)
                return;
            changingValues = true;
            double sum = 0;
            for (int i = 1; i < weights.Count; i++)
            {
                sum += weights[i].Weight;
            }
            if(sum > 1 && changed != null)
            {
                changed.Weight -= sum - 1;
                weights[0].Weight = 0;
            }
            else
            {
                weights[0].Weight = 1 - sum;
            }
            set.Clear();
            foreach (var item in weights)
            {
                set.Add(item.Item, (float)item.Weight);
            }
            changingValues = false;
        }

        private void Remove(WeightUI weight)
        {
            weights.Remove(weight);
            mainStack.Children.Remove(weight);
            OnValueChanged();
            // If there is only one weight, hide the remove button
            if (weights.Count == 1)
            {
                weights[0].RemoveButton.Visibility = Visibility.Collapsed;
            }
            // Disable the slider for the default weight (in case the default was just removed)
            weights[0].Slider.IsEnabled = false;
            // Re-allow adding (in case we had just added the last option)
            addButton.IsEnabled = true;
            // Add the item back to the add menu
            foreach(var item in contextMenu.Items)
            {
                if(item is MenuBoxItem menuItem && menuItem.Item.Equals(weight.Item))
                {
                    menuItem.Visibility = Visibility.Visible;
                    break;
                }
            }
        }

        public class MenuBoxItem : MenuItem
        {
            public T Item { get; set; }
        }

        private class WeightUI : StackPanel
        {
            public T Item { get; set; }
            public double Weight 
            {
                get => Slider.Value;
                set => Slider.Value = value; 
            }
            public Button RemoveButton { get; }
            public BoundSliderUI Slider { get; }

            public WeightUI(T item, float weight, WeightedSetUI<T> parent, bool isDefault, IReadOnlyList<MenuBoxItem> choices)
            {
                Orientation = Orientation.Horizontal;
                Item = item;
                this.Add(new Label() { Content = choices[FindIndex(item, choices)].Header, MinWidth = parent.CbWidth });
                // Slider
                Slider = new BoundSliderUI("Chance", weight, (_) => parent.OnValueChanged(this)) { IsEnabled = !isDefault };
                this.Add(Slider);
                // Remove Button
                RemoveButton = new Button() { Content = "-" };
                RemoveButton.Click += (_, _2) => parent.Remove(this);
                this.Add(RemoveButton);
            }
        }
    }
}
