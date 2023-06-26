using PokemonRandomizer.Backend.DataStructures.Trainers;
using PokemonRandomizer.Backend.EnumTypes;

namespace PokemonRandomizer.Backend.DataStructures
{
    public interface IDataTranslator
    {
        public PokemonBaseStats GetBaseStats(Pokemon p);

        public ItemData GetItemData(Item i);

        public MoveData GetMoveData(Move m);

        public Move GetTmMove(int index);
        public Move GetHmMove(int index);
        public Move GetTutorMove(int index);

        public Trainer GetTrainer(int trainerIndex);
    }
}
