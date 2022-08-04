using PokemonRandomizer.Backend.Utilities;

namespace PokemonRandomizer.Backend.DataStructures.Scripts
{
    public class SetWeatherCommand : Command
    {
        public Map.Weather weather;
        public override string ToString()
        {
            return $"Set weather to {weather.ToDisplayString()}";
        }
    }
}
