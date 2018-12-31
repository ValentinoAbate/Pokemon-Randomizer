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
        public WeightedSet<TypeEffectiveness> TypeEffectivenessRatios { get; }

        // Calculates the balancing metrics based on the given data
        public BalanceMetrics(RomData data)
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
                    TypeRatiosAll.Add(pkmn.types[0]);
                    TypeRatiosSingle.Add(pkmn.types[0]);
                }
                else
                {
                    TypeRatiosAll.Add(pkmn.types[0]);
                    TypeRatiosAll.Add(pkmn.types[1]);
                    TypeRatiosDualPrimary.Add(pkmn.types[0]);
                    TypeRatiosDualSecondary.Add(pkmn.types[1]);
                }
            }
            DualTypePercentage = data.Pokemon.Length / TypeRatiosDualPrimary.Count;
            #endregion

            #region Type Effectiveness Metrics
            TypeEffectivenessRatios = new WeightedSet<TypeEffectiveness>();
            foreach(var atkType in EnumUtils.GetValues<PokemonType>())
                foreach (var defType in EnumUtils.GetValues<PokemonType>())
                    TypeEffectivenessRatios.Add(data.TypeDefinitions.GetEffectiveness(atkType, defType));
            #endregion
            #endregion
        }
    }
}
