using PokemonRandomizer.Backend.Utilities;

namespace PokemonRandomizer.Backend.DataStructures.Scripts
{
    public class SetWeatherCommand : Command, IHasWeather
    {
        public Map.Weather Weather { get; set; }
        public override string ToString()
        {
            return $"Set weather to {Weather.ToDisplayString()}";
        }
    }
}
