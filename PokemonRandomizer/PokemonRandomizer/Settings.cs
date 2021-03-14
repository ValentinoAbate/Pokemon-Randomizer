using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Randomization;
using PokemonRandomizer.Backend.Utilities;
using System.Collections.Generic;

namespace PokemonRandomizer
{
    public class Settings
    {
        private readonly MainWindow window;

        public Settings(MainWindow window)
        {
            this.window = window;
        }

        #region Seeding
        public string Seed { get => window.tbSeed.Text; }
        public bool SetSeed { get => (bool)window.cbSeed.IsChecked; }
        #endregion

        #region Type Hacks (WIP, NOTHING YET)

        #endregion

        #region Type Relation Definitions

        /// <summary>
        /// Should the randomizer modify the type traits of the ??? type?
        /// </summary>
        public bool ModifyUnknownType { get => false; } //(bool)window.cbModifyUnknownType.IsChecked; }
        /// <summary>
        /// Should the randomizer hack the ??? to make it usable with mooves (if possible)
        /// </summary>
        public bool UseUnknownTypeForMoves { get => false; }
        /// <summary>
        /// Should the randomizer override UNKNOWN (the pokemon)'s type to the ??? type?
        /// </summary>
        public bool OverrideUnknownType { get => false; }
        /// <summary>
        /// How often should the randomizer give UNKNOWN (the pokemon) a secondary type?
        /// 0.0 - 1.0
        /// </summary>
        public double UnknownDualTypeChance { get => 0.0; }

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
        public TmMtCompatOption TmMtCompatSetting => TmMtCompatOption.Intelligent;
        public double TmMtTrueChance => 0.42;
        public double TmMtNoise => 0.15;
        public bool PreventHmMovesInTMsAndTutors => true;
        public bool PreventDuplicateTMsAndTutors => true;
        public bool KeepImportantTMsAndTutors => true;
        public HashSet<Move> ImportantTMsAndTutors { get; } = new HashSet<Move>()
        {
            Move.HEADBUTT,
            Move.FLASH,
            Move.ROCK_SMASH,
            Move.SECRET_POWER,
        };
        public double TMRandChance => 1;
        public double MoveTutorRandChance => 1;

        #endregion

        #region Pokemon Base Stats

        #region Typing
        public double SingleTypeRandChance { get => 0.0; }
        public double DualTypePrimaryRandChance { get => 0.0; }
        public double DualTypeSecondaryRandChance { get => 0.0; }
        #endregion

        #region Evolution

        public bool FixImpossibleEvos => true;
        public double ImpossibleEvoLevelStandardDev => 1;
        public enum TradeItemPokemonOption
        { 
            LevelUp,
            UseItem,
        }
        public TradeItemPokemonOption TradeItemEvoSetting => TradeItemPokemonOption.LevelUp;
        public double DunsparsePlaugeChance { get => 0.25; }

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
        public CatchRateOption CatchRateSetting { get => CatchRateOption.CompletelyRandom; }
        public bool KeepLegendaryCatchRates { get => true; }
        public byte CatchRateConstant { get => 100; }
        public byte IntelligentCatchRateBasicThreshold
        {
            get
            {
                if (intelligentCatchRateBasicThresholds.ContainsKey(CatchRateSetting))
                    return intelligentCatchRateBasicThresholds[CatchRateSetting];
                return 255;
            }
        }
        public byte IntelligentCatchRateEvolvedThreshold
        {
            get
            {
                if (intelligentCatchRateEvolvedThresholds.ContainsKey(CatchRateSetting))
                    return intelligentCatchRateEvolvedThresholds[CatchRateSetting];
                return 100;
            }
        }

        private readonly Dictionary<CatchRateOption, byte> intelligentCatchRateBasicThresholds = new Dictionary<CatchRateOption, byte>()
        {
            { CatchRateOption.IntelligentEasy, 190},
            { CatchRateOption.IntelligentNormal, 150},
            { CatchRateOption.IntelligentHard, 100},
        };
        private readonly Dictionary<CatchRateOption, byte> intelligentCatchRateEvolvedThresholds = new Dictionary<CatchRateOption, byte>()
        {
            { CatchRateOption.IntelligentEasy, 100},
            { CatchRateOption.IntelligentNormal, 70},
            { CatchRateOption.IntelligentHard, 45},
        };

        #endregion

        #region Learnsets

        public bool BanSelfdestruct => false;
        public bool AddMoves => true;
        public bool DisableAddingHmMoves => false;

