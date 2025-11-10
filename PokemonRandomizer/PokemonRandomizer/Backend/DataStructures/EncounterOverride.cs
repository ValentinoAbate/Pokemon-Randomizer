using PokemonRandomizer.Backend.EnumTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.DataStructures
{
    public class EncounterOverride : Encounter
    {
        public override Pokemon Pokemon 
        { 
            get => Mirror ? target.Pokemon : pokemonOverride; 
            set
            {
                if (Mirror)
                {
                    return;
                }
                pokemonOverride = value;
            }
        }
        public override int Level { get => target.Level; set { } }
        public override int MaxLevel { get => target.MaxLevel; set { } }
        public override bool IsReal => !Mirror && base.IsReal;
        public bool Mirror { get; set; }
        private Pokemon pokemonOverride;

        private readonly Encounter target;

        public EncounterOverride(Encounter target, Pokemon pokemon) : base()
        {
            this.target = target;
            Mirror = pokemon == target.Pokemon;
            pokemonOverride = pokemon;
        }

        public override string ToString()
        {
            return $"{base.ToString()} (overrides {target})";
        }
    }
}
