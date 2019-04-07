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
        public double SingleTypeMutationRate { get => window.mutSlSingleType.Value; }
        public double DualTypePrimaryMutationRate { get => 0.0; }
        public double DualTypeSecondaryMutationRate { get => 0.0; }
        #endregion

        #endregion

        #region Power Scaling
        public Backend.PowerScaling.Options TieringOptions { get => Backend.PowerScaling.Options.BaseStatsAggregate; }
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
            {"trainer", new SpeciesSettings()},
            {"wild", new SpeciesSettings() },
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

            public float Noise { get => 0.001f; }
            public float PowerScaleSimilarityMod { get => 0.15f; }
            public bool PowerScaleCull { get => true; }
            public float TypeSimilarityMod { get => 1f; }
            public bool TypeSimilarityCull { get => false; }
        }
        #endregion
    }
}
