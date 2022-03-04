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
        public override bool IsValid => Leaders.Count <= 0;
        public List<Trainer> Leaders { get; } = new List<Trainer>();
        public List<Trainer> GymTrainers { get; } = new List<Trainer>();

        public TrainerThemeData TrainerMetadata { get; set; }

        public override void ApplyTrainerThemeData()
        {
            if (!IsValid)
            {
                return;
            }
            ApplyThemeDataToGroup(Leaders, TrainerMetadata);
            ApplyThemeDataToGroup(GymTrainers, TrainerMetadata);
        }

        public override string ToString()
        {
            return IsValid ? "Invalid" : $"{Leaders[0].name}'s Gym ({TrainerMetadata})";
        }
    }
}
