using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.UI.Models
{
    using static Settings;
    using static Settings.TrainerSettings;
    public class TrainerDataModel : DataModel
    {
        TrainerCategory Category { get; }
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
        public bool OverrideSettings { get; set; }
        public TrainerCategory OverrideCategory { get; set; }
        public bool RandomizePokemon { get; set; } = false;
        public double PokemonRandChance { get; set; } = 1;
        public PokemonPcgStrategy PokemonStrategy { get; set; } = PokemonPcgStrategy.KeepParty;
        public PokemonSettings PokemonSettings { get; set; } = new PokemonSettings()
        {
            Noise = 0.01f,
            ForceHighestLegalEvolution = true,
        };
        public bool RandomizeBattleType { get; set; } = false;
        public double BattleTypeRandChance { get; set; } = 1;
        public BattleTypePcgStrategy BattleTypeStrategy { get; set; } = BattleTypePcgStrategy.KeepSameType;
        public double DoubleBattleChance { get; set; } = 1;

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
