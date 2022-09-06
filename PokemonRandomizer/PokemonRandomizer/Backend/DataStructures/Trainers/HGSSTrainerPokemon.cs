using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.DataStructures.Trainers
{
    public class HGSSTrainerPokemon : TrainerPokemon, IHasTrainerPokemonGenderOverride, IHasTrainerPokemonAbilityOverride, IHasTrainerPokemonBallSeal
    {
        public IHasTrainerPokemonGenderOverride.Type GenderOverride { get; set; }
        public IHasTrainerPokemonAbilityOverride.Type AbilityOverride { get; set; }
        public int BallSeal { get; set; }

        public override string ToString()
        {
            var ret = base.ToString();
            if(GenderOverride != IHasTrainerPokemonGenderOverride.Type.None)
            {
                ret += $" ({GenderOverride})";
            }
            if (AbilityOverride != IHasTrainerPokemonAbilityOverride.Type.None)
            {
                ret += $" ({AbilityOverride})";
            }
            return ret;
        }
    }
}
