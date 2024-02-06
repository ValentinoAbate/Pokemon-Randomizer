using System.Collections.Generic;

namespace PokemonRandomizer.UI.Models
{
    using Backend.DataStructures;
    using Utilities;
    using PokemonRandomizer.Backend.Randomization;
    using static Settings;

    public class WildEncounterDataModel : DataModel
    {
        public PokemonSettings PokemonSettings { get; set; } = new PokemonSettings()
        {
            BanLegendaries = true,
            Noise = 0.001f,
        };
        public Box<WildEncounterRandomizer.Strategy> Strategy { get; set; } = new Box<WildEncounterRandomizer.Strategy>(WildEncounterRandomizer.Strategy.Unchanged);
        public Box<bool> MatchAreaType { get; set; } = new Box<bool>(true);
        public Box<bool> MatchEncounterType { get; set; } = new Box<bool>(true);
        public Box<bool> MatchIndividualType { get; set; } = new Box<bool>(true);
        public Box<bool> MatchPower { get; set; } = new Box<bool>(true);
    }
}
