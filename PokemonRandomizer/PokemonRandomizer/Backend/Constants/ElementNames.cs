namespace PokemonRandomizer.Backend.Constants
{
    /// <summary>
    /// Contains symbolic constants for referencing the names of elements in a ROM info file.
    /// </summary>
    public abstract class ElementNames
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
        public const string pokemonPalettes = "pokemonPalettes";
        public const string pokemonPalettesShiny = "pokemonPalettesShiny";
        public const string eggMoves = "eggMoves";
        // Data elements
        public const string moveData = "moveData";
        public const string moveDescriptions = "moveDescriptions";
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
        public const string trainerClassNames = "trainerClassNames";
        public const string trainerSprites = "trainerSprites";
        public const string trainerPalettesFront = "trainerPalettesFront";
        public const string trades = "trades";
        public const string setBerryTreeScript = "setBerryTreeScript";
        public const string typeEffectiveness = "typeEffectiveness";

        // Trainer metadata
        public const string gymLeaders = "gymLeaders";
        public const string rivals = "rivals";
        public const string eliteFour = "eliteFour";
        public const string champion = "champion";
        public const string aceTrainers = "aceTrainers";
        public const string teamData = "teams";
        public const string specialBosses = "specialBosses";
        public const string nameTrainerTypeOverrides = "nameTrainerTypeOverrides";
        public const string classTrainerTypeOverrides = "classTrainerTypeOverrides";

        // Map metadata
        public const string nonHeaderWeatherIsAltRoute = "nonHeaderWeatherIsAltRoute";

        // Hacks and Tweaks / Misc
        public const string pcPotion = "pcPotion";
        public const string runIndoors = "runIndoors";
        public const string textSpeed = "textSpeed";
        public const string firstEncounter = "firstRoute";
        public const string forecastRoutine = "forecastFormChangeRoutine";

        public abstract class GenIII
        {
            public const string evolveWithoutNatDex = "evolveWithoutNationalDex";
            public const string stoneEvolveWithoutNatDex = "stoneEvolveWithoutNationalDex";
            public const string hailHack = "hailHack";
            public const string sunHack = "sunHack";
            public const string deoxysMewObeyFix = "deoxysMewObeyFix";
            public const string stevenAllyBattle = "stevenAllyBattle";
            public const string battleTents = "battleTents";
            public const string frontierHeldItems = "frontierHeldItems";
            public const string rouletteWagers = "rouletteWagers";
            public const string rouletteWagerTextFix = "rouletteWagerTextFix";
            public const string rouletteLowTableFix = "rouletteLowTableFix";
        }

        public abstract class GenIV
        {
            public const string trainerPokemon = "trainerPokemon";
            public const string text = "text";
            public const string trainerNames = "trainerNames";
            public const string moveNames = "moveNames";
            public const string itemNames = "itemNames";
            public const string itemDescriptions = "itemDescriptions";
            public const string abilityNames = "abilityNames";
        }
    }
}
