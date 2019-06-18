

namespace PokemonRandomizer.Backend.EnumTypes
{
    // All of the pokemon exp growth curves
    public enum ExpGrowthType : byte
    {
        // for more info on curves, see https://bulbapedia.bulbagarden.net/wiki/Experience
        // Name           // Lv100 Exp
        Medium_Fast,      // 1,000,000
        Erratic,          // 600,000
        Fluctuating,      // 1,640,000
        Medium_Slow,      // 1,059,860
        Fast,             // 800,000
        Slow,             // 1,250,000
    }
}
