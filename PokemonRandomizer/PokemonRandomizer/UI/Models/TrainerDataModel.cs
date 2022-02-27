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

        }

        // Pokemon Settings
        public Box<bool> RandomizePokemon { get; set; } = new Box<bool>(false);
        public Box<double> PokemonRandChance { get; set; } = new Box<double>(1);
        public Box<bool> TypeTheming { get; set; } = new Box<bool>(true);
        public Box<PokemonPcgStrategy> PokemonStrategy { get; set; } = new Box<PokemonPcgStrategy>(PokemonPcgStrategy.KeepParty);
        public Box<bool> BanLegendaries { get; set; } = new Box<bool>(true); 
        public Box<bool> BanLegendariesMiniboss { get; set; } = new Box<bool>(true); 
        public Box<bool> BanLegendariesBoss { get; set; } = new Box<bool>(false); 
        public Box<bool> RestrictIllegalEvolutions { get; set; } = new Box<bool>(true);
        public Box<bool> ForceHighestLegalEvolution { get; set; } = new Box<bool>(false);
        public Box<double> PokemonNoise { get; set; } = new Box<double>(0.003);
        // Battle Type Settings
        public Box<bool> RandomizeBattleType { get; set; } = new Box<bool>(false);
        public Box<double> BattleTypeRandChance { get; set; } = new Box<double>(1);
        public Box<BattleTypePcgStrategy> BattleTypeStrategy { get; set; } = new Box<BattleTypePcgStrategy>(BattleTypePcgStrategy.KeepSameType);
        public Box<double> DoubleBattleChance { get; set; } = new Box<double>(1);
        // Difficulty
        public Box<double> LevelMult { get; set; } = new Box<double>(1);
        public Box<double> MinIVs { get; set; } = new Box<double>(0);
        public Box<bool> SmartAI { get; set; } = new Box<bool>(false);

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
