using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;

namespace PokemonRandomizer.Backend.RomHandling.IndexTranslators
{
    public interface IIndexTranslator
    {
        public int PokemonToInternalIndex(Pokemon pokemon);
        public Pokemon InternalIndexToPokemon(int index);
        public int ItemToInternalIndex(Item item);
        public Item InternalIndexToItem(int index);
        public byte HeldItemEffectToInteralIndex(HeldItemEffect effect);
        public HeldItemEffect InternalIndexToHeldItemEffect(int index);
        public int AbilityToInternalIndex(Ability ability);
        public Ability InternalIndexToAbility(int index);
        public int MoveToInternalIndex(Move move) => (int)move;
        public Move InternalIndexToMove(int index) => (Move)index;
        public int MoveEffectToInternalIndex(MoveData.MoveEffect effect);
        public MoveData.MoveEffect InternalIndexToMoveEffect(int index);
    }
}
