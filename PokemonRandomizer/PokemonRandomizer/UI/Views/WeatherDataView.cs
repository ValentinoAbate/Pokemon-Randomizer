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
        private const string weatherWeightsTooltip = "The chances that determine which weather is chosen for each affected map";
        public CompositeCollection WeatherOptionDropdown => new CompositeCollection()
        {
            new ComboBoxItem() {Content="Unchanged", ToolTip="Leave overworld weather as it is in the base game"},
            new ComboBoxItem() {Content="Random", ToolTip="Choose a random overworld weather for each affected map. Uses weathers that have in-battle effects and those that are purely visual"},
            new ComboBoxItem() {Content="Random Battle Weather", ToolTip="Choose a random overworld weather with in-battle effects for each affected map using a balanced set of chances"},
            new ComboBoxItem() {Content="Custom Weather Chances", ToolTip="Set custom chances that determine which weather will be chosen for each affected map"},
        };

        public WeatherDataView(WeatherDataModel model)
        {
            var stack = CreateMainStack();
            stack.Header("Overworld Weather Randomization");
            var strategyDrop = stack.Add(new EnumComboBoxUI<WeatherOption>("Randomization Strategy", WeatherOptionDropdown, model.WeatherSetting) { ToolTip = "The strategy used to randomize overworld weather"});
            strategyDrop.BindVisibility(stack.Add(new WeightedSetUI<Weather>("Custom Weather Type", model.CustomWeatherWeights, GetWeatherWeightDropdown, weatherWeightsTooltip)), (int)WeatherOption.CustomWeighting);
            var settingsStack = stack.Add(strategyDrop.BindEnabled(CreateStack(), (int)WeatherOption.CompletelyRandom, (int)WeatherOption.InBattleWeather, (int)WeatherOption.CustomWeighting));
            settingsStack.Add(new RandomChanceUI("Apply to Route Weather", model.RandomizeRouteWeather, model.RouteWeatherRandChance) { ToolTip = "The chance that a given route's overworld weather will be randomized" });
            settingsStack.Add(new RandomChanceUI("Apply to Gym Weather", model.RandomizeGymWeather, model.GymWeatherRandChance) { ToolTip = "The chance that a given gym's overworld weather will be randomized" });
            settingsStack.Add(new RandomChanceUI("Apply to Town Weather", model.RandomizeTownWeather, model.TownWeatherRandChance) { ToolTip="The chance that a given town's overworld weather will be randomized" });
            settingsStack.Add(new BoundCheckBoxUI("Keep Existing Weather", model.KeepExistingWeather, "Keeps weather for places that already have special weather (e.g. the desert will still have sandstorm weather)"));
            var banFlashingCb = settingsStack.Add(new BoundCheckBoxUI("Ban Flashing Weather", model.BanFlashing, banFlashingTooltip));
            settingsStack.Add(banFlashingCb.BindVisibility(new Label() { Content = banFlashingWarning }));
        }

        private List<WeightedSetUI<Weather>.MenuBoxItem> GetWeatherWeightDropdown() => new List<WeightedSetUI<Weather>.MenuBoxItem>
        {
            new WeightedSetUI<Weather>.MenuBoxItem { Item = Weather.Rain, Header="Rain", ToolTip="Rainy weather that sets the in-battle weather to rain"},
            new WeightedSetUI<Weather>.MenuBoxItem { Item = Weather.RainThunderstorm, Header="Thunderstorm", ToolTip="A thunderstorm that sets the in-battle weather to rain"},
            new WeightedSetUI<Weather>.MenuBoxItem { Item = Weather.RainHeavyThunderstrorm, Header="Heavy Thunderstorm", ToolTip="A heavy thunderstorm that sets the in-battle weather to rain"},
            new WeightedSetUI<Weather>.MenuBoxItem { Item = Weather.RainSometimes1, Header="Rain (Sometimes) #1 (RSE Only)", ToolTip="The weather is either clear, rainy, or a thunderstorm depending on the real-time date\nSets the in-battle weather to rain except when clear"},
            new WeightedSetUI<Weather>.MenuBoxItem { Item = Weather.RainSometimes2, Header="Rain (Sometimes) #2 (RSE Only)", ToolTip="The weather is either clear or rainy depending on the real-time date\nSets the in-battle weather to rain when rainy"},
            new WeightedSetUI<Weather>.MenuBoxItem { Item = Weather.StrongSunlight, Header="Strong Sunlight", ToolTip="Strong sunlight that sets the in-battle weather to harsh sunlight"},
            new WeightedSetUI<Weather>.MenuBoxItem { Item = Weather.Sandstorm, Header="Sandstorm", ToolTip="A sandstorm that sets the in-battle weather to sandstorm and can affect the battle's terrain in certain circumances in RSE"},
            new WeightedSetUI<Weather>.MenuBoxItem { Item = Weather.Snow, Header="Snow", ToolTip="Snowy weather that that sets the in-battle weather to hail"},
            new WeightedSetUI<Weather>.MenuBoxItem { Item = Weather.Chaos, Header="Alternating Rain and Strong Sunlight (Emerald Only)", ToolTip="Alternate thunderstorms and harsh sunlight that set the in-battle weather to rain or harsh sunlight respectively"},
        };
    }
}
