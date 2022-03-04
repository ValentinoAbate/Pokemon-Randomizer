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
        public PokemonType[] PrimaryTypes { get; set; }
        public PokemonType[] SecondaryTypes { get; set; }
        public List<Trainer> TeamLeaders { get; } = new();
        public List<Trainer> TeamAdmins { get; } = new();
        public List<Trainer> TeamGrunts { get; } = new();
        public override void ApplyTrainerThemeData()
        {
            if (!IsValid)
            {
                return;
            }
            TrainerThemeData themeData;
            if(PrimaryTypes.Length + SecondaryTypes.Length > 0)
            {
                themeData = new TrainerThemeData()
                {
                    Theme = TrainerThemeData.TrainerTheme.Untyped,
                };
            }
            else
            {
                var types = new PokemonType[PrimaryTypes.Length + SecondaryTypes.Length];
                Array.Copy(PrimaryTypes, 0, types, 0, PrimaryTypes.Length);
                Array.Copy(SecondaryTypes, 0, types, 0, SecondaryTypes.Length);
                themeData = new TrainerThemeData()
                {
                    Theme = TrainerThemeData.TrainerTheme.Typed,
                    Types = types,
                };
            }
            ApplyThemeDataToGroup(TeamLeaders, themeData);
            ApplyThemeDataToGroup(TeamAdmins, themeData);
            ApplyThemeDataToGroup(TeamGrunts, themeData);
        }
    }
}
