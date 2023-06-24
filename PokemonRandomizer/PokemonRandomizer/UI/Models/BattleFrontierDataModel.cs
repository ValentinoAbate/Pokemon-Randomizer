using PokemonRandomizer.UI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PokemonRandomizer.Settings.SpecialMoveSettings;

namespace PokemonRandomizer.UI.Models
{
    public class BattleFrontierDataModel : DataModel
    {
        public enum FrontierPokemonRandStrategy
        {
            PowerScaled,
            AllStrongest,
            FixedLevel,
        }
        // Normal pokemon fields
        public Box<bool> RandomizePokemon { get; set; } = new Box<bool>();
        public Box<double> PokemonRandChance { get; set; } = new Box<double>(1);
        public Box<FrontierPokemonRandStrategy> PokemonRandStrategy { get; set; } = new Box<FrontierPokemonRandStrategy>(FrontierPokemonRandStrategy.PowerScaled);
        public SpecialMoveSettingsUI.SpecialMoveSettingsWrapper SpecialMoveSettings { get; set; } = new (UsageOption.Dynamic, Sources.All);
        public Box<bool> BanLegendaries { get; set; } = new Box<bool>(true);

        // Frontier Brain Fields
        public Box<bool> RandomizeBrainPokemon { get; set; } = new Box<bool>();
        public Box<double> BrainPokemonRandChance { get; set; } = new Box<double>(1);
        public SpecialMoveSettingsUI.SpecialMoveSettingsWrapper BrainSpecialMoveSettings { get; set; } = new(UsageOption.Dynamic, Sources.All);
        public Box<bool> BanBrainLegendaries { get; set; } = new Box<bool>(false);
        public Box<bool> KeepBrainLegendaries { get; set; } = new Box<bool>(true);
    }
}
