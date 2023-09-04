using PokemonRandomizer.Backend.Randomization;
using PokemonRandomizer.UI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PokemonRandomizer.Backend.Randomization.GameCornerRandomizer;

namespace PokemonRandomizer.UI.Models
{
    public class GameCornerDataModel : DataModel
    {
        // Roulette
        public Box<RouletteRandomizationOption> RouletteOption { get; set; } = new Box<RouletteRandomizationOption>(RouletteRandomizationOption.None);
        public Box<double> RouletteBase { get; set; } = new Box<double>(2);
    }
}
