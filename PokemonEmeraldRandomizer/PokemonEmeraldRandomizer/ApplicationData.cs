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
        public bool? SetSeed { get => window.cbSeed.IsChecked; }
        #endregion

        #region Type Definitions
        public bool? ModifyUnknownType { get => window.cbModifyUnknownType.IsChecked; }
        #endregion

        #region Pokemon

        #region Typing
        public double SingleTypeMutationRate { get => window.mutSlSingleType.Value; }
        public double DualTypePrimaryMutationRate { get => 0.05; }
        public double DualTypeSecondaryMutationRate { get => 0.05; }
        #endregion

        #endregion

        public ApplicationData(MainWindow window)
        {
            this.window = window;
        }
    }
}
