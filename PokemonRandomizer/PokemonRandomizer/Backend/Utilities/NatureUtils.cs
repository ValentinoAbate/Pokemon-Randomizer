using PokemonRandomizer.Backend.EnumTypes;
using static PokemonRandomizer.Backend.DataStructures.PokemonBaseStats;
using static PokemonRandomizer.Backend.EnumTypes.Nature;

namespace PokemonRandomizer.Backend.Utilities
{
    public static class NatureUtils
    {
        public const int numNatures = 25;
        public const int neuteralStatIndex = 0;
        private const float positiveNature = 1.1f;
        private const float negativeNature = 0.9f;

        public static float StatMultiplier(this Nature nature, int statIndex)
        {
            return statIndex switch
            {
                atkStatIndex when nature is LONELY or ADAMANT or NAUGHTY or BRAVE => positiveNature,
                atkStatIndex when nature is BOLD or MODEST or CALM or TIMID => negativeNature,
                defStatIndex when nature is BOLD or IMPISH or LAX or RELAXED => positiveNature,
                defStatIndex when nature is LONELY or MILD or GENTLE or HASTY => negativeNature,
                spdStatIndex when nature is TIMID or HASTY or JOLLY or NAIVE => positiveNature,
                spdStatIndex when nature is BRAVE or RELAXED or QUIET or SASSY => negativeNature,
                spAtkStatIndex when nature is MODEST or MILD or RASH or QUIET => positiveNature,
                spAtkStatIndex when nature is ADAMANT or IMPISH or CAREFUL or JOLLY => negativeNature,
                spDefStatIndex when nature is CALM or GENTLE or CAREFUL or SASSY => positiveNature,
                spDefStatIndex when nature is NAUGHTY or LAX or RASH or NAIVE => negativeNature,
                _ => 1
            };
        }

        public static int PositiveStatIndex(this Nature nature)
        {
            return nature switch
            {
                LONELY or ADAMANT or NAUGHTY or BRAVE => atkStatIndex,
                BOLD or IMPISH or LAX or RELAXED => defStatIndex,
                TIMID or HASTY or JOLLY or NAIVE => spdStatIndex,
                MODEST or MILD or RASH or QUIET => spAtkStatIndex,
                CALM or GENTLE or CAREFUL or SASSY => spDefStatIndex,
                _ => neuteralStatIndex
            };
        }

        public static int NegativeStatIndex(this Nature nature) 
        {
            return nature switch
            {
                BOLD or MODEST or CALM or TIMID => atkStatIndex,
                LONELY or MILD or GENTLE or HASTY => defStatIndex,
                BRAVE or RELAXED or QUIET or SASSY => spdStatIndex,
                ADAMANT or IMPISH or CAREFUL or JOLLY => spAtkStatIndex,
                NAUGHTY or LAX or RASH or NAIVE => spDefStatIndex,
                _ => neuteralStatIndex
            };
        }

        public static bool IsNeuteralNature(this Nature nature) 
        {
            return nature is HARDY or DOCILE or BASHFUL or QUIRKY or SERIOUS;
        }

        public static Nature GetNature(int positiveStatIndex, int negativeStatIndex)
        {
            return positiveStatIndex switch
            {
                atkStatIndex    => negativeStatIndex switch 
                {
                    atkStatIndex => HARDY,
                    defStatIndex => LONELY,
                    spdStatIndex => BRAVE,
                    spAtkStatIndex => ADAMANT,
                    spDefStatIndex => NAUGHTY,
                    _ => HARDY,
                },
                defStatIndex    => negativeStatIndex switch 
                {
                    atkStatIndex => BOLD,
                    defStatIndex => DOCILE,
                    spdStatIndex => RELAXED,
                    spAtkStatIndex => IMPISH,
                    spDefStatIndex => LAX,
                    _ => HARDY,
                },
                spdStatIndex    => negativeStatIndex switch
                {
                    atkStatIndex => TIMID,
                    defStatIndex => HASTY,
                    spdStatIndex => SERIOUS,
                    spAtkStatIndex => JOLLY,
                    spDefStatIndex => NAIVE,
                    _ => HARDY,
                },
                spAtkStatIndex  => negativeStatIndex switch
                {
                    atkStatIndex => MODEST,
                    defStatIndex => MILD,
                    spdStatIndex => QUIET,
                    spAtkStatIndex => BASHFUL,
                    spDefStatIndex => RASH,
                    _ => HARDY,
                },
                spDefStatIndex  => negativeStatIndex switch
                {
                    atkStatIndex => CALM,
                    defStatIndex => GENTLE,
                    spdStatIndex => SASSY,
                    spAtkStatIndex => CAREFUL,
                    spDefStatIndex => QUIRKY,
                    _ => HARDY,
                },
                _ => HARDY,
            };
        }
    }
}
