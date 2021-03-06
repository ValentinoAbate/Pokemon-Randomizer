﻿using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Randomization;
using PokemonRandomizer.Backend.Utilities;
using System.Collections.Generic;

namespace PokemonRandomizer
{
    public abstract class Settings
    {
        #region Seeding
        public abstract string Seed { get; }
        public abstract bool SetSeed { get; }
        #endregion

        #region Type Relation Definitions

        /// <summary>
        /// Should the randomizer modify the type traits of the ??? type?
        /// </summary>
        public abstract bool ModifyUnknownType { get; }
        /// <summary>
        /// Should the randomizer hack the ??? to make it usable with mooves (if possible)
        /// </summary>
        public abstract bool UseUnknownTypeForMoves { get; }
        /// <summary>
        /// Should the randomizer override UNKNOWN (the pokemon)'s type to the ??? type?
        /// </summary>
        public abstract bool OverrideUnknownType { get; }
        /// <summary>
        /// How often should the randomizer give UNKNOWN (the pokemon) a secondary type?
        /// 0.0 - 1.0
        /// </summary>
        public abstract double UnknownDualTypeChance { get; }

        #endregion

        #region TMs, HMs, and Move Tutors

        public enum TmMtCompatOption
        {
            Unchanged,
            AllOn,
            Random,
            RandomKeepNumber,
            Intelligent
        }
        public abstract TmMtCompatOption TmMtCompatSetting { get; }
        public abstract double TmMtTrueChance { get; }
        public abstract double TmMtNoise { get; }
        public abstract bool PreventHmMovesInTMsAndTutors {get; }
        public abstract bool PreventDuplicateTMsAndTutors {get; }
        public abstract bool KeepImportantTMsAndTutors { get; }
        public abstract HashSet<Move> ImportantTMsAndTutors { get; }
        public abstract double TMRandChance { get; }
        public abstract double MoveTutorRandChance { get; }

        #endregion

        #region Pokemon Base Stats

        #region Typing
        public abstract double SingleTypeRandChance { get; }
        public abstract double DualTypePrimaryRandChance { get; }
        public abstract double DualTypeSecondaryRandChance { get; }
        #endregion

        #region Evolution

        public abstract bool FixImpossibleEvos { get; }
        public abstract double ImpossibleEvoLevelStandardDev { get; }
        public enum TradeItemPokemonOption
        { 
            LevelUp,
            UseItem,
        }
        public abstract TradeItemPokemonOption TradeItemEvoSetting { get; }
        public abstract double DunsparsePlaugeChance { get; }

        #endregion

        #region Catch Rates

        public enum CatchRateOption
        {
            Unchanged,
            CompletelyRandom,
            Constant,
            IntelligentEasy,
            IntelligentNormal,
            IntelligentHard,
            AllEasiest,
        }
        public abstract CatchRateOption CatchRateSetting { get; }
        public abstract bool KeepLegendaryCatchRates { get; }
        public abstract byte CatchRateConstant { get; }
        public abstract byte IntelligentCatchRateBasicThreshold { get; }
        public abstract byte IntelligentCatchRateEvolvedThreshold { get; }

        #endregion

        #region Learnsets

        public abstract bool BanSelfdestruct { get; }
        public abstract bool AddMoves { get; }
        public abstract bool DisableAddingHmMoves { get; }

        public abstract double AddMovesChance { get; }
        public abstract double NumMovesStdDeviation { get; }
        public abstract double NumMovesMean { get; }

        public enum AddMoveSource
        { 
            Random,
            Damaging,
            Status,
            STAB,
            STABDamaging,
            STABStatus,
            EggMoves,
            CompatibleTms,
        }
        public abstract WeightedSet<AddMoveSource> AddMoveSourceWieghts { get; }

        #endregion

        #endregion

        #region Power Scaling
        public abstract PowerScaling.Options TieringOptions { get; }
        #endregion

        #region Trainers

        public enum TrainerOption
        {
            CompletelyRandom,
            KeepAce,
            Procedural,
        }

        public enum TrainerCategory
        {
            Trainer,
            AceTrainer,
            Rival,
            GymLeader,
            EliteFour,
            Champion,
        }

        public TrainerSettings GetTrainerSettings(TrainerCategory trainerClass)
        {
            return TrainerSettingsDict.ContainsKey(trainerClass) ? TrainerSettingsDict[trainerClass] : TrainerSettingsDict[TrainerCategory.Trainer];
        }
        protected abstract Dictionary<TrainerCategory, TrainerSettings> TrainerSettingsDict { get; }

