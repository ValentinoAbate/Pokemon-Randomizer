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
        // Pokedex Elements
        public const string nationalDexOrder = "nationalDexOrder";
        // Map elements
        public const string mapLabels = "mapLabels";
        public const string mapBankPointers = "mapBankPointers";
        public const string maps = "maps";
        // Pokemon Elements
        public const string evolutions = "evolutions";
        public const string tmHmCompat = "tmHmCompat";
        public const string pokemonBaseStats = "pokemonBaseStats";
        public const string pokemonNames = "pokemonNames";
        public const string tutorCompat = "moveTutorCompat";
        public const string movesets = "movesets";
        public const string starterPokemon = "starterPokemon";
        // Data elements
        public const string moveData = "moveData";
        public const string tutorMoves = "moveTutorMoves";
        public const string tmMoves = "tmMoves";
        public const string hmMoves = "hmMoves";
        public const string itemData = "itemDefinitions";
        public const string itemSprites = "itemSprites";
        public const string itemEffectsTable = "itemEffectsTable";
        public const string pickupItems = "pickupItems";
        public const string pickupRareItems = "pickupRareItems";
        public const string stoneEffect = "stoneEffect";
        public const string trainerBattles = "trainerBattles";
        public const string trades = "trades";

        // Hacks and Tweaks / Misc
        public const string pcPotion = "pcPotion";
        public const string runIndoors = "runIndoors";
        public const string textSpeed = "textSpeed";
        public const string evolveWithoutNatDex = "evolveWithoutNationalDex";
    }
}
