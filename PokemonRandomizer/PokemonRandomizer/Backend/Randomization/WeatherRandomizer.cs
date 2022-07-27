using PokemonRandomizer.Backend.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PokemonRandomizer.Backend.DataStructures.Map;

namespace PokemonRandomizer.Backend.Randomization
{
    internal class WeatherRandomizer
    {
        private readonly Random rand;
        public WeatherRandomizer(Random rand)
        {
            this.rand = rand;
        }
        public void RandomizeWeather(Map map, Settings s)
        {
            if (s.WeatherSetting == Settings.WeatherOption.Unchanged)
                return;
            // If Gym override is set and the map is a gym, proceed with randomization
            if (s.OverrideAllowGymWeather && map.IsGym)
            {
                if (rand.RollSuccess(s.GymWeatherRandChance))
                {
                    ChooseWeather(map, s, true);
                }
            }
            else if ((!s.OnlyChangeClearWeather || map.HasClearWeather) && s.WeatherRandChance.ContainsKey(map.mapType))
            {
                if (rand.RollSuccess(s.WeatherRandChance[map.mapType]))
                {
                    ChooseWeather(map, s, false);
                }
            }
        }

        private void ChooseWeather(Map m, Settings s2, bool gymOverride)
        {
            var choices = new WeightedSet<Weather>(s2.WeatherWeights);
            // Always unsafe unless map is specifically layered for this weather
            choices.RemoveIfContains(Weather.ClearWithCloudsInWater);
            if (s2.SafeUnderwaterWeather && !(m.mapType == Map.Type.Underwater))
            {
                choices.RemoveIfContains(Weather.UnderwaterMist);
            }
            if (!gymOverride && s2.SafeInsideWeather && !m.IsOutdoors)
            {
                choices.RemoveIfContains(Weather.Clear);
                choices.RemoveIfContains(Weather.Cloudy);
                choices.RemoveIfContains(Weather.Rain);
                choices.RemoveIfContains(Weather.RainThunderstorm);
                choices.RemoveIfContains(Weather.RainHeavyThunderstrorm);
                choices.RemoveIfContains(Weather.Sandstorm);
                choices.RemoveIfContains(Weather.Snow);
                choices.RemoveIfContains(Weather.SnowSteady);
                choices.RemoveIfContains(Weather.StrongSunlight);
            }
            if (choices.Count > 0)
                m.weather = rand.Choice(choices);
        }
    }
}
