using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;

namespace PokemonRandomizer.Backend.RomHandling.IndexTranslators
{
    public class Gen3IndexTranslator : IIndexTranslator
    {
        public static Gen3IndexTranslator Main { get; } = new Gen3IndexTranslator();
        public int PokemonToInternalIndex(Pokemon pokemon) => (int)pokemon;
        public Pokemon InternalIndexToPokemon(int index) => (Pokemon)index;
        public int ItemToInternalIndex(Item item) => (int)item;
        public Item InternalIndexToItem(int index) => (Item)index;
        public byte HeldItemEffectToInteralIndex(HeldItemEffect effect) => (byte)effect;
        public HeldItemEffect InternalIndexToHeldItemEffect(int index) => (HeldItemEffect)index;
        public int AbilityToInternalIndex(Ability ability) => (int)ability;
        public Ability InternalIndexToAbility(int index) => (Ability)index;
        public int MoveEffectToInternalIndex(MoveData.MoveEffect effect) => (int)effect;
        public MoveData.MoveEffect InternalIndexToMoveEffect(int index) => (MoveData.MoveEffect)index;
        public int MoveTargetsToInternalIndex(MoveData.Targets targets) => (int)targets;
        public MoveData.Targets InternalIndexToMoveTargets(int index) => (MoveData.Targets)index;
    }
}
