using PokemonRandomizer.Backend.Metadata;
using PokemonRandomizer.Backend.EnumTypes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PokemonRandomizer.Backend.Utilities.Repointing;

namespace PokemonRandomizer.Backend.DataStructures.Trainers
{
    public enum Gender
    {
        Male,
        Female,
        NonBinary,
        Other,
    }

    public class NormalTrainer : Trainer, IHasTrainerAI
    {
        // The all of the class names (mostly for debugging)
        public IReadOnlyList<string> ClassNames { get; set; }
        public override string Class => ClassNames != null && trainerClass < ClassNames.Count ? ClassNames[trainerClass] : nullName;


        public Gender gender;
        public byte musicIndex;
        public byte spriteIndex;
        public override string Name { get; set; }
        public Item[] useItems = new Item[4];
        public override bool IsDoubleBattle { get; set; }
        #region AI script flag documentation (Gen III)
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
        public BitArray AIFlags { get; set; }

        public override string ToString()
        {
            return $"{Class} {Name} ({TrainerCategory})";
        }
    }
}