        public double AddMovesChance => 1;
        public double NumMovesStdDeviation => 2;
        public double NumMovesMean => 1;

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
        public WeightedSet<AddMoveSource> AddMoveSourceWieghts { get; } = new WeightedSet<AddMoveSource>()
        {
            { AddMoveSource.Random, 0.01f },
            { AddMoveSource.EggMoves, 0.99f },
        };

        #endregion

        #endregion

        #region Power Scaling
        public PowerScaling.Options TieringOptions => PowerScaling.Options.BaseStatsAggregate;
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
            return trainerSettings.ContainsKey(trainerClass) ? trainerSettings[trainerClass] : trainerSettings[TrainerCategory.Trainer];
        }

        private readonly Dictionary<TrainerCategory, TrainerSettings> trainerSettings = new Dictionary<TrainerCategory, TrainerSettings>()
        {
            { TrainerCategory.Trainer, new TrainerSettings()
                {
                    PokemonSettings = new PkmnRandomizer.Settings()
                    {
                        BanLegendaries = false,
                        ForceHighestLegalEvolution = true,
                        WeightType = PkmnRandomizer.Settings.WeightingType.Group,
                        Noise = 0.002f,
                        PowerScaleSimilarityMod = 0.01f,
                        PowerScaleCull = true,
                        TypeSimilarityMod = 1f,
                        TypeSimilarityCull = false,
                    }
                }
            },
            { TrainerCategory.AceTrainer, new TrainerSettings()
                {
                    PokemonSettings = new PkmnRandomizer.Settings()
                    {
                        BanLegendaries = false,
                        ForceHighestLegalEvolution = true,
                        Noise = 0.001f,
                        PowerScaleSimilarityMod = 1f,
                        PowerScaleCull = true,
                        PowerThresholdStronger = 175,
                        TypeSimilarityMod = 0,
                        TypeSimilarityCull = false,
                    }
                }
            },
            { TrainerCategory.Rival, new TrainerSettings()
                {
                    PokemonSettings = new PkmnRandomizer.Settings()
                    {
                        BanLegendaries = false,
                        ForceHighestLegalEvolution = true,
                        WeightType = PkmnRandomizer.Settings.WeightingType.Individual,
                        Noise = 0.001f,
                        PowerScaleSimilarityMod = 1f,
                        PowerScaleCull = true,
                        PowerThresholdStronger = 175,
                        PowerThresholdWeaker = 100,
                        TypeSimilarityMod = 0,
                        TypeSimilarityCull = false,
                    }
                }
            },
            { TrainerCategory.GymLeader, new TrainerSettings()
                {
                    PokemonSettings = new PkmnRandomizer.Settings()
                    {
                        BanLegendaries = false,
                        ForceHighestLegalEvolution = true,
                        WeightType = PkmnRandomizer.Settings.WeightingType.Group,
                        Sharpness = 2,
                        Noise = 0.0001f,
                        PowerScaleSimilarityMod = 0.01f,
                        PowerScaleCull = true,
                        PowerThresholdStronger = 200,
                        PowerThresholdWeaker = 300,
                        TypeSimilarityMod = 1f,
                        TypeSimilarityCull = false,
                    }
                }
            },
            { TrainerCategory.EliteFour, new TrainerSettings()
                {
                    PokemonSettings = new PkmnRandomizer.Settings()
                    {
                        BanLegendaries = false,
                        RestrictIllegalEvolutions = false,
                        ForceHighestLegalEvolution = true,
                        WeightType = PkmnRandomizer.Settings.WeightingType.Group,
                        Sharpness = 2,
                        Noise = 0.0001f,
                        PowerScaleSimilarityMod = 0.1f,
                        PowerScaleCull = true,
                        PowerThresholdStronger = 250,
                        PowerThresholdWeaker = 300,
                        TypeSimilarityMod = 1f,
                        TypeSimilarityCull = false,
                    }
                }
            },
            { TrainerCategory.Champion, new TrainerSettings()
                {
                    PokemonSettings = new PkmnRandomizer.Settings()
                    {
                        BanLegendaries = false,
                        RestrictIllegalEvolutions = false,
                        ForceHighestLegalEvolution = true,
                        WeightType = PkmnRandomizer.Settings.WeightingType.Group,
                        Sharpness = 2,
                        Noise = 0.0001f,
                        PowerScaleSimilarityMod = 0.1f,
                        PowerScaleCull = true,
                        PowerThresholdStronger = 300,
                        PowerThresholdWeaker = 300,
                        TypeSimilarityMod = 1f,
                        TypeSimilarityCull = false,
                    }
                }
            },
        };
        public bool RandomizeWallyAce { get => true; }
        public TrainerOption WallySetting { get => TrainerOption.Procedural; }
        #endregion

