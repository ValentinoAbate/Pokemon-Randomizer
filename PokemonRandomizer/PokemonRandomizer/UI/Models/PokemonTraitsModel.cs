using PokemonRandomizer.Backend.Randomization;

namespace PokemonRandomizer.UI.Models
{
    using Utilities;
    using static PokemonRandomizer.Settings;
    using CatchRateOption = Settings.CatchRateOption;
    public class PokemonTraitsModel : DataModel
    {
        // Evolution parameters
        public Box<bool> FixImpossibleEvos { get; set; } = new Box<bool>(true);
        public Box<double> ImpossibleEvoLevelStandardDev { get; set; } = new Box<double>(1);
        public Box<bool> ConsiderEvolveByBeautyImpossible { get; set; } = new Box<bool>(true);
        public Box<TradeItemPokemonOption> TradeItemEvoSetting { get; set; } = new Box<TradeItemPokemonOption>(TradeItemPokemonOption.UseItem);
        public Box<bool> DunsparsePlague { get; set; } = new Box<bool>();
        public Box<double> DunsparsePlaugeChance { get; set; } = new Box<double>(0.1);

        // Catch rate parameters
        public Box<CatchRateOption> CatchRateSetting { get; set; } = new Box<CatchRateOption>(CatchRateOption.Unchanged);
        public Box<bool> KeepLegendaryCatchRates { get; set; } = new Box<bool>(true);
        public Box<double> CatchRateConstantDifficulty { get; set; } = new Box<double>(0.5);

        // Hatch rate parameters
        public Box<bool> FastHatching { get; set; } = new(false);
        
        // Yield parameters
        public Box<double> BaseExpYieldMultiplier { get; set; } = new Box<double>(1);
        public Box<bool> ZeroBaseEVs { get; set; } = new Box<bool>(false);

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
