using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.AppSettings
{
    using Backend.Randomization;
    using Backend.EnumTypes;
    using UI;
    using UI.Models;
    using PokemonRandomizer.Backend.DataStructures;

    public class AppSettings : HardCodedSettings
    {
        public RomMetadata Metadata { get; set; }
        private readonly TmHmTutorModel tmHmTutorData;
        private readonly PokemonTraitsModel pokemonData;
        private readonly InGameTradesDataModel tradeData;
        private readonly StartersDataModel starterData;
        private readonly WildEncounterDataModel wildEncounterData;
        private readonly Dictionary<TrainerCategory, TrainerDataModel> trainerData;
        private readonly ItemDataModel itemData;
        private readonly WeatherDataModel weatherData;
        private readonly MiscDataModel miscData;
        public AppSettings(RandomizerDataModel randomizerData) : base(randomizerData)
        {
        }

        public AppSettings(RandomizerDataModel randomizerData, SpecialPokemonDataModel specialPokemonData, TmHmTutorModel tmHmTutorData, PokemonTraitsModel pokemonData, WildEncounterDataModel wildEncounterData, IEnumerable<TrainerDataModel> trainerData, ItemDataModel itemData, WeatherDataModel weatherData, MiscDataModel miscData) : base(randomizerData)
        {
            starterData = specialPokemonData.StarterData;
            tradeData = specialPokemonData.TradeData;
            this.tmHmTutorData = tmHmTutorData;
            this.pokemonData = pokemonData;
            this.wildEncounterData = wildEncounterData;
            this.trainerData = trainerData.ToDictionary((tData) => tData.Category);
            this.itemData = itemData;
            this.weatherData = weatherData;
            this.miscData = miscData;
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
        // TODO:
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
        public override Pokemon[] CustomStarters => starterData.CustomStarters;
        public override bool StrongStarterTypeTriangle => starterData.StrongStarterTypeTriangle;
        public override PokemonSettings StarterPokemonSettings => new PokemonSettings()
        {
            BanLegendaries = starterData.BanLegendaries,
        };
        // TODO:
        // public override PokemonMetric[] StarterMetricData { get; } = new PokemonMetric[0]; (No UI for metric data yet) 
        public override bool SafeStarterMovesets => starterData.SafeStarterMovesets;

        #endregion

        #region Trade Pokemon

        public override double TradePokemonGiveRandChance => RandomChance(tradeData.RandomizeTradeGive, tradeData.TradePokemonGiveRandChance);
        public override double TradePokemonRecievedRandChance => RandomChance(tradeData.RandomizeTradeRecieve, tradeData.TradePokemonRecievedRandChance);
        public override PokemonSettings TradeSpeciesSettingsGive => tradeData.TradeSpeciesSettingsGive;
        public override PokemonSettings TradeSpeciesSettingsReceive => tradeData.TradeSpeciesSettingsRecieve;
        public override double TradeHeldItemRandChance => RandomChance(tradeData.RandomizeHeldItems, tradeData.HeldItemRandChance);
        public override ItemRandomizer.Settings TradeHeldItemSettings => tradeData.TradeHeldItemSettings;

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

        #region Maps

        public override WeatherOption WeatherSetting => weatherData.WeatherSetting;
        protected override WeightedSet<Map.Weather> CustomWeatherWeights => weatherData.CustomWeatherWeights;
        public override bool OverrideAllowGymWeather => weatherData.RandomizeGymWeather;
        public override double GymWeatherRandChance => weatherData.GymWeatherRandChance;
        public override Dictionary<Map.Type, double> WeatherRandChance
        {
            get
            {
                var dict = new Dictionary<Map.Type, double>(3);
                if (weatherData.RandomizeRouteWeather)
                {
                    dict.Add(Map.Type.Route, weatherData.RouteWeatherRandChance);
                }
                if (weatherData.RandomizeTownWeather)
                {
                    dict.Add(Map.Type.Village, weatherData.TownWeatherRandChance);
                    dict.Add(Map.Type.City, weatherData.TownWeatherRandChance);
                }
                return dict;
            }
        }
        public override bool OnlyChangeClearWeather => weatherData.KeepExistingWeather;
        public override HailHackOption HailHackSetting
        {
            get
            {
                if (Metadata == null || WeatherSetting == WeatherOption.Unchanged)
                    return HailHackOption.None;
                if (Metadata.IsEmerald)
                    return HailHackOption.Snow;
                if (Metadata.IsFireRed)
                    return HailHackOption.Both;
                return HailHackOption.None;
            }
        }

        #endregion

        #region Items

        //public override bool DontRandomizeTms => false; // Not actually implemented seems like

        public override PcItemOption PcPotionOption => itemData.PcPotionOption;
        public override Item CustomPcItem => itemData.CustomPcItem;
        public override ItemRandomizer.Settings PcItemSettings => itemData.PcItemSettings;

        public override double FieldItemRandChance => RandomChance(itemData.RandomizeFieldItems, itemData.FieldItemRandChance);
        public override ItemRandomizer.Settings FieldItemSettings => itemData.FieldItemSettings;
        public override bool UseSeperateHiddenItemSettings => itemData.UseSeperateHiddenItemSettings;
        public override double HiddenItemRandChance => RandomChance(itemData.RandomizeHiddenItems, itemData.HiddenItemRandChance);
        public override ItemRandomizer.Settings HiddenItemSettings => itemData.HiddenItemSettings;

        #endregion

        #region Misc

        public override bool RunIndoors => miscData.RunIndoors;

        public override bool EvolveWithoutNationalDex => miscData.EvolveWithoutNationalDex;

        public override bool CountRelicanthAsFossil => miscData.CountRelicanthAsFossil;

        #endregion

    }
}
