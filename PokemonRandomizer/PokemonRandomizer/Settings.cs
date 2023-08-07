using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.DataStructures.Trainers;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Randomization;
using PokemonRandomizer.Backend.Utilities;
using PokemonRandomizer.UI;
using PokemonRandomizer.UI.Models;
using PokemonRandomizer.UI.Utilities;
using System.Collections.Generic;
using static PokemonRandomizer.Settings.SpecialMoveSettings;
using static PokemonRandomizer.UI.Models.BattleFrontierDataModel;

namespace PokemonRandomizer
{
    public abstract class Settings
    {
        public Settings(ApplicationDataModel data)
        {
            UpdateData(data);
        }

        public virtual void UpdateData(ApplicationDataModel data)
        {

        }

        #region Type Relation Definitions

        /// <summary>
        /// Should the randomizer modify the type traits of the ??? type?
        /// </summary>
        public abstract bool ModifyUnknownType { get; }
        /// <summary>
        /// Should the randomizer hack the ??? to make it usable with mooves (if possible)
        /// </summary>
        public abstract bool UseUnknownTypeForMoves { get; }

        #endregion

        #region TMs, HMs, and Move Tutors

        public enum MoveCompatOption
        {
            Unchanged,
            AllOn,
            Random,
            RandomKeepNumber,
            Intelligent
        }
        public abstract MoveCompatOption MoveCompatSetting { get; }
        public abstract double MoveCompatTrueChance { get; }
        public abstract double IntelligentCompatNormalTrueChance { get; }
        public abstract double IntelligentCompatTrueChance { get; }
        public abstract bool ForceFullHmCompatibility { get; }
        public abstract bool PreventHmMovesInTMsAndTutors {get; }
        public abstract bool PreventDuplicateTMsAndTutors {get; }
        public abstract bool KeepImportantTMsAndTutors { get; }
        public abstract HashSet<Move> ImportantTMsAndTutors { get; }
        public abstract double TMRandChance { get; }
        public abstract double MoveTutorRandChance { get; }

        #endregion

        #region Pokemon Base Stats

        #region Variants

        public abstract double VariantChance { get; }
        public abstract string VariantSeed { get; }
        public abstract PokemonVariantRandomizer.Settings VariantSettings { get; }

        #endregion

        #region Evolution

        public abstract bool FixImpossibleEvos { get; }
        public abstract bool ConsiderEvolveByBeautyImpossible { get; }
        public abstract double ImpossibleEvoLevelStandardDev { get; }
        public enum TradeItemPokemonOption
        { 
            LevelUp,
            UseItem,
        }
        public abstract TradeItemPokemonOption TradeItemEvoSetting { get; }
        public abstract double DunsparcePlaugeChance { get; }
        public abstract bool ApplyDunsparcePlagueToFriendshipEvos { get; }

        #endregion

        #region Catch / Hatch Rates

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
        public abstract bool FastHatching { get; }

        #endregion

        #region Exp / EV Yields

        public abstract double BaseExpYieldMultiplier { get; }
        public abstract bool ZeroBaseEVs { get; }

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
        public abstract WeightedSet<AddMoveSource> AddMoveSourceWeights { get; }

        #endregion

        #endregion

        #region Power Scaling
        public abstract PowerScaling.Options TieringOptions { get; }
        #endregion

        #region Trainers

        public TrainerSettings BasicTrainerSettings => new()
        {
            RandomizePokemon = RandomizeTrainerPokemon,
            PokemonStrategy = RecurringTrainerPokemonStrategy,
            MetricType = TrainerTypeDataSource,
            RestrictIllegalEvolutions = TrainerRestrictIllegalEvolutions,
            ForceHighestLegalEvolution = TrainerForceHighestLegalEvolution,
            PokemonNoise = (float)TrainerPokemonNoise,
            DuplicateMultiplier = (float)TrainerPokemonDuplicateReductionMultiplier,
            RandomizeBattleType = RandomizeTrainerBattleType,
            BattleTypeStrategy = RecurringTrainerBattleTypeStrategy,
            DoubleBattleChance = DoubleBattleChance,
            PriorityThemeCategory = PriorityThemeCategory,
            LevelMultiplier = TrainerPokemonLevelMultiplier,
            MinIV = (int)System.Math.Round(TrainerPokemonMinIV),
            MinIV255 = MathUtils.MapInt(PokemonBaseStats.maxIV, byte.MaxValue, (int)System.Math.Round(TrainerPokemonMinIV)),
            UseSmartAI = UseSmartAI,
            ForceCustomMoves = TrainerForceCustomMovesets,
        };

