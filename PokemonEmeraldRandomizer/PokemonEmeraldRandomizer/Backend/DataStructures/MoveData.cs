using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer.Backend
{
    public class MoveData
    {
        public enum Type
        {
            Physical,
            Special,
            Status,
        }

        public enum MoveEffect
        {
            //Need to do science here
        }

        public enum Targets
        {
            SelectedTarget = 0,
            Special = 1, // Specific to certain attacks (counter is enemy, mtronone could hit anything)
            Unused = 2,
            Random = 4,
            BothEnemies = 8,
            Self = 16,
            EnemiesAndPartner = 32,
            OpponentField = 64, // For moves like spikes
        }

        // Data structure documentation: https://bulbapedia.bulbagarden.net/wiki/Move_data_structure_in_Generation_III

        public Move move;
        public MoveEffect effect;
        public int power;
        public PokemonType type;
        public int accuracy;
        public int pp;
        public int effectChance;
        public Targets targets;
        // This byte is signed, If it is strictly less than 0x80 (128) then it is positive. 
        // If not, the actual value equals: -1 * (256 - Current Value). Example: value 0xFE (254) must be treated as -2 instead.
        public int priority;
        // Flags documentation
        public bool Contact { get => flags[0]; }               // 0 - This moves makes contact with the target.
        public bool AffectedByProtect { get => flags[1]; }     // 1 - This move is affected by Protect.
        public bool AffectedByMagicCoat { get => flags[2]; }   // 2 - This move is affected by Magic Coat.
        public bool AffectedBySnatch { get => flags[3]; }      // 3 - This move is affected by Snatch.Note that this is mutually-exclusive with flag 2.
        public bool CanBeUsedByMirrorMove { get => flags[4]; } // 4 - This move may be used with Mirror Move
        public bool AffectedByKingRock { get => flags[5]; }    // 5 - This move is affected by the effects of King's Rock. The flinch effect is considered an additional effect for the purposes of Shield Dust, but not Serene Grace
        public BitArray flags;
        
        // Reads a MoveData from the rom's internal offset
        public MoveData(Rom rom)
        {
            effect = (MoveEffect)rom.ReadByte();
            power = rom.ReadByte();
            type = (PokemonType)rom.ReadByte();
            accuracy = rom.ReadByte();
            pp = rom.ReadByte();
            effectChance = rom.ReadByte();
            targets = (Targets)rom.ReadByte();
            priority = rom.ReadByte();
            flags = new BitArray(new byte[]{ rom.ReadByte() });
            rom.Skip(3);
        }

        public override string ToString()
        {
            return move.ToDisplayString();
        }
    }
}
