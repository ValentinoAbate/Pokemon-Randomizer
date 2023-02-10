using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.DataStructures.Trainers;
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

        public override void InitializeThemeData(IDataTranslator dataT, Settings s)
        {
            if (!IsValid)
            {
                return;
            }
            ThemeData = GetTrainerThemeData(Leaders[0], dataT, s.ApplyTheming(Trainer.Category.GymLeader));
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
            return !IsValid ? "Invalid" : $"{Leaders[0].Name}'s Gym ({ThemeData})";
        }
    }
}
