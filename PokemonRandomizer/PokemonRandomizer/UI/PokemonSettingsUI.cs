using System;
using System.Collections.Generic;
using System.Windows.Controls;
using static PokemonRandomizer.Settings;

namespace PokemonRandomizer.UI
{
    public class PokemonSettingsUI : ContentControl
    {
        public static IReadOnlyList<string> BasicPokemonMetricTypes { get; } = new List<string>()
        {
            PokemonMetric.powerIndividual,
            PokemonMetric.typeIndividual,
        };

        public PokemonSettingsUI(PokemonSettings settings) : this(settings, BasicPokemonMetricTypes, null) { }

        public PokemonSettingsUI(PokemonSettings settings, IEnumerable<string> metricTypeOptions, Action<MetricData> initialize)
        {
            var stack = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };
            if(settings.Data.Count > 0)
            {
                stack.Add(new MetricDataUI(settings.Data, metricTypeOptions, initialize));
                stack.Add(new Separator());
            }
            stack.Add(new BoundCheckBoxUI(settings.BanLegendaries, b => settings.BanLegendaries = b, "Ban Legendaries"));
            stack.Add(new BoundCheckBoxUI(settings.RestrictIllegalEvolutions, b => settings.RestrictIllegalEvolutions = b, "Ban Illegal Evolutions"));
            stack.Add(new BoundCheckBoxUI(settings.ForceHighestLegalEvolution, b => settings.ForceHighestLegalEvolution = b, "Force Highest Legal Evolution"));
            stack.Add(new BoundSliderUI("Ignore Restrictions Chance", settings.Noise, d => settings.Noise = (float)d, true, 0.001, 0, 0.025) { ToolTip = "The percentage chance a completely random pokemon will be chosen, (ignores restrictions such as evolution legality and bans)" });
            Content = stack;
        }
    }
}
