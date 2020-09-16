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
        public bool AddFairyType { get => false; }
        #endregion

        #region Type Relation Definitions
        /// <summary>
        /// Should the randomizer modify the type traits of the ??? type?
        /// </summary>
        public bool ModifyUnknownType { get => true; } //(bool)window.cbModifyUnknownType.IsChecked; }
        /// <summary>
        /// Should the randomizer override UNKNOWN (the pokemon)'s type to the ??? type?
        /// </summary>
        public bool OverrideUnknownType { get => true; }
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

        public TrainerOption RivalSetting { get => TrainerOption.Procedural; }
        public bool RandomizeWallyAce { get => true; }
        public TrainerOption WallySetting { get => TrainerOption.Procedural; }
        public TrainerOption GymLeaderSetting { get => TrainerOption.Procedural; }
        public double BattleTypeRandChance { get => 1; }
        public double DoubleBattleChance { get => 1; }
        public bool MakeSoloPokemonBattlesSingle { get => true; }
        #endregion

        #region Wild Pokemon
        public enum WildPokemonOption
        {
            CompletelyRandom,
            AreaOneToOne,
            GlobalOneToOne,
        }
        public WildPokemonOption WildPokemonSetting { get => WildPokemonOption.AreaOneToOne; }
        #endregion

        #region Starter Pokemon
        public enum StarterPokemonOption
        {
            CompletelyRandom,
            TypeTriangle,
        }
        public bool RandomizeStarters { get => true; }
        public StarterPokemonOption StarterSetting { get => StarterPokemonOption.CompletelyRandom; }
        public bool StrongStarterTypeTriangle { get => false; }
        /// <summary>
        /// Ensures that all starters have an attacking move at lvl1
        /// Currently just makes all starters additionally have tackle
        /// </summary>
        public bool SafeStarterMovesets { get => true; }
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
        public TmMtCompatOption TmMtCompatSetting { get => TmMtCompatOption.Intelligent; }
        public double TmMtTrueChance { get => 0.42; }
        public double TmMtNoise { get => 0.10; }
        public bool PreventHmMovesInTMsOrMoveTutors { get => true; }
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

        public double DunsparsePlaugeChance { get => 0.30; }
        #endregion

        #region Misc
        public bool RandomizePcPotion { get => true; }

        public bool RunIndoors { get => true; }
        #endregion

        public Settings(MainWindow window)
        {
            this.window = window;
        }

        #region Species Randomization


        /// <summary> get the species randomization settings associated with a speific target group </summary>
        public SpeciesSettings GetSpeciesSettings(SpeciesSettings.Class target)
        {
            return speciesSettings[target];
        }
        public Dictionary<SpeciesSettings.Class, SpeciesSettings> speciesSettings = new Dictionary<SpeciesSettings.Class, SpeciesSettings>()
        {
            {SpeciesSettings.Class.Starter, new SpeciesSettings()
                {
                    BanLegendaries = true,
                    Noise = 1f,
                    PowerScaleSimilarityMod = 0.1f,
                    PowerScaleCull = true,
                    PowerThresholdStronger = 250,
                    PowerThresholdWeaker = 200,
                    TypeSimilarityMod = 0,
                    TypeSimilarityCull = false,
                }
            },
            {SpeciesSettings.Class.Wild, new SpeciesSettings()
                {
                    BanLegendaries = true,
                    Noise = 0.005f,
                    PowerScaleSimilarityMod = 0.15f,
                    PowerScaleCull = true,
                    PowerThresholdStronger = 250,
                    PowerThresholdWeaker = 200,
                    TypeSimilarityMod = 1f,
                    TypeSimilarityCull = false,
                }
            },
            {SpeciesSettings.Class.Trainer, new SpeciesSettings()
                {
                    BanLegendaries = false,
                    ForceHighestLegalEvolution = true,
                    Noise = 0.005f,
                    PowerScaleSimilarityMod = 0.15f,
                    PowerScaleCull = true,
                    TypeSimilarityMod = 1f,
                    TypeSimilarityCull = false,
                }
            },
            {SpeciesSettings.Class.AceTrainer, new SpeciesSettings()
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
            },
            {SpeciesSettings.Class.Rival, new SpeciesSettings()
                {
                    BanLegendaries = false,
                    ForceHighestLegalEvolution = true,
                    Noise = 0.001f,
                    PowerScaleSimilarityMod = 1f,
                    PowerScaleCull = true,
                    PowerThresholdStronger = 175,
                    PowerThresholdWeaker = 100,
                    TypeSimilarityMod = 0,
                    TypeSimilarityCull = false,
                }
            },
            {SpeciesSettings.Class.GymLeader, new SpeciesSettings()
                {
                    BanLegendaries = false,
                    IllegalEvolutionLeeway = 2,
                    ItemEvolutionLevel = 20,
                    FriendshipEvolutionLevel = 20,
                    TradeEvolutionLevel = 30,
                    ForceHighestLegalEvolution = true,
                    Noise = 0.001f,
                    PowerScaleSimilarityMod = 0.15f,
                    PowerScaleCull = true,
                    PowerThresholdStronger = 200,
                    PowerThresholdWeaker = 200,
                    TypeSimilarityMod = 1f,
                    TypeSimilarityCull = false,
                }
            },
            {SpeciesSettings.Class.EliteFour, new SpeciesSettings()
                {
                    BanLegendaries = false,
                    RestrictIllegalEvolutions = false,
                    ForceHighestLegalEvolution = true,
                    Noise = 0.001f,
                    PowerScaleSimilarityMod = 0.15f,
                    PowerScaleCull = true,
                    PowerThresholdStronger = 250,
                    PowerThresholdWeaker = 200,
                    TypeSimilarityMod = 1f,
                    TypeSimilarityCull = false,
                }
            },
            {SpeciesSettings.Class.Champion, new SpeciesSettings()
                {
                    BanLegendaries = false,
                    RestrictIllegalEvolutions = false,
                    ForceHighestLegalEvolution = true,
                    Noise = 0.001f,
                    PowerScaleSimilarityMod = 0.15f,
                    PowerScaleCull = true,
                    PowerThresholdStronger = 300,
                    PowerThresholdWeaker = 100,
                    TypeSimilarityMod = 1f,
                    TypeSimilarityCull = false,
                }
            },
        };
        public class SpeciesSettings
        {
            public enum Class
            { 
                Starter,
                Wild,
                Trainer,
                AceTrainer,
                Rival,
                GymLeader,
                EliteFour,
                Champion,
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
            public int BabyFriendshipEvolutionLevel { get => 3; }
            #endregion

            public bool BanLegendaries { get; set; } = false;
            public float Noise { get; set; } = 0;
            public float PowerScaleSimilarityMod { get; set; } = 0;
            public bool PowerScaleCull { get; set; } = false;
            public int PowerThresholdStronger { get; set; } = 100;
            public int PowerThresholdWeaker { get; set; } = 100;
            public float TypeSimilarityMod { get; set; } = 0;
            public bool TypeSimilarityCull { get; set; } = false;
        }

        #endregion
    }
}
