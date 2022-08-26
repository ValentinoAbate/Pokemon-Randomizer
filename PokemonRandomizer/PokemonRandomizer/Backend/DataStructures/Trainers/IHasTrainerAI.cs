using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.DataStructures.Trainers
{
    public interface IHasTrainerAI
    {
        public BitArray AIFlags { get; set; }
    }
}
