using PokemonRandomizer.Backend.DataStructures;
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

        public void InitializeThemeData(PokemonType[] PrimaryTypes, PokemonType[] SecondaryTypes)
        {
            ThemeData = new TrainerThemeData();
            ThemeData.SetTypes(PrimaryTypes, SecondaryTypes, SecondaryTypes.Length > 0 ? 0.6 : 0);
        }

        public override void ApplyTrainerThemeData()
        {
            if (!IsValid)
            {
                return;
            }
            ApplyThemeDataToGroup(TeamLeaders, ThemeData);
            ApplyThemeDataToGroup(TeamAdmins, ThemeData);
            ApplyThemeDataToGroup(TeamGrunts, ThemeData);
        }

        public override string ToString()
        {
            return !IsValid ? "Invalid" : $"{ThemeData})";
        }
    }
}
