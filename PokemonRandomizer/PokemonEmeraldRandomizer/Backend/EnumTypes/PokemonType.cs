

namespace PokemonRandomizer.Backend.EnumTypes
{
    // All of the pokemon types. These should map integer wise to the game-defined types
    public enum PokemonType : byte
    {
        NRM, // Normal
        FTG, // Fighting
        FLY, // Flying
        PSN, // Poison
        GRD, // Ground
        RCK, // Rock
        BUG, // Bug (the best type)
        GHO, // Ghost
        STL, // Steel
        Unknown, // Question (???). This is curse's type
        FIR, // Fire
        WAT, // Water
        GRS, // Grass
        ELE, // Electric
        PSY, // Psychic
        ICE, // Ice
        DRG, // Dragon
        DRK, // Dark
        FAI, // Fairy
    }
}
