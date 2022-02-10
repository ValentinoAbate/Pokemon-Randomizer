using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;

namespace PokemonRandomizer.Backend.DataStructures
{
    // Normally effective attacks are not kept track of
    public enum TypeEffectiveness
    {
        // This does not appear in actual TypeEffectiveness definitions (used for random weightings)
        // Do not write this value to the rom
        Normal = -1,
        NoEffect,
        NotVeryEffective = 5,
        SuperEffective = 20,
    }

    public class TypeEffectivenessChart
    {
        // The byte sequence that marks the end of the data structure
        public static readonly byte[] endSequence = { 0xFF, 0xFF, 0x00 };
        // The byte sequence that marks the end of the non-ignoreAfterForesight relations
        public static readonly byte[] separatorSequence = { 0xFE, 0xFE, 0x00 };
        // The current amount of typerelations
        public int Count { get => typeRelations.Count + ignoreAfterForesight.Count; }
        // The amount of relations when read from the ROM (set manually)
        public int InitCount { get; set; }
        // A list of the type pairings (not including the ones ignored after foresight)
        public List<TypePair> Keys { get => typeRelations.Keys.ToList(); }
        // A list of the type pairings ignored after foresight
        public List<TypePair> KeysIgnoreAfterForesight { get => ignoreAfterForesight.Keys.ToList(); }
        // All of the type relations except for those ignored after foresight is used
        private readonly Dictionary<TypePair, TypeEffectiveness> typeRelations = new Dictionary<TypePair, TypeEffectiveness>();
        // The type relations that are ignored after foresight is used
        private readonly Dictionary<TypePair, TypeEffectiveness> ignoreAfterForesight = new Dictionary<TypePair, TypeEffectiveness>();
        // Helper method to make it easier to add to the list
        public void Add(PokemonType atType, PokemonType dfType, TypeEffectiveness e, bool ignoreAfterForesight = false)
        {
            var dict = ignoreAfterForesight ? this.ignoreAfterForesight : typeRelations;
            var tPair = new TypePair(atType, dfType);
            if (dict.ContainsKey(tPair))
                return;
            // Add the new type relation to the proper list
            dict.Add(tPair, e); 
        }
        // Set value of type relation (add if not present, else update)
        public void Set(PokemonType atType, PokemonType dfType, TypeEffectiveness e, bool ignoreAfterForesight = false)
        {
            var dict = ignoreAfterForesight ? this.ignoreAfterForesight : typeRelations;
            var tPair = new TypePair(atType, dfType);
            if (dict.ContainsKey(tPair))
            {
                if (e == TypeEffectiveness.Normal)
                    dict.Remove(tPair);
                else
                    dict[tPair] = e;
            }
            else
                dict.Add(tPair, e);
        }
        // Return the effectiveness of one type attacking another
        public TypeEffectiveness GetEffectiveness(TypePair tPair)
        {
            if (typeRelations.ContainsKey(tPair))
                return typeRelations[tPair];
            if (ignoreAfterForesight.ContainsKey(tPair))
                return ignoreAfterForesight[tPair];
            return TypeEffectiveness.Normal;
        }
        // Return the effectiveness of one type attacking another
        public TypeEffectiveness GetEffectiveness(PokemonType atType, PokemonType dfType)
        {
            return GetEffectiveness(new TypePair(atType, dfType));
        }
        // Return the effectiveness of one type pair attacking another type pair
        public TypeEffectiveness GetEffectiveness(PokemonType atType1, PokemonType atType2, PokemonType dfType1, PokemonType dfType2)
        {
            bool atkSingleTyped = atType1 == atType2;
            bool defSingleTyped = dfType1 == dfType2;
            if (atkSingleTyped && defSingleTyped)
                    return GetEffectiveness(atType1, dfType1);
            var atkTypes = atkSingleTyped ? new PokemonType[1] { atType1 } : new PokemonType[2] { atType1, atType2 };
            var defTypes = defSingleTyped ? new PokemonType[1] { dfType1 } : new PokemonType[2] { dfType1, dfType2 };

            const int noEffectConst = -100;
            var scores = new List<int>();
            foreach(var t in atkTypes)
            {
                int score = 0;
                foreach (var def in defTypes)
                {
                    var effect = GetEffectiveness(t, def);
                    if (effect == TypeEffectiveness.SuperEffective)
                        ++score;
                    else if (effect == TypeEffectiveness.NotVeryEffective)
                        --score;
                    else if (effect == TypeEffectiveness.NoEffect)
                    {
                        score = noEffectConst;
                        break;
                    }
                }
                if (score >= 1)
                    return TypeEffectiveness.SuperEffective;
                scores.Add(score);
            }
            if (scores.All((i) => i == noEffectConst))
                return TypeEffectiveness.NoEffect;
            else if (scores.All((i) => i <= -1))
                return TypeEffectiveness.NotVeryEffective;
            else
                return TypeEffectiveness.Normal;
        }
        // Returns true if this type relation is in the chart (i.e. it is not normal damage)
        public bool ContainsRelation(PokemonType atType, PokemonType dfType)
        {
            var tPair = new TypePair(atType, dfType);
            return typeRelations.ContainsKey(tPair) || ignoreAfterForesight.ContainsKey(tPair);
        }
        // Returns true if the relation is ignored after FORESIGHT/ODEUR SLEUTH is used
        public bool IgnoredAfterForesight(PokemonType atType, PokemonType dfType)
        {
            return ignoreAfterForesight.ContainsKey(new TypePair(atType, dfType));
        }
        // A class for storing a pairing of two types. Is hashable
        public class TypePair : IEquatable<TypePair>
        {
            public PokemonType attackingType;
            public PokemonType defendingType;
            public TypePair(PokemonType atType, PokemonType dfType)
            {
                attackingType = atType;
                defendingType = dfType;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + attackingType.GetHashCode();
                    hash = hash * 31 + defendingType.GetHashCode();
                    return hash;
                }

            }
            public override string ToString()
            {
                return attackingType.ToDisplayString() + " vs " + defendingType.ToDisplayString();
            }
            public bool Equals(TypePair other)
            {
                return (attackingType == other.attackingType) && (defendingType == other.defendingType);
            }
        }
    }
}
