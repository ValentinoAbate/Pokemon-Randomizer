using PokemonRandomizer.Backend.DataStructures;
using System;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.Metadata
{
    public abstract class TrainerOrganizationMetadata
    {
        public abstract bool IsValid { get; }
        public abstract void ApplyTrainerThemeData();
        protected void ApplyThemeDataToGroup(IEnumerable<Trainer> group, TrainerThemeData data)
        {
            foreach (var trainer in group)
            {
                trainer.ThemeData = data;
            }
        }
    }
}
