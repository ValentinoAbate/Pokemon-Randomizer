using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.DataStructures.Trainers;
using System;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.Metadata
{
    public class EliteFourMetadata : TrainerOrganizationMetadata
    {
        public override bool IsValid => EliteFour.Count == 4;

        public EliteFourMember Champion { get; } = new();
        public Dictionary<string, EliteFourMember> EliteFour { get; } = new(4);

        public void InitializeThemeData(IDataTranslator dataT, Settings settings)
        {
            if (!IsValid)
            {
                return;
            }
            if(Champion.IsValid)
            {
                Champion.ThemeData = GetTrainerThemeData(Champion.FirstBattle, dataT, settings.ApplyTheming(Trainer.Category.Champion));
            }
            bool e4Theming = settings.ApplyTheming(Trainer.Category.EliteFour);
            foreach(var kvp in EliteFour)
            {
                var member = kvp.Value;
                if (member.IsValid)
                {
                    member.ThemeData = GetTrainerThemeData(member.FirstBattle, dataT, e4Theming);
                }
            }
        }

        public override void ApplyTrainerThemeData(Settings settings)
        {
            if (!IsValid)
            {
                return;
            }
            if (settings.ApplyTheming(Trainer.Category.Champion) && Champion.IsValid)
            {
                ApplyThemeDataToGroup(Champion.Battles, Champion.ThemeData);
            }
            if (settings.ApplyTheming(Trainer.Category.EliteFour))
            {
                foreach (var kvp in EliteFour)
                {
                    var member = kvp.Value;
                    if (member.IsValid)
                    {
                        ApplyThemeDataToGroup(member.Battles, member.ThemeData);
                    }
                }
            }
        }

        public class EliteFourMember
        {
            public string Name { get; private set; }
            public bool IsValid => Battles.Count > 0;
            public Trainer FirstBattle => Battles.Count > 0 ? Battles[0] : null;
            public List<Trainer> Battles { get; } = new List<Trainer>();
            public TrainerThemeData ThemeData { get; set; }

            public void Add(Trainer trainer)
            {
                Name = trainer.Name;
                Battles.Add(trainer);
            }

            public override string ToString()
            {
                return $"{Name.ToUpper()} ({ThemeData})";
            }
        }

    }
}
