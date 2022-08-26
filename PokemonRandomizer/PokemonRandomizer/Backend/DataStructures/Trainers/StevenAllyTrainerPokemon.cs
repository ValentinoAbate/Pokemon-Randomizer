namespace PokemonRandomizer.Backend.DataStructures.Trainers
{
    public class StevenAllyTrainerPokemon : TrainerPokemon, IHasTrainerPokemonNature, IHasTrainerPokemonEvs
    {
        public int Nature { get; set; }
        public byte[] EVs { get; set; }
    }
}
