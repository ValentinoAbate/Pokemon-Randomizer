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
        public void RandomizeWeather(Map map, Settings settings)
        {
            if (settings.WeatherSetting == Settings.WeatherOption.Unchanged)
                return;
            // If Gym override is set and the map is a gym, proceed with randomization
            if (settings.OverrideAllowGymWeather && map.IsGym)
            {
                if (rand.RollSuccess(settings.GymWeatherRandChance))
                {
                    ChooseWeather(map, settings, true);
                }
            }
            else if ((!settings.OnlyChangeClearWeather || map.HasClearWeather) && settings.WeatherRandChance.ContainsKey(map.mapType))
            {
                if (rand.RollSuccess(settings.WeatherRandChance[map.mapType]))
                {
                    ChooseWeather(map, settings, false);
                }
            }
        }

        private void ChooseWeather(Map m, Settings settings, bool gymOverride)
        {
            var choices = new WeightedSet<Weather>(settings.WeatherWeights);
            // Always unsafe unless map is specifically layered for this weather
            choices.RemoveIfContains(Weather.ClearWithCloudsInWater);
            if (settings.SafeUnderwaterWeather && !(m.mapType == Map.Type.Underwater))
            {
                choices.RemoveIfContains(Weather.UnderwaterMist);
            }
            if (!gymOverride && settings.SafeInsideWeather && !m.IsOutdoors)
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
                choices.RemoveIfContains(Weather.Chaos);
                choices.RemoveIfContains(Weather.RainSometimes1);
                choices.RemoveIfContains(Weather.RainSometimes2);
            }
            if (choices.Count > 0)
                m.weather = rand.Choice(choices);
        }
    }
}
