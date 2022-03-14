using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Randomization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.Backend.Metadata
{
    public abstract class TrainerOrganizationMetadata
    {
        public abstract bool IsValid { get; }
        public abstract void ApplyTrainerThemeData(Settings settings);
        protected void ApplyThemeDataToGroup(IEnumerable<Trainer> group, TrainerThemeData data)
        {
            foreach (var trainer in group)
            {
                trainer.ThemeData = data;
            }
        }

        protected TrainerThemeData GetTrainerThemeData(Trainer trainer, IDataTranslator dataT, bool applyTheming)
        {
            if (!applyTheming)
            {
                return new TrainerThemeData() { Theme = TrainerThemeData.TrainerTheme.Untyped };
            }
            if(trainer.ThemeData != null)
            {
                return trainer.ThemeData;
            }
            return GetTrainerTypeThemeData(trainer, dataT);
        }

        protected TrainerThemeData GetTrainerTypeThemeData(Trainer trainer, IDataTranslator dataT)
        {
            // Calculate Trainer Type Occurence
            var typeSet = PokemonMetrics.TypeOccurence(trainer.Pokemon.Select(p => dataT.GetBaseStats(p.species)));
            // Find maximum occurence(s)
            var max = float.MinValue;
            var types = new List<PokemonType>(6);
            foreach (var kvp in typeSet)
            {
                if (kvp.Value > max)
                {
                    max = kvp.Value;
                    types.Clear();
                    types.Add(kvp.Key);
                }
                else if (kvp.Value == max)
                {
                    types.Add(kvp.Key);
                }
            }
            // Create Theme Data
            return new TrainerThemeData()
            {
                Types = types.ToArray(),
                Theme = TrainerThemeData.TrainerTheme.Typed
            };
        }
    }
}
