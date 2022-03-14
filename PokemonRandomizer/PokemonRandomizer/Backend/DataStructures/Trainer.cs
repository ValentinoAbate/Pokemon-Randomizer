using PokemonRandomizer.Backend.Metadata;
using PokemonRandomizer.Backend.EnumTypes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PokemonRandomizer.Backend.Utilities.Repointing;

namespace PokemonRandomizer.Backend.DataStructures
{
    public enum Gender
    {
        Male,
        Female,
        NonBinary,
        Other,
    }

    public class Trainer
    {
        public const int maxPokemon = 6;
        public enum Category
        {
            Trainer,
            AceTrainer,
            Rival,
            TeamGrunt,
            TeamAdmin,
            TeamLeader,
            GymLeader,
            GymTrainer,
            EliteFour,
            Champion,
            CatchingTutTrainer, // Wally, Etc
            SpecialBoss, // Post-game steven in emerald, Red, etc
        }

        public static int AverageLevelComparer(Trainer t1, Trainer t2) => t1.AvgLvl.CompareTo(t2.AvgLvl);
        public bool Invalid => string.IsNullOrWhiteSpace(name) || name == nullName;

        public const string nullName = "??????";
        public const int nameLength = 12;
        // The all of the class names (mostly for debugging)
        public List<string> classNames;
        public string Class => trainerClass < classNames.Count ? classNames[trainerClass] : nullName;
        public double AvgLvl => Pokemon.Count > 0 ? Pokemon.Average((p) => p.level) : 0;

        public Category TrainerCategory { get; set; }
        public TrainerThemeData ThemeData { get; set; }

        public int offset;
        public int trainerClass;
        public Gender gender;
        public byte musicIndex;
        public byte spriteIndex;
        public string name;
        public Item[] useItems = new Item[4];
        public bool isDoubleBattle;
        public BitArray AIFlags;

        public List<TrainerPokemon> Pokemon => PokemonData.Pokemon;
        public TrainerPokemon.DataType DataType
        {
            get => PokemonData.DataType;
            set => PokemonData.DataType = value;
        } 
        public TrainerPokemonData PokemonData { get; private set; }

        public int pokemonOffset;

        // Read a trainer from the rom's internal offset
        public Trainer(Rom rom, List<string> classNames)
        {
            this.classNames = classNames;
            var dataType = (TrainerPokemon.DataType)rom.ReadByte();
            trainerClass = rom.ReadByte();
            // Read Gender (byte 2 bit 0)
            gender = (Gender)((rom.Peek() & 0x80) >> 7);
            // Read music track index (byte 2 bits 1-7)
            musicIndex = (byte)(rom.ReadByte() & 0x7F);
            // Read sprite index (byte 3)
            spriteIndex = rom.ReadByte();
            // Read name (I think bytes 4 - 15?)
            name = rom.ReadFixedLengthString(nameLength);
            // Read items (bytes 16-23)
            for (int i = 0; i < 4; ++i)
                useItems[i] = (Item)rom.ReadUInt16();
            // Read double battle (byte 24)
            isDoubleBattle = rom.ReadByte() == 1;
            // What is in bytes 25-27?
            rom.Skip(3);
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
            // -------> If self HP low  : self HP med list - EXPLOSION + RAGE, LOCK_ON, PSYCH_UP, MIRROR_COAT, SOLARBEAM, ERUPTION
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
            AIFlags = new BitArray(new int[] { rom.ReadUInt32() });
            int numPokemon = rom.ReadByte();
            // What is in bytes 33-35?
            rom.Skip(3);
            // Bytes 36-39 (end of data)
            pokemonOffset = rom.ReadPointer();
            // Save the internal offset before chasing pointers
            rom.SaveOffset();

            #region Read pokemon from pokemonOffset
            rom.Seek(pokemonOffset);
            PokemonData = new TrainerPokemonData(dataType, numPokemon);
            // The pokemon data structures will be either 8 or 16 bytes depending on the dataType of the trainer
            for (int i = 0; i < numPokemon; ++i)
            {
                var p = new TrainerPokemon();
                p.dataType = dataType;
                p.IVLevel = rom.ReadUInt16();
                p.level = rom.ReadUInt16();
                p.species = (Pokemon)rom.ReadUInt16();
                if (dataType == TrainerPokemon.DataType.Basic)
                    rom.Skip(2); // Skip padding
                else if (dataType == TrainerPokemon.DataType.HeldItem)
                    p.heldItem = (Item)rom.ReadUInt16();
                else if (dataType == TrainerPokemon.DataType.SpecialMoves)
                {
                    for (int j = 0; j < 4; ++j)
                        p.moves[j] = (Move)rom.ReadUInt16();
                    rom.Skip(2);
                }
                else if (dataType == TrainerPokemon.DataType.SpecialMovesAndHeldItem)
                {
                    p.heldItem = (Item)rom.ReadUInt16();
                    for (int j = 0; j < 4; ++j)
                        p.moves[j] = (Move)rom.ReadUInt16();
                }                   
                PokemonData.Pokemon.Add(p);
            }
            #endregion
            // Return to the trainers
            rom.LoadOffset();
        }

        public void EnsureSafeBattleType()
        {
            if(Pokemon.Count <= 1)
            {
                isDoubleBattle = false;
            }
        }

        public override string ToString()
        {
            return $"{Class} {name} ({TrainerCategory})";
        }

        public class TrainerPokemonData : IRepointable
        {
            public bool NeedsRepoint => DataType != originalDataType || Pokemon.Count > originalPokemonCount;
            public TrainerPokemon.DataType DataType 
            { 
                get => dataType;
                set
                {
                    if (dataType == value)
                        return;
                    dataType = value;
                    foreach(var pokemon in Pokemon)
                    {
                        pokemon.dataType = value;
                    }
                }
            }
            private TrainerPokemon.DataType dataType;
            private readonly TrainerPokemon.DataType originalDataType; 
            public List<TrainerPokemon> Pokemon { get; }
            private readonly int originalPokemonCount;
            public TrainerPokemonData(TrainerPokemon.DataType dataType, int numPokemon)
            {
                Pokemon = new List<TrainerPokemon>(numPokemon);
                originalPokemonCount = numPokemon;
                this.dataType = dataType;
                originalDataType = dataType;
            }
        }

    }
}
