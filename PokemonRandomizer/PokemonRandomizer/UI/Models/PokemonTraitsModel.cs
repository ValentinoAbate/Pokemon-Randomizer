using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace PokemonRandomizer.UI
{
    using CatchRateOption = Settings.CatchRateOption;
    public class PokemonTraitsModel : DataModel
    {
        // Type parameters
        public bool RandomizeSingleType { get; set; }
        public double SingleTypeRandChance { get; set; }
        public bool RandomizeDualTypePrimary { get; set; }
        public double DualTypePrimaryRandChance { get; set; }
        public bool RandomizeDualTypeSecondary { get; set; }
        public double DualTypeSecondaryRandChance { get; set; }

        // Evolution parameters
        public bool FixImpossibleEvos { get; set; } = true;
        public double ImpossibleEvoLevelStandardDev { get; set; } = 1;
        public bool ConsiderEvolveByBeautyImpossible { get; set; } = true;
        public static CompositeCollection CompatOptionDropdown { get; } = new CompositeCollection()
        {
            new ComboBoxItem() { Content="Level Up", ToolTip = "Pokemon that normally evolve by trading with an item will evolve by level-up. Slowpoke and Clamperl will evolve with wurmple logic" },
            new ComboBoxItem() { Content="Use Item", ToolTip = "Pokemon that normally evolve by trading with an item will evolve when that item is used on them"},
        };
        public Settings.TradeItemPokemonOption TradeItemEvoSetting { get; set; } = Settings.TradeItemPokemonOption.LevelUp;
        public bool DunsparsePlague { get; set; } = false;
        public double DunsparsePlaugeChance { get; set; } = 0;

        // Catch rate parameters
        private const string intelligentCatchRateTooltip = "Intelligently make some pokemon easier to catch because they can be found at the beginning of the game";
        public static CompositeCollection CatchRateOptionDropdown { get; } = new CompositeCollection()
        {
            new ComboBoxItem() { Content="Unchanged"},
            new ComboBoxItem() { Content="Random"},
            new ComboBoxItem() { Content="Constant", ToolTip = "Set all pokemon to the same catch difficulty"},
            new ComboBoxItem() { Content="Intelligent (Easy)", ToolTip = intelligentCatchRateTooltip + " (Easy)"},
            new ComboBoxItem() { Content="Intelligent (Normal)", ToolTip = intelligentCatchRateTooltip},
            new ComboBoxItem() { Content="Intelligent (Hard)", ToolTip = intelligentCatchRateTooltip + " (Hard)"},
            new ComboBoxItem() { Content="All Easiest", ToolTip = "All pokemon are as easy to catch as possible"},
        };
        public CatchRateOption CatchRateSetting { get; set; } = CatchRateOption.Unchanged;
        public bool KeepLegendaryCatchRates { get; set; } = true;
        public double CatchRateConstantDifficulty { get; set; } = 0.5;

        // Learnset parameters

        public bool BanSelfdestruct { get; set; }
        public bool AddMoves { get; set; }
        public double AddMovesChance { get; set; } = 1;
        public bool DisableAddingHmMoves { get; set; } = true;
        public double NumMovesStdDeviation { get; set; } = 1;
        public double NumMovesMean { get; set; } = 1;
        public double NumMovesMin { get; set; } = 0;
    }
}
