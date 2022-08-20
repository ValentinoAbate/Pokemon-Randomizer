using PokemonRandomizer.Backend.EnumTypes;

namespace PokemonRandomizer.Backend.DataStructures.Scripts
{
    public abstract class ItemCommand : Command, IHasCommandInputType
    {
        public CommandInputType InputType { get; set; }
        public abstract Item Item { get; set; }
        public virtual bool IsItemSource => false;
    }
}
