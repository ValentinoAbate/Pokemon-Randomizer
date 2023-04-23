using PokemonRandomizer.Backend.EnumTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.DataStructures.Trainers
{
    public class FrontierTrainerPokemon : TrainerPokemon, IHasTrainerPokemonNature, IHasFrontierTrainerEvs
    {
        public Nature Nature { get; set; }
        public IHasFrontierTrainerEvs.EvFlags Evs { get; set; }
        public int HeldItemIndex { get; set; }
    }
}
