using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.RomHandling.IndexTranslators;
using System.Runtime.CompilerServices;

namespace PokemonRandomizer.Backend.RomHandling
{
    public abstract class RomHandler
    {
        protected abstract IIndexTranslator IndexTranslator { get; }

        #region IEnumTranslator Shortcuts
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int PokemonToInternalIndex(Pokemon pokemon) => IndexTranslator.PokemonToInternalIndex(pokemon);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Pokemon InternalIndexToPokemon(int index) => IndexTranslator.InternalIndexToPokemon(index);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int ItemToInternalIndex(Item item) => IndexTranslator.ItemToInternalIndex(item);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Item InternalIndexToItem(int index) => IndexTranslator.InternalIndexToItem(index);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected byte HeldItemEffectToInternalIndex(HeldItemEffect effect) => IndexTranslator.HeldItemEffectToInteralIndex(effect);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected HeldItemEffect InternalIndexToHeldItemEffect(int index) => IndexTranslator.InternalIndexToHeldItemEffect(index);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int AbilityToInternalIndex(Ability ability) => IndexTranslator.AbilityToInternalIndex(ability);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Ability InternalIndexToAbility(int index) => IndexTranslator.InternalIndexToAbility(index);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int MoveToInternalIndex(Move move) => IndexTranslator.MoveToInternalIndex(move);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Move InternalIndexToMove(int index) => IndexTranslator.InternalIndexToMove(index);
        #endregion
    }
}
