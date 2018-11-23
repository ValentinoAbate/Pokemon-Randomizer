using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer.Backend
{
    public class BalanceMetrics
    {

        public WeightedSet<PokemonType> TypeRatiosAll { get; }
        public WeightedSet<PokemonType> TypeRatiosSingle { get; }
        public WeightedSet<PokemonType> TypeRatiosDualPrimary { get; }
        public WeightedSet<PokemonType> TypeRatiosDualSecondary { get; }
        public float DualTypePercentage { get; }

        // Calculates the balancing metrics based on the given data
        public BalanceMetrics(ROMData data)
        {
            #region Pokemon Base Stat Metrics

            #region Type Metrics
            TypeRatiosAll = new WeightedSet<PokemonType>();
            TypeRatiosSingle = new WeightedSet<PokemonType>();
            TypeRatiosDualPrimary = new WeightedSet<PokemonType>();
            TypeRatiosDualSecondary = new WeightedSet<PokemonType>();
            foreach(PokemonBaseStats pkmn in data.Pokemon)
            {
                // if pkmn is single-typed
                if(pkmn.IsSingleTyped)
                {
                    TypeRatiosAll.AddWeight(pkmn.types[0]);
                    TypeRatiosSingle.AddWeight(pkmn.types[0]);
                }
                else
                {
                    TypeRatiosAll.AddWeight(pkmn.types[0]);
                    TypeRatiosAll.AddWeight(pkmn.types[1]);
                    TypeRatiosDualPrimary.AddWeight(pkmn.types[0]);
                    TypeRatiosDualSecondary.AddWeight(pkmn.types[1]);
                }
            }
            DualTypePercentage = data.Pokemon.Length / TypeRatiosDualPrimary.Count;
            #endregion
            #endregion
        }
    }
}
