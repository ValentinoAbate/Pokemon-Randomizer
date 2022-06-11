using PokemonRandomizer.Backend.EnumTypes;
using System;

namespace PokemonRandomizer.Backend.Utilities
{
    public static class AbilityUtils
    {
        public static Ability PostGen3InternalToAbility(int postGen3Index)
        {
            var ability = (Ability)postGen3Index;
            if(ability >= Ability.Cacophony)
            {
                return ability + 1;
            }
            return ability;
        }

        public static int AbilityToPostGen3(Ability ability)
        {
            if(ability >= Ability.Air_Lock)
            {
                return (int)ability - 1;
            }
            return (int)ability;
        }
    }
}
