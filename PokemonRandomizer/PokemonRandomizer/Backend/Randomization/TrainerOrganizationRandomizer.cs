﻿using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Metadata;
using PokemonRandomizer.Backend.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using PokemonRandomizer.Backend.Utilities.Debug;
using PokemonRandomizer.Backend.DataStructures.Trainers;

namespace PokemonRandomizer.Backend.Randomization
{
    public class TrainerOrganizationRandomizer
    {
        private readonly Random rand;
        private readonly VariantPaletteModifier paletteModifier;
        public TrainerOrganizationRandomizer(Random rand, VariantPaletteModifier paletteModifier)
        {
            this.rand = rand;
            this.paletteModifier = paletteModifier;
        }

        private bool IsRandom(Settings.TrainerOrgTypeTheme theme) => theme == Settings.TrainerOrgTypeTheme.Random;

        public void RandomizeGymsAndEliteFour(IEnumerable<GymMetadata> gyms, EliteFourMetadata eliteFour, IEnumerable<PokemonType> allTypes, Settings settings, Dictionary<string, List<string>> randomizationResults)
        {
            // If none of the categories are actually going to be randomized, return early
            if(!IsRandom(settings.GymTypeTheming) && !IsRandom(settings.EliteFourTheming) && !IsRandom(settings.ChampionTheming))
            {
                return;
            }
            var typeSet = allTypes.ToHashSet();
            // If no duplicates, remove any original types that won't be randomized
            if (settings.GymEliteFourDupePrevention == Settings.GymEliteFourPreventDupesSetting.All)
            {
                var originalThemes = new List<TrainerThemeData>(16); // 8 gyms + 4 elite 4 + 1 champ
                if (settings.ApplyTheming(Trainer.Category.GymLeader) && !IsRandom(settings.GymTypeTheming))
                {
                    originalThemes.AddRange(gyms.Select(g => g.ThemeData));
                }
                if (settings.ApplyTheming(Trainer.Category.EliteFour) && !IsRandom(settings.EliteFourTheming))
                {
                    originalThemes.AddRange(eliteFour.EliteFour.Select(kvp => kvp.Value.ThemeData));
                    if(settings.ChampionTheming == Settings.TrainerOrgTypeTheme.Default && eliteFour.Champion.IsValid)
                    {
                        originalThemes.Add(eliteFour.Champion.ThemeData);
                    }
                }
                if (settings.ApplyTheming(Trainer.Category.Champion) && !IsRandom(settings.ChampionTheming) && eliteFour.Champion.IsValid)
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
            bool preventDupes = settings.GymEliteFourDupePrevention != Settings.GymEliteFourPreventDupesSetting.None;
            // Randomize Gyms
            if (IsRandom(settings.GymTypeTheming))
            {
                foreach(var gym in gyms)
                {
                    RandomizeGymOrEliteFourThemeData(gym.ThemeData, typeSet, allTypes, preventDupes);
                }
            }
            if(IsRandom(settings.EliteFourTheming))
            {
                foreach (var kvp in eliteFour.EliteFour)
                {
                    RandomizeGymOrEliteFourThemeData(kvp.Value.ThemeData, typeSet, allTypes, preventDupes);
                }
                // If on the "Same as Elite Four Setting" the randomize the champ here
                if (settings.ChampionTheming == Settings.TrainerOrgTypeTheme.Default && eliteFour.Champion.IsValid)
                {
                    RandomizeGymOrEliteFourThemeData(eliteFour.Champion.ThemeData, typeSet, allTypes, preventDupes);
                }
            }
            if (IsRandom(settings.ChampionTheming) && eliteFour.Champion.IsValid)
            {
                RandomizeGymOrEliteFourThemeData(eliteFour.Champion.ThemeData, typeSet, allTypes, preventDupes);
            }
            randomizationResults.Add("Gyms", gyms.Select(g => g.ToString()).ToList());
            randomizationResults.Add("Elite 4", eliteFour.EliteFour.Select(kvp => kvp.Value.ToString()).ToList());
            if (eliteFour.Champion.IsValid)
            {
                randomizationResults.Add("Champion", new List<string>() { eliteFour.Champion.ToString() });
            }
        }

        private void RandomizeGymOrEliteFourThemeData(TrainerThemeData data, HashSet<PokemonType> typeSet, IEnumerable<PokemonType> allTypes, bool preventDupes)
        {
            // Later allow for randomly chosen theme
            var newType = ChooseType(data.Types, typeSet, allTypes, preventDupes);
            data.SetTypes(new PokemonType[] { newType }, Array.Empty<PokemonType>(), 1);
        }

        public void RandomizeVillainousTeams(IEnumerable<VillainousTeamMetadata> teams, IEnumerable<PokemonType> allTypes, Settings settings, Dictionary<string, List<string>> randomizationResults)
        {
            if (!IsRandom(settings.TeamTypeTheming))
                return;
            var typeSet = allTypes.ToHashSet();
            if (rand.RollSuccess(settings.TeamDualTypeChance))
            {
                foreach (var team in teams)
                {
                    var theme = team.ThemeData;
                    var teamTypes = new List<PokemonType>(theme.Types);
                    var newPrimaryType = ChooseType(teamTypes, typeSet, allTypes, true);
                    var newSecondaryType = ChooseType(teamTypes, typeSet, allTypes, true);
                    theme.SetTypes(new PokemonType[] { newPrimaryType }, new PokemonType[] { newSecondaryType }, theme.PrimaryTypeChance);
                    ModifyTeamPalettes(settings, team, newPrimaryType);
                    team.Randomized = true;
                }
            }
            else
            {
                foreach (var team in teams)
                {
                    var theme = team.ThemeData;
                    var teamTypes = new List<PokemonType>(theme.Types);
                    var newType = ChooseType(teamTypes, typeSet, allTypes, true);
                    theme.SetTypes(new PokemonType[] { newType }, Array.Empty<PokemonType>(), 1);
                    ModifyTeamPalettes(settings, team, newType);
                    team.Randomized = true;
                }
            }
            randomizationResults.Add("Villainous Teams", teams.Select(t => t.ToString()).ToList());
        }

        private void ModifyTeamPalettes(Settings settings, VillainousTeamMetadata team, PokemonType type)
        {
            foreach (var (palette, paletteData, paletteType) in team.Palettes)
            {
                if (paletteType == VillainousTeamMetadata.SpecialPaletteType.GymLeader && settings.PriorityThemeCategory != Trainer.Category.TeamLeader)
                    continue;
                if (paletteType == VillainousTeamMetadata.SpecialPaletteType.Grunt && !settings.GruntTheming)
                    continue;
                paletteModifier.ModifyPalette(palette, paletteData, new PokemonType[] { type });
            }
        }

        private PokemonType ChooseType(IReadOnlyCollection<PokemonType> originals, HashSet<PokemonType> types, IEnumerable<PokemonType> all, bool preventDupes)
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
            if (preventDupes)
            {
                types.Remove(newType);
            }
            foreach (var type in addBackTypes)
            {
                types.Add(type);
            }
            return newType;
        }
    }
}
