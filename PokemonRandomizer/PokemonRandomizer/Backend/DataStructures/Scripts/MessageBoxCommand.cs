namespace PokemonRandomizer.Backend.DataStructures.Scripts
{
    public class MessageBoxCommand : TextCommand, IHasCommandInputType
    {
        public int specialCode;
        public int value;
        public override string Text { get; set; }
        public CommandInputType InputType { get; set; }

        public override string ToString()
        {
            return $"Msgbox {specialCode:x2}: \"{Text}\"";
        }
    }
}
