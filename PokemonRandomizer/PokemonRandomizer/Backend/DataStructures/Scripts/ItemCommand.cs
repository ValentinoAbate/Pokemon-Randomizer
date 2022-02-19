using PokemonRandomizer.Backend.EnumTypes;

namespace PokemonRandomizer.Backend.DataStructures.Scripts
{
    public abstract class ItemCommand : Command
    {
        public enum Type
        {
            Normal, // The give item event gives the item stored in item
            Variable, // The command gives an item stored in the variable (int)item
            Unknown // Unknown, happens in lilycove city once, can research more later
        }
        public abstract Item Item { get; set; }
        public abstract Type ItemType { get; set; }
        public virtual bool IsItemSource => false;
    }
}
