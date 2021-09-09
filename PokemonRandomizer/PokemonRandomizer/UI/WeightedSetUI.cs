using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace PokemonRandomizer.UI
{
    using Backend.Randomization;
    using System.Windows;

    public class WeightedSetUI<T> : Border
    {
        private static int FindIndex(T item, IReadOnlyList<ChoiceBoxItem> choices)
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

        private readonly Func<IReadOnlyList<ChoiceBoxItem>> getChoiceList;
        private readonly List<WeightUI> weights = new List<WeightUI>();
        private readonly StackPanel mainStack;
        private readonly WeightedSet<T> set;


        public WeightedSetUI(string name, WeightedSet<T> set, Func<IReadOnlyList<ChoiceBoxItem>> getChoiceList, float cbWidth = 150)
        {
            CbWidth = cbWidth;
            this.getChoiceList = getChoiceList;
            this.set = set;
            mainStack = new StackPanel() { Orientation = Orientation.Vertical };
            var controlStack = new StackPanel { Orientation = Orientation.Horizontal };
            controlStack.Add(new Label() { Content = name });
            var addButton = new Button() { Content = "Add Weight", Height = 17, Width = 100, Margin = new Thickness(0, 0, 0, 1), FontSize = 11 };
            addButton.Click += AddButtonClick;
            controlStack.Add(addButton);
            mainStack.Add(controlStack);
            mainStack.Add(new Separator());
            foreach(var item in set)
            {
                Add(item.Key, item.Value);
            }
            Child = mainStack;
        }

        private void AddButtonClick(object sender, RoutedEventArgs e)
        {
            Add(default, 0);
            OnValueChanged();
        }

        private void Add(T item, float weight)
        {
            var newWeight = new WeightUI(item, weight, getChoiceList(), this, weights.Count == 0);
            mainStack.Add(newWeight);
            weights.Add(newWeight);
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
        }

        public class ChoiceBoxItem : ComboBoxItem
        {
            public T Item { get; set; }
        }
        private class WeightUI : StackPanel
        {
            public T Item { get; set; }
            public double Weight 
            {
                get => slider.Value;
                set => slider.Value = value; 
            }

            private readonly BoundSliderUI slider;

            public WeightUI(T item, float weight, IReadOnlyList<ChoiceBoxItem> choices, WeightedSetUI<T> parent, bool isDefault)
            {
                Orientation = Orientation.Horizontal;
                Item = item;
                var cb = this.Add(new BoundComboBoxUI("", choices, FindIndex(item, choices), i => Item = choices[i].Item));
                cb.ComboBox.MinWidth = parent.CbWidth;
                slider = new BoundSliderUI("Chance", weight, (_) => parent.OnValueChanged(this)) { IsEnabled = !isDefault };
                this.Add(slider);
                if (!isDefault)
                {
                    // Remove Button
                    var removeButton = new Button() { Content = "-" };
                    removeButton.Click += (_, _2) => parent.Remove(this);
                    this.Add(removeButton);
                }
            }
        }
    }
}
