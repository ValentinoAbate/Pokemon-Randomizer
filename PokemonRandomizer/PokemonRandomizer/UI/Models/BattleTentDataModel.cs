using PokemonRandomizer.Backend.Randomization;
using PokemonRandomizer.UI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PokemonRandomizer.Settings.SpecialMoveSettings;
using static PokemonRandomizer.UI.Models.BattleFrontierDataModel;

namespace PokemonRandomizer.UI.Models
{
    public class BattleTentDataModel : DataModel
    {
        // Non-rental pokemon (fallarbor + verdanturf)

        public Box<bool> RandomizePokemon { get; set; } = new Box<bool>();
        public Box<double> PokemonRandChance { get; set; } = new Box<double>(1);
        public Box<FrontierPokemonRandStrategy> PokemonRandStrategy { get; set; } = new Box<FrontierPokemonRandStrategy>(FrontierPokemonRandStrategy.PowerScaled);
        public SpecialMoveSettingsUI.SpecialMoveSettingsWrapper SpecialMoveSettings { get; set; } = new(UsageOption.Dynamic, Sources.All);
        public Box<bool> BanLegendaries { get; set; } = new Box<bool>(true);

        // Rental Pokemon (Slateport Tent) settings

        public Box<bool> RandomizeRentalPokemon { get; set; } = new Box<bool>();
        public Box<double> RentalPokemonRandChance { get; set; } = new Box<double>(1);
        public Box<FrontierPokemonRandStrategy> RentalPokemonRandStrategy { get; set; } = new Box<FrontierPokemonRandStrategy>(FrontierPokemonRandStrategy.Level30);
        public SpecialMoveSettingsUI.SpecialMoveSettingsWrapper RentalSpecialMoveSettings { get; set; } = new(UsageOption.Dynamic, Sources.All);
        public Box<bool> BanRentalLegendaries { get; set; } = new Box<bool>(true);

        // Prizes

        public Box<bool> RandomizePrizes { get; set; } = new Box<bool>(false);
    }
}
