using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.Randomization;
using System.Collections.Generic;
using static PokemonRandomizer.Settings;

namespace PokemonRandomizer.UI.Models
{
    using Utilities;
    public class WeatherDataModel : DataModel
    {
        private const double defaultWeatherRandChance = 0.33;
        public Box<WeatherOption> WeatherSetting { get; } = new Box<WeatherOption>(WeatherOption.Unchanged);

        public Box<bool> RandomizeRouteWeather { get; } = new Box<bool>(true); 
        public Box<double> RouteWeatherRandChance { get; } = new Box<double>(defaultWeatherRandChance);

        public Box<bool> RandomizeGymWeather { get; } = new Box<bool>(true);
        public Box<double> GymWeatherRandChance { get; } = new Box<double>(defaultWeatherRandChance);

        public Box<bool> RandomizeTownWeather { get; } = new Box<bool>(true);
        public Box<double> TownWeatherRandChance { get; } = new Box<double>(defaultWeatherRandChance);

        /// <summary>
        /// If this is true, only maps that started with clear weather will be random (the desert will still have sandstorm, etc)
        /// </summary>
        public Box<bool> KeepExistingWeather { get; } = new Box<bool>(true);

        public WeightedSet<Map.Weather> CustomWeatherWeights { get; } = new WeightedSet<Map.Weather>
        {
            { Map.Weather.Rain, 0.25f },
            { Map.Weather.Snow, 0.25f },
            { Map.Weather.StrongSunlight, 0.25f },
            { Map.Weather.Sandstorm, 0.25f },
        };
    }
}
