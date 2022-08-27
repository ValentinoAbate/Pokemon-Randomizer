using System;

namespace PokemonRandomizer.Backend.DataStructures.Trainers
{
    public class StevenAllyTrainerPokemon : TrainerPokemon, IHasTrainerPokemonNature, IHasTrainerPokemonEvs
    {
        public int Nature { get; set; }
        public byte[] EVs { get; set; }

        public override TrainerPokemon Clone()
        {
            var other = new StevenAllyTrainerPokemon();
            other.CopyBasicValuesFrom(this);
            other.Nature = Nature;
            other.EVs = new byte[EVs.Length];
            Array.Copy(EVs, other.EVs, EVs.Length);
            return other;
        }
    }
}
