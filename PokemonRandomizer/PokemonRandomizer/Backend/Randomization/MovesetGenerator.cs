using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.Backend.Randomization
{
    public static class MovesetGenerator
    {
        public static Move[] DefaultMoveset(PokemonBaseStats pokemon, int level)
        {
            var learnSet = pokemon.learnSet;
            var moves = new Stack<Move>();
            foreach(var item in learnSet)
            {
                if (item.learnLvl > level)
                    break;
                moves.Push(item.move);
            }
            while (moves.Count < 4)
                moves.Push(Move.None);
            return new Move[] { moves.Pop(), moves.Pop(), moves.Pop(), moves.Pop() };
        }

        public static Move[] SmartMoveSet(Random rand, RomData data, PokemonBaseStats pokemon, int level)
        {
            MoveData GetData(LearnSet.Entry e) => data.MoveData[(int)e.move];
            int PowerComparison(LearnSet.Entry e1, LearnSet.Entry e2) => GetData(e2).power.CompareTo(GetData(e1).power);
            // Initialize move choices
            List<LearnSet.Entry> availableMoves = new List<LearnSet.Entry>();
            availableMoves.AddRange(pokemon.learnSet.Where((e) => e.learnLvl <= level));
            // Add movesets from evolved from
            for (var pkmn = pokemon; pkmn.evolvesFrom.Count > 0; pkmn = data.PokemonLookup[pkmn.evolvesFrom[0].Pokemon])
            {
                availableMoves.AddRange(pkmn.learnSet.Where((e) => e.learnLvl <= level));
            }
            // Remove duplicate moves
            availableMoves = availableMoves.Distinct().ToList();
            //var highestPower = availableMoves.Max((m) => data.MoveData[(int)m].power);
            //for (int i = 0; i < pokemon.TMCompat.Length; ++i)
            //{
            //    if (pokemon.TMCompat.Get(i) && data.MoveData[(int)data.TMMoves[i]].power <= highestPower)
            //        availableMoves.Add(data.TMMoves[i]);
            //}

            // Initialize returns
            Move[] ret = new Move[4] { Move.None, Move.None, Move.None, Move.None };
            if (availableMoves.Count <= 0)
                return ret;

            IEnumerable<LearnSet.Entry> GetNonStabMoves() => availableMoves.Where((e) => GetData(e).power > 0 && !pokemon.types.Contains(GetData(e).type));
            IEnumerable<LearnSet.Entry> GetZeroPowerMoves() => availableMoves.Where((e) => GetData(e).power <= 0);
            float PowerWeightScale(LearnSet.Entry e) => (float)Math.Pow(GetData(e).power, 2);
            float LevelWeightScale(LearnSet.Entry e) => (float)Math.Pow(e.learnLvl, 2);
            // Choose first move

            // Try and find the most powerful STAB move available for type 1
            var type1StabMoves = availableMoves.Where((e) => GetData(e).power > 0 && GetData(e).type == pokemon.types[0]).ToList();
            var nonStabMoves =  new WeightedSet<LearnSet.Entry>(GetNonStabMoves(), PowerWeightScale);
            var noPowerMoves = new WeightedSet<LearnSet.Entry>(GetZeroPowerMoves(), LevelWeightScale);
            if (type1StabMoves.Count > 0)
            {
                type1StabMoves.Sort(PowerComparison);
                ret[0] = type1StabMoves[0].move;
            }
            else
            {
                // Choose the highest powered non-stab move
                if (nonStabMoves.Count > 0)
                {
                    ret[0] = rand.Choice(nonStabMoves).move;
                }
                else
                {
                    ret[0] = rand.Choice(availableMoves).move;
                }
            }
            availableMoves.RemoveAll((e) => e.move == ret[0]);
            if (availableMoves.Count <= 0)
                return ret;
            nonStabMoves = new WeightedSet<LearnSet.Entry>(GetNonStabMoves(), PowerWeightScale);
            noPowerMoves = new WeightedSet<LearnSet.Entry>(GetZeroPowerMoves(), LevelWeightScale);
            // Choose second move

            if (pokemon.IsSingleTyped)
            {
                // Choose the highest powered non-stab move
                if (nonStabMoves.Count > 0)
                {
                    ret[1] = rand.Choice(nonStabMoves).move;
                }
                else
                {
                    ret[1] = rand.Choice(availableMoves).move;
                }
            }
            else // Dual-Typed
            {
                // Try and find the most powerful STAB move available for type 2
                var type2StabMoves = availableMoves.Where((e) => GetData(e).power > 0 && GetData(e).type == pokemon.types[1]).ToList();
                if(type2StabMoves.Count > 0)
                {
                    type2StabMoves.Sort(PowerComparison);
                    ret[1] = type2StabMoves[0].move;
                }
                else
                {
                    // Choose the highest powered non-stab move
                    if (nonStabMoves.Count > 0)
                    {
                        ret[1] = rand.Choice(nonStabMoves).move;
                    }
                    else
                    {
                        ret[1] = rand.Choice(availableMoves).move;
                    }
                }
            }
            availableMoves.RemoveAll((e) => e.move == ret[1]);
            if (availableMoves.Count <= 0)
                return ret;
            nonStabMoves = new WeightedSet<LearnSet.Entry>(GetNonStabMoves(), PowerWeightScale);
            noPowerMoves = new WeightedSet<LearnSet.Entry>(GetZeroPowerMoves(), LevelWeightScale);
            // Choose third move

            if (!pokemon.IsSingleTyped && nonStabMoves.Count > 0 && nonStabMoves.Max((e) => GetData(e.Key).power) >= 60)
            {
                ret[2] = rand.Choice(nonStabMoves).move;
            }
            else if (noPowerMoves.Count > 0)
            {
                ret[2] = rand.Choice(noPowerMoves).move;
            }
            else
            {
                ret[2] = rand.Choice(availableMoves).move;
            }
            availableMoves.RemoveAll((e) => e.move == ret[2]);
            if (availableMoves.Count <= 0)
                return ret;
            nonStabMoves = new WeightedSet<LearnSet.Entry>(GetNonStabMoves(), PowerWeightScale);
            noPowerMoves = new WeightedSet<LearnSet.Entry>(GetZeroPowerMoves(), LevelWeightScale);
            // Choose fourth move

            if (noPowerMoves.Count > 0)
            {
                ret[3] = rand.Choice(noPowerMoves).move;
            }
            else
            {
                ret[3] = rand.Choice(availableMoves).move;
            }

            return ret;
        }
    }
}
