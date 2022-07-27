using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.RomHandling.IndexTranslators
{
    public class Gen4IndexTranslator : IIndexTranslator
    {
        public static Gen4IndexTranslator Main { get; } = new Gen4IndexTranslator();
        public int PokemonToInternalIndex(Pokemon pokemon) => PokemonUtils.PokemonToGen4Internal(pokemon);
        public Pokemon InternalIndexToPokemon(int index) => PokemonUtils.Gen4InternalToPokemon(index);
        public int ItemToInternalIndex(Item item) => ItemUtils.ItemToGen4Internal(item);
        public Item InternalIndexToItem(int index) => ItemUtils.Gen4InternalToItem(index);
        public int AbilityToInternalIndex(Ability ability) => AbilityUtils.AbilityToPostGen3(ability);
        public Ability InternalIndexToAbility(int index) => AbilityUtils.PostGen3InternalToAbility(index);
    }
}
