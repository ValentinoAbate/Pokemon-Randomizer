using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.DataStructures.Trainers
{
    public interface IHasTrainerPokemonGenderOverride
    {
        public enum Type 
        { 
            None,
            ForceFemale,
            ForceMale,
        }

        public Type GenderOverride { get; set; }
    }
}
