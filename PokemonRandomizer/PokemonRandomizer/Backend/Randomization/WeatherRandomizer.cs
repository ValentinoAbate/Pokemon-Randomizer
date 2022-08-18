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
                choices.RemoveIfContains(Weather.FallingAsh);
                choices.RemoveIfContains(Weather.StrongSunlight);
                choices.RemoveIfContains(Weather.Chaos);
                choices.RemoveIfContains(Weather.RainSometimes1);
                choices.RemoveIfContains(Weather.RainSometimes2);
            }
            if (choices.Count > 0)
            {
                var newWeather = rand.Choice(choices);
                if(newWeather == m.weather)
                {
                    return;
                }
                if (!m.HasClearWeather)
                {
                    m.weather = newWeather;
                    return;
                }
                var nonHeaderWeathers = new Dictionary<Weather, List<IHasWeather>>(3);
                GetNonHeaderWeathers(m, ref nonHeaderWeathers);
                if(nonHeaderWeathers.Count <= 0)
                {
                    m.weather = newWeather;
                    return;
                }
                // TODO: Desert exception
                //if (settings.randomizeAllWeather(map))
                //{
                //    m.weather = newWeather;
                //    choices.Remove(newWeather);
                //    if (choices.Count <= 0)
                //    {
                //        return;
                //    }
                //    newWeather = rand.Choice(choices);
                //}
                if (settings.OnlyChangeClearWeather)
                {
                    return;
                }
                foreach(var kvp in nonHeaderWeathers)
                {
                    foreach(var setWeather in kvp.Value)
                    {
                        setWeather.Weather = newWeather;
                    }
                    choices.Remove(newWeather);
                    if(choices.Count <= 0)
                    {
                        return;
                    }
                    newWeather = rand.Choice(choices);
                }
                // If settings says override all weathers
            }
        }

        private void GetNonHeaderWeathers(Map map, ref Dictionary<Weather, List<IHasWeather>> nonHeaderWeathers)
        {
            nonHeaderWeathers.Clear();
            foreach (var trigger in map.eventData.triggerEvents)
            {
                if (trigger.IsWeatherTrigger)
                {
                    if (IsEligibleNonHeaderWeather(trigger.Weather))
                    {
                        nonHeaderWeathers.AddOrAppend(trigger.Weather, trigger);
                    }
                }
                else if (trigger.script != null)
                {
                    FindWeatherCommands(trigger.script, nonHeaderWeathers);
                }
            }
            foreach (var mapScript in map.scriptData.scripts)
            {
                FindWeatherCommands(mapScript.script, nonHeaderWeathers);
            }
        }

        private void FindWeatherCommands(Script script, Dictionary<Weather, List<IHasWeather>> weatherCommands)
        {
            void ProcessCommand(Command command)
            {
                if (command is SetWeatherCommand weatherCommand && IsEligibleNonHeaderWeather(weatherCommand.Weather))
                {
                    weatherCommands.AddOrAppend(weatherCommand.Weather, weatherCommand);
                }
            }
            script?.ApplyRecursively(ProcessCommand);
        }

        private static bool IsEligibleNonHeaderWeather(Weather weather)
        {
            return !IsWeatherClear(weather) && weather != Weather.Chaos;
        }
    }
}