        public bool ApplyTheming(Trainer trainer) => ApplyTheming(trainer.TrainerCategory);

        public bool ApplyTheming(Trainer.Category category)
        {
            return category switch
            {
                Trainer.Category.TeamGrunt => GruntTheming ? ApplyTheming(TeamTypeTheming, TrainerTypeTheming) : TrainerTypeTheming,
                Trainer.Category.TeamAdmin or Trainer.Category.TeamLeader => ApplyTheming(TeamTypeTheming, TrainerTypeTheming),
                Trainer.Category.GymLeader or Trainer.Category.GymTrainer => ApplyTheming(GymTypeTheming, TrainerTypeTheming),
                Trainer.Category.EliteFour => ApplyTheming(EliteFourTheming, TrainerTypeTheming),
                Trainer.Category.Champion => ApplyTheming(ChampionTheming, ApplyTheming(EliteFourTheming, TrainerTypeTheming)),
                Trainer.Category.Rival or Trainer.Category.AceTrainer or Trainer.Category.CatchingTutTrainer => false,
                _ => TrainerTypeTheming,
            };
        }

        public bool ApplyTheming(TrainerOrgTypeTheme theme, bool defaultValue)
        {
            return theme switch
            {
                TrainerOrgTypeTheme.Default => defaultValue,
                TrainerOrgTypeTheme.Off => false,
                TrainerOrgTypeTheme.On or TrainerOrgTypeTheme.Random => true,
                _ => false,
            };
        }

        public bool BanLegendaries(Trainer.Category category)
        {
            if (IsBoss(category))
            {
                return BanLegendariesBoss;
            }
            if (IsMiniboss(category))
            {
                return BanLegendariesMiniboss;
            }
            return BanLegendariesTrainer;
        }

        public int NumBonusPokmon(Trainer.Category category)
        {
            if (IsBoss(category))
            {
                return ApplyBonusPokemonBoss ? BonusPokemon : 0;
            }
            if (IsMiniboss(category))
            {
                return ApplyBonusPokemonTrainer ? BonusPokemon : 0;
            }
            return ApplyBonusPokemonTrainer ? BonusPokemon : 0;
        }

        private bool IsBoss(Trainer.Category category)
        {
            return category is Trainer.Category.TeamLeader or Trainer.Category.GymLeader or Trainer.Category.EliteFour
                or Trainer.Category.Champion or Trainer.Category.SpecialBoss;
        }

        private bool IsMiniboss(Trainer.Category category)
        {
            return category is Trainer.Category.AceTrainer or Trainer.Category.Rival or Trainer.Category.TeamAdmin
                or Trainer.Category.CatchingTutTrainer;
        }

        // Trainer Pokemon Settings
        public abstract bool RandomizeTrainerPokemon { get; }
        public abstract bool TrainerTypeTheming { get; }
        protected abstract TrainerSettings.TrainerTypeDataSource TrainerTypeDataSource { get; }
        protected abstract bool BanLegendariesTrainer { get; }
        protected abstract bool BanLegendariesMiniboss { get; }
        protected abstract bool BanLegendariesBoss { get; }
        public abstract bool TrainerRestrictIllegalEvolutions { get; }
        public abstract bool TrainerForceHighestLegalEvolution { get; }
        public abstract double TrainerPokemonNoise { get; }
        public abstract double TrainerPokemonDuplicateReductionMultiplier { get; }
        public abstract TrainerSettings.PokemonPcgStrategy RecurringTrainerPokemonStrategy { get; }
        protected abstract bool TrainerForceCustomMovesets { get; }
        protected abstract int BonusPokemon { get; }
        protected abstract bool ApplyBonusPokemonTrainer { get; }
        protected abstract bool ApplyBonusPokemonMiniboss { get; }
        protected abstract bool ApplyBonusPokemonBoss { get; }

