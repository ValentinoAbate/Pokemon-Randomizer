using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.DataStructures.Scripts;
using PokemonRandomizer.Backend.Utilities;
using System;
using System.Collections.Generic;
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
            else if (settings.WeatherRandChance.ContainsKey(map.mapType))
            {
                if (rand.RollSuccess(settings.WeatherRandChance[map.mapType]))
                {
                    ChooseWeather(map, settings, false);
                }
            }
        }

        private void ChooseWeather(Map m, Settings settings, bool gymOverride)
        {
            // Find weather commands and triggers
            var nonHeaderWeathers = new Dictionary<Weather, List<IHasWeather>>(3);
            var headerWeathers = new List<IHasWeather>();
            GetNonHeaderWeathers(m, ref nonHeaderWeathers, ref headerWeathers);
            if (nonHeaderWeathers.Count <= 0)
            {
                if (CanChangeWeather(m.weather, settings) && TryGetNewWeather(GetWeatherChoices(m, settings, gymOverride), out Weather newWeather))
                {
                    m.weather = newWeather;
                    // Set triggers
                    foreach (var trigger in headerWeathers)
                    {
                        trigger.Weather = newWeather;
                    }
                }
                return;
            }
            // Get Choices
            var choices = GetWeatherChoices(m, settings, gymOverride);
            // Set non-header weather if non header weather is considered a separate route (RSE desert, etc)
            if (m.IsNonHeaderWeatherSeparateRoute && CanChangeWeather(m.weather, settings))
            {
                if (!TryGetNewWeather(choices, out Weather newWeather))
                {
                    return; // No more weathers
                }
                // Set header
                m.weather = newWeather;
                // Set triggers
                foreach(var trigger in headerWeathers)
                {
                    trigger.Weather = newWeather;
                }
            }
            // Process non-header weathers
            foreach (var (weather, triggers) in nonHeaderWeathers)
            {
                if (!CanChangeWeather(weather, settings))
                {
                    continue;
                }
                if (!TryGetNewWeather(choices, out Weather newWeather))
                {
                    return; // No more weathers
                }
                foreach (var setWeather in triggers)
                {
                    setWeather.Weather = newWeather;
                }
            }
        }

        private WeightedSet<Weather> GetWeatherChoices(Map m, Settings settings, bool gymOverride)
        {
            var choices = new WeightedSet<Weather>(settings.WeatherWeights);
            // Always unsafe unless map is specifically layered for this weather
            choices.RemoveIfContains(Weather.ClearWithCloudsInWater);
            // Remove underwater weather if not underwater
            if (settings.SafeUnderwaterWeather && m.mapType != Map.Type.Underwater)
            {
                choices.RemoveIfContains(Weather.UnderwaterMist);
            }
            // Remove outdoor weather if map is inside (unless gym override is true)
            if (!gymOverride && settings.SafeInsideWeather && !m.IsOutdoors)
            {
                choices.RemoveIfContains(Weather.Clear);
                choices.RemoveIfContains(Weather.Cloudy);
                choices.RemoveIfContains(Weather.Rain);
                choices.RemoveIfContains(Weather.RainThunderstorm);
                choices.RemoveIfContains(Weather.RainHeavyThunderstrorm);
                choices.RemoveIfContains(Weather.Sandstorm);
                choices.RemoveIfContains(Weather.Snow);
                choices.RemoveIfContains(Weather.FallingAsh);
                choices.RemoveIfContains(Weather.StrongSunlight);
                choices.RemoveIfContains(Weather.Chaos);
                choices.RemoveIfContains(Weather.RainSometimes1);
                choices.RemoveIfContains(Weather.RainSometimes2);
            }
            if (settings.BanFlashingWeather)
            {
                choices.RemoveIfContains(Weather.RainThunderstorm);
                choices.RemoveIfContains(Weather.RainHeavyThunderstrorm);
                choices.RemoveIfContains(Weather.StrongSunlight);
                choices.RemoveIfContains(Weather.Chaos);
                choices.RemoveIfContains(Weather.RainSometimes1);
                choices.RemoveIfContains(Weather.RainSometimes2);
            }
            return choices;
        }

        private void GetNonHeaderWeathers(Map map, ref Dictionary<Weather, List<IHasWeather>> nonHeaderWeathers, ref List<IHasWeather> headerWeather)
        {
            nonHeaderWeathers.Clear();
            foreach (var trigger in map.eventData.triggerEvents)
            {
                if (trigger.IsWeatherTrigger)
                {
                    if (IsEligibleNonHeaderWeather(map, trigger.Weather))
                    {
                        nonHeaderWeathers.AddOrAppend(trigger.Weather, trigger);
                    }
                    else if(IsHeaderWeather(map, trigger.Weather))
                    {
                        headerWeather.Add(trigger);
                    }
                }
                else if (trigger.script != null)
                {
                    FindWeatherCommands(map, trigger.script, nonHeaderWeathers, headerWeather);
                }
            }
            // Only check map scripts if there are trigger events
            // Otherwise, we pick up stuff like terra / marine cave
            if (nonHeaderWeathers.Count <= 0)
                return;
            foreach (var mapScript in map.scriptData.scripts)
            {
                FindWeatherCommands(map, mapScript.script, nonHeaderWeathers, headerWeather);
            }
        }

        private void FindWeatherCommands(Map m, Script script, Dictionary<Weather, List<IHasWeather>> weatherCommands, List<IHasWeather> headerCommands)
        {
            void ProcessCommand(Command command)
            {
                if (command is SetWeatherCommand weatherCommand)
                {
                    if (IsEligibleNonHeaderWeather(m, weatherCommand.Weather))
                    {
                        weatherCommands.AddOrAppend(weatherCommand.Weather, weatherCommand);
                    }
                    else if(IsHeaderWeather(m, weatherCommand.Weather))
                    {
                        headerCommands.Add(weatherCommand);
                    }
                }
            }
            script?.ApplyRecursively(ProcessCommand);
        }

        private bool TryGetNewWeather(WeightedSet<Weather> weathers, out Weather choice)
        {
            if (weathers.Count <= 0)
            {
                choice = Weather.House;
                return false;
            }
            choice = rand.Choice(weathers);
            weathers.Remove(choice);
            return true;
        }

        private static bool CanChangeWeather(Weather weather, Settings settings)
        {
            return IsWeatherClear(weather) || !settings.OnlyChangeClearWeather;
        }

        private static bool IsEligibleNonHeaderWeather(Map m, Weather weather)
        {
            return !IsHeaderWeather(m, weather) && weather != Weather.Chaos;
        }

        private static bool IsHeaderWeather(Map m, Weather weather)
        {
            return weather == m.weather || m.HasClearWeather && IsWeatherClear(weather);
        }
    }
}
