using System.Collections.Generic;

namespace PokemonRandomizer.Backend.Utilities
{
    using EnumTypes;
    /// <summary>
    /// Utility class used to make special species classifications
    /// Should probably think of a better way to do this later!
    /// </summary>
    public static class SpeciesUtils
    {
        public static bool IsBaby(this PokemonSpecies species) => babyPokemon.Contains(species);
        private static readonly HashSet<PokemonSpecies> babyPokemon = new HashSet<PokemonSpecies>
        {
            PokemonSpecies.AZURILL,
            PokemonSpecies.CLEFFA,
            PokemonSpecies.ELEKID,
            PokemonSpecies.IGGLYBUFF,
            PokemonSpecies.MAGBY,
            PokemonSpecies.SMOOCHUM,
            PokemonSpecies.TOGEPI,
            PokemonSpecies.PICHU,
            PokemonSpecies.TYROGUE,
            PokemonSpecies.WYNAUT,
        };
        public static bool IsLegendary(this PokemonSpecies species) => legendaries.Contains(species);
        private static readonly HashSet<PokemonSpecies> legendaries = new HashSet<PokemonSpecies>
        {
            PokemonSpecies.ARTICUNO,
            PokemonSpecies.ZAPDOS,
            PokemonSpecies.MOLTRES,
            PokemonSpecies.MEW,
            PokemonSpecies.MEWTWO,
            PokemonSpecies.SUICUNE,
            PokemonSpecies.RAIKOU,
            PokemonSpecies.ENTEI,
            PokemonSpecies.LUGIA,
            PokemonSpecies.HOーOH,
            PokemonSpecies.CELEBI,
            PokemonSpecies.REGICE,
            PokemonSpecies.REGIROCK,
            PokemonSpecies.REGISTEEL,
            PokemonSpecies.KYOGRE,
            PokemonSpecies.GROUDON,
            PokemonSpecies.RAYQUAZA,
            PokemonSpecies.DEOXYS,
            PokemonSpecies.JIRACHI,
            PokemonSpecies.LATIAS,
            PokemonSpecies.LATIOS,
        };
        public static bool IsFossil(this PokemonSpecies species) => fossilPokemon.Contains(species);
        private static readonly HashSet<PokemonSpecies> fossilPokemon = new HashSet<PokemonSpecies>
        {
            PokemonSpecies.KABUTO,
            PokemonSpecies.KABUTOPS,
            PokemonSpecies.OMANYTE,
            PokemonSpecies.OMASTAR,
            PokemonSpecies.AERODACTYL,
            PokemonSpecies.LILEEP,
            PokemonSpecies.CRADILY,
            PokemonSpecies.ANORITH,
            PokemonSpecies.ARMALDO,
        };
    }
}
