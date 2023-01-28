using System.Windows.Controls;

namespace PokemonRandomizer.UI.Views
{
    using Backend.DataStructures;
    using Models;
    using System.Collections.Generic;
    using System.Windows.Data;
    using static Settings;
    using static Backend.DataStructures.Map;

    public class WeatherDataView : DataView<WeatherDataModel>
    {
        private const string banFlashingTooltip = "Will not choose weather that contains flashing (strong sunlight or lightning) as a random choice. " + banFlashingWarning;
        private const string banFlashingWarning = "WARNING: \"Ban Flashing Weather\" only applies to randomly chosen weather, so any unrandomized weathers of these types will be unchanged";
        public CompositeCollection WeatherOptionDropdown => new CompositeCollection()
        {
            new ComboBoxItem() {Content="Unchanged"},
            new ComboBoxItem() {Content="Random"},
            new ComboBoxItem() {Content="Random Battle Weather"},
            new ComboBoxItem() {Content="Custom Weather Weights"},
        };

        public WeatherDataView(WeatherDataModel model)
        {
            var stack = CreateMainStack();
            stack.Add(new Label() { Content = "Weather Randomization" });
            stack.Add(new Separator());
            var strategyDrop = stack.Add(new EnumComboBoxUI<WeatherOption>("Randomization Strategy", WeatherOptionDropdown, model.WeatherSetting));
            strategyDrop.BindVisibility(stack.Add(new WeightedSetUI<Weather>("Custom Weather Type", model.CustomWeatherWeights, GetWeatherWeightDropdown)), (int)WeatherOption.CustomWeighting);
            var settingsStack = stack.Add(strategyDrop.BindEnabled(CreateStack(), (int)WeatherOption.CompletelyRandom, (int)WeatherOption.InBattleWeather, (int)WeatherOption.CustomWeighting));
            settingsStack.Add(new RandomChanceUI("Apply to Route Weather", model.RandomizeRouteWeather, model.RouteWeatherRandChance));
            settingsStack.Add(new RandomChanceUI("Apply to Gym Weather", model.RandomizeGymWeather, model.GymWeatherRandChance));
            settingsStack.Add(new RandomChanceUI("Apply to Town Weather", model.RandomizeTownWeather, model.TownWeatherRandChance));
            settingsStack.Add(new BoundCheckBoxUI(model.KeepExistingWeather, "Keep Existing Weather", "Keeps weather for places that already have special weather (e.g. the desert will still have sandstorm weather)"));
            var banFlashingCb = settingsStack.Add(new BoundCheckBoxUI(model.BanFlashing, "Ban Flashing Weather", banFlashingTooltip));
            settingsStack.Add(banFlashingCb.BindVisibility(new Label() { Content = banFlashingWarning }));
        }

        private List<WeightedSetUI<Weather>.MenuBoxItem> GetWeatherWeightDropdown() => new List<WeightedSetUI<Weather>.MenuBoxItem>
        {
            new WeightedSetUI<Weather>.MenuBoxItem { Item = Weather.Rain, Header="Rain"},
            new WeightedSetUI<Weather>.MenuBoxItem { Item = Weather.RainThunderstorm, Header="Thunderstorm"},
            new WeightedSetUI<Weather>.MenuBoxItem { Item = Weather.RainHeavyThunderstrorm, Header="Heavy Thunderstorm"},
            new WeightedSetUI<Weather>.MenuBoxItem { Item = Weather.RainSometimes1, Header="Rain (Sometimes) #1"},
            new WeightedSetUI<Weather>.MenuBoxItem { Item = Weather.RainSometimes2, Header="Rain (Sometimes) #2"},
            new WeightedSetUI<Weather>.MenuBoxItem { Item = Weather.StrongSunlight, Header="Strong Sunlight (RSE + FR v1.0 Only)"},
            new WeightedSetUI<Weather>.MenuBoxItem { Item = Weather.Sandstorm, Header="Sandstorm"},
            new WeightedSetUI<Weather>.MenuBoxItem { Item = Weather.Snow, Header="Snow"},
            new WeightedSetUI<Weather>.MenuBoxItem { Item = Weather.Chaos, Header="Alternating Rain and Strong Sunlight (Emerald Only)"},
        };
    }
}
