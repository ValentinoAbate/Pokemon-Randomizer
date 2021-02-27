using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;

namespace PokemonRandomizer.Backend.DataStructures.Scripts
{
    public class GiveItemCommand : Command
    {
        public enum MessageType : byte
        {
            Obtain,
            Find,
        }

        public Item item;
        public int amount;
        public MessageType messageType;

        public override string ToString()
        {
            return "give " + amount + " " + item.ToDisplayString() + " (" + messageType.ToDisplayString() + ")";
        }
    }
}
