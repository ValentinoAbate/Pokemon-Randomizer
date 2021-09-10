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
        public CompositeCollection WeatherOptionDropdown => new CompositeCollection()
        {
            new ComboBoxItem() {Content="Unchanged"},
            new ComboBoxItem() {Content="Random"},
            new ComboBoxItem() {Content="Random Battle Weather"},
            new ComboBoxItem() {Content="Custom Weather Weights"},
        };

        public WeatherDataView(WeatherDataModel model)
        {
            var stack = new StackPanel() { Orientation = Orientation.Vertical };

            stack.Add(new Label() { Content = "Weather Randomization" });
            stack.Add(new Separator());
            var strategyDrop = stack.Add(new EnumComboBoxUI<WeatherOption>("Randomization Strategy", WeatherOptionDropdown, model.WeatherSetting));
            strategyDrop.BindVisibility(stack.Add(new WeightedSetUI<Weather>("Custom Weather Type", model.CustomWeatherWeights, GetWeatherWeightDropdown)), (int)WeatherOption.CustomWeighting);
            stack.Add(new RandomChanceUI("Apply to Route Weather", model.RandomizeRouteWeather, model.RouteWeatherRandChance));
            stack.Add(new RandomChanceUI("Apply to Gym Weather", model.RandomizeGymWeather, model.GymWeatherRandChance));
            stack.Add(new RandomChanceUI("Apply to Town Weather", model.RandomizeTownWeather, model.TownWeatherRandChance));
            stack.Add(new BoundCheckBoxUI(model.KeepExistingWeather, "Keep Existing Weather", "Keeps weather for places that already have special weather (e.g. the desert will still have sandstorm weather)"));

            Content = stack;
        }

        private List<WeightedSetUI<Weather>.MenuBoxItem> GetWeatherWeightDropdown() => new List<WeightedSetUI<Weather>.MenuBoxItem>
        {
            new WeightedSetUI<Weather>.MenuBoxItem { Item = Weather.Rain, Header="Rain"},
            new WeightedSetUI<Weather>.MenuBoxItem { Item = Weather.RainThunderstorm, Header="Thunderstorm"},
            new WeightedSetUI<Weather>.MenuBoxItem { Item = Weather.RainHeavyThunderstrorm, Header="Heavy Thunderstorm"},
            new WeightedSetUI<Weather>.MenuBoxItem { Item = Weather.StrongSunlight, Header="Strong Sunlight"},
            new WeightedSetUI<Weather>.MenuBoxItem { Item = Weather.Sandstorm, Header="Sandstorm"},
            new WeightedSetUI<Weather>.MenuBoxItem { Item = Weather.Snow, Header="Snow", ToolTip = "For supported games (Emerald), will look nicer and affect in-battle weather"},
        };
    }
}
