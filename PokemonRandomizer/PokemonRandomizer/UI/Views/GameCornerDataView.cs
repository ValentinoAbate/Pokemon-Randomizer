using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.Randomization;
using PokemonRandomizer.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using static PokemonRandomizer.Backend.Randomization.GameCornerRandomizer;

namespace PokemonRandomizer.UI.Views
{
    public class GameCornerDataView : DataView<GameCornerDataModel>
    {
        private static CompositeCollection RouletteOptionDropdown => new CompositeCollection()
        {
            new ComboBoxItem() {Content="None", ToolTip="Roulette wagers will be left as they are in the base game"},
            new ComboBoxItem() {Content="Fixed", ToolTip=$"Set the base wager to a fixed value between 2 and {maxBaseWager}. The right table's wager will be the base wager times 3, except when the game corner special event is active (in which case it will be the base wager times 6)" +
                $"\nWARNING: the coin payout animation for wagers higher higher than 16 can take a very long time to complete. It is recommended to set the base wager to 16 or lower"},
            new ComboBoxItem() {Content="Random", ToolTip=$"Set the base wager to a random value between 2 and {maxRandomBaseWager}. The right table's wager will be the base wager times 3, except when the game corner special event is active (in which case it will be the base wager times 6)" },
        };
        public GameCornerDataView(GameCornerDataModel model, RomMetadata metadata)
        {
            var stack = CreateMainStack();
            // Roulette
            if (metadata.IsEmerald)
            {
                stack.Header("Roulette");
                var rouletteOption = stack.Add(new EnumComboBoxUI<RouletteRandomizationOption>("Base Wager Randomization Strategy", RouletteOptionDropdown, model.RouletteOption));
                stack.Add(rouletteOption.BindVisibility(new BoundSliderUI("Fixed Base Wager", model.RouletteBase, false, 1, 2, maxBaseWager), (int)RouletteRandomizationOption.FixedBase));               
            }
        }
    }
}
