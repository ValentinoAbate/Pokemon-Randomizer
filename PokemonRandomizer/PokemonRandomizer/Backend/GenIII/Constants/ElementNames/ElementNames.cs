using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.GenIII.Constants.ElementNames
{
    /// <summary>
    /// Contains symbolic constants for referencing the names of elements in a Gen III info file.
    /// </summary>
    public static class ElementNames
    {
        public const string itemData = "itemDefinitions";
        public const string pcPotion = "pcPotion";
        // Map elements
        public const string mapLabels = "mapLabels";
        public const string mapBankPointers = "mapBankPointers";
        public const string maps = "maps";
        // Pokemon Elements
        public const string evolutions = "evolutions";
        public const string tmHmCompat = "tmHmCompat";
        public const string pokemonBaseStats = "pokemonBaseStats";
        public const string tutorCompat = "moveTutorCompat";
        public const string movesets = "movesets";
        // Data elements
        public const string moveData = "moveData";
        public const string tutorMoves = "moveTutorMoves";
        // Hacks and Tweaks / Misc
        public const string runIndoors = "runIndoors";
        public const string textSpeed = "textSpeed";
        public const string evolveWithoutNatDex = "evolveWithoutNationalDex";
    }
}
