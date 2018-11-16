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
    public enum Attack
    {
        None,POUND,KARATE_CHOP,DOUBLESLAP,COMET_PUNCH,MEGA_PUNCH,PAY_DAY,FIRE_PUNCH,
        ICE_PUNCH,THUNDERPUNCH,SCRATCH,VICEGRIP,GUILLOTINE,RAZOR_WIND,SWORDS_DANCE,CUT,GUST,WING_ATTACK,
        WHIRLWIND,FLY,BIND,SLAM,VINE_WHIP,STOMP,DOUBLE_KICK,MEGA_KICK,JUMP_KICK,ROLLING_KICK,SANDᅳATTACK,
        HEADBUTT,HORN_ATTACK,FURY_ATTACK,HORN_DRILL,TACKLE,BODY_SLAM,WRAP,TAKE_DOWN,THRASH,DOUBLEᅳEDGE,
        TAIL_WHIP,POISON_STING,TWINEEDLE,PIN_MISSILE,LEER,BITE,GROWL,ROAR,SING,SUPERSONIC,SONICBOOM,
        DISABLE,ACID,EMBER,FLAMETHROWER,MIST,WATER_GUN,HYDRO_PUMP,SURF,ICE_BEAM,BLIZZARD,PSYBEAM,
        BUBBLEBEAM,AURORA_BEAM,HYPER_BEAM,PECK,DRILL_PECK,SUBMISSION,LOW_KICK,COUNTER,SEISMIC_TOSS,STRENGTH,
        ABSORB,MEGA_DRAIN,LEECH_SEED,GROWTH,RAZOR_LEAF,SOLARBEAM,POISONPOWDER,STUN_SPORE,SLEEP_POWDER,
        PETAL_DANCE,STRING_SHOT,DRAGON_RAGE,FIRE_SPIN,THUNDERSHOCK,THUNDERBOLT,THUNDER_WAVE,THUNDER,ROCK_THROW,
        EARTHQUAKE,FISSURE,DIG,TOXIC,CONFUSION,PSYCHIC,HYPNOSIS,MEDITATE,AGILITY,QUICK_ATTACK,RAGE,
        TELEPORT,NIGHT_SHADE,MIMIC,SCREECH,DOUBLE_TEAM,RECOVER,HARDEN,MINIMIZE,SMOKESCREEN,CONFUSE_RAY,
        WITHDRAW,DEFENSE_CURL,BARRIER,LIGHT_SCREEN,HAZE,REFLECT,FOCUS_ENERGY,BIDE,METRONOME,MIRROR_MOVE,
        SELFDESTRUCT,EGG_BOMB,LICK,SMOG,SLUDGE,BONE_CLUB,FIRE_BLAST,WATERFALL,CLAMP,SWIFT,SKULL_BASH,
        SPIKE_CANNON,CONSTRICT,AMNESIA,KINESIS,SOFTBOILED,HI_JUMP_KICK,GLARE,DREAM_EATER,POISON_GAS,BARRAGE,
        LEECH_LIFE,LOVELY_KISS,SKY_ATTACK,TRANSFORM,BUBBLE,DIZZY_PUNCH,SPORE,FLASH,PSYWAVE,SPLASH,ACID_ARMOR,
        CRABHAMMER,EXPLOSION,FURY_SWIPES,BONEMERANG,REST,ROCK_SLIDE,HYPER_FANG,SHARPEN,CONVERSION,TRI_ATTACK,
        SUPER_FANG,SLASH,SUBSTITUTE,STRUGGLE,SKETCH,TRIPLE_KICK,THIEF,SPIDER_WEB,MIND_READER,NIGHTMARE,
        FLAME_WHEEL,SNORE,CURSE,FLAIL,CONVERSION_2,AEROBLAST,COTTON_SPORE,REVERSAL,SPITE,POWDER_SNOW,PROTECT,
        MACH_PUNCH,SCARY_FACE,FAINT_ATTACK,SWEET_KISS,BELLY_DRUM,SLUDGE_BOMB,MUDᅳSLAP,OCTAZOOKA,SPIKES,
        ZAP_CANNON,FORESIGHT,DESTINY_BOND,PERISH_SONG,ICY_WIND,DETECT,BONE_RUSH,LOCKᅳON,OUTRAGE,SANDSTORM,
        GIGA_DRAIN,ENDURE,CHARM,ROLLOUT,FALSE_SWIPE,SWAGGER,MILK_DRINK,SPARK,FURY_CUTTER,STEEL_WING,MEAN_LOOK,
        ATTRACT,SLEEP_TALK,HEAL_BELL,RETURN,PRESENT,FRUSTRATION,SAFEGUARD,PAIN_SPLIT,SACRED_FIRE,MAGNITUDE,
        DYNAMICPUNCH,MEGAHORN,DRAGONBREATH,BATON_PASS,ENCORE,PURSUIT,RAPID_SPIN,SWEET_SCENT,IRON_TAIL,METAL_CLAW,
        VITAL_THROW,MORNING_SUN,SYNTHESIS,MOONLIGHT,HIDDEN_POWER,CROSS_CHOP,TWISTER,RAIN_DANCE,SUNNY_DAY,CRUNCH,
        MIRROR_COAT,PSYCH_UP,EXTREMESPEED,ANCIENTPOWER,SHADOW_BALL,FUTURE_SIGHT,ROCK_SMASH,WHIRLPOOL,BEAT_UP,
        FAKE_OUT,UPROAR,STOCKPILE,SPIT_UP,SWALLOW,HEAT_WAVE,HAIL,TORMENT,FLATTER,WILLᅳOᅳWISP,MEMENTO,FACADE,
        FOCUS_PUNCH,SMELLINGSALT,FOLLOW_ME,NATURE_POWER,CHARGE,TAUNT,HELPING_HAND,TRICK,ROLE_PLAY,WISH,ASSIST,
        INGRAIN,SUPERPOWER,MAGIC_COAT,RECYCLE,REVENGE,BRICK_BREAK,YAWN,KNOCK_OFF,ENDEAVOR,ERUPTION,SKILL_SWAP,
        IMPRISON,REFRESH,GRUDGE,SNATCH,SECRET_POWER,DIVE,ARM_THRUST,CAMOUFLAGE,TAIL_GLOW,LUSTER_PURGE,MIST_BALL,
        FEATHERDANCE,TEETER_DANCE,BLAZE_KICK,MUD_SPORT,ICE_BALL,NEEDLE_ARM,SLACK_OFF,HYPER_VOICE,POISON_FANG,
        CRUSH_CLAW,BLAST_BURN,HYDRO_CANNON,METEOR_MASH,ASTONISH,WEATHER_BALL,AROMATHERAPY,FAKE_TEARS,AIR_CUTTER,
        OVERHEAT,ODOR_SLEUTH,ROCK_TOMB,SILVER_WIND,METAL_SOUND,GRASSWHISTLE,TICKLE,COSMIC_POWER,WATER_SPOUT,
        SIGNAL_BEAM,SHADOW_PUNCH,EXTRASENSORY,SKY_UPPERCUT,SAND_TOMB,SHEER_COLD,MUDDY_WATER,BULLET_SEED,AERIAL_ACE,
        ICICLE_SPEAR,IRON_DEFENSE,BLOCK,HOWL,DRAGON_CLAW,FRENZY_PLANT,BULK_UP,BOUNCE,MUD_SHOT,POISON_TAIL,COVET,
        VOLT_TACKLE,MAGICAL_LEAF,WATER_SPORT,CALM_MIND,LEAF_BLADE,DRAGON_DANCE,ROCK_BLAST,SHOCK_WAVE,WATER_PULSE,
        DOOM_DESIRE,PSYCHO_BOOST
    }
    public class PokemonDefinition
    {
        private static char enumDashChar = 'ᅳ';
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
