using System.Collections.Generic;

namespace PokemonRandomizer.Backend.Utilities
{
    using EnumTypes;
    /// <summary>
    /// Utility class used to make special species classifications.
    /// Should probably think of a better way to do this later!
    /// </summary>
    public static class PokemonUtils
    {
        public static bool IsBaby(this Pokemon species) => babyPokemon.Contains(species);
        private static readonly HashSet<Pokemon> babyPokemon = new HashSet<Pokemon>
        {
            Pokemon.AZURILL,
            Pokemon.CLEFFA,
            Pokemon.ELEKID,
            Pokemon.IGGLYBUFF,
            Pokemon.MAGBY,
            Pokemon.SMOOCHUM,
            Pokemon.TOGEPI,
            Pokemon.PICHU,
            Pokemon.TYROGUE,
            Pokemon.WYNAUT,
        };
        public static bool IsLegendary(this Pokemon species) => legendaries.Contains(species);
        private static readonly HashSet<Pokemon> legendaries = new HashSet<Pokemon>
        {
            Pokemon.ARTICUNO,
            Pokemon.ZAPDOS,
            Pokemon.MOLTRES,
            Pokemon.MEW,
            Pokemon.MEWTWO,
            Pokemon.SUICUNE,
            Pokemon.RAIKOU,
            Pokemon.ENTEI,
            Pokemon.LUGIA,
            Pokemon.HOーOH,
            Pokemon.CELEBI,
            Pokemon.REGICE,
            Pokemon.REGIROCK,
            Pokemon.REGISTEEL,
            Pokemon.KYOGRE,
            Pokemon.GROUDON,
            Pokemon.RAYQUAZA,
            Pokemon.DEOXYS,
            Pokemon.JIRACHI,
            Pokemon.LATIAS,
            Pokemon.LATIOS,
        };
        public static bool IsFossil(this Pokemon species) => fossilPokemon.Contains(species);
        private static readonly HashSet<Pokemon> fossilPokemon = new HashSet<Pokemon>
        {
            Pokemon.KABUTO,
            Pokemon.KABUTOPS,
            Pokemon.OMANYTE,
            Pokemon.OMASTAR,
            Pokemon.AERODACTYL,
            Pokemon.LILEEP,
            Pokemon.CRADILY,
            Pokemon.ANORITH,
            Pokemon.ARMALDO,
        };
    }
}
