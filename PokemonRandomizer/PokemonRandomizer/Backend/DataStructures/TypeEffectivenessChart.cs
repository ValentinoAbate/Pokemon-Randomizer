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
        public const int maxTypes = 20; // Upper bound for number of types. Arbitrary number greater than vanilla types
        // The byte sequence that marks the end of the data structure
        public static readonly byte[] endSequence = { 0xFF, 0xFF, 0x00 };
        // The byte sequence that marks the end of the non-ignoreAfterForesight relations
        public static readonly byte[] separatorSequence = { 0xFE, 0xFE, 0x00 };
        public static bool HasAbilityImmunity(PokemonType type)
        {
            // ELE: Volt absorb, Motor Drive, lightningrod (gen V+)
            // WAT: Water absorb, dry skin, storm drain (gen V+)
            // FIR: Flash fire
            // GRD: Levitate
            // GRS: Sap Sipper (gen V+)
            return type is PokemonType.ELE or PokemonType.WAT or PokemonType.FIR or PokemonType.GRD;
        }
        public static bool HasAbilityImmunity(PokemonBaseStats defender, PokemonType atkType)
        {
            return atkType switch
            {
                PokemonType.GRD => defender.HasAbility(Ability.Levitate),
                PokemonType.FIR => defender.HasAbility(Ability.Flash_Fire),
                PokemonType.WAT => defender.HasAbility(Ability.Water_Absorb) || defender.HasAbility(Ability.Dry_Skin), // Note gen V +storm drain
                PokemonType.ELE => defender.HasAbility(Ability.Volt_Absorb) || defender.HasAbility(Ability.Motor_Drive), // Note Gen V : lightningrod can also cause immunity
                _ => false
            };
        }
        // The current amount of typerelations
        public int Count { get => TypeRelations.Count + IgnoreAfterForesight.Count; }
        // The amount of relations when read from the ROM (set manually)
        public int InitCount { get; set; }
        // All of the type relations except for those ignored after foresight is used
        public Dictionary<TypePair, TypeEffectiveness> TypeRelations { get; } = new Dictionary<TypePair, TypeEffectiveness>();
        // The type relations that are ignored after foresight is used
        public Dictionary<TypePair, TypeEffectiveness> IgnoreAfterForesight { get; } = new Dictionary<TypePair, TypeEffectiveness>();
        // Helper method to make it easier to add to the list
        public void Add(PokemonType atType, PokemonType dfType, TypeEffectiveness e, bool ignoreAfterForesight = false)
        {
            var dict = ignoreAfterForesight ? this.IgnoreAfterForesight : TypeRelations;
            var tPair = new TypePair(atType, dfType);
            if (dict.ContainsKey(tPair))
                return;
            // Add the new type relation to the proper list
            dict.Add(tPair, e); 
        }
        // Set value of type relation (add if not present, else update)
        public void Set(PokemonType atType, PokemonType dfType, TypeEffectiveness e, bool ignoreAfterForesight = false)
        {
            var dict = ignoreAfterForesight ? IgnoreAfterForesight : TypeRelations;
            var tPair = new TypePair(atType, dfType);
            if (dict.ContainsKey(tPair))
            {
                if (e == TypeEffectiveness.Normal)
                {
                    dict.Remove(tPair);
                }
                else
                {
                    dict[tPair] = e;
                }
            }
            else
            {
                dict.Add(tPair, e);
            }
        }
        // Return the effectiveness of one type attacking another
        public TypeEffectiveness GetEffectiveness(TypePair tPair)
        {
            if (TypeRelations.ContainsKey(tPair))
                return TypeRelations[tPair];
            if (IgnoreAfterForesight.ContainsKey(tPair))
                return IgnoreAfterForesight[tPair];
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
            return TypeRelations.ContainsKey(tPair) || IgnoreAfterForesight.ContainsKey(tPair);
        }
        // Returns true if the relation is ignored after FORESIGHT/ODEUR SLEUTH is used
        public bool IgnoredAfterForesight(PokemonType atType, PokemonType dfType)
        {
            return IgnoreAfterForesight.ContainsKey(new TypePair(atType, dfType));
        }
        // Returns true if the given single type has no weaknesses
        // Assumes that there are no contradictory entries
        public bool HasNoWeaknesses(PokemonType singleType)
        {
            foreach(var (typePair, effectiveness) in TypeRelations)
            {
                if (typePair.defendingType == singleType && effectiveness == TypeEffectiveness.SuperEffective)
                {
                    return false;
                }
            }
            foreach (var (typePair, effectiveness) in IgnoreAfterForesight)
            {
                if (typePair.defendingType == singleType && effectiveness == TypeEffectiveness.SuperEffective)
                {
                    return false;
                }
            }
            return true;
        }

        public bool HasNoWeaknesses(PokemonType primaryType, PokemonType secondaryType)
        {
            if(primaryType == secondaryType)
            {
                return HasNoWeaknesses(primaryType);
            }
            var weaknesses = new HashSet<PokemonType>(maxTypes);
            var resists = new HashSet<PokemonType>(maxTypes);
            void ProcessTypeRelation(TypePair typePair, TypeEffectiveness effectiveness)
            {
                if (typePair.defendingType != primaryType && typePair.defendingType != secondaryType)
                {
                    return;
                }
                if (effectiveness == TypeEffectiveness.SuperEffective)
                {
                    if (!weaknesses.Contains(typePair.attackingType))
                    {
                        weaknesses.Add(typePair.attackingType);
                    }
                }
                else if (effectiveness is TypeEffectiveness.NotVeryEffective or TypeEffectiveness.NoEffect)
                {
                    if (!resists.Contains(typePair.attackingType))
                    {
                        resists.Add(typePair.attackingType);
                    }
                }
            }
            // Process Normal Type Relations
            foreach(var (typePair, effectiveness) in TypeRelations)
            {
                ProcessTypeRelation(typePair, effectiveness);
            }
            // Process Ignore After Foresight Type Relations
            foreach (var (typePair, effectiveness) in IgnoreAfterForesight)
            {
                ProcessTypeRelation(typePair, effectiveness);
            }
            // If there are more weaknesses than resistances, it is impossible for all type combinations to be covered
            if (weaknesses.Count > resists.Count)
            {
                return false;
            }
            foreach(var weakness in weaknesses)
            {
                if (!resists.Contains(weakness))
                {
                    return false;
                }
            }
            return true;
        }

        public bool HasPerfectCoverage(PokemonType atkType)
        {
            // TODO: mold breaker exception
            if (HasAbilityImmunity(atkType))
                return false;
            foreach (var (typePair, effectiveness) in TypeRelations)
            {
                if (typePair.attackingType == atkType && effectiveness == TypeEffectiveness.NoEffect)
                {
                    return false;
                }
            }
            // TODO: scappy exception
            foreach (var (typePair, effectiveness) in IgnoreAfterForesight)
            {
                if (typePair.attackingType == atkType && effectiveness == TypeEffectiveness.NoEffect)
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsImmune(PokemonBaseStats defender, PokemonType atkType)
        {
            if (HasAbilityImmunity(defender, atkType))
                return true;
            if (GetEffectiveness(atkType, defender.PrimaryType) == TypeEffectiveness.NoEffect)
                return true;
            return defender.IsDualTyped && GetEffectiveness(atkType, defender.SecondaryType) == TypeEffectiveness.NoEffect;
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
