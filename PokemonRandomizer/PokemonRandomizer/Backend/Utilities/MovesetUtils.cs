using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using System;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.Utilities
{
    public static class MovesetUtils
    {
        private static bool HasAttackingMoveHelper(IReadOnlyCollection<Move> moveset, MoveData.Type category, IDataTranslator dataT)
        {
            foreach (var move in moveset) 
            {
                if(move == Move.None)
                {
                    continue;
                }
                var moveData = dataT.GetMoveData(move);
                if(moveData.AffectedByAttackingStat && moveData.MoveCategory == category) 
                {
                    return true;
                }
            }
            return false;
        }

        public static bool HasSpecialMove(IReadOnlyCollection<Move> moveset, IDataTranslator dataT)
        {
            return HasAttackingMoveHelper(moveset, MoveData.Type.Special, dataT);
        }

        public static bool HasPhysicalMove(IReadOnlyCollection<Move> moveset, IDataTranslator dataT)
        {
            return HasAttackingMoveHelper(moveset, MoveData.Type.Physical, dataT);
        }
    }
}
