using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer
{
    public class Settings
    {
        private MainWindow window;

        #region Seeding
        public string Seed { get => window.tbSeed.Text; }
        public bool SetSeed { get => (bool)window.cbSeed.IsChecked; }
        #endregion

        #region Type Hacks
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
        public CatchRateOption CatchRateSetting { get => CatchRateOption.IntelligentNormal; }
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
        /// WARNING: enabling this feature causes movesets to expand, which may cause longer write times
        /// </summary>
        public bool SafeStarterMovesets { get => false; }
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
        public TmMtCompatOption TmMtCompatSetting { get => TmMtCompatOption.AllOn; }
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

        public double DunsparsePlaugeChance { get => 0; }
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
        public SpeciesSettings GetSpeciesSettings(string target)
        {
            return speciesSettings[target];
        }
        public Dictionary<string, SpeciesSettings> speciesSettings = new Dictionary<string, SpeciesSettings>()
        {
            {"starter", new SpeciesSettings()
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
            {"wild", new SpeciesSettings()
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
            {"trainer", new SpeciesSettings()
                {
                    BanLegendaries = true,
                    Noise = 0.005f,
                    PowerScaleSimilarityMod = 0.15f,
                    PowerScaleCull = true,
                    TypeSimilarityMod = 1f,
                    TypeSimilarityCull = false,
                }
            },
            {"rival", new SpeciesSettings()
                {
                    BanLegendaries = true,
                    ForceHighestLegalEvolution = true,
                    Noise = 0.001f,
                    PowerScaleSimilarityMod = 1f,
                    PowerScaleCull = true,
                    PowerThresholdStronger = 175,
                    PowerThresholdWeaker = 50,
                    TypeSimilarityMod = 0,
                    TypeSimilarityCull = false,
                }
            },
            {"champion", new SpeciesSettings()
                {
                    ForceHighestLegalEvolution = true,
                    Noise = 0.001f,
                    PowerScaleSimilarityMod = 0.15f,
                    PowerScaleCull = true,
                    PowerThresholdStronger = 500,
                    PowerThresholdWeaker = 50,
                    TypeSimilarityMod = 1f,
                    TypeSimilarityCull = false,
                }
            },
            {"eliteFour", new SpeciesSettings()
                {
                    BanLegendaries = true,
                    ForceHighestLegalEvolution = true,
                    Noise = 0.001f,
                    PowerScaleSimilarityMod = 0.15f,
                    PowerScaleCull = true,
                    PowerThresholdStronger = 300,
                    PowerThresholdWeaker = 50,
                    TypeSimilarityMod = 1f,
                    TypeSimilarityCull = false,
                }
            },
            {"gymLeader", new SpeciesSettings()
                {
                    BanLegendaries = true,
                    ForceHighestLegalEvolution = true,
                    Noise = 0.001f,
                    PowerScaleSimilarityMod = 0.15f,
                    PowerScaleCull = true,
                    PowerThresholdStronger = 250,
                    PowerThresholdWeaker = 50,
                    TypeSimilarityMod = 1f,
                    TypeSimilarityCull = false,
                }
            },
            {"aceTrainer", new SpeciesSettings()
                {
                    BanLegendaries = true,
                    ForceHighestLegalEvolution = true,
                    Noise = 0.001f,
                    PowerScaleSimilarityMod = 1f,
                    PowerScaleCull = true,
                    PowerThresholdStronger = 175,
                    PowerThresholdWeaker = 200,
                    TypeSimilarityMod = 0,
                    TypeSimilarityCull = false,
                }
            },
        };
        public class SpeciesSettings
        {
            #region Evolution Settings
            public bool DisableIllegalEvolutions { get => true; }
            public bool SetLevelsOnArtificialEvos { get => true; }
            public int ItemEvolutionLevel { get => 27; }
            public int TradeEvolutionLevel { get => 32; }
            public int FriendshipEvolutionLevel { get => 25; }
            public int BeautyEvolutionLevel { get => 32; }
            public int BabyFriendshipEvolutionLevel { get => 3; }
            #endregion

            public bool ForceHighestLegalEvolution { get; set; } = false;
            public bool BanLegendaries { get; set; } = false;
            public float Noise { get; set; } = 0;
            public float PowerScaleSimilarityMod { get; set; } = 0;
            public bool PowerScaleCull { get; set; }
            public int PowerThresholdStronger { get; set; } = 100;
            public int PowerThresholdWeaker { get; set; } = 100;
            public float TypeSimilarityMod { get; set; } = 0;
            public bool TypeSimilarityCull { get; set; }
        }

        #endregion
    }
}
