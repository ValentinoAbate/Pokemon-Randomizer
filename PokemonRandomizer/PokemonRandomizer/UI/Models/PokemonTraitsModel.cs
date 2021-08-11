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
        public Box<bool> KeepLegendaryCatchRates { get; set; } = new Box<bool>(true);
        public Box<double> CatchRateConstantDifficulty { get; set; } = new Box<double>(0.5);

        // Learnset parameters

        public Box<bool> BanSelfdestruct { get; set; } = new Box<bool>();
        public Box<bool> AddMoves { get; set; } = new Box<bool>();
        public Box<double> AddMovesChance { get; set; } = new Box<double>(1);
        public Box<bool> DisableAddingHmMoves { get; set; } = new Box<bool>(true);
        public Box<double> NumMovesStdDeviation { get; set; } = new Box<double>(1);
        public Box<double> NumMovesMean { get; set; } = new Box<double>(1);
        public Box<double> NumMovesMin { get; set; } = new Box<double>(0);
        public WeightedSet<AddMoveSource> AddMoveSourceWeights {get; set;} = new WeightedSet<AddMoveSource>() { AddMoveSource.EggMoves };
    }
}
