using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Controls;

namespace PokemonRandomizer.UI
{
    public class TmHmTutorModel : DataModel
    {
        public static CompositeCollection CompatOptionDropdown { get; } = new CompositeCollection()
        {
            new ComboBoxItem() {Content="Unchanged" },
            new ComboBoxItem() {Content="All On" },
            new ComboBoxItem() {Content="Random" },
            new ComboBoxItem() {Content="Random (Keep Number)", ToolTip="Randomly select compatibility, but keep the same number the pokemon had in the base ROM"},
            new ComboBoxItem() {Content="Intelligent", ToolTip=""},
        };
        public enum CompatOption
        {
            Unchanged,
            AllOn,
            Random,
            RandomKeepNumber,
            Intelligent
        }
        public CompatOption TmCompatOption { get; set; } = CompatOption.Intelligent;
        public CompatOption TutorCompatOption { get; set; } = CompatOption.Intelligent;
        public CompatOption HmCompatOption { get; set; } = CompatOption.Unchanged;
        public double RandomCompatTrueChance { get; set; } = 0.42;
        public double IntelligentCompatNoise { get; set; } = 0.15;
        public bool NoHmMovesInTMsAndTutors { get; set; } = true;
        public bool NoDuplicateTMsAndTutors { get; set; } = true;
        public bool KeepImportantTmsAndTutors { get; set; } = true;

        public double TMRandChance { get; set; }
        public double MoveTutorRandChance { get; set; }
    }
}
