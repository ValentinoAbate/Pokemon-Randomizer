using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Metadata;
using PokemonRandomizer.Backend.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonRandomizer.Backend.Utilities.Debug;

namespace PokemonRandomizer.Backend.Randomization
{
    public class TrainerOrganizationRandomizer
    {
        private readonly Random rand;
        public TrainerOrganizationRandomizer(Random rand)
        {
            this.rand = rand;
        }

        private bool IsRandom(Settings.TrainerOrgTypeTheme theme) => theme == Settings.TrainerOrgTypeTheme.Random;

        public void RandomizeGymsAndEliteFour(IEnumerable<GymMetadata> gyms, EliteFourMetadata eliteFour, IEnumerable<PokemonType> allTypes, Settings settings)
        {
            // If none of the categories are actually going to be randomized, return early
            if(!IsRandom(settings.GymTypeTheming) && !IsRandom(settings.EliteFourTheming) && !IsRandom(settings.ChampionTheming))
            {
                return;
            }
            var typeSet = allTypes.ToHashSet();
            // If no duplicates, remove any original types that won't be randomized
            if (settings.NoDuplicateGymsAndEliteFour)
            {
                var originalThemes = new List<TrainerThemeData>(16); // 8 gyms + 4 elite 4 + 1 champ
                if (!settings.ApplyTheming(Trainer.Category.GymLeader))
                {
                    originalThemes.AddRange(gyms.Select(g => g.ThemeData));
                }
                if (!settings.ApplyTheming(Trainer.Category.EliteFour))
                {
                    originalThemes.AddRange(eliteFour.EliteFour.Select(kvp => kvp.Value.ThemeData));
                }
                if (!settings.ApplyTheming(Trainer.Category.Champion))
                {
                    originalThemes.Add(eliteFour.Champion.ThemeData);
                }
                foreach(var theme in originalThemes)
                {
                    if(theme.Theme != TrainerThemeData.TrainerTheme.Typed)
                    {
                        continue;
                    }
                    foreach(var type in theme.Types)
                    {
                        if (!typeSet.Contains(type))
                            continue;
                        typeSet.Remove(type);
                    }
                }
            }
            // Randomize Gyms
            if(IsRandom(settings.GymTypeTheming))
            {
                foreach(var gym in gyms)
                {
                    RandomizeGymOrEliteFourThemeData(gym.ThemeData, typeSet, allTypes);
                }
            }
            if(IsRandom(settings.EliteFourTheming))
            {
                foreach (var kvp in eliteFour.EliteFour)
                {
                    RandomizeGymOrEliteFourThemeData(kvp.Value.ThemeData, typeSet, allTypes);
                }
                // If on the "Same as Elite Four Setting" the randomize the champ here
                if (settings.ChampionTheming == Settings.TrainerOrgTypeTheme.Default)
                {
                    RandomizeGymOrEliteFourThemeData(eliteFour.Champion.ThemeData, typeSet, allTypes);
                }
            }
            if (IsRandom(settings.ChampionTheming))
            {
                RandomizeGymOrEliteFourThemeData(eliteFour.Champion.ThemeData, typeSet, allTypes);
            }
        }

        private void RandomizeGymOrEliteFourThemeData(TrainerThemeData data, HashSet<PokemonType> typeSet, IEnumerable<PokemonType> allTypes)
        {
            // Later allow for randomly chosen theme
            var newType = ChooseType(data.Types, typeSet, allTypes);
            data.SetTypes(new PokemonType[] { newType }, Array.Empty<PokemonType>(), 1);
        }

        public void RandomizeVillainousTeams(IEnumerable<VillainousTeamMetadata> teams, IEnumerable<PokemonType> allTypes, Settings settings)
        {
            if (!IsRandom(settings.TeamTypeTheming))
                return;
            var typeSet = allTypes.ToHashSet();
            if (settings.KeepTeamSubtypes)
            {
                foreach(var team in teams)
                {
                    var theme = team.ThemeData;
                    var teamTypes = theme.Types.Concat(theme.SecondaryTypes).ToArray();
                    var newType = ChooseType(teamTypes, typeSet, allTypes);
                    theme.SetTypes(new PokemonType[] { newType }, theme.SecondaryTypes, theme.PrimaryTypeChance);
                }
            }
            else
            {
                foreach (var team in teams)
                {
                    var theme = team.ThemeData;
                    var teamTypes = new List<PokemonType>(theme.Types.Length + theme.SecondaryTypes.Length + 1);
                    teamTypes.AddRange(theme.Types);
                    teamTypes.AddRange(theme.SecondaryTypes);
                    var newPrimaryType = ChooseType(teamTypes, typeSet, allTypes);
                    var newSecondaryType = ChooseType(teamTypes, typeSet, allTypes);
                    theme.SetTypes(new PokemonType[] { newPrimaryType }, new PokemonType[] { newSecondaryType }, theme.PrimaryTypeChance);
                }
            }
        }

        private PokemonType ChooseType(IReadOnlyCollection<PokemonType> originals, HashSet<PokemonType> types, IEnumerable<PokemonType> all)
        {
            var addBackTypes = new HashSet<PokemonType>(originals.Count);
            foreach(var type in originals)
            {
                if (types.Contains(type))
                {
                    addBackTypes.Add(type);
                    types.Remove(type);
                }
            }
            // If we are out of choices, refill
            if (!types.Any())
            {
                foreach(var t in all)
                {
                    if(originals.Contains(t))
                    {
                        if (!addBackTypes.Contains(t))
                        {
                            addBackTypes.Add(t);
                        }
                    }
                    else
                    {
                        types.Add(t);
                    }
                }
            }
            // Error state
            if (!types.Any())
            { 
                foreach(var type in addBackTypes)
                {
                    types.Add(type);
                }
                Logger.main.Error("Trainer Org Type Choice Error: No valid type to choose. Returning an original type if one exists, else NRM");
                return originals.Count > 0 ? originals.First() : PokemonType.NRM;
            }
            // Choose type
            var newType = rand.Choice(types);
            types.Remove(newType);
            foreach (var type in addBackTypes)
            {
                types.Add(type);
            }
            return newType;
        }
    }
}
