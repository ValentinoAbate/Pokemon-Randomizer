using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;
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
        public IHasFrontierTrainerEvs.EvFlags EVs { get; set; }
        public int HeldItemIndex { get; set; }

        public override string ToString()
        {
            return $"{species.ToDisplayString()} ({Nature})";
        }

        public override TrainerPokemon Clone()
        {
            var other = new FrontierTrainerPokemon();
            other.CopyBasicValuesFrom(this);
            other.Nature = Nature;
            other.EVs = EVs;
            other.HeldItemIndex = HeldItemIndex;
            return other;
        }
    }
}