        // Battle Type Settings
        public abstract bool RandomizeTrainerBattleType { get; }
        public abstract TrainerSettings.BattleTypePcgStrategy RecurringTrainerBattleTypeStrategy { get; }
        public abstract double DoubleBattleChance { get; }

        // Difficulty Settings
        public abstract double TrainerPokemonLevelMultiplier { get; }
        public abstract double TrainerPokemonMinIV { get; }
        public abstract bool UseSmartAI { get; }

        // Trainer Organization Settings
        public enum TrainerOrgTypeTheme
        {
            Default,
            Off,
            On,
            Random,
        }

        public enum GymEliteFourPreventDupesSetting
        {
            None,
            RandomizedOnly,
            All,
        }

        public abstract TrainerOrgTypeTheme GymTypeTheming { get; }
        public abstract TrainerOrgTypeTheme EliteFourTheming { get; }
        public abstract TrainerOrgTypeTheme ChampionTheming { get; }
        public abstract GymEliteFourPreventDupesSetting GymEliteFourDupePrevention { get; }
        public abstract TrainerOrgTypeTheme TeamTypeTheming { get; }
        public abstract bool GruntTheming { get; }
        public abstract double TeamDualTypeChance { get; }
        public abstract Trainer.Category PriorityThemeCategory { get; }
        public abstract TrainerOrgTypeTheme SmallOrgTypeTheming { get; }

        // Misc
        public abstract bool RandomizeWallyAce { get; }

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

        #region Static Encounters

        public enum LegendaryRandSetting
        {
            Randomize,
            DontRandomize,
            RandomizeEnsureLegendary,
        }

        public abstract double StaticEncounterRandChance { get; }
        public abstract PokemonSettings StaticEncounterSettings { get; }
        public abstract LegendaryRandSetting StaticLegendaryRandomizationStrategy { get; }
        public abstract bool PreventDuplicateStaticEncounters { get; }
        public abstract bool RemapStaticEncounters { get; }

        #endregion

        #region Trade Pokemon
        public enum TradePokemonIVSetting
        {
            Unchanged,
            Randomize,
            Maximize
        }
        public abstract double TradePokemonGiveRandChance { get; }
        public abstract double TradePokemonRecievedRandChance { get; }
        public abstract PokemonSettings TradeSpeciesSettingsGive { get; }
        public abstract PokemonSettings TradeSpeciesSettingsReceive { get; }
        public abstract TradePokemonIVSetting TradePokemonIVOption { get; }
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

        public abstract bool WriteCatchingTutPokemon { get; }

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
        public abstract bool BanFlashingWeather { get; }
        [System.Flags]
        public enum HailHackOption
        { 
            None = 0,
            Snow = 1,
            FallingAsh = 2,
            Both = Snow | FallingAsh,
        }

