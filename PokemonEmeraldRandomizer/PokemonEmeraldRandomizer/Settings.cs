using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer
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
        public Backend.PowerScaling.Options TieringOptions { get => Backend.PowerScaling.Options.BaseStatsAggregate; }
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
        public bool SpecialAceTrainers { get => true; }
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
        public StarterPokemonOption StarterSetting { get => StarterPokemonOption.TypeTriangle; }
        public bool StrongStarterTypeTriangle { get => true; }
        #endregion

        #region Evolution
        public double DunsparsePlaugeChance { get => 0; }
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
            {"trainer", new SpeciesSettings()
                {
                    Noise = 0.005f,
                    PowerScaleSimilarityMod = 0.15f,
                    PowerScaleCull = true,
                    PowerScaleThreshold = 100,
                    TypeSimilarityMod = 1f,
                    TypeSimilarityCull = false,
                }
            },
            {"wild", new SpeciesSettings()
                {
                    BanLegendaries = true,
                    Noise = 0.005f,
                    PowerScaleSimilarityMod = 0.15f,
                    PowerScaleCull = true,
                    PowerScaleThreshold = 250,
                    TypeSimilarityMod = 1f,
                    TypeSimilarityCull = false,
                }
            },
            {"starter", new SpeciesSettings()
                {
                    BanLegendaries = true,
                    Noise = 1f,
                    PowerScaleSimilarityMod = 0.1f,
                    PowerScaleCull = true,
                    PowerScaleThreshold = 250,
                    TypeSimilarityMod = 0,
                    TypeSimilarityCull = false,
                }
            },
            {"rival", new SpeciesSettings()
                {
                    Noise = 0.001f,
                    PowerScaleSimilarityMod = 1f,
                    PowerScaleCull = true,
                    PowerScaleThreshold = 175,
                    TypeSimilarityMod = 0,
                    TypeSimilarityCull = false,
                }
            },
            {"aceTrainer", new SpeciesSettings()
                {
                    Noise = 0.001f,
                    PowerScaleSimilarityMod = 1f,
                    PowerScaleCull = true,
                    PowerScaleThreshold = 175,
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

            public bool BanLegendaries { get; set; } = false;
            public float Noise { get; set; } = 0;
            public float PowerScaleSimilarityMod { get; set; } = 0;
            public bool PowerScaleCull { get; set; }
            public int PowerScaleThreshold { get; set; }
            public float TypeSimilarityMod { get; set; } = 0;
            public bool TypeSimilarityCull { get; set; }
        }
        #endregion
    }
}
