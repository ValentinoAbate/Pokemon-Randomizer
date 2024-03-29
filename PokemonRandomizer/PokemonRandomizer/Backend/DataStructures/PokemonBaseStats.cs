﻿using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;
using PokemonRandomizer.Backend.Utilities.Debug;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PokemonRandomizer.Backend.DataStructures
{
    public class PokemonBaseStats
    {
        public const int numStats = 6;
        public const int hpStatIndex = 0;
        public const int atkStatIndex = 1;
        public const int defStatIndex = 2;
        public const int spdStatIndex = 3;
        public const int spAtkStatIndex = 4;
        public const int spDefStatIndex = 5;
        public const int maxIV = 31;
        public Pokemon species;

        #region Helper Properties

        public bool IsSingleTyped => types[0] == types[1];
        public bool IsDualTyped => types[0] != types[1];
        public bool IsLegendary => species.IsLegendary();
        public bool HasRealEvolution => evolvesTo.Count(e => e.IsRealEvolution) > 0;
        public bool IsBasicOrEvolvesFromBaby => IsBasic || EvolvesFromBaby;
        public bool IsBasic => evolvesFrom.Count == 0;
        public bool IsBaby => species.IsBaby();
        public bool EvolvesFromBaby => evolvesFrom.FirstOrDefault(e => e.Pokemon.IsBaby()) != null;
        public Evolution FirstEmptyEvolution => evolvesTo.FirstOrDefault(e => !e.IsRealEvolution);

        #endregion

        #region Basic 28 byte Data Structure

        #region Data structure documentation
        // Base HP             byte    0
        // Base Attack         byte    1
        // Base Defense        byte    2
        // Base Speed          byte    3
        // Base Sp. Attack     byte    4
        // Base Sp. Defense    byte    5
        // Type 1              byte    6
        // Type 2              byte    7
        // Catch rate          byte    8
        // Base Exp. yield     byte    9
        // Effort yield        word    10
        // Item 1              word    12
        // Item 2              word    14
        // Gender              byte    16
        // Egg cycles          byte    17
        // Base friendship     byte    18
        // Level - up type     byte    19
        // Egg Group 1         byte    20
        // Egg Group 2         byte    21
        // Ability 1           byte    22
        // Ability 2           byte    23
        // Safari Zone rate    byte    24
        // Color and Flip      byte    25
        // Padding*            word    26.
        #endregion

        #region Stats

        public byte[] stats = new byte[numStats];
        public byte Hp { get => stats[hpStatIndex]; set => stats[hpStatIndex] = value; }
        public byte Attack { get => stats[atkStatIndex]; set => stats[atkStatIndex] = value; }
        public byte Defense { get => stats[defStatIndex]; set => stats[defStatIndex] = value; }
        public byte Speed { get => stats[spdStatIndex]; set => stats[spdStatIndex] = value; }
        public byte SpAttack { get => stats[spAtkStatIndex]; set => stats[spAtkStatIndex] = value; }
        public byte SpDefense { get => stats[spDefStatIndex]; set => stats[spDefStatIndex] = value; }
        public int EffectiveAttack => HasAbility(Ability.Huge_Power) || HasAbility(Ability.Pure_Power) ? Attack * 2 : Attack; // TODO: hustle
        public int BST => stats.Sum(b => b);
        // Note: only accurate for Gen III+
        public int GetStatAtLevel(int statIndex, int level, int EVs, int IVs, Nature nature)
        {
            if(statIndex < 0 || statIndex >= stats.Length)
                return 0;
            // SHEDINJA always has 1 hp, regardless of EVs / IVs
            if(species == Pokemon.SHEDINJA && statIndex == Hp)
            {
                return 1;
            }
            float baseValue = (((stats[statIndex] * 2) + IVs + (EVs / 4)) * level) / 100f;
            if(statIndex == Hp) 
            {
                return (int)Math.Floor(baseValue + level + 10);
            }
            return (int)Math.Floor((baseValue + 5) * nature.StatMultiplier(statIndex));
        }

        #endregion

        #region EVs

        public int[] evYields = new int[numStats]; // How many evs you get when you defeat this pokemon
        public int HpEvYield { get => evYields[0]; set => evYields[0] = value; }
        public int AttackEvYield { get => evYields[1]; set => evYields[1] = value; }
        public int DefenseEvYield { get => evYields[2]; set => evYields[2] = value; }
        public int SpeedEvYield { get => evYields[3]; set => evYields[3] = value; }
        public int SpAttackEvYield { get => evYields[4]; set => evYields[4] = value; }
        public int SpDefenseEvYield { get => evYields[5]; set => evYields[5] = value; }

        #endregion

        #region Types

        public bool OriginallySingleTyped => OriginalPrimaryType == OriginalSecondaryType;
        public bool OriginallyDualTyped => OriginalPrimaryType != OriginalSecondaryType;
        public PokemonType OriginalPrimaryType
        {
            get => OriginalTypes[0];
        }
        public PokemonType OriginalSecondaryType
        {
            get => OriginalTypes[1];
        }
        public IReadOnlyList<PokemonType> OriginalTypes { get; private set; }
        public PokemonType PrimaryType
        {
            get => types[0];
            set => types[0] = value;
        }
        public PokemonType SecondaryType
        {
            get => types[1];
            set => types[1] = value;
        }
        public PokemonType[] types = new PokemonType[2];

        #endregion

        public Ability[] abilities = new Ability[] { Ability.NONE, Ability.NONE };
        public Item[] heldItems = new Item[] { Item.None, Item.None };
        public byte genderRatio; // The gender ratio (0 is always male, 254 is always female, 255 is none)
        public ExpGrowthType growthType;
        public EggGroup[] eggGroups = new EggGroup[2];
        public byte eggCycles; // How many cycles it takes for eggs to hatch (256 steps per cycle)
        public byte baseFriendship;
        public byte catchRate; // how easy it is to catch the pokemon (higher is easier)
        public byte baseExpYield; // base exp gained if defeated (max 255, higher is more)
        public byte safariZoneRunRate;
        public bool flip;
        public SearchColor searchColor;

        #endregion

        #region Move Data (TM, HM, Tutor compatibility)

        public List<Move> eggMoves;
        public LearnSet learnSet;
        public LearnSet OriginalLearnset { get; private set; }
        public BitArray TMCompat; //Compatibility with TMs 01-50 (index maps to RomData.TMMoves)
        public BitArray HMCompat; //Compatibility with HMs 01-08 (index maps to RomData.HMMoves)
        public BitArray moveTutorCompat; //Compatibility with moveTutors (index maps to RomData.tutorMoves)
        public HashSet<Move> originalTmHmMtMoves = new HashSet<Move>();
        public HashSet<Move> originalUnlearnableTmHmMtMoves = new HashSet<Move>();

        #endregion

        #region Graphical Data

        public Palette palette;
        public int paletteIndex;
        public Palette shinyPalette;
        public int shinyPaletteIndex;

        #endregion

        public string Name { get; set; }

        public Evolution[] evolvesTo;
        // Just for lookup purposes, not to be written to file
        public List<Evolution> evolvesFrom = new List<Evolution>();
        public int NationalDexIndex { get; set; }
        public bool IsVariant { get; set; } = false;
        public IReadOnlyCollection<PokemonType> VariantTypes
        {
            get
            {
                if (!IsVariant)
                {
                    return Array.Empty<PokemonType>();
                }
                if (PrimaryType != OriginalPrimaryType)
                {
                    // If the pokemon is currently single typed or if the second type is the same as the original, there can only be one variant type
                    if (IsSingleTyped || SecondaryType == OriginalSecondaryType)
                    {
                        return new PokemonType[] { PrimaryType };
                    }
                    // Both types must be variant types
                    return types; // Safe because this property returns a read-only collection
                }
                if(SecondaryType != OriginalSecondaryType)
                {
                    return new PokemonType[] { SecondaryType };
                }
                Logger.main.Warning($"Pokemon {Name} is marked as variant but has no variant types");
                return Array.Empty<PokemonType>();
            }
        }

        public void SetOriginalValues()
        {
            OriginalTypes = new PokemonType[] { PrimaryType, SecondaryType };
            OriginalLearnset = new LearnSet(learnSet);
        }

        public bool HasAbility(Ability a)
        {
            return abilities[0] == a || abilities[1] == a;
        }

        public bool IsType(PokemonType type)
        {
            return PrimaryType == type || SecondaryType == type;
        }

        public bool IsType(params PokemonType[] types)
        {
            foreach (var type in types)
            {
                if (IsType(type))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsSameTypeAs(PokemonBaseStats other)
        {
            return PrimaryType == other.PrimaryType && SecondaryType == other.SecondaryType;
        }

        public void SetSingleType(PokemonType type)
        {
            PrimaryType = SecondaryType = type;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            string dexName = Name;
            sb.Append(dexName + new string(' ', 15 - dexName.Length) + "| ");
            for (int i = 0; i < 6; i++)
            {
                if (stats[i] < 10)
                {
                    sb.Append("  ");
                }
                else if (stats[i] < 100)
                {
                    sb.Append(" ");
                }
                sb.Append(stats[i] + " ");
            }
            sb.Append("  |  ");
            for (int i = 0; i < 6; i++)
            {
                if (evYields[i] == 0)
                {
                    sb.Append("- ");
                }
                else
                {
                    sb.Append(evYields[i] + " ");
                }
            }
            sb.Append("  | ");
            sb.Append(types[0].ToDisplayString());
            if (types[0] == types[1])
                sb.Append("     |  ");
            else
                sb.Append("/" + types[1].ToDisplayString() + " |  ");
            foreach (Ability ability in abilities)
            {
                string abil = ability == Ability.NONE ? "---" : ability.ToDisplayString();
                sb.Append(abil);
                for (int i = abil.Length; i <= 12; i++)
                {
                    sb.Append(" ");
                }
                sb.Append(" | ");
            }
            foreach (Item item in heldItems)
            {
                string itemText = item == Item.None ? "---" : item.ToDisplayString();
                sb.Append(itemText);
                for (int i = itemText.Length; i <= 12; i++)
                {
                    sb.Append(" ");
                }
                sb.Append(" | ");
            }
            return sb.ToString();
        }
    }
}
