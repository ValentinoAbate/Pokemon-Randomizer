using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Randomization;
using PokemonRandomizer.Backend.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.Backend.Metadata
{
    public class GymMetadata : TrainerOrganizationMetadata
    {
        public override bool IsValid => Leaders.Count > 0;
        public List<Trainer> Leaders { get; } = new List<Trainer>();
        public List<Trainer> GymTrainers { get; } = new List<Trainer>();

        public TrainerThemeData ThemeData { get; set; }

        public void InitializeCategory()
        {
            foreach(var trainer in GymTrainers)
            {
                trainer.TrainerCategory = Trainer.Category.GymTrainer;
            }
        }

        public void InitializeThemeData(IDataTranslator dataT)
        {
            if (!IsValid)
            {
                return;
            }
            var leader = Leaders[0];
            // If there is already a theme present on the leader, use that theme
            if(leader.ThemeData != null)
            {
                ThemeData = leader.ThemeData;
                return;
            }
            // Calculate Gym Type Occurence
            var typeSet = new WeightedSet<PokemonType>();
            foreach(var pokemon in leader.pokemon)
            {
                var stats = dataT.GetBaseStats(pokemon.species);
                typeSet.Add(stats.OriginalPrimaryType);
                if (stats.IsDualTyped)
                {
                    typeSet.Add(stats.OriginalSecondaryType);
                }
            }
            // Find maximum occurence(s)
            var max = float.MinValue;
            var types = new List<PokemonType>(6);
            foreach(var kvp in typeSet)
            {
                if(kvp.Value > max)
                {
                    max = kvp.Value;
                    types.Clear();
                    types.Add(kvp.Key);
                }
                else if(kvp.Value == max){
                    types.Add(kvp.Key);
                }
            }
            // Create Theme Data
            ThemeData = new TrainerThemeData()
            {
                Types = types.ToArray(),
                Theme = TrainerThemeData.TrainerTheme.Typed
            };
        }

        public override void ApplyTrainerThemeData(Settings settings)
        {
            if (!IsValid)
            {
                return;
            }
            if (settings.ApplyTheming(Trainer.Category.GymLeader))
            {
                ApplyThemeDataToGroup(Leaders, ThemeData);
            }
            if (settings.ApplyTheming(Trainer.Category.GymTrainer))
            {
                ApplyThemeDataToGroup(GymTrainers, ThemeData);
            }
        }

        public override string ToString()
        {
            return !IsValid ? "Invalid" : $"{Leaders[0].name}'s Gym ({ThemeData})";
        }
    }
}
