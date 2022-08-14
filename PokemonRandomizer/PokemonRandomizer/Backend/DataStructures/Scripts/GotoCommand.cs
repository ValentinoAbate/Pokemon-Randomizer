namespace PokemonRandomizer.Backend.DataStructures.Scripts
{
    public class GotoCommand : Command
    {
        public int offset;
        public Script script;
        public override string ToString()
        {
            return "goto " + offset.ToString("X2");
        }
    }
}
