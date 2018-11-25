using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer.Backend
{
    // Normally effective attacks are not kept track of
    public enum TypeEffectiveness
    {
        NoEffect,
        NotVeryEffective = 5,
        SuperEffective = 20,
    }

    public class TypeEffectivenessChart
    {
        // All of the type relations except for those ignored after foresight is used
        public List<TypeRelation> typeRelations = new List<TypeRelation>();
        // The type relations that are ignored after foresight is used
        public List<TypeRelation> ignoreAfterForesight = new List<TypeRelation>();

        public void Add(PokemonType atType, PokemonType dfType, TypeEffectiveness e, bool ignoreAfterForesight = false)
        {
            // Add the new type relation to the proper list
            (ignoreAfterForesight ? this.ignoreAfterForesight : typeRelations).Add(new TypeRelation(atType, dfType, e)); 
        }

        public class TypeRelation : IEquatable<TypeRelation>
        {
            public PokemonType attackingType;
            public PokemonType defendingType;
            public TypeEffectiveness effectiveness;
            public TypeRelation(PokemonType atType, PokemonType dfType, TypeEffectiveness e)
            {
                attackingType = atType;
                defendingType = dfType;
                effectiveness = e;
            }

            public bool Equals(TypeRelation other)
            {
                return (attackingType == other.attackingType) && (defendingType == other.defendingType) && (effectiveness == other.effectiveness);
            }
        }
    }
}
