using System.Collections.Generic;

namespace PokemonRandomizer.UI.Models
{
    using Utilities;
    using static Settings;
    using static Settings.TrainerSettings;
    public class TrainerDataModel : DataModel
    {
        public TrainerCategory Category { get; }
        public bool OverrideSettings { get; set; }
        public TrainerCategory OverrideCategory { get; set; }
        public override string Name { get; }

        public TrainerDataModel() : this(TrainerCategory.Trainer, defaultName)
        {

        }

        public TrainerDataModel(TrainerCategory category, string name)
        {
            Category = category;
            Name = name;
            switch (Category)
            {
                case TrainerCategory.Trainer:
                    PokemonSettings.Data = new List<MetricData>()
                    {
                        new MetricData(PokemonMetric.typeIndividual),
                        new MetricData(PokemonMetric.powerIndividual, 2),
                        MetricData.Empty,
                    };
                    PokemonSettings.BanLegendaries = true;
                    break;
                case TrainerCategory.AceTrainer:
                    PokemonSettings.BanLegendaries = true;
                    goto case TrainerCategory.Rival;
                case TrainerCategory.Rival:
                    PokemonSettings.Data = new List<MetricData>()
                    {
                        MetricData.Empty,
                        MetricData.Empty,
                        MetricData.Empty,
                    };
                    break;
                case TrainerCategory.GymLeader:
                    PokemonSettings.Data = new List<MetricData>()
                    {
                        new MetricData(PokemonMetric.typeTrainerParty),
                        new MetricData(PokemonMetric.powerIndividual, 3),
                        MetricData.Empty,
                    };
                    break;
                case TrainerCategory.EliteFour:
                case TrainerCategory.Champion:
                    PokemonSettings.Data = new List<MetricData>()
                    {
                        new MetricData(PokemonMetric.typeTrainerParty),
                        MetricData.Empty,
                        MetricData.Empty,
                    };
                    break;
            }
            foreach(var metric in PokemonSettings.Data)
            {
                InitializeMetric(metric);
            }
        }

        public Box<bool> RandomizePokemon { get; set; } = new Box<bool>(false);
        public Box<double> PokemonRandChance { get; set; } = new Box<double>(1);
        public Box<PokemonPcgStrategy> PokemonStrategy { get; set; } = new Box<PokemonPcgStrategy>(PokemonPcgStrategy.KeepParty);
        public PokemonSettings PokemonSettings { get; set; } = new PokemonSettings()
        {
            Noise = 0.01f,
            ForceHighestLegalEvolution = true,
        };
        public Box<bool> RandomizeBattleType { get; set; } = new Box<bool>(false);
        public Box<double> BattleTypeRandChance { get; set; } = new Box<double>(1);
        public Box<BattleTypePcgStrategy> BattleTypeStrategy { get; set; } = new Box<BattleTypePcgStrategy>(BattleTypePcgStrategy.KeepSameType);
        public Box<double> DoubleBattleChance { get; set; } = new Box<double>(1);

        public void InitializeMetric(MetricData metric)
        {
            if (metric.DataSource == PokemonMetric.typeTrainerParty)
            {
                metric.Sharpness = 10000;
                metric.Filter = 0.2f;
            }
            else
            {
                metric.Reset();
            }
            return;
        }
    }
}
