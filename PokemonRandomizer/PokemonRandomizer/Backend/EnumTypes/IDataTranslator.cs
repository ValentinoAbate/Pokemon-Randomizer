namespace PokemonRandomizer.Backend.EnumTypes
{
    using DataStructures;
    public interface IDataTranslator
    {
        public PokemonBaseStats GetBaseStats(Pokemon p);

        public ItemData GetItemData(Item i);

        public MoveData GetMoveData(Move m);
    }
}
