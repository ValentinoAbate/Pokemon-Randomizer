using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;
using System;

namespace PokemonRandomizer.Backend.RomHandling.IndexTranslators
{
    public class Gen4IndexTranslator : IIndexTranslator
    {
        public static Gen4IndexTranslator Main { get; } = new Gen4IndexTranslator();
        public int PokemonToInternalIndex(Pokemon pokemon) => PokemonUtils.PokemonToGen4Internal(pokemon);
        public Pokemon InternalIndexToPokemon(int index) => PokemonUtils.Gen4InternalToPokemon(index);
        public int ItemToInternalIndex(Item item) => ItemUtils.ItemToGen4Internal(item);
        public Item InternalIndexToItem(int index) => ItemUtils.Gen4InternalToItem(index);
        public byte HeldItemEffectToInteralIndex(HeldItemEffect effect) => throw new NotImplementedException();
        public HeldItemEffect InternalIndexToHeldItemEffect(int index) => throw new NotImplementedException();
        public int AbilityToInternalIndex(Ability ability) => AbilityUtils.AbilityToPostGen3(ability);
        public Ability InternalIndexToAbility(int index) => AbilityUtils.PostGen3InternalToAbility(index);
        public int MoveEffectToInternalIndex(MoveData.MoveEffect effect) => MoveDataUtils.MoveEffectToGen4Internal(effect);
        public MoveData.MoveEffect InternalIndexToMoveEffect(int index) => MoveDataUtils.Gen4InternalToMoveEffect(index);
    }
}
