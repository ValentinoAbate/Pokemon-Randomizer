using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.DataStructures.Trainers
{
    public interface IHasTrainerPokemonFixedIVs
    {
        public int FixedIVs { get; set; }
    }
}
