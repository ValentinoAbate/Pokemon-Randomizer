using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.AppSettings
{
    using PokemonRandomizer.Backend.Randomization;
    using UI;
    using UI.Models;
    public class AppSettings : HardCodedSettings
    {
        private readonly TmHmTutorModel tmHmTutorData;
        private readonly PokemonTraitsModel pokemonData;
        private readonly StartersDataModel starterData;
        private readonly WildEncounterDataModel wildEncounterData;
        private readonly Dictionary<TrainerCategory, TrainerDataModel> trainerData;
        public AppSettings(MainWindow window) : base(window)
        {
        }

        public AppSettings(MainWindow window, StartersDataModel starterData, TmHmTutorModel tmHmTutorData, PokemonTraitsModel pokemonData, WildEncounterDataModel wildEncounterData, IEnumerable<TrainerDataModel> trainerData) : base(window)
        {
            this.starterData = starterData;
            this.tmHmTutorData = tmHmTutorData;
            this.pokemonData = pokemonData;
            this.wildEncounterData = wildEncounterData;
            this.trainerData = trainerData.ToDictionary((tData) => tData.Category);
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
        public override bool ConsiderEvolveByBeautyImpossible => pokemonData.ConsiderEvolveByBeautyImpossible;
        public override double ImpossibleEvoLevelStandardDev => pokemonData.ImpossibleEvoLevelStandardDev;
        public override TradeItemPokemonOption TradeItemEvoSetting => pokemonData.TradeItemEvoSetting;
        public override double DunsparsePlaugeChance => RandomChance(pokemonData.DunsparsePlague, pokemonData.DunsparsePlaugeChance);

        #endregion

        #region Catch Rates

        public override CatchRateOption CatchRateSetting => pokemonData.CatchRateSetting;
        public override bool KeepLegendaryCatchRates => pokemonData.KeepLegendaryCatchRates;
        public override byte CatchRateConstant => DoubleToByteInverse(pokemonData.CatchRateConstantDifficulty);

        #endregion

        #region Learnsets

        public override bool BanSelfdestruct => pokemonData.BanSelfdestruct;
        public override bool AddMoves => pokemonData.AddMoves;    
        public override bool DisableAddingHmMoves => pokemonData.DisableAddingHmMoves;
        public override double AddMovesChance => pokemonData.AddMovesChance;
        public override double NumMovesStdDeviation => pokemonData.NumMovesStdDeviation;
        public override double NumMovesMean => pokemonData.NumMovesMean;
        public override WeightedSet<AddMoveSource> AddMoveSourceWeights => pokemonData.AddMoveSourceWeights;

        #endregion

        #endregion

        #region Starter Pokemon

        public override StarterPokemonOption StarterSetting => starterData.StarterSetting;
        // TODO:
        // public override Pokemon[] CustomStarters => starterData.CustomStarters; (Need string-to-pokemon translation)
        public override bool StrongStarterTypeTriangle => starterData.StrongStarterTypeTriangle;
        public override PokemonSettings StarterPokemonSettings => new PokemonSettings()
        {
            BanLegendaries = starterData.BanLegendaries,
        };
        // TODO:
        // public override PokemonMetric[] StarterMetricData { get; } = new PokemonMetric[0]; (No UI for metric data yet) 
        public override bool SafeStarterMovesets => starterData.SafeStarterMovesets;

        #endregion

        #region Trainers

        public override TrainerSettings GetTrainerSettings(TrainerCategory trainerClass)
        {
            static TrainerSettings TrainerDataToSettings(TrainerDataModel model)
            {
                return new TrainerSettings()
                {
                    PokemonRandChance = RandomChance(model.RandomizePokemon, model.PokemonRandChance),
                    PokemonStrategy = model.PokemonStrategy,
                    PokemonSettings = model.PokemonSettings,
                    BattleTypeRandChance = RandomChance(model.RandomizeBattleType, model.BattleTypeRandChance),
                    BattleTypeStrategy = model.BattleTypeStrategy,
                    DoubleBattleChance = model.DoubleBattleChance,
                };
            }
            if (trainerData.ContainsKey(trainerClass))
            {
                return TrainerDataToSettings(trainerData[trainerClass]);
            }
            if(trainerData.ContainsKey(TrainerCategory.Trainer))
            {
                return TrainerDataToSettings(trainerData[TrainerCategory.Trainer]);
            }
            return base.GetTrainerSettings(trainerClass);
        }

        #endregion

        #region Wild Pokemon

        public override WildEncounterRandomizer.Strategy EncounterStrategy => wildEncounterData.Strategy;
        public override PokemonSettings EncounterSettings => wildEncounterData.PokemonSettings;

        #endregion

    }
}
