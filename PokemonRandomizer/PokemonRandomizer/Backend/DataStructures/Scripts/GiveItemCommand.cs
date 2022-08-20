using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;

namespace PokemonRandomizer.Backend.DataStructures.Scripts
{
    public class GiveItemCommand : ItemCommand
    {
        public enum MessageType : byte
        {
            Obtain,
            Find,
        }
        public override Item Item { get; set; }
        public int amount;
        public MessageType messageType;

        public override bool IsItemSource => true;

        public override string ToString()
        {
            return $"give {amount} {Item.ToDisplayString()} ({messageType.ToDisplayString()})";
        }
    }
}
