using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer
{
    public class ApplicationData
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

        #region Trainer Pokemon
        public float TrainerPokemonNoise { get => 0.0f; }
        public float TrainerPowerScaleSimilarityMod { get => 0.25f; }
        public bool TrainerPowerScaleCull { get => true; }
        public float TrainerTypeSimilarityMod { get => 1f; }
        public bool TrainerTypeSimilarityCull { get => true; }
        #endregion

        public ApplicationData(MainWindow window)
        {
            this.window = window;
        }
    }
}
