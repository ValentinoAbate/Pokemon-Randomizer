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
        public bool ModifyUnknownType { get => (bool)window.cbModifyUnknownType.IsChecked; }
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
        public double TMRandChance { get => 0.5; }
        public double MoveTutorRandChance { get => 1; }
        #endregion

        #region Evolution
        public enum EvolutionOption
        {
            Unchanged,
            DisableIllegal,
            ForceHighestLegal,
        }

        public double DunsparsePlaugeChance { get => 0.001; }
        #endregion

        #region Misc
        public bool RandomizePcPotion { get => true; }
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
