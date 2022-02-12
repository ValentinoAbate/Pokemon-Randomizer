using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.DataStructures.Scripts
{
    public class TrainerBattleCommand : Command
    {
        // Documentation: https://www.pokecommunity.com/showpost.php?p=9844310&postcount=32
        public enum Type
        {
            Standard,                     // 0x00 word-trainerID word-unknown @EncounterText @DefeatText
            GymLeader,                    // 0x01 word-trainerID word-unknown @EncounterText @DefeatText @PostBattleScript
            MatchCallRegister,            // 0x02 word-trainerID word-unknown @EncounterText @DefeatText @PostBattleScript (Nav Register Pointer)
            ContinueScriptAfterBattle,    // 0x03 word-trainerID word-unknown @DefeatText
            DoubleBattle,                 // 0x04 word-trainerID word-unknown @EncounterText @DefeatText @OnlyOnePokemonText
            Rematch,                      // 0x05 word-trainerID word-unknown @EncounterText @DefeatText (VS Seeker in FRLG, Match Call in RSE)
            DoubleBattleWithExtraScript,  // 0x06 word-trainerID word-unknown @EncounterText @DefeatText @OnlyOnePokemonText @PostBattleScript (Gabby & Ty, etc.)
            DoubleBattleRematch,          // 0x07 word-trainerID word-unknown @EncounterText @DefeatText @OnlyOnePokemonText
            DoubleBattleGymLeader,        // 0x08 word-trainerID word-unknown @EncounterText @DefeatText @OnlyOnePokemonText @PostBattleScript
            ProfessorOakTutorial,         // 0x09 word-trainerID word-unknown @DefeatText @WinText
        }
        public Type trainerType;
        public int trainerIndex;
        public int unknown; // Not sure what this does, seems to be used in flannery's gym
        public int encounterTextOffset;
        public int defeatedTextOffset;
        public int onlyOnePokemonTextOffset;
        public int winTextOffset; // Only used by Type.ProfessorOakTutorial if the rival wins
        public int postBattleScriptOffset;
        public Script postBattleScript;

        public override string ToString()
        {
            return $"Trainer Battle ({trainerType}: {trainerIndex:x2})";
        }
    }
}
