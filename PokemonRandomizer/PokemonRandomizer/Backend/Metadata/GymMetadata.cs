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
        public bool Invalid => Leaders.Count <= 0;
        public List<Trainer> Leaders { get; set; } = new List<Trainer>();
        public List<Trainer> GymTrainers { get; set; } = new List<Trainer>();

        public TrainerMetadata TrainerMetadata { get; set; }

        public override void ApplyTrainerMetadata()
        {
            if (Invalid)
                return;
            foreach(var trainer in Leaders)
            {
                trainer.Metadata = TrainerMetadata;
            }
            foreach(var trainer in GymTrainers)
            {
                trainer.Metadata = TrainerMetadata;
            }
        }

        public override string ToString()
        {
            return Invalid ? "Invalid" : $"{Leaders[0].name}'s Gym ({TrainerMetadata})";
        }
    }
}
