using PokemonRandomizer.Backend.EnumTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.RomHandling.IndexTranslators
{
    public class Gen3IndexTranslator : IIndexTranslator
    {
        public static Gen3IndexTranslator Main { get; } = new Gen3IndexTranslator();
        public int PokemonToInternalIndex(Pokemon pokemon) => (int)pokemon;
        public Pokemon InternalIndexToPokemon(int index) => (Pokemon)index;
        public int ItemToInternalIndex(Item item) => (int)item;
        public Item InternalIndexToItem(int index) => (Item)index;
        public int AbilityToInternalIndex(Ability ability) => (int)ability;
        public Ability InternalIndexToAbility(int index) => (Ability)index;
    }
}