        public abstract bool RandomizeWallyAce { get; }
        public abstract TrainerOption WallySetting { get; }
        #endregion

        #region Wild Pokemon
        public abstract WildEncounterRandomizer.Strategy EncounterStrategy { get; }
        public abstract PokemonSettings EncounterSettings { get; }

        #endregion

        #region Special Pokemon

        #region Gift Pokemon
        public abstract double GiftPokemonRandChance { get; }
        public abstract PokemonSettings GiftSpeciesSettings { get; }
        public abstract bool EnsureFossilRevivesAreFossilPokemon { get; }
        public abstract bool EnsureGiftEggsAreBabyPokemon { get; }
        #endregion

        #region Trade Pokemon
        public abstract double TradePokemonGiveRandChance { get; }
        public abstract double TradePokemonRecievedRandChance { get; }
        public abstract PokemonSettings TradeSpeciesSettingsGive { get; }
        public abstract MetricData[] TradeSpeciesMetricsGive { get; }
        public abstract PokemonSettings TradeSpeciesSettingsReceive { get; }
        public abstract MetricData[] TradeSpeciesMetricsRecieve { get; }
        public abstract double TradeHeldItemRandChance { get; }
        public abstract ItemRandomizer.Settings TradeHeldItemSettings { get; }

        #endregion

        #region Starter Pokemon
        public enum StarterPokemonOption
        {
            Unchanged,
            Random,
            RandomTypeTriangle,
            Custom,
        }
        public abstract StarterPokemonOption StarterSetting { get; }
        public abstract bool StrongStarterTypeTriangle { get; }
        public abstract Pokemon[] CustomStarters { get; }
        public abstract PokemonSettings StarterPokemonSettings { get; }
        public abstract PokemonMetric[] StarterMetricData { get; }
        /// <summary>
        /// Ensures that all starters have an attacking move at lvl1
        /// Currently just makes all starters additionally have tackle
        /// </summary>
        public abstract bool SafeStarterMovesets { get; }
        #endregion

        #endregion

        #region Maps

        public enum WeatherOption
        {
            Unchanged,
            CompletelyRandom,
            InBattleWeather,
            CustomWeighting,
            //Storms (Local Area Based) - Add later
        }

        public abstract WeatherOption WeatherSetting { get; }
        /// <summary>
        /// If true, ensures that underwater weather won't be put anywhere except for underwater maps
        /// </summary>
        public abstract bool SafeUnderwaterWeather { get; }
        /// <summary>
        /// If true, outside weather won't be put inside
        /// </summary>
        public abstract bool SafeInsideWeather { get; }
        /// <summary>
        /// Allows gym maps to have weather even if inside maps aren't weather randomized
        /// Allows outside weather to be put in gyms
        /// Uses GymWeatherRandChance instead of the normal chance
        /// </summary>
        public abstract bool OverrideAllowGymWeather { get; }
        /// <summary>
        /// The chance that a gym will have weather if OverrideAllowGymWeather is true
        /// </summary>
        public abstract double GymWeatherRandChance { get; }
        /// <summary>
        /// If this is true, only maps that started with clear weather will be random (the desert will still have sandstorm, etc)
        /// </summary>
        public abstract bool OnlyChangeClearWeather { get; }
        [System.Flags]
        public enum HailHackOption
        { 
            None = 0,
            Snow = 1,
            SteadySnow = 2,
            Both = Snow | SteadySnow,
        }

        /// <summary>
        /// Controls which gen 3 snow weathers will affect battle
        /// </summary>
        public abstract HailHackOption HailHackSetting { get; }
        private const double defaultWeatherRandChance = 0.33;
        /// <summary>
        /// The chance any given map type will have its weather randomized. If the map type is not in this map, that type of map will not be randomized
        /// </summary>
        public abstract Dictionary<Map.Type, double> WeatherRandChance { get; }
        protected abstract WeightedSet<Map.Weather> CustomWeatherWeights { get; }
        protected abstract WeightedSet<Map.Weather> BattleWeatherBalancedWeights { get; }
        /// <summary>
        /// Weighting for each weather type. Depenend on the current weather setting
        /// May split weather settings by map type
        /// </summary>
        public WeightedSet<Map.Weather> WeatherWeights 
        { 
            get
            {
                switch (WeatherSetting)
                {
                    case WeatherOption.InBattleWeather:
                        if(HailHackSetting != HailHackOption.None)
                        {
                            var modWeights = new WeightedSet<Map.Weather>(BattleWeatherBalancedWeights);
                            modWeights.RemoveWhere((w) => Map.WeatherAffectsBattle(w, HailHackSetting));
                            return modWeights;
                        }
                        return BattleWeatherBalancedWeights;
                    case WeatherOption.CustomWeighting:
                        return CustomWeatherWeights;
                    default:
                        return new WeightedSet<Map.Weather>(EnumUtils.GetValues<Map.Weather>());
                }
            }
        }

