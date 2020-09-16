using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PokemonRandomizer.Backend.DataStructures
{
    public class PokemonBaseStats
    {
        //Special Pokemon Sets (do better later)
        public static HashSet<PokemonSpecies> babyPokemon = new HashSet<PokemonSpecies>
        {
            PokemonSpecies.AZURILL,
            PokemonSpecies.CLEFFA,
            PokemonSpecies.ELEKID,
            PokemonSpecies.IGGLYBUFF,
            PokemonSpecies.MAGBY,
            PokemonSpecies.SMOOCHUM,
            PokemonSpecies.TOGEPI,
            PokemonSpecies.PICHU,
            PokemonSpecies.TYROGUE,
            PokemonSpecies.WYNAUT,
        };
        public static HashSet<PokemonSpecies> legendaries = new HashSet<PokemonSpecies>
        {
            PokemonSpecies.ARTICUNO,
            PokemonSpecies.ZAPDOS,
            PokemonSpecies.MOLTRES,
            PokemonSpecies.MEW,
            PokemonSpecies.MEWTWO,
            PokemonSpecies.SUICUNE,
            PokemonSpecies.RAIKOU,
            PokemonSpecies.ENTEI,
            PokemonSpecies.LUGIA,
            PokemonSpecies.HOーOH,
            PokemonSpecies.CELEBI,
            PokemonSpecies.REGICE,
            PokemonSpecies.REGIROCK,
            PokemonSpecies.REGISTEEL,
            PokemonSpecies.KYOGRE,
            PokemonSpecies.GROUDON,
            PokemonSpecies.RAYQUAZA,
            PokemonSpecies.DEOXYS,
            PokemonSpecies.JIRACHI,
            PokemonSpecies.LATIAS,
            PokemonSpecies.LATIOS,
        };
        public readonly PokemonSpecies species;

        #region Helper Properties

        public int DexIndex { get { return PokedexUtils.PokedexIndex(species); } }
        public bool IsSingleTyped { get { return types[0] == types[1]; } }
        public bool IsLegendary { get => legendaries.Contains(species); }

        public bool IsBasicOrEvolvesFromBaby { get => IsBasic || EvolvesFromBaby; }
        public bool IsBasic { get => evolvesFrom.Count == 0; }
        public bool EvolvesFromBaby { get => evolvesFrom.FirstOrDefault((e) => babyPokemon.Contains(e.Pokemon)) != null; }

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
        public byte[] stats = new byte[6];
        public byte Hp { get { return stats[0]; } set { stats[0] = value; } }
        public byte Attack { get { return stats[1]; } set { stats[1] = value; } }
        public byte Defense { get { return stats[2]; } set { stats[2] = value; } }
        public byte Speed { get { return stats[3]; } set { stats[3] = value; } }
        public byte SpAttack { get { return stats[4]; } set { stats[4] = value; } }
        public byte SpDefense { get { return stats[5]; } set { stats[5] = value; } }
        #endregion

        public int[] evYields = new int[6]; // How many evs you get when you defeat this pokemon
        public PokemonType[] types = new PokemonType[2];
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
        public LearnSet learnSet;
        public BitArray TMCompat; //Compatibility with TMs 01-50 (index maps to RomData.TMMoves)
        public BitArray HMCompat; //Compatibility with HMs 01-08 (index maps to RomData.HMMoves)
        public BitArray moveTutorCompat; //Compatibility with moveTutors (index maps to RomData.tutorMoves)
        #endregion

        public Evolution[] evolvesTo;
        // Just for lookup purposes, not to be written to file
        public List<Evolution> evolvesFrom = new List<Evolution>();

        public PokemonBaseStats(Rom data, int offset, PokemonSpecies species)
        {
            // Set species
            this.species = species;
            data.Seek(offset);
            // fill in stats (hp/at/df/sp/sa/sd)
            stats = data.ReadBlock(6);
            // fill in types
            types[0] = (PokemonType)data.ReadByte();
            types[1] = (PokemonType)data.ReadByte();
            catchRate = data.ReadByte();
            baseExpYield = data.ReadByte();
            // fill in ev yields (stored in the first 12 bits of data[10-11])
            evYields = data.ReadBits(12, 2);
            heldItems[0] = (Item)data.ReadUInt16(); // (data[13] * 256 + data[12]);
            heldItems[1] = (Item)data.ReadUInt16(); // (data[15] * 256 + data[14]);
            genderRatio = data.ReadByte();
            eggCycles = data.ReadByte();
            baseFriendship = data.ReadByte();
            growthType = (ExpGrowthType)data.ReadByte();
            // fill in egg groups
            eggGroups[0] = (EggGroup)data.ReadByte();
            eggGroups[1] = (EggGroup)data.ReadByte();
            // fill in abilities
            abilities[0] = (Ability)data.ReadByte();
            abilities[1] = (Ability)data.ReadByte();
            safariZoneRunRate = data.ReadByte();
            byte searchFlip = data.ReadByte();
            // read color
            searchColor = (SearchColor)((searchFlip & 0b1111_1110) >> 1);
            // read flip
            flip = (searchFlip & 0b0000_0001) == 1;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            string dexName = PokedexUtils.DexName(species);
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
            sb.Append("\n" + learnSet.ToString());
            return sb.ToString();
        }
    }
}
