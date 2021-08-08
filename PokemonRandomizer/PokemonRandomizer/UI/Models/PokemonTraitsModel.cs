using PokemonRandomizer.Backend.Randomization;

namespace PokemonRandomizer.UI
{
    using static PokemonRandomizer.Settings;
    using CatchRateOption = Settings.CatchRateOption;
    public class PokemonTraitsModel : DataModel
    {
        // Type parameters
        public bool RandomizeSingleType { get; set; }
        public double SingleTypeRandChance { get; set; }
        public bool RandomizeDualTypePrimary { get; set; }
        public double DualTypePrimaryRandChance { get; set; }
        public bool RandomizeDualTypeSecondary { get; set; }
        public double DualTypeSecondaryRandChance { get; set; }

        // Evolution parameters
        public bool FixImpossibleEvos { get; set; } = true;
        public double ImpossibleEvoLevelStandardDev { get; set; } = 1;
        public bool ConsiderEvolveByBeautyImpossible { get; set; } = true;
        public TradeItemPokemonOption TradeItemEvoSetting { get; set; } = TradeItemPokemonOption.LevelUp;
        public bool DunsparsePlague { get; set; } = false;
        public double DunsparsePlaugeChance { get; set; } = 0;

        public CatchRateOption CatchRateSetting { get; set; } = CatchRateOption.Unchanged;
        public bool KeepLegendaryCatchRates { get; set; } = true;
        public double CatchRateConstantDifficulty { get; set; } = 0.5;

        // Learnset parameters

        public bool BanSelfdestruct { get; set; }
        public bool AddMoves { get; set; }
        public double AddMovesChance { get; set; } = 1;
        public bool DisableAddingHmMoves { get; set; } = true;
        public double NumMovesStdDeviation { get; set; } = 1;
        public double NumMovesMean { get; set; } = 1;
        public double NumMovesMin { get; set; } = 0;
        public WeightedSet<AddMoveSource> AddMoveSourceWeights {get; set;} = new WeightedSet<AddMoveSource>() { AddMoveSource.EggMoves };
    }
}
