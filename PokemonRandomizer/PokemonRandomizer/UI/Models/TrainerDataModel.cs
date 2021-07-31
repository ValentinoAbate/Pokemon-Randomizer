using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.UI.Models
{
    using static Settings;
    using static Settings.TrainerSettings;
    public class TrainerDataModel : DataModel
    {
        public bool RandomizePokemon { get; set; } = false;
        public double PokemonRandChance { get; set; } = 1;
        public PokemonPcgStrategy PokemonStrategy { get; set; } = PokemonPcgStrategy.KeepParty;
        public PokemonSettings PokemonSettings { get; set; } = new PokemonSettings();
        public bool RandomizeBattleType { get; set; } = false;
        public double BattleTypeRandChance { get; set; } = 1;
        public BattleTypePcgStrategy BattleTypeStrategy { get; set; } = BattleTypePcgStrategy.KeepSameType;
        public double DoubleBattleChance { get; set; } = 1;
    }
}
