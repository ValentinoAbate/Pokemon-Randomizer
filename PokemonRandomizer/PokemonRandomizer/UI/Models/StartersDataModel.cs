using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using PokemonRandomizer.Backend.EnumTypes;

namespace PokemonRandomizer.UI
{
    public class StartersDataModel : DataModel
    {
        public static CompositeCollection StarterOptionDropdown { get; } = new CompositeCollection()
        {
            new ComboBoxItem() {Content="Unchanged" },
            new ComboBoxItem() {Content="Random" },
            new ComboBoxItem() {Content="Random Type Triangle", ToolTip="Randomly select three starters where each is strong against the next"},
            new ComboBoxItem() {Content="Custom", ToolTip="Set 1 or more custom starters"},
        };
        public Settings.StarterPokemonOption StarterSetting { get; set; } = Settings.StarterPokemonOption.Unchanged;
        public bool StrongStarterTypeTriangle { get; set; } = false;
        public string[] CustomStarters { get; } = new string[]
        {
            "Random",
            "Random",
            "Random",
        };
        public bool BanLegendaries { get; set; } = true;

        /// <summary>
        /// Ensures that all starters have an attacking move at lvl1
        /// Currently just makes all starters additionally have tackle
        /// </summary>
        public bool SafeStarterMovesets { get; set; } = true;
    }
}
