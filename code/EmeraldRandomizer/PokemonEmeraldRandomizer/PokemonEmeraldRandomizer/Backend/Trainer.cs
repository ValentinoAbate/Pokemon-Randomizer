using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer.Backend
{
    public enum TrainerClass
    {
        PKMN_TRAINER1,
        PKMN_TRAINER2,
        HIKER,
        TEAM_AQUA,
        BREEDER,
        COOLTRAINER,
        BIRD_KEEPER,
        COLLECTOR,
        SWIMMER_MALE,
        TEAM_MAGMA,
        EXPERT,
        AQUA_ADMIN,
        BLACK_BELT,
        AQUA_LEADER,
        HEX_MANIAC,
        AROMA_LADY,
        RUIN_MANIAC,
        INTERVIEWER,
        TUBER_FEMALE,
        TUBER_MALE,
        LADY,
        BEAUTY,
        RICH_BOY,
        POKEMANIAC,
        GUITARIST,
        KINDLER,
        CAMPER,
        PICKNICKER,
        BUG_MANIAC,
        PSYCHIC,
        GENTLEMAN,
        ELITE_FOUR,
        LEADER,
        SCHOOL_KID,
        SR_AND_JR,
        WINSTRATE,
        POKEFAN,
        YOUNGSTER,
        CHAMPION,
        FISHERMAN,
        TRIATHLETE,
        DRAGON_TAMER,
        NINJA_BOY,
        BATTLE_GIRL,
        PARASOL_LADY,
        SWIMMER_FEMALE,
        TWINS,
        SAILOR,
        COOLTRAINER2,
        MAGMA_ADMIN,
        PKMN_TRAINER3,
        BUG_CATCHER,
        PKMN_RANGER,
        MAGMA_LEADER,
        LASS,
        YOUNG_COUPLE,
        OLD_COUPLE,
        SIS_AND_BRO,
        SALON_MAIDEN,
        DOME_ACE,
        PALACE_MAVEN,
        ARENA_TYCOON,
        FACTORY_HEAD,
        PIKE_QUEEN,
        PYRAMID_ACE,
        PKMN_TRAINER4,
    }

    public enum Gender
    {
        Male,
        Female,
        NonBinary,
        Other,
    }

    public class Trainer
    {
        public int offset;
        public TrainerPokemon.DataType dataType;
        public TrainerPokemon[] pokemon;
        public TrainerClass trainerClass;
        public Gender gender;
        public byte musicIndex;
        public byte spriteIndex;
        public string name;
        public Item[] useItems = new Item[4];
        public bool isDoubleBattle;
        public BitArray AIFlags;

        public Trainer(byte[] rom, int offset)
        {
            dataType = (TrainerPokemon.DataType)rom[offset];
            trainerClass = (TrainerClass)rom[offset + 1];
            // Read Gender (byte 2 bit 0)
            gender = (Gender)((rom[offset + 2] & 0x80) >> 7);
            // Read music track index (byte 2 bits 1-7)
            musicIndex = (byte)(rom[offset + 2] & 0x7F);
            // Read sprite index (byte 3)
            spriteIndex = rom[offset + 3];
            // Read name (I think bytes 4 - 15?)
            name = rom.readString(offset + 4, 12);
            // Read items (bytes 16-23)
            for (int i = 0; i < 4; ++i)
                useItems[i] = (Item)rom.ReadUInt16(offset + 16 + (i * 2));
            // Read double battle (byte 24)
            isDoubleBattle = rom[offset + 24] == 1;
            // What is in bytes 25-27?
            // Read AI flags
            #region AI script flag documentation
            // Reference: https://www.pokecommunity.com/showthread.php?t=333767 (thanks Knizz and DaniilS) (may be partially incorrect)
            // Reference: https://github.com/pret/pokeemerald/blob/master/data/battle_ai_scripts.s (Thanks DizzyEggg et al.)
            // Flag #        Name (from pokeemerald)           Summary/Notes (from my observations of the pokeemerald source, may be incomplete/incorrect)
            // -------------------------------------------------------------------------------------------------------------------------------------------------
            // Flag 0      - AI_CheckBadMove                   Checks if Fissure or Horn drill would be negated, checks for soundproof negation
            // Flag 1      - AI_TryToFaint                     Tries to cause the taget to faint, favors quick attack and explosion
            // Flag 2      - AI_CheckViability                 Checks viability of MANY effects (sleep, dream eater, etc) see pokeemerald source
            // Flag 3      - AI_SetupFirstTurn                 Some moves (stat boosts, status effects, light screen, etc) get priority first turn (~70% chance)
            // -------> Effects encouraged: 
            // -------> Any stat (At/Df/SpAtk/SpDef/Sp/Acc/Evade) up/down 1/2 levels, Any special condition CONVERSION, LIGHT_SCREEN, 
            // -------> FOCUS_ENERGY, REFLECT, SUBSTITUTE, LEECH_SEED, MINIMIZE, CURSE, SWAGGER, CAMOUFLAGE, YAWN, DEFENSE_CURL, TORMENT
            // -------> Effects encouraged: FLATTER, WILL_O_WISP, INGRAIN, IMPRISON, TEETER_DANCE, TICKLE, COSMIC_POWER, BULK_UP, CALM_MIND
            //
            // Flag 4      - AI_Risky                          Risky effects (Sleep, explosion, focus punch, etc) get priority (~50% chance)
            // -------> Effects encouraged:
            // -------> SLEEP, EXPLOSION, MIRROR_MOVE, OHKO, HIGH_CRITICAL, CONFUSE, METRONOME, PSYWAVE, COUNTER, DESTINY_BOND, SWAGGER
            // -------> ATTRACT, PRESENT, ALL_STATS_UP_HIT, BELLY_DRUM, MIRROR_COAT, FOCUS_PUNCH, REVENGE, TEETER_DANCE
            //
            // Flag 5      - AI_PreferStrongestMove            Seems to actually give priority to non-damaging moves (~40% chance)
            // Flag 6      - AI_PreferBatonPass                If has baton pass - tries to buff stats or use protect/detect until stats are high, then passes
            // Flag 7      - AI_DoubleBattle                   Favors double battle combos
            // -------> Effects encouraged: 
            // -------> earthquake/magnitude (if partner has fly/levitate, avoids if ally weak to ground), flash fire, lightning rod/Ground immunity combos,
            // -------> skill swap for allies w/Truant and enemies w/ huge power and shadow tag, something w/ guts?, helping hand
            //
            // Flag 8      - AI_HPAware                        discourages certain effects at different self and enemy Hp levels
            // -------> Effects discouraged:
            // -------> If self HP High : EXPLOSION, RESTORE_HP, REST, DESTINY_BOND, FLAIL, ENDURE, MORNING_SUN, SYNTHESIS, MOONLIGHT, SOFTBOILED, MEMENTO, GRUDGE, OVERHEAT
            // -------> If self HP med  : Any stat up/down 1/2 lvls, EXPLOSION, BIDE, CONVERSION, LIGHT_SCREEN, MIST, FOCUS_ENERGY, CONVERSION_2, SAFEGUARD, BELLY_DRUM, TICKLE, COSMIC_POWER, BULK_UP, CALM_MIND, DRAGON_DANCE
            // -------> If self HP low  : seld HP med list - EXPLOSION + RAGE, LOCK_ON, PSYCH_UP, MIRROR_COAT, SOLARBEAM, ERUPTION
            // -------> If enemy HP high: None
            // -------> If enemy HP med : Any stat up/down 1/2 lvls, MIST, FOCUS_ENERGY, POISON, PAIN_SPLIT, PERISH_SONG, SAFEGUARD, TICKLE, COSMIC_POWER, BULK_UP, CALM_MIND, DRAGON_DANCE
            // -------> If enemy HP low : enemy HP med list + All special conditions (icl TOXIC), EXPLOSION, BIDE, CONVERSION, LIGHT_SCREEN, OHKO, SUPER_FANG, MIST, FOCUS_ENERGY
            // -------> If enemy HP low : CONVERSION_2, LOCK_ON, SPITE, SWAGGER, FURY_CUTTER, ATTRACT, PSYCH_UP, MIRROR_COAT, WILL_O_WISP\
            //
            // Flag 9      - AI_Unknown (broken/unfinished)
            // Flags 10-28 - Unused                            Can be customized and replaced with hacks
            // Flag 29     - AI_Roaming                        Roaming pkmn AI pokemon attempts to flee if able (not used for trainers)
            // Flag 30     - AI_Safari                         Safari Zone AI pokemon may flee (not used for trainers)
            // Flag 31     - AI_FirstBattle                    Run if at low hp (not used for trainers, unused)
            #endregion
            AIFlags = new BitArray(new int[] { rom.ReadUInt32(offset + 28) });
            int numPokemon = rom[offset + 32];
            // What is in bytes 33-35?
            int pokemonPtr = rom.ReadPointer(offset + 36);

            #region Read pokemon from pokemonPtr
            pokemon = new TrainerPokemon[numPokemon];
            // The pokemon data structures will be either 8 or 16 bytes depending on the dataType of the trainer
            int pkmnDataBytes = (dataType == TrainerPokemon.DataType.Basic || dataType == TrainerPokemon.DataType.HeldItem) ? 8 : 16;
            for (int i = 0; i < numPokemon; ++i)
            {
                TrainerPokemon p = new TrainerPokemon();
                int ptr = pokemonPtr + (i * pkmnDataBytes);
                p.dataType = dataType;
                p.IVLevel = rom.ReadUInt16(ptr);
                p.level = rom.ReadUInt16(ptr + 2);
                p.species = (PokemonSpecies)rom.ReadUInt16(ptr + 4);
                if (dataType == TrainerPokemon.DataType.HeldItem || dataType == TrainerPokemon.DataType.SpecialMovesAndHeldItem)
                    p.heldItem = (Item)rom.ReadUInt16(ptr + 6);
                if(dataType == TrainerPokemon.DataType.SpecialMoves || dataType == TrainerPokemon.DataType.SpecialMovesAndHeldItem)
                {
                    int moveStartAddy = dataType == TrainerPokemon.DataType.SpecialMoves ? 6 : 8;
                    for (int j = 0; j < 4; ++j)
                        p.moves[j] = (Move)rom.ReadUInt16(ptr + moveStartAddy + (j * 2));
                }
                pokemon[i] = p;
            }
            #endregion
        }

        public override string ToString()
        {
            return trainerClass.ToDisplayString() + " " + name;
        }

        //Commented code below from Dabomstew's Universal Randomizer source. Thanks Dabomstew!

        //TrainerTable=310030
        //NumberOfTrainers=854
        //TrainerClasses=30FCD4
        //NumberOfTrainerClasses = 66
        //TrainerImageTable=305654
        //NumberOfTrainerImages=92
        //TrainerPaletteTable=30593C
        //TrainerMoneyTable = 31AEB8

        //public List<String> getTrainerClassNames()
        //{
        //    int baseOffset = romEntry.getValue("TrainerClassNames");
        //    int amount = romEntry.getValue("TrainerClassCount");
        //    int length = romEntry.getValue("TrainerClassNameLength");
        //    List<String> trainerClasses = new ArrayList<String>();
        //    for (int i = 0; i < amount; i++)
        //    {
        //        trainerClasses.add(readVariableLengthString(baseOffset + i * length));
        //    }
        //    return trainerClasses;
        //}

        //        TrainerData=0x310030
        //TrainerEntrySize=40
        //TrainerCount=0x357
        //TrainerClassNames=0x30FCD4
        //TrainerClassCount=66
        //TrainerClassNameLength=13
        //TrainerNameLength=12
        //DoublesTrainerClasses=[34, 46, 55, 56, 57]


        // Gym Trainers
        //tag(trs, "GYM1", 0x140, 0x141, 0x23B);
        //tag(trs, "GYM2", 0x1AA, 0x1A9, 0xB3, 0x23C, 0x23D, 0x23E);
        //tag(trs, "GYM3", 0xBF, 0x143, 0xC2, 0x289, 0x322);
        //tag(trs, "GYM4", 0x288, 0xC9, 0xCB, 0x28A, 0xCA, 0xCC, 0x1F5, 0xCD);
        //tag(trs, "GYM5", 0x47, 0x59, 0x49, 0x5A, 0x48, 0x5B, 0x4A);
        //tag(trs, "GYM6", 0x192, 0x28F, 0x191, 0x28E, 0x194, 0x323);
        //tag(trs, "GYM7", 0xE9, 0xEA, 0xEB, 0xF4, 0xF5, 0xF6, 0x24F, 0x248, 0x247, 0x249, 0x246, 0x23F);
        //tag(trs, "GYM8", 0x265, 0x80, 0x1F6, 0x73, 0x81, 0x76, 0x82, 0x12D, 0x83, 0x266);

        //// Gym Leaders + Emerald Rematches!
        //tag(trs, "GYM1", 0x109, 0x302, 0x303, 0x304, 0x305);
        //tag(trs, "GYM2", 0x10A, 0x306, 0x307, 0x308, 0x309);
        //tag(trs, "GYM3", 0x10B, 0x30A, 0x30B, 0x30C, 0x30D);
        //tag(trs, "GYM4", 0x10C, 0x30E, 0x30F, 0x310, 0x311);
        //tag(trs, "GYM5", 0x10D, 0x312, 0x313, 0x314, 0x315);
        //tag(trs, "GYM6", 0x10E, 0x316, 0x317, 0x318, 0x319);
        //tag(trs, "GYM7", 0x10F, 0x31A, 0x31B, 0x31C, 0x31D);
        //tag(trs, "GYM8", 0x110, 0x31E, 0x31F, 0x320, 0x321);

        //// Elite 4
        //tag(trs, 0x105, "ELITE1");
        //tag(trs, 0x106, "ELITE2");
        //tag(trs, 0x107, "ELITE3");
        //tag(trs, 0x108, "ELITE4");
        //tag(trs, 0x14F, "CHAMPION");

        //// Brendan
        //tag(trs, 0x208, "RIVAL1-2");
        //tag(trs, 0x20B, "RIVAL1-0");
        //tag(trs, 0x20E, "RIVAL1-1");

        //tag(trs, 0x251, "RIVAL2-2");
        //tag(trs, 0x250, "RIVAL2-0");
        //tag(trs, 0x257, "RIVAL2-1");

        //tag(trs, 0x209, "RIVAL3-2");
        //tag(trs, 0x20C, "RIVAL3-0");
        //tag(trs, 0x20F, "RIVAL3-1");

        //tag(trs, 0x20A, "RIVAL4-2");
        //tag(trs, 0x20D, "RIVAL4-0");
        //tag(trs, 0x210, "RIVAL4-1");

        //tag(trs, 0x295, "RIVAL5-2");
        //tag(trs, 0x296, "RIVAL5-0");
        //tag(trs, 0x297, "RIVAL5-1");

        //// May
        //tag(trs, 0x211, "RIVAL1-2");
        //tag(trs, 0x214, "RIVAL1-0");
        //tag(trs, 0x217, "RIVAL1-1");

        //tag(trs, 0x258, "RIVAL2-2");
        //tag(trs, 0x300, "RIVAL2-0");
        //tag(trs, 0x301, "RIVAL2-1");

        //tag(trs, 0x212, "RIVAL3-2");
        //tag(trs, 0x215, "RIVAL3-0");
        //tag(trs, 0x218, "RIVAL3-1");

        //tag(trs, 0x213, "RIVAL4-2");
        //tag(trs, 0x216, "RIVAL4-0");
        //tag(trs, 0x219, "RIVAL4-1");

        //tag(trs, 0x298, "RIVAL5-2");
        //tag(trs, 0x299, "RIVAL5-0");
        //tag(trs, 0x29A, "RIVAL5-1");

        //// Themed
        //tag(trs, "THEMED:MAXIE", 0x259, 0x25A, 0x2DE);
        //tag(trs, "THEMED:TABITHA", 0x202, 0x255, 0x2DC);
        //tag(trs, "THEMED:ARCHIE", 0x22);
        //tag(trs, "THEMED:MATT", 0x1E);
        //tag(trs, "THEMED:SHELLY", 0x20, 0x21);

        //// Steven
        //tag(trs, 0x324, "UBER");
    }
}
