namespace PokemonRandomizer.Backend.DataStructures.Trainers
{
    public class StevenAllyTrainerPokemon : TrainerPokemon, IHasTrainerPokemonNature, IHasTrainerPokemonEvs, IHasTrainerPokemonFixedIVs
    {
        public int Nature { get; set; }

        public int[] EVs { get; } = new int[PokemonBaseStats.numStats];

        public int FixedIVs { get; set; }
    }
}
