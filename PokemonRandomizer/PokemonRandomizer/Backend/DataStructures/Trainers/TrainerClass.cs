namespace PokemonRandomizer.Backend.DataStructures.Trainers
{
    public class TrainerClass
    {
        public int ClassNum { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
