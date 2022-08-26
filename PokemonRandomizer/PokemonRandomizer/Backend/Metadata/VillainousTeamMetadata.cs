using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.DataStructures.Trainers;
using PokemonRandomizer.Backend.EnumTypes;
using System;
using System.Collections.Generic;


namespace PokemonRandomizer.Backend.Metadata
{
    public class VillainousTeamMetadata : TrainerOrganizationMetadata
    {
        public override bool IsValid => TeamLeaders.Count > 0;
        public Pokemon TeamLegendary { get; set; } // Currently Unused
        public List<Trainer> TeamLeaders { get; } = new();
        public List<Trainer> TeamAdmins { get; } = new();
        public List<Trainer> TeamGrunts { get; } = new();
        public TrainerThemeData ThemeData { get; set; }

        public bool Randomized { get; set; } = false;

        public void InitializeThemeData(PokemonType[] PrimaryTypes, PokemonType[] SecondaryTypes)
        {
            ThemeData = new TrainerThemeData();
            ThemeData.SetTypes(PrimaryTypes, SecondaryTypes, SecondaryTypes.Length > 0 ? 0.70 : 0);
        }

        public override void ApplyTrainerThemeData(Settings settings)
        {
            // Only actually apply theme data when randomized
            if (!IsValid || !Randomized)
            {
                return;
            }
            if (settings.ApplyTheming(Trainer.Category.TeamLeader))
            {
                ApplyThemeDataToGroup(TeamLeaders, ThemeData);
            }
            if (settings.ApplyTheming(Trainer.Category.TeamAdmin))
            {
                ApplyThemeDataToGroup(TeamAdmins, ThemeData);
            }
            if (settings.ApplyTheming(Trainer.Category.TeamGrunt))
            {
                ApplyThemeDataToGroup(TeamGrunts, ThemeData);
            }
        }

        public override string ToString()
        {
            if(!IsValid)
            {
                return "Invalid";
            }
            if(TeamGrunts.Count > 0)
            {
                return $"{TeamGrunts[0].Class.ToUpper()} ({ThemeData})";
            }
            return $"{TeamLeaders[0].Name}'s Team ({ThemeData})";
        }
    }
}
