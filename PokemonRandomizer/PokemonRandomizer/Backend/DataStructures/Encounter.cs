using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;

namespace PokemonRandomizer.Backend.DataStructures
{
    public class Encounter
    {
        public virtual Pokemon Pokemon { get; set; }
        public virtual int Level { get; set; }
        public virtual int MaxLevel { get; set; }
        public virtual bool IsReal => Pokemon != Pokemon.None;

        public Encounter(Pokemon pokemon, int level) : this(pokemon, level, level)
        {

        }
        public Encounter(Pokemon pokemon, int level, int maxLevel)
        {
            Pokemon = pokemon;
            Level = level;
            MaxLevel = maxLevel;
        }
        protected Encounter()
        {

        }
        public override string ToString()
        {
            return Pokemon.ToDisplayString() + " Lv" + (Level == MaxLevel ? Level.ToString() : Level + "-" + MaxLevel);
        }
    }
}