        #endregion

        #region Items

        public abstract bool DontRandomizeTms { get; }

        public enum PcItemOption
        {
            Unchanged,
            Random,
            Custom,
        }

        public abstract PcItemOption PcPotionOption { get; }
        public abstract Item CustomPcItem { get; }
        public abstract ItemRandomizer.Settings PcItemSettings { get; }

        public abstract double FieldItemRandChance { get; }
        public abstract ItemRandomizer.Settings FieldItemSettings { get; }
        public abstract bool UseSeperateHiddenItemSettings { get; }
        public abstract double HiddenItemRandChance { get; }
        public abstract ItemRandomizer.Settings HiddenItemSettings { get; }

        #endregion

        #region Misc

        public abstract bool RunIndoors { get; }

        public abstract bool EvolveWithoutNationalDex { get; }

        public abstract bool CountRelicanthAsFossil { get; }

        #endregion

        // This feature will generate 6 pokemon
        #region Dream Team

        public enum DreamTeamSetting
        {
            None,
            Custom,
            Random,
        }

        public abstract DreamTeamSetting DreamTeamOption { get; }

        public abstract Pokemon[] CustomDreamTeam { get; }

        public abstract DreamTeamSettings DreamTeamOptions { get; }

        public class DreamTeamSettings
        {
            public bool BanLegendaries => true;
            public bool BanIllegalEvolutions => true;
            public bool UseTotalBST => true;
            public float BstTotalUpperBound => 2200;
            public float BstTotalLowerBound => 40;
            public bool UseTypeFilter => TypeFilter.Length > 0;
            public PokemonType[] TypeFilter => new PokemonType[0];
        }

        #endregion

        #region Species Randomization

        public class TrainerSettings
        {
            public enum PokemonPcgStrategy
            {
                None,
                KeepAce,
                KeepParty,
            }
            public enum BattleTypePcgStrategy
            {
                None,
                KeepSameType,
            }

            public double PokemonRandChance { get; set; } = 1;
            public PokemonPcgStrategy PokemonStrategy { get; set; } = PokemonPcgStrategy.KeepParty;
            public PokemonSettings PokemonSettings { get; set; } = new PokemonSettings();
            public double BattleTypeRandChance { get; set; } = 1;
            public BattleTypePcgStrategy BattleTypeStrategy { get; set; } = BattleTypePcgStrategy.KeepSameType;
            public double DoubleBattleChance { get; set; } = 1;
        }

        public class PokemonSettings
        {
            public MetricData[] Data { get; set; } = new MetricData[0];
            public bool RestrictIllegalEvolutions { get; set; } = true;
            public bool ForceHighestLegalEvolution { get; set; } = false;
            public bool BanLegendaries { get; set; } = false;
            public float Noise { get; set; } = 0;
        }

        public class MetricData
        {
            public float Filter { get; }
            public float Sharpness { get; }
            public int Priority { get; }
            public string DataSource { get; }
            public List<string> Flags { get; set; } = new List<string>();

            public MetricData(string dataSource, int priority = 0, float sharpness = 1, float filter = 0)
            {
                DataSource = dataSource;
                Priority = priority;
                Filter = filter;
                Sharpness = sharpness;
            }
        }

        public abstract class PokemonMetric
        {
            public const string typeIndividual = nameof(typeIndividual);
            public const string typeEncounterSet = nameof(typeEncounterSet);
            public const string typeEncounterBankType = nameof(typeEncounterBankType);
            public const string typeTrainerParty = nameof(typeTrainerParty);
            public const string typeTrainerClass = nameof(typeTrainerClass);
            public const string powerIndividual = nameof(powerIndividual);
        }
        #endregion
    }
}
