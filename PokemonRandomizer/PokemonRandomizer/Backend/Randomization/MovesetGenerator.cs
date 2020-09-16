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
            foreach (var item in learnSet)
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
            MoveData GetData(Move m) => data.MoveData[(int)m];
            bool IsStab(Move m) => pokemon.types.Contains(GetData(m).type);
            // Initialize move choices
            var availableMoves = new Dictionary<Move, int>();
            foreach(var entry in pokemon.learnSet.Where((e) => e.learnLvl <= level))
            {
                var move = entry.move;
                var learnLevel = entry.learnLvl;
                if (availableMoves.ContainsKey(move))
                {
                    if (availableMoves[move] < learnLevel)
                        availableMoves[move] = learnLevel;
                }
                else
                {
                    availableMoves.Add(move, learnLevel);
                }
            }
            // Add movesets from evolved from
            for (var pkmn = pokemon; pkmn.evolvesFrom.Count > 0; pkmn = data.PokemonLookup[pkmn.evolvesFrom[0].Pokemon])
            {
                foreach(var entry in pkmn.learnSet.Where((e) => e.learnLvl <= level))
                {
                    var move = entry.move;
                    var learnLevel = entry.learnLvl;
                    if (availableMoves.ContainsKey(move))
                    {
                        if(availableMoves[move] < learnLevel)
                            availableMoves[move] = learnLevel;
                    }
                    else
                    {
                        availableMoves.Add(move, learnLevel);
                    }
                }
            }

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
            IEnumerable<Move> GetAttackMoves() => availableMoves.Keys.Where((m) => GetData(m).power > 0);
            //IEnumerable<LearnSet.Entry> GetNonStabMoves() => GetAttackMoves().Where((e) => !pokemon.types.Contains(GetData(e).type));
            IEnumerable<Move> GetZeroPowerMoves() => availableMoves.Keys.Where((m) => GetData(m).power <= 0);
            float PowerWeightScale(Move e) => (float)Math.Pow(GetData(e).EffectivePower * (IsStab(e) ? 1.5 : 1), 3);
            float RedundantTypeFactor(Move m)
            {
                int ind = ret.ToList().FindIndex((m2) => m2 != Move.None && GetData(m).type == GetData(m2).type);
                return ind == -1 ? 1 : 0.25f;

            }
            float TypeWeightScale(Move m) => (IsStab(m) ? 2f : 1) * RedundantTypeFactor(m);
            float LevelWeightScale(Move e) => (float)Math.Pow(availableMoves[e], 2);
            float LevelWeightScaleLinear(Move e) => Math.Max(1, availableMoves[e] / 4);
            float LevelWeightScaleLog(Move e) => (float)Math.Max(1, Math.Log(availableMoves[e]));
            Move FallbackMoveChoice() => rand.Choice(new WeightedSet<Move>(availableMoves.Keys, LevelWeightScale));

            // Choose first move - attempt to choose an attack move
            var attackMoves = new WeightedSet<Move>(GetAttackMoves(), (e) => PowerWeightScale(e) * TypeWeightScale(e) * LevelWeightScaleLog(e));
            if (attackMoves.Count > 0)
            {
                ret[0] = rand.Choice(attackMoves);
            }
            else
            {
                ret[0] = FallbackMoveChoice();
            }
            availableMoves.Remove(ret[0]);
            if (availableMoves.Count <= 0)
                return ret;

            // Choose second move - attempt to choose another attack move
            attackMoves = new WeightedSet<Move>(GetAttackMoves(), (e) => PowerWeightScale(e) * TypeWeightScale(e) * LevelWeightScaleLog(e));
            if (attackMoves.Count > 0)
            {
                ret[1] = rand.Choice(attackMoves);
            }
            else
            {
                ret[1] = FallbackMoveChoice();
            }
            availableMoves.Remove(ret[1]);
            if (availableMoves.Count <= 0)
                return ret;

            // Choose third move - Attempt to choose a status move
            var noPowerMoves = new WeightedSet<Move>(GetZeroPowerMoves(), LevelWeightScale);
            if (noPowerMoves.Count > 0)
            {
                ret[2] = rand.Choice(noPowerMoves);
            }
            else
            {
                ret[2] = FallbackMoveChoice();
            }
            availableMoves.Remove(ret[2]);
            if (availableMoves.Count <= 0)
                return ret;

            // Choose fourth move
            ret[3] = FallbackMoveChoice();

            return ret;
        }
    }
}
