using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PokemonRandomizer.UI
{
    public class PokemonSettingsUI : ContentControl
    {
        public PokemonSettingsUI(Settings.PokemonSettings settings)
        {
            var stack = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };
            stack.Add(new BoundCheckBoxUI(settings.BanLegendaries, b => settings.BanLegendaries = b, "Ban Legendaries"));
            stack.Add(new BoundCheckBoxUI(settings.RestrictIllegalEvolutions, b => settings.RestrictIllegalEvolutions = b, "Ban Illegal Evolutions"));
            stack.Add(new BoundCheckBoxUI(settings.ForceHighestLegalEvolution, b => settings.ForceHighestLegalEvolution = b, "Force Highest Legal Evolution"));
            stack.Add(new BoundSliderUI("Noise", settings.Noise, d => settings.Noise = (float)d));
            Content = stack;
        }
    }
}
