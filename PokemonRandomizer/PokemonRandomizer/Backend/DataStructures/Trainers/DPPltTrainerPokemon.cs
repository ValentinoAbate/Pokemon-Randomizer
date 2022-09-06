using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.DataStructures.Trainers
{
    public class DPPltTrainerPokemon : TrainerPokemon, IHasTrainerPokemonBallSeal, IHasTrainerPokemonForm
    {
        public int Form { get; set; }
        public int BallSeal { get; set; }

        public override string ToString()
        {
            var ret = base.ToString();
            if(Form != 0)
            {
                ret += $" (Form {Form})";
            }
            return ret; 
        }
    }
}
