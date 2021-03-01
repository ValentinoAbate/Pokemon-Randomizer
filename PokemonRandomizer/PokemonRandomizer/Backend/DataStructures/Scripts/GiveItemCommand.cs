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

        public enum Type
        {
            Normal, // The give item event gives the item stored in item
            Variable, // The command gives an item stored in the variable (int)item
            Unknown // Unknown, happens in lilycove city once, can research more later
        }

        public Type type = Type.Normal;
        public Item item;
        public int amount;
        public MessageType messageType;

        public override string ToString()
        {
            return "give " + amount + " " + item.ToDisplayString() + " (" + messageType.ToDisplayString() + ")";
        }
    }
}
