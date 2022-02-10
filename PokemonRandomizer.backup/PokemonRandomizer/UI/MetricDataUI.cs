using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Data;
using static PokemonRandomizer.Settings;

namespace PokemonRandomizer.UI
{
    public class MetricDataUI : ContentControl
    {
        public CompositeCollection PriorityDropdown => new CompositeCollection()
        {
            new ComboBoxItem() { Content="Highest" },
            new ComboBoxItem() { Content="High" },
            new ComboBoxItem() { Content="Medium" },
            new ComboBoxItem() { Content="Low" },
            new ComboBoxItem() { Content="Lowest" },
        };
        public MetricDataUI(IList<MetricData> data, IEnumerable<string> typeOptions, Action<MetricData> initialize)
        {
            var mainStack = new StackPanel() { Orientation = Orientation.Vertical };
            mainStack.Add(new Label() { Content = "Metric Data" });
            var realTypeOptions = new List<string>(typeOptions);
            if (realTypeOptions.Count <= 0 || realTypeOptions[0] != MetricData.emptyMetric)
                realTypeOptions.Insert(0, MetricData.emptyMetric);
            for (int i = 0; i < data.Count; i++)
            {
                MetricData metricData = data[i];
                var dataStack = new StackPanel() { Orientation = Orientation.Horizontal };
                dataStack.Add(new Label() { Content = "Metric " + (i + 1) });
                void ChangeMetricType(int i)
                {
                    string newType = realTypeOptions[i];
                    if (metricData.DataSource == newType)
                        return;
                    metricData.DataSource = newType;
                    initialize?.Invoke(metricData);
                }
                var typeCb = new BoundComboBoxUI("Type", realTypeOptions, realTypeOptions.IndexOf(metricData.DataSource), ChangeMetricType);
                typeCb.ComboBox.MinWidth = 200;
                var priorityCb = new BoundComboBoxUI("Priority", PriorityDropdown, metricData.Priority, i => metricData.Priority = i);
                priorityCb.ComboBox.MinWidth = 100;
                dataStack.Add(typeCb, priorityCb);
                mainStack.Add(dataStack);
            }
            Content = mainStack;
        }
    }
}
