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
        /// Should the randomizer override UNKNOWN (the pokemon)'s type to the ??? type?
        /// </summary>
        public bool OverrideUnknownType { get => false; }
        /// <summary>
        /// How often should the randomizer give UNKNOWN (the pokemon) a secondary type?
        /// 0.0 - 1.0
        /// </summary>
        public double UnknownDualTypeChance { get => 0.0; }

        #endregion

        #region Pokemon Base Stats

        #region Typing
        public double SingleTypeRandChance { get => window.mutSlSingleType.Value; }
        public double DualTypePrimaryRandChance { get => 0.0; }
        public double DualTypeSecondaryRandChance { get => 0.0; }
        #endregion

        #region Evolution

        public bool FixImpossibleEvos { get => true; }

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
        public Backend.Randomization.PowerScaling.Options TieringOptions { get => Backend.Randomization.PowerScaling.Options.BaseStatsAggregate; }
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
                    SpeciesSettings = new SpeciesSettings()
                    {
                        BanLegendaries = false,
                        ForceHighestLegalEvolution = true,
                        WeightType = SpeciesSettings.WeightingType.Group,
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
                    SpeciesSettings = new SpeciesSettings()
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
                    SpeciesSettings = new SpeciesSettings()
                    {
                        BanLegendaries = false,
                        ForceHighestLegalEvolution = true,
                        WeightType = SpeciesSettings.WeightingType.Individual,
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
                    SpeciesSettings = new SpeciesSettings()
                    {
                        BanLegendaries = false,
                        IllegalEvolutionLeeway = 2,
                        ItemEvolutionLevel = 20,
                        FriendshipEvolutionLevel = 20,
                        TradeEvolutionLevel = 30,
                        ForceHighestLegalEvolution = true,
                        WeightType = SpeciesSettings.WeightingType.Group,
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
                    SpeciesSettings = new SpeciesSettings()
                    {
                        BanLegendaries = false,
                        RestrictIllegalEvolutions = false,
                        ForceHighestLegalEvolution = true,
                        WeightType = SpeciesSettings.WeightingType.Group,
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
                    SpeciesSettings = new SpeciesSettings()
                    {
                        BanLegendaries = false,
                        RestrictIllegalEvolutions = false,
                        ForceHighestLegalEvolution = true,
                        WeightType = SpeciesSettings.WeightingType.Group,
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
        public WildPokemonOption WildPokemonSetting => WildPokemonOption.AreaOneToOne;
        public SpeciesSettings WildSpeciesSettings { get; } = new SpeciesSettings()
        {
            BanLegendaries = true,
            WeightType = SpeciesSettings.WeightingType.Group,
            //Sharpness = 1.1f,
            Noise = 0.001f,
            PowerScaleSimilarityMod = 0.01f,
            PowerScaleCull = true,
            PowerThresholdStronger = 300,
            PowerThresholdWeaker = 200,
            TypeSimilarityMod = 1f,
            TypeSimilarityCull = false,
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
        public PokemonSpecies[] CustomStarters { get; } = new PokemonSpecies[3]
        {
            PokemonSpecies.KECLEON,
            PokemonSpecies.KECLEON,
            PokemonSpecies.KECLEON,
        };
        public SpeciesSettings StarterSpeciesSettings { get; } = new SpeciesSettings()
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
        /// <summary>
        /// Ensures that all starters have an attacking move at lvl1
        /// Currently just makes all starters additionally have tackle
        /// </summary>
        public bool SafeStarterMovesets { get => true; }
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
        /// The chance that a gym will have weather if OverrideAllowGymWeather is tru
        /// </summary>
        public double GymWeatherRandChance => 0.5;
        /// <summary>
        /// If this is true, only maps that started with clear weather will be random (the desert will still have sandstorm, etc)
        /// </summary>
        public bool OnlyChangeClearWeather => true;
        /// <summary>
        /// If this is true, snow and steady snow will be considered battle weather and will actually make hail happen
        /// </summary>
        public bool UseHailHack => true;
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
            { Map.Weather.Snow, 0.9f },
            { Map.Weather.SnowSteady, 0.1f },
            { Map.Weather.StrongSunlight, 1 },
            { Map.Weather.Sandstorm, 0.8f },
        };
        private readonly WeightedSet<Map.Weather> battleWeatherBalancedWeights = new WeightedSet<Map.Weather>
        {
            { Map.Weather.Rain, 0.85f },
            { Map.Weather.RainThunderstorm, 0.125f },
            { Map.Weather.RainHeavyThunderstrorm, 0.025f },
            { Map.Weather.Snow, 0.75f },
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
                        if(!UseHailHack)
                        {
                            var modWeights = new WeightedSet<Map.Weather>(battleWeatherBalancedWeights);
                            modWeights.RemoveWhere((w) => Map.WeatherAffectsBattle(w, UseHailHack));
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

        #region TMs, HMs, and Move Tutors

        public enum TmMtCompatOption
        {
            Unchanged,
            AllOn,
            Random,
            RandomKeepNumber,
            Intelligent
        }
        public TmMtCompatOption TmMtCompatSetting { get => TmMtCompatOption.RandomKeepNumber; }
        public double TmMtTrueChance { get => 0.42; }
        public double TmMtNoise { get => 0.15; }
        public bool PreventHmMovesInTMsAndMoveTutors { get => true; }
        public bool PreventDuplicateTMsAndMoveTutors { get => true; }
        public double TMRandChance { get => 1; }
        public double MoveTutorRandChance { get => 1; }

        #endregion

        #region Evolution

        public enum EvolutionOption
        {
            Unchanged,
            DisableIllegal,
            ForceHighestLegal,
        }
        public double DunsparsePlaugeChance { get => 0.25; }

        #endregion

        #region Misc

        public enum PcItemOption
        { 
            Unchanged,
            Random,
            Custom,
        }

        public PcItemOption PcPotionOption => PcItemOption.Random;

        public Item CustomPcItem => Item.Mach_Bike;

        public ItemSettings PcItemSettings => new ItemSettings()
        { 
            SamePocketChance = 0.75,
        };

        public bool RunIndoors => true;

        #endregion

        public Settings(MainWindow window)
        {
            this.window = window;
        }

        #region Species Randomization
       
        public class SpeciesSettings
        {
            public enum WeightingType
            {
                Individual,
                Group,
            }

            #region Evolution Settings
            public bool RestrictIllegalEvolutions { get; set; } = true;
            public int IllegalEvolutionLeeway { get; set; } = 0;
            public bool ForceHighestLegalEvolution { get; set; } = false;
            public bool SetLevelsOnArtificialEvos { get => true; }
            public int ItemEvolutionLevel { get; set; } = 27;
            public int TradeEvolutionLevel { get; set; } = 32;
            public int FriendshipEvolutionLevel { get; set; } = 25;
            public int BeautyEvolutionLevel { get => 32; }
            public int BabyFriendshipEvolutionLevel { get; set; } = 10;
            #endregion

            public bool BanLegendaries { get; set; } = false;
            public WeightingType WeightType { get; set; } = WeightingType.Individual;
            public float Sharpness { get; set; } = 0;
            public float Noise { get; set; } = 0;
            public float PowerScaleSimilarityMod { get; set; } = 0;
            public bool PowerScaleCull { get; set; } = false;
            public int PowerThresholdStronger { get; set; } = 100;
            public int PowerThresholdWeaker { get; set; } = 100;
            public float TypeSimilarityMod { get; set; } = 0;
            public bool TypeSimilarityCull { get; set; } = false;
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
            public SpeciesSettings SpeciesSettings { get; set; } = new SpeciesSettings();
            public double BattleTypeRandChance { get; set; } = 1;
            public BattleTypePcgStrategy BattleTypeStrategy { get; set; } = BattleTypePcgStrategy.KeepSameType;
            public double DoubleBattleChance { get; set; } = 1;
            /// <summary>
            /// WARNING: Setting this to false will cause these battles to be anomalous
            /// </summary>
            public bool MakeSoloPokemonBattlesSingle => true;
        }

        public class ItemSettings
        {
            public double SamePocketChance { get; set; } = 1;
        
        }

        #endregion
    }
}
