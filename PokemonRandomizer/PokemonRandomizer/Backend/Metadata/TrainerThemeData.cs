using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Randomization;
using PokemonRandomizer.Backend.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.Backend.Metadata
{
    public class TrainerThemeData
    {
        public enum TrainerTheme 
        {
            Typed,
            Untyped,
            SpecificPokemon,
            Legendaries,
            // Celebrity Trainer (Someone who wouldn't normally be in the leader position)
            // Weather
        }

        public TrainerTheme Theme { get; set; }

        public PokemonType[] Types { get; set; } = Array.Empty<PokemonType>();

        public Pokemon[] SpecificPokemon { get; set; } = Array.Empty<Pokemon>();

        public List<Metric<Pokemon>> GetPokemonMetrics(IEnumerable<Pokemon> all, Pokemon pokemon, Trainer trainer, IDataTranslator dataT)
        {
            return new List<Metric<Pokemon>>() { new Metric<Pokemon>(Theme switch
            {
                TrainerTheme.Typed => new WeightedSet<Pokemon>(all.Where(p => Types.Any(t => dataT.GetBaseStats(p).IsType(t)))),
                TrainerTheme.SpecificPokemon => new WeightedSet<Pokemon>(all.Where(SpecificPokemon.Contains)),
                TrainerTheme.Legendaries => new WeightedSet<Pokemon>(all.Where(PokemonUtils.IsLegendary)),
                _ => new WeightedSet<Pokemon>(all),
            }, 0, 1)};
        }

        public override string ToString()
        {
            return Theme switch
            {
                TrainerTheme.Typed => string.Join('/', Types),
                TrainerTheme.SpecificPokemon => $"Specific Pokemon: {string.Join(", ", SpecificPokemon)}",
                _ => Theme.ToString(),
            };
        }
    }
}
