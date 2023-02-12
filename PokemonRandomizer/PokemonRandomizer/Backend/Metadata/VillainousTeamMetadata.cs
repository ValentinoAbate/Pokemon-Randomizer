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
        public List<(Palette pal, Randomization.VariantPaletteModifier.PaletteData palData, bool isGymLeader)> Palettes { get; } = new();

        public PokemonType[] DefaultPrimaryTypes { get; set; }
        public PokemonType[] DefaultSecondaryTypes { get; set; }

        public bool Randomized { get; set; } = false;

        public override void InitializeThemeData(IDataTranslator dataT, Settings settings)
        {
            if(settings.TeamTypeTheming is Settings.TrainerOrgTypeTheme.Off)
            {
                ThemeData = new TrainerThemeData() { Theme = TrainerThemeData.TrainerTheme.Untyped };
            }
            else if(settings.TeamTypeTheming is Settings.TrainerOrgTypeTheme.On or Settings.TrainerOrgTypeTheme.Random)
            {
                ThemeData = new TrainerThemeData();
                ThemeData.SetTypes(DefaultPrimaryTypes, DefaultSecondaryTypes, DefaultSecondaryTypes.Length > 0 ? 0.70 : 0);
            }
            else
            {
                ThemeData = null;
            }
        }

        public override void ApplyTrainerThemeData(Settings settings)
        {
            // Only actually apply theme data when randomized or off
            if (!IsValid || ThemeData == null)
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
                return $"{TeamGrunts[0].ClassName.ToUpper()} ({ThemeData})";
            }
            return $"{TeamLeaders[0].Name}'s Team ({ThemeData})";
        }
    }
}
