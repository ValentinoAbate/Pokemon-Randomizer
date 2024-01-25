using System;
using System.Collections.Generic;
using System.Windows.Controls;
using static PokemonRandomizer.Settings;

namespace PokemonRandomizer.UI
{
    public class PokemonSettingsUI : ContentControl
    {
        public const string legalEvoTooltip = "Ensure that randomly chosen pokemon are at a legal evolution state for the level they appear at (except when restictions are ignored)" +
            "\nEvolutions that don't happen at specific levels such as friendship evolutions will have their legal levels approximated";
        public const string highestLegalEvoTooltip = "Ensure that randomly chosen pokemon are at the highest legal evolution state for the level they appear at (except when restictions are ignored)" +
    "\nEvolutions that don't happen at specific levels such as friendship evolutions will have their legal levels approximated";
        public const string banLegendariesTooltip = "Prevent legendary pokemon from being randomly chosen (except when restictions are ignored)";
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
                stack.Separator();
            }
            stack.Add(new BoundCheckBoxUI(settings.BanLegendaries, b => settings.BanLegendaries = b, "Ban Legendaries", banLegendariesTooltip));
            stack.Add(new BoundCheckBoxUI(settings.RestrictIllegalEvolutions, b => settings.RestrictIllegalEvolutions = b, "Ban Illegal Evolutions", legalEvoTooltip));
            stack.Add(new BoundCheckBoxUI(settings.ForceHighestLegalEvolution, b => settings.ForceHighestLegalEvolution = b, "Force Highest Legal Evolution", highestLegalEvoTooltip));
            stack.Add(new BoundSliderUI("Ignore Restrictions Chance", settings.Noise, d => settings.Noise = (float)d, true, 0.001, 0, 0.025) { ToolTip = "The percentage chance (per random pokemon choice) that restrictions such as evolution legality checks and legendary bans will be ignored" });
            Content = stack;
        }
    }
}