        #region Wild Pokemon
        public enum WildPokemonOption
        {
            Unchanged,
            Individual,
            AreaOneToOne,
            GlobalOneToOne,
        }
        public WildEncounterRandomizer.Strategy EncounterStrategy => WildEncounterRandomizer.Strategy.AreaOneToOne;
        public PkmnRandomizer.Settings EncounterSettings { get; } = new PkmnRandomizer.Settings()
        {
            BanLegendaries = true,
            Noise = 0.001f,
        };

        public MetricData[] EncounterMetrics { get; } = new MetricData[]
        {
            new MetricData(PokemonMetric.typeEncounterSet, 0),
            new MetricData(PokemonMetric.typeEncounterBankType, 0, 3f, 0.1f)
            { 
                Flags = new List<string> 
                { 
                    EncounterSet.Type.Surf.ToString(),
                    EncounterSet.Type.Fish.ToString(),
                    EncounterSet.Type.RockSmash.ToString(),
                    EncounterSet.Type.Headbutt.ToString(),
                }
            },
            new MetricData(PokemonMetric.typeIndividual, 1),
            new MetricData(PokemonMetric.powerIndividual, 1),
        };
        #endregion

        #region Special Pokemon

        #region Gift Pokemon
        public double GiftPokemonRandChance => 1;
        public PkmnRandomizer.Settings GiftSpeciesSettings { get; } = new PkmnRandomizer.Settings()
        {
            BanLegendaries = true,
            Noise = 1f,
            PowerScaleSimilarityMod = 0.1f,
            TypeSimilarityMod = 0f,
        };
        public bool EnsureFossilRevivesAreFossilPokemon => true;
        public bool EnsureGiftEggsAreBabyPokemon => true;
        #endregion

        #region Trade Pokemon
        public double TradePokemonGiveRandChance => 1;
        public double TradePokemonRecievedRandChance => 1;
        public PkmnRandomizer.Settings TradeSpeciesSettingsGive { get; } = new PkmnRandomizer.Settings()
        {
            BanLegendaries = true,
            Noise = 1f,
            PowerScaleSimilarityMod = 0.1f,
            TypeSimilarityMod = 0f,
        };
        public MetricData[] TradeSpeciesMetricsGive { get; } = new MetricData[]
        {
            new MetricData(PokemonMetric.powerIndividual)
        };
        public PkmnRandomizer.Settings TradeSpeciesSettingsReceive { get; } = new PkmnRandomizer.Settings()
        {
            BanLegendaries = true,
            Noise = 1f,
            PowerScaleSimilarityMod = 0.1f,
            TypeSimilarityMod = 0f,
        };
        public MetricData[] TradeSpeciesMetricsRecieve { get; } = new MetricData[]
        {
            new MetricData(PokemonMetric.powerIndividual)
        };
        public double TradeHeldItemRandChance => 1;
        public ItemRandomizer.Settings TradeHeldItemSettings { get; } = new ItemRandomizer.Settings()
        {
            SamePocketChance = 0.75,
        };

        #endregion

        #region Starter Pokemon
        public enum StarterPokemonOption
        {
            Unchanged,
            Random,
            RandomTypeTriangle,
            Custom,
        }
        public StarterPokemonOption StarterSetting { get => StarterPokemonOption.RandomTypeTriangle; }
        public bool StrongStarterTypeTriangle { get => false; }
        public Pokemon[] CustomStarters { get; } = new Pokemon[3]
        {
            Pokemon.KECLEON,
            Pokemon.KECLEON,
            Pokemon.KECLEON,
        };
        public PkmnRandomizer.Settings StarterPokemonSettings { get; } = new PkmnRandomizer.Settings()
        {
            BanLegendaries = true,
            Noise = 1f,
            PowerScaleSimilarityMod = 0.1f,
            PowerScaleCull = true,
            PowerThresholdStronger = 300,
            PowerThresholdWeaker = 200,
            TypeSimilarityMod = 0,
            TypeSimilarityCull = false,
        };
        public PokemonMetric[] StarterMetricData { get; } = new PokemonMetric[0];
        /// <summary>
        /// Ensures that all starters have an attacking move at lvl1
        /// Currently just makes all starters additionally have tackle
        /// </summary>
        public bool SafeStarterMovesets { get => true; }
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

