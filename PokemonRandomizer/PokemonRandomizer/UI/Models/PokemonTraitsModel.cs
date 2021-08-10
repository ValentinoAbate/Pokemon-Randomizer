using PokemonRandomizer.Backend.Randomization;

namespace PokemonRandomizer.UI
{
    using Utilities;
    using static PokemonRandomizer.Settings;
    using CatchRateOption = Settings.CatchRateOption;
    public class PokemonTraitsModel : DataModel
    {
        private const double defaultTypeRandChance = 0.1;
        // Type parameters
        public Box<bool> RandomizeSingleType { get; set; } = new Box<bool>();
        public Box<double> SingleTypeRandChance { get; set; } = new Box<double>(defaultTypeRandChance);
        public Box<bool> RandomizeDualTypePrimary { get; set; } = new Box<bool>();
        public Box<double> DualTypePrimaryRandChance { get; set; } = new Box<double>(defaultTypeRandChance);
        public Box<bool> RandomizeDualTypeSecondary { get; set; } = new Box<bool>();
        public Box<double> DualTypeSecondaryRandChance { get; set; } = new Box<double>(defaultTypeRandChance);

        // Evolution parameters
        public bool FixImpossibleEvos { get; set; } = true;
        public Box<double> ImpossibleEvoLevelStandardDev { get; set; } = new Box<double>(1);
        public Box<bool> ConsiderEvolveByBeautyImpossible { get; set; } = new Box<bool>(true);
        public Box<TradeItemPokemonOption> TradeItemEvoSetting { get; set; } = new Box<TradeItemPokemonOption>(TradeItemPokemonOption.LevelUp);
        public Box<bool> DunsparsePlague { get; set; } = new Box<bool>();
        public Box<double> DunsparsePlaugeChance { get; set; } = new Box<double>(0.1);

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