        /// <summary>
        /// Controls which gen 3 snow weathers will affect battle
        /// </summary>
        public abstract HailHackOption HailHackSetting { get; }
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
        public virtual WeightedSet<Map.Weather> WeatherWeights 
        { 
            get
            {
                switch (WeatherSetting)
                {
                    case WeatherOption.InBattleWeather:
                        if(HailHackSetting != HailHackOption.None)
                        {
                            var modWeights = new WeightedSet<Map.Weather>(BattleWeatherBalancedWeights);
                            modWeights.RemoveWhere((w) => !Map.WeatherAffectsBattle(w, HailHackSetting));
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

        public abstract ItemRandomizer.RandomizerSettings ItemRandomizationSettings { get; }

        public enum PcItemOption
        {
            Unchanged,
            Random,
            Custom,
        }

        public abstract PcItemOption PcPotionOption { get; }
        public abstract Item CustomPcItem { get; }
        public abstract ItemRandomizer.Settings PcItemSettings { get; }
        public abstract bool AddCustomItemToPokemarts { get; }
        public abstract Item CustomMartItem { get; }
        public abstract bool OverrideCustomMartItemPrice { get; }
        public abstract int CustomMartItemPrice { get; }
        public abstract bool DiscountSoldGiftItems { get; }

        public abstract double FieldItemRandChance { get; }
        public abstract ItemRandomizer.Settings FieldItemSettings { get; }
        public abstract bool UseSeperateHiddenItemSettings { get; }
        public abstract double HiddenItemRandChance { get; }
        public abstract ItemRandomizer.Settings HiddenItemSettings { get; }

        public abstract double PickupItemRandChance { get; }
        public abstract ItemRandomizer.Settings PickupItemSettings { get; }

        public abstract double BerryTreeRandChance { get; }
        public abstract bool BanMinigameBerries { get; }
        public abstract bool BanEvBerries { get; }
        public abstract bool RemapBerries { get; }

        #endregion

        #region Battle Frontier and Minigames

        // Battle Frontier
        public abstract double BattleFrontierPokemonRandChance { get; }
        public abstract FrontierPokemonRandStrategy BattleFrontierPokemonRandStrategy { get; }
        public abstract SpecialMoveSettings BattleFrontierSpecialMoveSettings { get; }
        public abstract bool BattleFrontierBanLegendaries { get; }

        public abstract double FrontierBrainPokemonRandChance { get; }
        public abstract SpecialMoveSettings FrontierBrainSpecialMoveSettings { get; }
        public abstract bool FrontierBrainBanLegendaries { get; }
        public abstract bool FrontierBrainKeepLegendaries { get; }

        // Battle Tent Settings
        public abstract BattleTentRandomizer.Settings GetBattleTentSettings(BattleTent tent);

        #endregion

        #region Misc
        public abstract bool UpgradeUnown { get; }
        public abstract bool UpgradeCastform { get; }
        public abstract bool DistributeWeatherAbilities { get; }

        // Gen II-IV Hacks and Tweaks
        public abstract bool UpdateDOTMoves { get; }

        // Gen III Hacks and Tweaks
        public abstract bool RunIndoors { get; }
        public abstract bool StartWithNationalDex { get; }
        public enum MysteryGiftItemSetting
        {
            None,
            StartingItem,
            AllowInRandomization
        }
        public abstract bool EnableMysteyGiftEvents { get; }
        public abstract  MysteryGiftItemSetting MysteryGiftItemAcquisitionSetting { get; }

        // FRLG Hacks and Tweaks
        public abstract bool EvolveWithoutNationalDex { get; }

        // RSE Hacks and Tweaks
        public abstract bool EasyFirstRivalBattle { get; }

        // FRLG + E Hacks and Tweaks
        public abstract bool DeoxysMewObeyFix { get; }

        // Randomizer Settings
        public abstract bool CountRelicanthAsFossil { get; }

        public abstract TypeChartRandomizer.Option TypeChartRandomizationSetting { get; }

        #endregion

        // This feature will generate 6 pokemon
        #region Dream Team

        public enum DreamTeamSetting
        {
            None,
            Custom,
            Random,
        }

        public enum DreamTeamBstTotalOption
        {
            None,
            TotalMin,
            TotalMax,
            IndividualMin,
            IndividualMax
        }

        public abstract DreamTeamSetting DreamTeamOption { get; }

        public abstract Pokemon[] CustomDreamTeam { get; }

        public abstract DreamTeamSettings DreamTeamOptions { get; }

        public class DreamTeamSettings
        {
            public bool BanLegendaries { get; set; } = true;
            public bool BanIllegalEvolutions { get; set; } = true;
            public DreamTeamBstTotalOption BstSetting { get; set; } = DreamTeamBstTotalOption.None;
            public float BstLimit { get; set; } = 2200;
            public bool UseTypeFilter { get; set; } = false;
            public PokemonType[] TypeFilter { get; set; } = new PokemonType[0];
            public bool PrioritizeVariants { get; set; } = false;
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
            public enum TrainerTypeDataSource
            {
                Individual,
                Party
            }

            public bool RandomizePokemon { get; set; }
            public PokemonPcgStrategy PokemonStrategy { get; set; } = PokemonPcgStrategy.KeepParty;
            public bool RestrictIllegalEvolutions { get; set; } = true;
            public bool ForceHighestLegalEvolution { get; set; } = false;
            public float PokemonNoise { get; set; } = 0;
            public float DuplicateMultiplier { get; set; } = 1;
            public TrainerTypeDataSource MetricType { get; set; } = TrainerTypeDataSource.Individual;
            public Trainer.Category PriorityThemeCategory { get; set; } = Trainer.Category.GymLeader;
            public bool RandomizeBattleType { get; set; }
            public BattleTypePcgStrategy BattleTypeStrategy { get; set; } = BattleTypePcgStrategy.KeepSameType;
            public double DoubleBattleChance { get; set; } = 1;

            // Difficulty Settings
            public double LevelMultiplier { get; set; } = 0;
            public int MinIV { get; set; } = 0;
            public int MinIV255 { get; set; } = 0;
            public bool UseSmartAI { get; set; } = true;
            public bool ForceCustomMoves { get; set; } = true;
        }

        public class PokemonSettings
        {
            public List<MetricData> Data { get; set; } = new List<MetricData>();
            public bool RestrictIllegalEvolutions { get; set; } = true;
            public bool ForceHighestLegalEvolution { get; set; } = false;
            public bool BanLegendaries { get; set; } = false;
            public float Noise { get; set; } = 0;

            public PokemonSettings() { }

            public PokemonSettings(PokemonSettings other)
            {
                Data = new List<MetricData>(other.Data);
                RestrictIllegalEvolutions = other.RestrictIllegalEvolutions;
                ForceHighestLegalEvolution = other.ForceHighestLegalEvolution;
                BanLegendaries = other.BanLegendaries;
                Noise = other.Noise;
            }
        }

        public class SpecialMoveSettings
        {
            public enum UsageOption
            {
                None,
                Constant,
                Dynamic,
                Unlimited,
            }

            [System.Flags]
            public enum Sources
            {
                None = 0,
                TM = 1,
                HM = 2,
                Tutor = 4,
                Egg = 8,
                Special = 16,
                All = TM | HM | Tutor | Egg | Special,
            }

            public UsageOption Usage { get; set; } = UsageOption.None;
            public Sources AllowedSources { get; set; } = Sources.None;
        }

        public class MetricData
        {
            public static MetricData Empty => new MetricData(emptyMetric, 4);
            public const float defaultSharpness = 1;
            public const float defaultFilter = 0;
            public const string emptyMetric = "none";
            public float Filter { get; set; }
            public float Sharpness { get; set; }
            public int Priority { get; set;  }
            public string DataSource { get; set; }
            public List<string> Flags { get; set; } = new List<string>();

            public MetricData(string dataSource, int priority = 0, float sharpness = defaultSharpness, float filter = defaultFilter)
            {
                DataSource = dataSource;
                Priority = priority;
                Filter = filter;
                Sharpness = sharpness;
            }

            public void Reset()
            {
                Flags.Clear();
                Sharpness = defaultSharpness;
                Filter = defaultFilter;
            }
        }

        public abstract class PokemonMetric
        {
            public const string typeIndividual = "Type (Individual)";
            public const string typeEncounterSet = "Type (Encounter Set)";
            public const string typeEncounterBankType = "Type (Encounter Bank Type)";
            public const string typeTrainerParty = "Type (Trainer Party)";
            public const string typeTrainerClass = "Type (Trainer Class)";
            public const string powerIndividual = "Power (Individual)";
        }
        #endregion
    }
}
