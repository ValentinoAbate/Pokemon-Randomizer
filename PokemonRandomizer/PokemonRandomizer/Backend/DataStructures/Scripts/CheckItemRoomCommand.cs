using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;

namespace PokemonRandomizer.Backend.DataStructures.Scripts
{
    public class CheckItemRoomCommand : ItemCommand
    {
        public override Type ItemType { get; set; } = Type.Normal;
        public override Item Item { get; set; }
        public int quantity;

        public override string ToString()
        {
            return $"Check if bag can fit {Item.ToDisplayString()} x{quantity}";
        }
    }
}
