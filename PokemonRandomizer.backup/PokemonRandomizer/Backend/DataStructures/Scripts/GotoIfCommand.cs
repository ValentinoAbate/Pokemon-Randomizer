namespace PokemonRandomizer.Backend.DataStructures.Scripts
{
    public class GotoIfCommand : GotoCommand
    {
        public byte condition;

        public override string ToString()
        {
            return base.ToString() + " if " + condition;
        }
    }
}
