﻿using System.Collections.Generic;

namespace PokemonRandomizer.UI.Models
{
    using Utilities;
    using static Settings;
    using static Settings.TrainerSettings;
    public class TrainerDataModel : DataModel
    {
        public enum DuplicateReductionOption
        {
            None,
            Weak,
            Moderate,
            Strong,
            Strict,
        }

        // Pokemon Settings
        public Box<bool> RandomizePokemon { get; set; } = new Box<bool>(false);
        public Box<bool> TypeTheming { get; set; } = new Box<bool>(true);
        public Box<TrainerTypeDataSource> TypeDataSource { get; set; } = new Box<TrainerTypeDataSource>(TrainerTypeDataSource.Individual);
        public Box<PokemonPcgStrategy> PokemonStrategy { get; set; } = new Box<PokemonPcgStrategy>(PokemonPcgStrategy.KeepParty);
        public Box<bool> BanLegendaries { get; set; } = new Box<bool>(true); 
        public Box<bool> BanLegendariesMiniboss { get; set; } = new Box<bool>(true); 
        public Box<bool> BanLegendariesBoss { get; set; } = new Box<bool>(false); 
        public Box<bool> RestrictIllegalEvolutions { get; set; } = new Box<bool>(true);
        public Box<bool> ForceHighestLegalEvolution { get; set; } = new Box<bool>(true);
        public Box<double> PokemonNoise { get; set; } = new Box<double>(0.001);
        public Box<DuplicateReductionOption> DuplicateReduction { get; set; } = new Box<DuplicateReductionOption>(DuplicateReductionOption.Moderate);
        // Battle Type Settings
        public Box<bool> RandomizeBattleType { get; set; } = new Box<bool>(false);
        public Box<BattleTypePcgStrategy> BattleTypeStrategy { get; set; } = new Box<BattleTypePcgStrategy>(BattleTypePcgStrategy.KeepSameType);
        public Box<double> DoubleBattleChance { get; set; } = new Box<double>(1);
        // Difficulty
        public Box<double> LevelMult { get; set; } = new Box<double>(1);
        public Box<double> MinIVs { get; set; } = new Box<double>(0);
        public Box<bool> SmartAI { get; set; } = new Box<bool>(false);
        public Box<bool> ForceCustomMoves { get; set; } = new Box<bool>(true);
        public Box<double> NumBonusPokemon { get; set; } = new Box<double>(0);
        public Box<bool> BonusPokemon { get; set; } = new Box<bool>(false);
        public Box<bool> BonusPokemonMiniboss { get; set; } = new Box<bool>(false);
        public Box<bool> BonusPokemonBoss { get; set; } = new Box<bool>(true);

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
