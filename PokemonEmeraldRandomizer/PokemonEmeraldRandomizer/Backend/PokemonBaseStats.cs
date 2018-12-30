using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer.Backend
{
    // Every valid species of pokemon. Some names are modified to be in the enum
    public enum PokemonSpecies : ushort
    {
        BULBASAUR = 1, IVYSAUR, VENUSAUR, CHARMANDER, CHARMELEON, CHARIZARD, SQUIRTLE, WARTORTLE,
        BLASTOISE, CATERPIE, METAPOD, BUTTERFREE, WEEDLE, KAKUNA, BEEDRILL, PIDGEY, PIDGEOTTO, PIDGEOT, RATTATA,
        RATICATE, SPEAROW, FEAROW, EKANS, ARBOK, PIKACHU, RAICHU, SANDSHREW, SANDSLASH, NIDORAN_GAL, NIDORINA,
        NIDOQUEEN, NIDORAN_BOI, NIDORINO, NIDOKING, CLEFAIRY, CLEFABLE, VULPIX, NINETALES, JIGGLYPUFF, WIGGLYTUFF,
        ZUBAT, GOLBAT, ODDISH, GLOOM, VILEPLUME, PARAS, PARASECT, VENONAT, VENOMOTH, DIGLETT, DUGTRIO, MEOWTH,
        PERSIAN, PSYDUCK, GOLDUCK, MANKEY, PRIMEAPE, GROWLITHE, ARCANINE, POLIWAG, POLIWHIRL, POLIWRATH, ABRA,
        KADABRA, ALAKAZAM, MACHOP, MACHOKE, MACHAMP, BELLSPROUT, WEEPINBELL, VICTREEBEL, TENTACOOL, TENTACRUEL,
        GEODUDE, GRAVELER, GOLEM, PONYTA, RAPIDASH, SLOWPOKE, SLOWBRO, MAGNEMITE, MAGNETON, FARFETCHD, DODUO,
        DODRIO, SEEL, DEWGONG, GRIMER, MUK, SHELLDER, CLOYSTER, GASTLY, HAUNTER, GENGAR, ONIX, DROWZEE, HYPNO,
        KRABBY, KINGLER, VOLTORB, ELECTRODE, EXEGGCUTE, EXEGGUTOR, CUBONE, MAROWAK, HITMONLEE, HITMONCHAN,
        LICKITUNG, KOFFING, WEEZING, RHYHORN, RHYDON, CHANSEY, TANGELA, KANGASKHAN, HORSEA, SEADRA, GOLDEEN,
        SEAKING, STARYU, STARMIE, MR_MIME, SCYTHER, JYNX, ELECTABUZZ, MAGMAR, PINSIR, TAUROS, MAGIKARP,
        GYARADOS, LAPRAS, DITTO, EEVEE, VAPOREON, JOLTEON, FLAREON, PORYGON, OMANYTE, OMASTAR, KABUTO,
        KABUTOPS, AERODACTYL, SNORLAX, ARTICUNO, ZAPDOS, MOLTRES, DRATINI, DRAGONAIR, DRAGONITE, MEWTWO, MEW,
        CHIKORITA, BAYLEEF, MEGANIUM, CYNDAQUIL, QUILAVA, TYPHLOSION, TOTODILE, CROCONAW, FERALIGATR, SENTRET,
        FURRET, HOOTHOOT, NOCTOWL, LEDYBA, LEDIAN, SPINARAK, ARIADOS, CROBAT, CHINCHOU, LANTURN, PICHU, CLEFFA,
        IGGLYBUFF, TOGEPI, TOGETIC, NATU, XATU, MAREEP, FLAAFFY, AMPHAROS, BELLOSSOM, MARILL, AZUMARILL,
        SUDOWOODO, POLITOED, HOPPIP, SKIPLOOM, JUMPLUFF, AIPOM, SUNKERN, SUNFLORA, YANMA, WOOPER, QUAGSIRE,
        ESPEON, UMBREON, MURKROW, SLOWKING, MISDREAVUS, UNOWN, WOBBUFFET, GIRAFARIG, PINECO, FORRETRESS,
        DUNSPARCE, GLIGAR, STEELIX, SNUBBULL, GRANBULL, QWILFISH, SCIZOR, SHUCKLE, HERACROSS, SNEASEL,
        TEDDIURSA, URSARING, SLUGMA, MAGCARGO, SWINUB, PILOSWINE, CORSOLA, REMORAID, OCTILLERY, DELIBIRD,
        MANTINE, SKARMORY, HOUNDOUR, HOUNDOOM, KINGDRA, PHANPY, DONPHAN, PORYGON2, STANTLER, SMEARGLE, TYROGUE,
        HITMONTOP, SMOOCHUM, ELEKID, MAGBY, MILTANK, BLISSEY, RAIKOU, ENTEI, SUICUNE, LARVITAR, PUPITAR,
        TYRANITAR, LUGIA, HOーOH, CELEBI, TREECKO = 277, GROVYLE, SCEPTILE, TORCHIC, COMBUSKEN, BLAZIKEN, MUDKIP,
        MARSHTOMP, SWAMPERT, POOCHYENA, MIGHTYENA, ZIGZAGOON, LINOONE, WURMPLE, SILCOON, BEAUTIFLY, CASCOON,
        DUSTOX, LOTAD, LOMBRE, LUDICOLO, SEEDOT, NUZLEAF, SHIFTRY, NINCADA, NINJASK, SHEDINJA, TAILLOW,
        SWELLOW, SHROOMISH, BRELOOM, SPINDA, WINGULL, PELIPPER, SURSKIT, MASQUERAIN, WAILMER, WAILORD, SKITTY,
        DELCATTY, KECLEON, BALTOY, CLAYDOL, NOSEPASS, TORKOAL, SABLEYE, BARBOACH, WHISCASH, LUVDISC, CORPHISH,
        CRAWDAUNT, FEEBAS, MILOTIC, CARVANHA, SHARPEDO, TRAPINCH, VIBRAVA, FLYGON, MAKUHITA, HARIYAMA,
        ELECTRIKE, MANECTRIC, NUMEL, CAMERUPT, SPHEAL, SEALEO, WALREIN, CACNEA, CACTURNE, SNORUNT, GLALIE,
        LUNATONE, SOLROCK, AZURILL, SPOINK, GRUMPIG, PLUSLE, MINUN, MAWILE, MEDITITE, MEDICHAM, SWABLU,
        ALTARIA, WYNAUT, DUSKULL, DUSCLOPS, ROSELIA, SLAKOTH, VIGOROTH, SLAKING, GULPIN, SWALOT, TROPIUS,
        WHISMUR, LOUDRED, EXPLOUD, CLAMPERL, HUNTAIL, GOREBYSS, ABSOL, SHUPPET, BANETTE, SEVIPER, ZANGOOSE,
        RELICANTH, ARON, LAIRON, AGGRON, CASTFORM, VOLBEAT, ILLUMISE, LILEEP, CRADILY, ANORITH, ARMALDO,
        RALTS, KIRLIA, GARDEVOIR, BAGON, SHELGON, SALAMENCE, BELDUM, METANG, METAGROSS, REGIROCK, REGICE,
        REGISTEEL, KYOGRE, GROUDON, RAYQUAZA, LATIAS, LATIOS, JIRACHI, DEOXYS, CHIMECHO
    }
    // All of the pokemon types. These should map integer wise to the game-defined types
    public enum PokemonType : byte
    {
        NRM, //Normal
        FTG, //Fighting
        FLY, //Flying
        PSN, //Poison
        GRD, //Ground
        RCK, //Rock
        BUG, //Bug (the best type)
        GHO, //Ghost
        STL, //Steel
        Unknown, //Question (???). I think this is curse's type?
        FIR, //Fire
        WAT, //Water
        GRS, //Grass
        ELE, //Electric
        PSY, //Psychic
        ICE, //Ice
        DRG, //Dragon
        DRK, //Dark
    }
    // All of the pokemon abilities
    public enum Ability : byte
    {
        NONE, Stench, Drizzle, Speed_Boost, Battle_Armor,
        Sturdy, Damp, Limber, Sand_Veil, Static, Volt_Absorb, Water_Absorb, Oblivious, Cloud_Nine,
        Compoundeyes, Insomnia, Color_Change, Immunity, Flash_Fire, Shield_Dust, Own_Tempo, Suction_Cups,
        Intimidate, Shadow_Tag, Rough_Skin, Wonder_Guard, Levitate, Effect_Spore, Synchronize, Clear_Body,
        Natural_Cure, Lightningrod, Serene_Grace, Swift_Swim, Chlorophyll, Illuminate, Trace, Huge_Power,
        Poison_Point, Inner_Focus, Magma_Armor, Water_Veil, Magnet_Pull, Soundproof, Rain_Dish,
        Sand_Stream, Pressure, Thick_Fat, Early_Bird, Flame_Body, Run_Away, Keen_Eye, Hyper_Cutter,
        Pickup, Truant, Hustle, Cute_Charm, Plus, Minus, Forecast, Sticky_Hold, Shed_Skin, Guts,
        Marvel_Scale, Liquid_Ooze, Overgrow, Blaze, Torrent, Swarm, Rock_Head, Drought, Arena_Trap,
        Vital_Spirit, White_Smoke, Pure_Power, Shell_Armor, Cacophony, Air_Lock
    }
    // All of the pokemon moves
    public enum Move : ushort
    {
        None, POUND, KARATE_CHOP, DOUBLESLAP, COMET_PUNCH, MEGA_PUNCH, PAY_DAY, FIRE_PUNCH,
        ICE_PUNCH, THUNDERPUNCH, SCRATCH, VICEGRIP, GUILLOTINE, RAZOR_WIND, SWORDS_DANCE, CUT, GUST, WING_ATTACK,
        WHIRLWIND, FLY, BIND, SLAM, VINE_WHIP, STOMP, DOUBLE_KICK, MEGA_KICK, JUMP_KICK, ROLLING_KICK, SANDーATTACK,
        HEADBUTT, HORN_ATTACK, FURY_ATTACK, HORN_DRILL, TACKLE, BODY_SLAM, WRAP, TAKE_DOWN, THRASH, DOUBLEーEDGE,
        TAIL_WHIP, POISON_STING, TWINEEDLE, PIN_MISSILE, LEER, BITE, GROWL, ROAR, SING, SUPERSONIC, SONICBOOM,
        DISABLE, ACID, EMBER, FLAMETHROWER, MIST, WATER_GUN, HYDRO_PUMP, SURF, ICE_BEAM, BLIZZARD, PSYBEAM,
        BUBBLEBEAM, AURORA_BEAM, HYPER_BEAM, PECK, DRILL_PECK, SUBMISSION, LOW_KICK, COUNTER, SEISMIC_TOSS, STRENGTH,
        ABSORB, MEGA_DRAIN, LEECH_SEED, GROWTH, RAZOR_LEAF, SOLARBEAM, POISONPOWDER, STUN_SPORE, SLEEP_POWDER,
        PETAL_DANCE, STRING_SHOT, DRAGON_RAGE, FIRE_SPIN, THUNDERSHOCK, THUNDERBOLT, THUNDER_WAVE, THUNDER, ROCK_THROW,
        EARTHQUAKE, FISSURE, DIG, TOXIC, CONFUSION, PSYCHIC, HYPNOSIS, MEDITATE, AGILITY, QUICK_ATTACK, RAGE,
        TELEPORT, NIGHT_SHADE, MIMIC, SCREECH, DOUBLE_TEAM, RECOVER, HARDEN, MINIMIZE, SMOKESCREEN, CONFUSE_RAY,
        WITHDRAW, DEFENSE_CURL, BARRIER, LIGHT_SCREEN, HAZE, REFLECT, FOCUS_ENERGY, BIDE, METRONOME, MIRROR_MOVE,
        SELFDESTRUCT, EGG_BOMB, LICK, SMOG, SLUDGE, BONE_CLUB, FIRE_BLAST, WATERFALL, CLAMP, SWIFT, SKULL_BASH,
        SPIKE_CANNON, CONSTRICT, AMNESIA, KINESIS, SOFTBOILED, HI_JUMP_KICK, GLARE, DREAM_EATER, POISON_GAS, BARRAGE,
        LEECH_LIFE, LOVELY_KISS, SKY_ATTACK, TRANSFORM, BUBBLE, DIZZY_PUNCH, SPORE, FLASH, PSYWAVE, SPLASH, ACID_ARMOR,
        CRABHAMMER, EXPLOSION, FURY_SWIPES, BONEMERANG, REST, ROCK_SLIDE, HYPER_FANG, SHARPEN, CONVERSION, TRI_ATTACK,
        SUPER_FANG, SLASH, SUBSTITUTE, STRUGGLE, SKETCH, TRIPLE_KICK, THIEF, SPIDER_WEB, MIND_READER, NIGHTMARE,
        FLAME_WHEEL, SNORE, CURSE, FLAIL, CONVERSION_2, AEROBLAST, COTTON_SPORE, REVERSAL, SPITE, POWDER_SNOW, PROTECT,
        MACH_PUNCH, SCARY_FACE, FAINT_ATTACK, SWEET_KISS, BELLY_DRUM, SLUDGE_BOMB, MUDーSLAP, OCTAZOOKA, SPIKES,
        ZAP_CANNON, FORESIGHT, DESTINY_BOND, PERISH_SONG, ICY_WIND, DETECT, BONE_RUSH, LOCKーON, OUTRAGE, SANDSTORM,
        GIGA_DRAIN, ENDURE, CHARM, ROLLOUT, FALSE_SWIPE, SWAGGER, MILK_DRINK, SPARK, FURY_CUTTER, STEEL_WING, MEAN_LOOK,
        ATTRACT, SLEEP_TALK, HEAL_BELL, RETURN, PRESENT, FRUSTRATION, SAFEGUARD, PAIN_SPLIT, SACRED_FIRE, MAGNITUDE,
        DYNAMICPUNCH, MEGAHORN, DRAGONBREATH, BATON_PASS, ENCORE, PURSUIT, RAPID_SPIN, SWEET_SCENT, IRON_TAIL, METAL_CLAW,
        VITAL_THROW, MORNING_SUN, SYNTHESIS, MOONLIGHT, HIDDEN_POWER, CROSS_CHOP, TWISTER, RAIN_DANCE, SUNNY_DAY, CRUNCH,
        MIRROR_COAT, PSYCH_UP, EXTREMESPEED, ANCIENTPOWER, SHADOW_BALL, FUTURE_SIGHT, ROCK_SMASH, WHIRLPOOL, BEAT_UP,
        FAKE_OUT, UPROAR, STOCKPILE, SPIT_UP, SWALLOW, HEAT_WAVE, HAIL, TORMENT, FLATTER, WILLーOーWISP, MEMENTO, FACADE,
        FOCUS_PUNCH, SMELLINGSALT, FOLLOW_ME, NATURE_POWER, CHARGE, TAUNT, HELPING_HAND, TRICK, ROLE_PLAY, WISH, ASSIST,
        INGRAIN, SUPERPOWER, MAGIC_COAT, RECYCLE, REVENGE, BRICK_BREAK, YAWN, KNOCK_OFF, ENDEAVOR, ERUPTION, SKILL_SWAP,
        IMPRISON, REFRESH, GRUDGE, SNATCH, SECRET_POWER, DIVE, ARM_THRUST, CAMOUFLAGE, TAIL_GLOW, LUSTER_PURGE, MIST_BALL,
        FEATHERDANCE, TEETER_DANCE, BLAZE_KICK, MUD_SPORT, ICE_BALL, NEEDLE_ARM, SLACK_OFF, HYPER_VOICE, POISON_FANG,
        CRUSH_CLAW, BLAST_BURN, HYDRO_CANNON, METEOR_MASH, ASTONISH, WEATHER_BALL, AROMATHERAPY, FAKE_TEARS, AIR_CUTTER,
        OVERHEAT, ODOR_SLEUTH, ROCK_TOMB, SILVER_WIND, METAL_SOUND, GRASSWHISTLE, TICKLE, COSMIC_POWER, WATER_SPOUT,
        SIGNAL_BEAM, SHADOW_PUNCH, EXTRASENSORY, SKY_UPPERCUT, SAND_TOMB, SHEER_COLD, MUDDY_WATER, BULLET_SEED, AERIAL_ACE,
        ICICLE_SPEAR, IRON_DEFENSE, BLOCK, HOWL, DRAGON_CLAW, FRENZY_PLANT, BULK_UP, BOUNCE, MUD_SHOT, POISON_TAIL, COVET,
        VOLT_TACKLE, MAGICAL_LEAF, WATER_SPORT, CALM_MIND, LEAF_BLADE, DRAGON_DANCE, ROCK_BLAST, SHOCK_WAVE, WATER_PULSE,
        DOOM_DESIRE, PSYCHO_BOOST
    }
    // All of the pokemon exp growth curves
    public enum ExpGrowthType : byte
    {
        //for more info on curves, see https://bulbapedia.bulbagarden.net/wiki/Experience
        //Name           //Lv100 Exp
        Medium_Fast,     //1,000,000
        Erratic,         //600,000
        Fluctuating,     //1,640,000
        Medium_Slow,     //1,059,860
        Fast,            //800,000
        Slow, 	         //1,250,000
    }
    // All of the pokemon Egg groups
    public enum EggGroup : byte
    {
        Monster = 1,
        Water_1,
        Bug,
        Flying,
        Field,
        Fairy,
        Grass,
        HumanーLike,
        Water_3,
        Mineral,
        Amorphous,
        Water_2,
        Ditto,
        Dragon,
        Undiscovered,
    }
    // All of the search colors
    public enum SearchColor : byte
    {
        Red,
        Blue,
        Yellow,
        Green,
        Black,
        Brown,
        Purple,
        Gray,
        White,
        Pink,
    }
    // All of the pokemon Habitats (used by the Fire Red/Leaf Green Pokedex)
    public enum Habitat
    {
        Grassland,
        Forest,
        WatersーEdge,
        Sea,
        Cave,
        Mountain,
        RoughーTerrain,
        Urban,
        Rare,
    }

    public class PokemonBaseStats
    {
        public readonly PokemonSpecies species;
        public int DexIndex { get { return PokedexUtils.PokedexIndex(species); } }
        public bool IsSingleTyped { get { return types[0] == types[1]; } }

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
        private byte[] stats = new byte[6];
        public byte Hp { get { return stats[0]; } set { stats[0] = value; } }
        public byte Attack { get { return stats[1]; } set { stats[1] = value; } }
        public byte Defense { get { return stats[2]; } set { stats[2] = value; } }
        public byte Speed { get { return stats[3]; } set { stats[3] = value; } }
        public byte SpAttack { get { return stats[4]; } set { stats[4] = value; } }
        public byte SpDefense { get { return stats[5]; } set { stats[5] = value; } }
        #endregion

        private byte[] evYields = new byte[6]; // How many evs you get when you defeat this pokemon
        public PokemonType[] types = new PokemonType[2];
        public Ability[] abilities = new Ability[] { Ability.NONE, Ability.NONE };
        public Item[] heldItems = new Item[] { Item.None, Item.None };
        public byte genderRatio; // The gender ratio (0 is always male, 254 is always female, 255 is none)
        public ExpGrowthType growthType;
        public EggGroup[] eggGroups = new EggGroup[2];
        public byte eggCycles; // How many cycles it takes for eggs to hatch (256 steps per cycle)
        public byte catchRate; // how easy it is to catch the pokemon (higher is easier)
        public byte baseExpYield; // base exp gained if defeated (max 255, higher is more)
        public byte safariZoneRunRate;
        public bool flip;
        public SearchColor searchColor;
        #endregion

        #region Move Data (TM, HM, Tutor compatibility)
        public MoveSet moveSet;
        public BitArray TMCompat; //Compatibility with TMs 01-50 (index maps to ROMData.TMMoves)
        public BitArray HMCompat; //Compatibility with HMs 01-08 (index maps to ROMData.HMMoves)
        public BitArray moveTutorCompat; //Compatibility with moveTutors (index maps to ROMData.tutorMoves)
        #endregion

        public Evolution[] evolutions;

        public PokemonBaseStats(byte[] data, PokemonSpecies species)
        {
#if DEBUG
            if (data.Length < 28)
                throw new System.Exception("Error parsing Pokemon Data: Expected 28 bytes, got " + data.Length);
#endif
            // Set species
            this.species = species;
            // fill in stats (hp/at/df/sp/sa/sd)
            Array.ConstrainedCopy(data, 0, stats, 0, 6);
            // fill in types
            types[0] = (PokemonType)data[6];
            types[1] = (PokemonType)data[7];
            catchRate = data[8];
            baseExpYield = data[9];
            // fill in ev yields (stored in the first 12 bits of data[10-11])
            for (int i = 0; i < 6; i++)
                evYields[i] = (byte)((data[10 + i / 4] >> ((i * 2) % 8)) & 3);
            heldItems[0] = (Item)data.ReadUInt16(12); // (data[13] * 256 + data[12]);
            heldItems[1] = (Item)data.ReadUInt16(14); // (data[15] * 256 + data[14]);
            genderRatio = data[16];
            eggCycles = data[17];
            growthType = (ExpGrowthType)data[19];
            // fill in egg groups
            eggGroups[0] = (EggGroup)data[20];
            eggGroups[1] = (EggGroup)data[21];
            // fill in abilities
            abilities[0] = (Ability)data[22];
            abilities[1] = (Ability)data[23];
            safariZoneRunRate = data[24];
            // read color
            searchColor = (SearchColor)((data[25] & 0b1111_1110) >> 1);
            // read flip
            flip = (data[25] & 0b0000_0001) == 1;
        }
        public byte[] ToByteArray()
        {
            byte[] data = new byte[28];
            // fill in stats (hp/at/df/sp/sa/sd)
            Array.ConstrainedCopy(stats, 0, data, 0, 6);
            // fill in types
            data[6] = (byte) types[0];
            data[7] = (byte) types[1];
            data[8] = catchRate;
            data[9] = baseExpYield;
            // TEMPORARY put EV yields later
            data[10] = 0x00;
            data[11] = 0x00;
            data.WriteUInt16(12, (int)heldItems[0]);
            data.WriteUInt16(14, (int)heldItems[1]);
            data[16] = genderRatio;
            data[17] = eggCycles;
            data[19] = (byte)growthType;
            // fill in egg groups
            data[20] = (byte)eggGroups[0];
            data[21] = (byte)eggGroups[1];
            // fill in abilities
            data[22] = (byte)abilities[0];
            data[23] = (byte)abilities[1];
            data[24] = safariZoneRunRate;
            data[25] = (byte)(((byte)searchColor << 1) + Convert.ToByte(flip));
            // Padding
            data[26] = 0x00;
            data[27] = 0x00;
            return data;
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
            sb.Append("\n" + moveSet.ToString());
            return sb.ToString();
        }
    }
}
