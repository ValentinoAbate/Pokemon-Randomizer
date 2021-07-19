using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.AppSettings
{
    using PokemonRandomizer.Backend.EnumTypes;
    using UI;
    public class AppSettings : HardCodedSettings
    {
        private readonly TmHmTutorModel tmHmTutorData;
        private readonly PokemonTraitsModel pokemonData;
        private readonly StartersDataModel starterData;
        public AppSettings(MainWindow window) : base(window)
        {
        }

        public AppSettings(MainWindow window, StartersDataModel starterData, TmHmTutorModel tmHmTutorData, PokemonTraitsModel pokemonData) : base(window)
        {
            this.starterData = starterData;
            this.tmHmTutorData = tmHmTutorData;
            this.pokemonData = pokemonData;
        }

        private static double RandomChance(bool enabled, double chance) => enabled ? chance : 0;

        private static byte DoubleToByte(double d) => (byte)Math.Round(d * byte.MaxValue);

        private static byte DoubleToByteInverse(double d) => (byte)(byte.MaxValue - Math.Round(d * byte.MaxValue));

        #region TMs, HMs, and Move Tutors

        public override TmMtCompatOption TmMtCompatSetting => tmHmTutorData.TmCompatOption;
        public override TmMtCompatOption HmCompatSetting => tmHmTutorData.HmCompatOption;
        public override double TmMtTrueChance => tmHmTutorData.RandomCompatTrueChance;
        public override double TmMtNoise => tmHmTutorData.IntelligentCompatNoise;
        public override bool PreventHmMovesInTMsAndTutors => tmHmTutorData.NoHmMovesInTMsAndTutors;
        public override bool PreventDuplicateTMsAndTutors => tmHmTutorData.NoDuplicateTMsAndTutors;
        public override bool KeepImportantTMsAndTutors => tmHmTutorData.KeepImportantTmsAndTutors;
        // public override HashSet<Move> ImportantTMsAndTutors => (No UI yet)
        public override double TMRandChance => RandomChance(tmHmTutorData.RandomizeTMs, tmHmTutorData.TMRandChance);
        public override double MoveTutorRandChance => RandomChance(tmHmTutorData.RandomizeMoveTutors, tmHmTutorData.MoveTutorRandChance);

        #endregion

        #region Pokemon Base Stats

        #region Typing
        public override double SingleTypeRandChance => RandomChance(pokemonData.RandomizeSingleType, pokemonData.SingleTypeRandChance);
        public override double DualTypePrimaryRandChance => RandomChance(pokemonData.RandomizeDualTypePrimary, pokemonData.DualTypePrimaryRandChance);
        public override double DualTypeSecondaryRandChance => RandomChance(pokemonData.RandomizeDualTypeSecondary, pokemonData.DualTypeSecondaryRandChance);
        #endregion

        #region Evolution

        public override bool FixImpossibleEvos => pokemonData.FixImpossibleEvos;
        public override double ImpossibleEvoLevelStandardDev => pokemonData.ImpossibleEvoLevelStandardDev;
        public override TradeItemPokemonOption TradeItemEvoSetting => pokemonData.TradeItemEvoSetting;
        public override double DunsparsePlaugeChance => RandomChance(pokemonData.DunsparsePlague, pokemonData.DunsparsePlaugeChance);

        #endregion

        #region Catch Rates

        public override CatchRateOption CatchRateSetting => pokemonData.CatchRateSetting;
        public override bool KeepLegendaryCatchRates => pokemonData.KeepLegendaryCatchRates;
        public override byte CatchRateConstant => DoubleToByteInverse(pokemonData.CatchRateConstantDifficulty);

        #endregion

        //#region Learnsets

        //public override bool BanSelfdestruct => false;
        //public override bool AddMoves => true;
        //public override bool DisableAddingHmMoves => false;

        //public override double AddMovesChance => 1;
        //public override double NumMovesStdDeviation => 2;
        //public override double NumMovesMean => 1;

        //public override WeightedSet<AddMoveSource> AddMoveSourceWieghts { get; } = new WeightedSet<AddMoveSource>()
        //{
        //    { AddMoveSource.Random, 0.01f },
        //    { AddMoveSource.EggMoves, 0.99f },
        //};

        //#endregion

        #endregion

        #region Starter Pokemon

        public override StarterPokemonOption StarterSetting => starterData.StarterSetting;
        // public override Pokemon[] CustomStarters => starterData.CustomStarters; (Need string-to-pokemon translation)
        public override bool StrongStarterTypeTriangle => starterData.StrongStarterTypeTriangle;
        public override PokemonSettings StarterPokemonSettings => new PokemonSettings()
        {
            BanLegendaries = starterData.BanLegendaries,
        };
        // public override PokemonMetric[] StarterMetricData { get; } = new PokemonMetric[0]; (No UI for metric data yet) 
        public override bool SafeStarterMovesets => starterData.SafeStarterMovesets;

        #endregion

    }
}
