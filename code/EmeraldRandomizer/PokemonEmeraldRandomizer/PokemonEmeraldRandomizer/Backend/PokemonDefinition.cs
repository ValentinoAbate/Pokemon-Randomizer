using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer.Backend
{
    //All of the pokemon types. These should map integer wise to the game-defined types
    public enum PokemonType
    {
        None = -1,
        NRM, //Normal
        FTG, //Fighting
        FLY, //Flying
        PSN, //Poison
        GRD, //Ground
        RCK, //Rock
        BUG, //Bug (the best type)
        GHO, //Ghost
        STL, //Steel
        Invalid, //Question (???). I think this is curse's type?
        FIR, //Fire
        WAT, //Water
        GRS, //Grass
        ELE, //Electric
        PSY, //Psychic
        ICE, //Ice
        DRG, //Dragon
        DRK, //Dark
    }
    public enum Ability
    {
        NONE, Stench, Drizzle, SpeedBoost, BattleArmor,
        Sturdy, Damp, Limber, SandVeil, Static, VoltAbsorb, WaterAbsorb, Oblivious, CloudNine,
        Compoundeyes, Insomnia, ColorChange, Immunity, FlashFire, ShieldDust, OwnTempo, SuctionCups,
        Intimidate, ShadowTag, RoughSkin, WonderGuard, Levitate, EffectSpore, Synchronize, ClearBody,
        NaturalCure, Lightningrod, SereneGrace, SwiftSwim, Chlorophyll, Illuminate, Trace, HugePower,
        PoisonPoint, InnerFocus, MagmaArmor, WaterVeil, MagnetPull, Soundproof, RainDish,
        SandStream, Pressure, ThickFat, EarlyBird, FlameBody, RunAway, KeenEye, HyperCutter,
        Pickup, Truant, Hustle, CuteCharm, Plus, Minus, Forecast, StickyHold, ShedSkin, Guts,
        MarvelScale, LiquidOoze, Overgrow, Blaze, Torrent, Swarm, RockHead, Drought, ArenaTrap,
        VitalSpirit, WhiteSmoke, PurePower, ShellArmor, Cacophony, AirLock
    }
    public class PokemonDefinition
    {
        public PokemonType[] types = new PokemonType[] { PokemonType.None, PokemonType.None };
        public Ability[] abilities = new Ability[] { Ability.NONE, Ability.NONE };
        public Item[] heldItems = new Item[] { Item.None, Item.None };
        #region Stats
        private byte[] stats = new byte[6];
        public byte Hp { get { return stats[0]; } set { stats[0] = value; } }
        public byte Attack { get { return stats[1]; } set { stats[1] = value; } }
        public byte Defense { get { return stats[2]; } set { stats[2] = value; } }
        public byte Speed { get { return stats[3]; } set { stats[3] = value; } }
        public byte SpAttack { get { return stats[4]; } set { stats[4] = value; } }
        public byte SpDefense { get { return stats[5]; } set { stats[5] = value; } }
        #endregion

        private byte[] evs = new byte[6];
        public PokemonDefinition(byte[] data)//Add adress as an argument
        {
            //abilities = new int[] { 1, 0 };
            //heldItems = new int[] { 0, 0 };
            //statSwaps = new int[] { 0, 1, 2, 3, 4, 5 };
            //TMcompat = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            //tutorCompat = new int[] { 0, 0, 0, 0 };
            //if (data.Length < 28)
            //{
            //    Console.WriteLine("Error parsing Pokemon Data: Expected 28 bytes, got " + data.Length);
            //}
            //else
            //{
            //    for (int i = 0; i < 6; i++)
            //    {  //fill in stats (hp/at/df/sp/sa/sd)
            //        setStat(i, data[i]);
            //        evs[i] = (data[10 + i / 4] >> ((i * 2) % 8)) & 3;
            //    }
            //    setType(0, data[6]);
            //    setType(1, data[7]);
            //    setAbility(0, data[22]);
            //    setAbility(1, data[23]);
            //    heldItems = new int[] { data[13] * 256 + data[12], data[15] * 256 + data[14] };
            //}
        }
        public override string ToString()
        {
            string ret = string.Empty;
            //ret += NAME (NEED POKEDEX ORDER VS ROM ORDER)
            StringBuilder sb = new StringBuilder();
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
                if (evs[i] == 0)
                {
                    sb.Append("- ");
                }
                else
                {
                    sb.Append(evs[i] + " ");
                }
            }
            sb.Append("  | ");
            sb.Append(types[0].ToString());
            if (types[0] == types[1])
                sb.Append("     |  ");
            else
                sb.Append("/" + types[1].ToString() + " |  ");
            foreach (Ability ability in abilities){
                string abil = ability == Ability.NONE ? "---" : ability.ToString();
                sb.Append(abil);
                for (int i = abil.Length; i <= 12; i++)
                {
                    sb.Append(" ");
                } sb.Append(" | ");
            }
            foreach (Item item in heldItems){
                string itemText = item == Item.None ? "---" : item.ToString();
                sb.Append(item);
                for (int i=itemText.Length;i<=12;i++){
                    sb.Append(" ");
                } sb.Append(" | ");
            }
            return sb.ToString();
        }
    }
}
