using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer.Backend
{
    public class BalanceMetrics
    {
        public WeightedSet<PokemonType> typeRatiosAll = new WeightedSet<PokemonType>();
        public WeightedSet<PokemonType> typeRatiosPrimary = new WeightedSet<PokemonType>();
        public WeightedSet<PokemonType> typeRatiosSecondary = new WeightedSet<PokemonType>();
        // Calculates the balancing metrics based on the given data
        public BalanceMetrics(ROMData data)
        {
            foreach(PokemonBaseStats pkmn in data.Pokemon)
            {
                // if pkmn is single-typed
                if(pkmn.IsSingleTyped)
                {
                    typeRatiosAll.AddWeight(pkmn.types[0]);
                    typeRatiosPrimary.AddWeight(pkmn.types[0]);
                }
                else
                {
                    typeRatiosAll.AddWeight(pkmn.types[0]);
                    typeRatiosAll.AddWeight(pkmn.types[1]);
                    typeRatiosPrimary.AddWeight(pkmn.types[0]);
                    typeRatiosSecondary.AddWeight(pkmn.types[1]);
                }
            }
        }
    }
}
