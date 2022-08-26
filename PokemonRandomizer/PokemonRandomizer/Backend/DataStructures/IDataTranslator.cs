using PokemonRandomizer.Backend.DataStructures.Trainers;
using PokemonRandomizer.Backend.EnumTypes;

namespace PokemonRandomizer.Backend.DataStructures
{
    public interface IDataTranslator
    {
        public PokemonBaseStats GetBaseStats(Pokemon p);

        public ItemData GetItemData(Item i);

        public MoveData GetMoveData(Move m);

        public Trainer GetTrainer(int trainerIndex);
    }
}
