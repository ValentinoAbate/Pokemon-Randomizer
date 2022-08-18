using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.DataStructures.Scripts;
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
                SetWeather(m, newWeather);
            }
        }

        private void SetWeather(Map map, Weather newWeather)
        {
            if (!map.HasClearWeather)
            {
                map.weather = newWeather;
                return;
            }
            bool isHeaderWeather = true;
            var setWeatherCommands = new List<SetWeatherCommand>();
            var weatherTriggers = new List<MapEventData.TriggerEvent>(map.eventData.triggerEvents.Count);
            foreach (var trigger in map.eventData.triggerEvents)
            {
                if (trigger.IsWeatherTrigger)
                {
                    if(trigger.Weather != map.weather)
                    {
                        isHeaderWeather = false;
                    }
                    weatherTriggers.Add(trigger);
                }
                else if (trigger.script != null)
                {
                    FindWeatherCommands(map, trigger.script, setWeatherCommands, out bool foundNonHeaderWeather);
                    if (foundNonHeaderWeather)
                    {
                        isHeaderWeather = false;
                    }
                }
            }
            foreach (var mapScript in map.scriptData.scripts)
            {
                FindWeatherCommands(map, mapScript.script, setWeatherCommands, out bool foundNonHeaderWeather);
                if (foundNonHeaderWeather)
                {
                    isHeaderWeather = false;
                }
            }
            if (isHeaderWeather)
            {
                map.weather = newWeather;
            }
            else
            {
                foreach(var trigger in weatherTriggers)
                {
                    if (trigger.Weather == map.weather)
                    {
                        continue; // TODO: Desert exception
                    }
                    trigger.Weather = newWeather;
                }
                foreach(var command in setWeatherCommands)
                {
                    if(command.weather == map.weather)
                    {
                        continue; // TODO: Desert exception
                    }
                    command.weather = newWeather;
                }
            }
        }

        private void FindWeatherCommands(Map map, Script script, List<SetWeatherCommand> commands, out bool foundNonHeaderWeather)
        {
            bool found = false;
            void ProcessCommand(Command command)
            {
                if (command is SetWeatherCommand weatherCommand)
                {
                    if (weatherCommand.weather != map.weather)
                    {
                        found = true;
                    }
                    commands.Add(weatherCommand);
                }
            }
            script?.ApplyRecursively(ProcessCommand);
            foundNonHeaderWeather = found;
        }

    }
}
