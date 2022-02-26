using System.Collections.Generic;

namespace PokemonRandomizer.UI.Models
{
    using Utilities;
    using static Settings;
    using static Settings.TrainerSettings;
    public class TrainerDataModel : DataModel
    {
        public TrainerDataModel()
        {
            PokemonSettings.Data = new List<MetricData>()
            {
                new MetricData(PokemonMetric.typeIndividual),
                new MetricData(PokemonMetric.powerIndividual, 2),
                MetricData.Empty,
            };
            PokemonSettings.BanLegendaries = true;
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
                metric.Sharpness = 10;
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
