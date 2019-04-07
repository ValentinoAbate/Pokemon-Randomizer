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

        #region Trainer Settings
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
                    Noise = 0.01f,
                    PowerScaleSimilarityMod = 0.15f,
                    PowerScaleCull = true,
                    PowerScaleThreshold = 100,
                    TypeSimilarityMod = 1f,
                    TypeSimilarityCull = false,
                }
            },
            {"wild", new SpeciesSettings()
                {
                    Noise = 0.01f,
                    PowerScaleSimilarityMod = 0.15f,
                    PowerScaleCull = true,
                    PowerScaleThreshold = 250,
                    TypeSimilarityMod = 1f,
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
            public int FriendshipEvolutionLevel { get => 3; }
            public int BeautyEvolutionLevel { get => 32; }
            #endregion

            public float Noise { get; set; }
            public float PowerScaleSimilarityMod { get; set; }
            public bool PowerScaleCull { get; set; }
            public int PowerScaleThreshold { get; set; }
            public float TypeSimilarityMod { get; set; }
            public bool TypeSimilarityCull { get; set; }
        }
        #endregion
    }
}
