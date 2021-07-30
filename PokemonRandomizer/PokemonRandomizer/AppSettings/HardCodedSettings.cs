﻿using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.AppSettings
{
    using Backend.DataStructures;
    using Backend.EnumTypes;
    using Backend.Randomization;
    using Backend.Utilities;
    public class HardCodedSettings : Settings
    {
        private readonly MainWindow window;

        public HardCodedSettings(MainWindow window)
        {
            this.window = window;
        }

        #region Seeding
        public override string Seed => window.tbSeed.Text;
        public override bool SetSeed => (bool)window.cbSeed.IsChecked;
        #endregion

        #region Type Hacks (WIP, NOTHING YET)

        #endregion

        #region Type Relation Definitions

        /// <summary>
        /// Should the randomizer modify the type traits of the ??? type?
        /// </summary>
        public override bool ModifyUnknownType => false; //(bool)window.cbModifyUnknownType.IsChecked; }
        /// <summary>
        /// Should the randomizer hack the ??? to make it usable with mooves (if possible)
        /// </summary>
        public override bool UseUnknownTypeForMoves => false;
        /// <summary>
        /// Should the randomizer override UNKNOWN (the pokemon)'s type to the ??? type?
        /// </summary>
        public override bool OverrideUnknownType => false;
        /// <summary>
        /// How often should the randomizer give UNKNOWN (the pokemon) a secondary type?
        /// 0.0 - 1.0
        /// </summary>
        public override double UnknownDualTypeChance => 0.0;

        #endregion

        #region TMs, HMs, and Move Tutors

        public override TmMtCompatOption TmMtCompatSetting => TmMtCompatOption.Intelligent;
        public override TmMtCompatOption HmCompatSetting => TmMtCompatOption.Unchanged;
        public override double TmMtTrueChance => 0.42;
        public override double TmMtNoise => 0.15;
        public override bool PreventHmMovesInTMsAndTutors => true;
        public override bool PreventDuplicateTMsAndTutors => true;
        public override bool KeepImportantTMsAndTutors => true;
        public override HashSet<Move> ImportantTMsAndTutors { get; } = new HashSet<Move>()
        {
            Move.HEADBUTT,
            Move.FLASH,
            Move.ROCK_SMASH,
            Move.SECRET_POWER,
        };
        public override double TMRandChance => 1;
        public override double MoveTutorRandChance => 1;

        #endregion

        #region Pokemon Base Stats

        #region Typing
        public override double SingleTypeRandChance => 0.0;
        public override double DualTypePrimaryRandChance => 0.0;
        public override double DualTypeSecondaryRandChance => 0.0;
        #endregion

        #region Evolution

        public override bool FixImpossibleEvos => true;
        public override bool ConsiderEvolveByBeautyImpossible => true;
        public override double ImpossibleEvoLevelStandardDev => 1;
        public override TradeItemPokemonOption TradeItemEvoSetting => TradeItemPokemonOption.LevelUp;
        public override double DunsparsePlaugeChance => 0.25;

        #endregion

        #region Catch Rates

        public override CatchRateOption CatchRateSetting => CatchRateOption.CompletelyRandom;
        public override bool KeepLegendaryCatchRates => true;
        public override byte CatchRateConstant => 100;
        public override byte IntelligentCatchRateBasicThreshold
        {
            get
            {
                if (intelligentCatchRateBasicThresholds.ContainsKey(CatchRateSetting))
                    return intelligentCatchRateBasicThresholds[CatchRateSetting];
                return 255;
            }
        }
        public override byte IntelligentCatchRateEvolvedThreshold
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

        public override bool BanSelfdestruct => false;
        public override bool AddMoves => true;
        public override bool DisableAddingHmMoves => false;

        public override double AddMovesChance => 1;
        public override double NumMovesStdDeviation => 2;
        public override double NumMovesMean => 1;

        public override WeightedSet<AddMoveSource> AddMoveSourceWieghts { get; } = new WeightedSet<AddMoveSource>()
        {
            { AddMoveSource.Random, 0.01f },
            { AddMoveSource.EggMoves, 0.99f },
        };

        #endregion

        #endregion

        #region Power Scaling
        public override PowerScaling.Options TieringOptions => PowerScaling.Options.BaseStatsAggregate;
        #endregion

        #region Trainers

        protected override Dictionary<TrainerCategory, TrainerSettings> TrainerSettingsDict { get; } = new Dictionary<TrainerCategory, TrainerSettings>()
        {
            { TrainerCategory.Trainer, new TrainerSettings()
                {
                    PokemonSettings = new PokemonSettings()
                    {
                        BanLegendaries = false,
                        ForceHighestLegalEvolution = true,
                        Noise = 0.01f,
                        Data = new List<MetricData>()
                        {
                            new MetricData(PokemonMetric.typeIndividual),
                            new MetricData(PokemonMetric.powerIndividual, 2),
                        }
                    }
                }
            },
            { TrainerCategory.AceTrainer, new TrainerSettings()
                {
                    PokemonSettings = new PokemonSettings()
                    {
                        BanLegendaries = false,
                        ForceHighestLegalEvolution = true,
                        Noise = 0.01f,
                    }
                }
            },
            { TrainerCategory.Rival, new TrainerSettings()
                {
                    PokemonSettings = new PokemonSettings()
                    {
                        BanLegendaries = false,
                        ForceHighestLegalEvolution = true,
                        Noise = 0.01f,
                    }
                }
            },
            { TrainerCategory.GymLeader, new TrainerSettings()
                {
                    PokemonSettings = new PokemonSettings()
                    {
                        BanLegendaries = false,
                        ForceHighestLegalEvolution = true,
                        Data = new List<MetricData>()
                        {
                            new MetricData(PokemonMetric.typeTrainerParty, 0, 10000, 0.2f),
                            new MetricData(PokemonMetric.powerIndividual, 3),
                            //new MetricData(PokemonMetric.typeIndividual, 1),
                        }
                    }
                }
            },
            { TrainerCategory.EliteFour, new TrainerSettings()
                {
                    PokemonSettings = new PokemonSettings()
                    {
                        BanLegendaries = false,
                        RestrictIllegalEvolutions = false,
                        ForceHighestLegalEvolution = true,
                        Data = new List<MetricData>()
                        {
                            new MetricData(PokemonMetric.typeTrainerParty, 0, 1000, 0.2f),
                            //new MetricData(PokemonMetric.typeIndividual, 1),
                        }
                    }
                }
            },
            { TrainerCategory.Champion, new TrainerSettings()
                {
                    PokemonSettings = new PokemonSettings()
                    {
                        BanLegendaries = false,
                        RestrictIllegalEvolutions = false,
                        ForceHighestLegalEvolution = true,
                        Noise = 0.001f,
                        Data = new List<MetricData>()
                        {
                            new MetricData(PokemonMetric.typeTrainerParty, 0, 1000, 0.2f),
                            //new MetricData(PokemonMetric.typeIndividual, 1),
                        }
                    }
                }
            },
        };
        public override bool RandomizeWallyAce => true;
        public override TrainerOption WallySetting => TrainerOption.Procedural;

        #endregion

        #region Wild Pokemon
        public override WildEncounterRandomizer.Strategy EncounterStrategy => WildEncounterRandomizer.Strategy.AreaOneToOne;
        public override PokemonSettings EncounterSettings { get; } = new PokemonSettings()
        {
            BanLegendaries = true,
            Noise = 0.001f,
            Data = new List<MetricData>()
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
            }
        };

        #endregion

        #region Special Pokemon

        #region Gift Pokemon
        public override double GiftPokemonRandChance => 1;
        public override PokemonSettings GiftSpeciesSettings { get; } = new PokemonSettings()
        {
            BanLegendaries = true,
        };
        public override bool EnsureFossilRevivesAreFossilPokemon => true;
        public override bool EnsureGiftEggsAreBabyPokemon => true;
        #endregion

        #region Trade Pokemon
        public override double TradePokemonGiveRandChance => 1;
        public override double TradePokemonRecievedRandChance => 1;
        public override PokemonSettings TradeSpeciesSettingsGive { get; } = new PokemonSettings()
        {
            BanLegendaries = true,
        };
        public override MetricData[] TradeSpeciesMetricsGive { get; } = new MetricData[]
        {
            new MetricData(PokemonMetric.powerIndividual)
        };
        public override PokemonSettings TradeSpeciesSettingsReceive { get; } = new PokemonSettings()
        {
            BanLegendaries = true,
        };
        public override MetricData[] TradeSpeciesMetricsRecieve { get; } = new MetricData[]
        {
            new MetricData(PokemonMetric.powerIndividual)
        };
        public override double TradeHeldItemRandChance => 1;
        public override ItemRandomizer.Settings TradeHeldItemSettings { get; } = new ItemRandomizer.Settings()
        {
            SamePocketChance = 0.75,
        };

        #endregion

        #region Starter Pokemon

        public override StarterPokemonOption StarterSetting { get => StarterPokemonOption.RandomTypeTriangle; }
        public override bool StrongStarterTypeTriangle { get => false; }
        public override Pokemon[] CustomStarters { get; } = new Pokemon[3]
        {
            Pokemon.KECLEON,
            Pokemon.KECLEON,
            Pokemon.KECLEON,
        };
        public override PokemonSettings StarterPokemonSettings { get; } = new PokemonSettings()
        {
            BanLegendaries = true,
        };
        public override PokemonMetric[] StarterMetricData { get; } = new PokemonMetric[0];
        /// <summary>
        /// Ensures that all starters have an attacking move at lvl1
        /// Currently just makes all starters additionally have tackle
        /// </summary>
        public override bool SafeStarterMovesets { get => true; }
        #endregion

        #endregion

        #region Maps

        public override WeatherOption WeatherSetting => WeatherOption.CustomWeighting;
        /// <summary>
        /// If true, ensures that underwater weather won't be put anywhere except for underwater maps
        /// </summary>
        public override bool SafeUnderwaterWeather => true;
        /// <summary>
        /// If true, outside weather won't be put inside
        /// </summary>
        public override bool SafeInsideWeather => true;
        /// <summary>
        /// Allows gym maps to have weather even if inside maps aren't weather randomized
        /// Allows outside weather to be put in gyms
        /// Uses GymWeatherRandChance instead of the normal chance
        /// </summary>
        public override bool OverrideAllowGymWeather => true;
        /// <summary>
        /// The chance that a gym will have weather if OverrideAllowGymWeather is true
        /// </summary>
        public override double GymWeatherRandChance => 0.5;
        /// <summary>
        /// If this is true, only maps that started with clear weather will be random (the desert will still have sandstorm, etc)
        /// </summary>
        public override bool OnlyChangeClearWeather => true;
        /// <summary>
        /// Controls which gen 3 snow weathers will affect battle
        /// </summary>
        public override HailHackOption HailHackSetting => HailHackOption.Snow;
        private const double defaultWeatherRandChance = 0.33;
        /// <summary>
        /// The chance any given map type will have its weather randomized. If the map type is not in this map, that type of map will not be randomized
        /// </summary>
        public override Dictionary<Map.Type, double> WeatherRandChance { get; } = new Dictionary<Map.Type, double>
        {
            { Map.Type.Route, defaultWeatherRandChance }
        };
        protected override WeightedSet<Map.Weather> CustomWeatherWeights { get; } = new WeightedSet<Map.Weather>
        {
            { Map.Weather.Rain, 0.85f },
            { Map.Weather.RainThunderstorm, 0.125f },
            { Map.Weather.RainHeavyThunderstrorm, 0.025f },
            { Map.Weather.Snow, 1f },
            { Map.Weather.StrongSunlight, 1 },
            { Map.Weather.Sandstorm, 0.8f },
        };
        protected override WeightedSet<Map.Weather> BattleWeatherBalancedWeights { get; } = new WeightedSet<Map.Weather>
        {
            { Map.Weather.Rain, 0.85f },
            { Map.Weather.RainThunderstorm, 0.125f },
            { Map.Weather.RainHeavyThunderstrorm, 0.025f },
            { Map.Weather.Snow, 0.85f },
            { Map.Weather.SnowSteady, 0.1f },
            { Map.Weather.StrongSunlight, 0.9f },
            { Map.Weather.Sandstorm, 0.6f },
        };

        #endregion

        #region Items

        public override bool DontRandomizeTms => false;

        public override PcItemOption PcPotionOption => PcItemOption.Custom;
        public override Item CustomPcItem => Item.Metal_Coat;
        public override ItemRandomizer.Settings PcItemSettings { get; } = new ItemRandomizer.Settings()
        {
            SamePocketChance = 0.75,
        };

        public override double FieldItemRandChance => 1;
        public override ItemRandomizer.Settings FieldItemSettings { get; } = new ItemRandomizer.Settings()
        {
            SamePocketChance = 0.75,
        };
        public override bool UseSeperateHiddenItemSettings => true;
        public override double HiddenItemRandChance => 1;
        public override ItemRandomizer.Settings HiddenItemSettings { get; } = new ItemRandomizer.Settings()
        {
            SamePocketChance = 0.75,
        };

        #endregion

        #region Misc

        public override bool RunIndoors => true;

        public override bool EvolveWithoutNationalDex => true;

        public override bool CountRelicanthAsFossil => true;

        #endregion

        // This feature will generate 6 pokemon
        #region Dream Team

        public override DreamTeamSetting DreamTeamOption => DreamTeamSetting.Random;

        public override Pokemon[] CustomDreamTeam { get; } = new Pokemon[6];

        public override DreamTeamSettings DreamTeamOptions { get; } = new DreamTeamSettings();

        #endregion
    }
}
