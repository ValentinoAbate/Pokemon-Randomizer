using System.Windows.Controls;
using ItemSettings = PokemonRandomizer.Backend.Randomization.ItemRandomizer.Settings;

namespace PokemonRandomizer.UI
{
    public class ItemSettingsUI : ContentControl
    {
        public ItemSettingsUI(ItemSettings settings, bool showNoneToOtherChance = true)
        {
            var stack = new StackPanel { Orientation = Orientation.Vertical };
            stack.Add(new BoundSliderUI("Force Same Pocket Chance", settings.SamePocketChance, d => settings.SamePocketChance = d));
            if (showNoneToOtherChance)
            {
                stack.Add(new BoundSliderUI("None to Other Chance", settings.NoneToOtherChance, d => settings.NoneToOtherChance = d));
            }
            stack.Add(new BoundCheckBoxUI(settings.BanMail, b => settings.BanMail = b, "Ban Mail"));
            Content = stack;
        }
    }
}