        public WeatherOption WeatherSetting => WeatherOption.CustomWeighting;
        /// <summary>
        /// If true, ensures that underwater weather won't be put anywhere except for underwater maps
        /// </summary>
        public bool SafeUnderwaterWeather => true;
        /// <summary>
        /// If true, outside weather won't be put inside
        /// </summary>
        public bool SafeInsideWeather => true;
        /// <summary>
        /// Allows gym maps to have weather even if inside maps aren't weather randomized
        /// Allows outside weather to be put in gyms
        /// Uses GymWeatherRandChance instead of the normal chance
        /// </summary>
        public bool OverrideAllowGymWeather => true;
        /// <summary>
        /// The chance that a gym will have weather if OverrideAllowGymWeather is true
        /// </summary>
        public double GymWeatherRandChance => 0.5;
        /// <summary>
        /// If this is true, only maps that started with clear weather will be random (the desert will still have sandstorm, etc)
        /// </summary>
        public bool OnlyChangeClearWeather => true;
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
        public HailHackOption HailHackSetting => HailHackOption.Snow;
        private const double defaultWeatherRandChance = 0.33;
        /// <summary>
        /// The chance any given map type will have its weather randomized. If the map type is not in this map, that type of map will not be randomized
        /// </summary>
        public Dictionary<Map.Type, double> WeatherRandChance { get; } = new Dictionary<Map.Type, double>
        {
            { Map.Type.Route, defaultWeatherRandChance }
        };
        private readonly WeightedSet<Map.Weather> customWeights = new WeightedSet<Map.Weather>
        {
            { Map.Weather.Rain, 0.85f },
            { Map.Weather.RainThunderstorm, 0.125f },
            { Map.Weather.RainHeavyThunderstrorm, 0.025f },
            { Map.Weather.Snow, 1f },
            { Map.Weather.StrongSunlight, 1 },
            { Map.Weather.Sandstorm, 0.8f },
        };
        private readonly WeightedSet<Map.Weather> battleWeatherBalancedWeights = new WeightedSet<Map.Weather>
        {
            { Map.Weather.Rain, 0.85f },
            { Map.Weather.RainThunderstorm, 0.125f },
            { Map.Weather.RainHeavyThunderstrorm, 0.025f },
            { Map.Weather.Snow, 0.85f },
            { Map.Weather.SnowSteady, 0.1f },
            { Map.Weather.StrongSunlight, 0.9f },
            { Map.Weather.Sandstorm, 0.6f },
        };
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
                            var modWeights = new WeightedSet<Map.Weather>(battleWeatherBalancedWeights);
                            modWeights.RemoveWhere((w) => Map.WeatherAffectsBattle(w, HailHackSetting));
                            return modWeights;
                        }
                        return battleWeatherBalancedWeights;
                    case WeatherOption.CustomWeighting:
                        return customWeights;
                    default:
                        return new WeightedSet<Map.Weather>(EnumUtils.GetValues<Map.Weather>(), 1); ;
                }
            }
        }

        #endregion

        #region Items

        public bool DontRandomizeTms => false;

        public enum PcItemOption
        {
            Unchanged,
            Random,
            Custom,
        }

        public PcItemOption PcPotionOption => PcItemOption.Custom;
        public Item CustomPcItem => Item.Metal_Coat;
        public ItemRandomizer.Settings PcItemSettings { get; } = new ItemRandomizer.Settings()
        {
            SamePocketChance = 0.75,
        };  

        public double FieldItemRandChance => 1;
        public ItemRandomizer.Settings FieldItemSettings { get; } = new ItemRandomizer.Settings()
        {
            SamePocketChance = 0.75,
        };
        public bool UseSeperateHiddenItemSettings => true;
        public double HiddenItemRandChance => 1;
        public ItemRandomizer.Settings HiddenItemSettings { get; } = new ItemRandomizer.Settings()
        {
            SamePocketChance = 0.75,
        };

        #endregion

        #region Misc

        public bool RunIndoors => true;

        public bool EvolveWithoutNationalDex => true;

        public bool CountRelicanthAsFossil => true;

        #endregion

        #region Species Randomization

        public enum EvolutionOption
        {
            Unchanged,
            DisableIllegal,
            ForceHighestLegal,
        }

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
            public PkmnRandomizer.Settings PokemonSettings { get; set; } = new PkmnRandomizer.Settings();
            public MetricData[] PokemonMetrics { get; set; } = new MetricData[] { new MetricData(PokemonMetric.typeIndividual) };
            public double BattleTypeRandChance { get; set; } = 1;
            public BattleTypePcgStrategy BattleTypeStrategy { get; set; } = BattleTypePcgStrategy.KeepSameType;
            public double DoubleBattleChance { get; set; } = 1;
            /// <summary>
            /// WARNING: Setting this to false will cause these battles to be anomalous
            /// </summary>
            public bool MakeSoloPokemonBattlesSingle => true;
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
