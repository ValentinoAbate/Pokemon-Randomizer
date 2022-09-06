using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.DataStructures.Trainers
{
    public interface IHasTrainerPokemonAbilityOverride
    {
        public enum Type
        {
            None,
            ForceAbility1,
            ForceAbility2,
        }

        public Type AbilityOverride { get; set; }
    }
}
