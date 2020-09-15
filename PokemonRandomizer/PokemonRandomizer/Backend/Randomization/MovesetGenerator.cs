using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
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
            // Initialize move choices
            List<Move> availableMoves = new List<Move>();
            availableMoves.AddRange(pokemon.learnSet.Where((e) => e.learnLvl <= level).Select((e) => e.move));
            // Add movesets from evolved from
            for (var pkmn = pokemon; pkmn.evolvesFrom.Count > 0; pkmn = data.PokemonLookup[pkmn.evolvesFrom[0].Pokemon])
            {
                availableMoves.AddRange(pkmn.learnSet.Where((e) => e.learnLvl <= level).Select((e) => e.move));
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


            // Choose first move

            // Try and find the most powerful STAB move available for type 1
            var type1StabMoves = availableMoves.Where((move) => data.MoveData[(int)move].type == pokemon.types[0]).ToList();
            var nonStabMoves = availableMoves.Where((move) => !pokemon.types.Contains(data.MoveData[(int)move].type)).ToList();
            nonStabMoves.Sort((m1, m2) => data.MoveData[(int)m2].power.CompareTo(data.MoveData[(int)m1].power));
            var noPowerMoves = availableMoves.Where((move) => data.MoveData[(int)move].power <= 0).ToList();
            if (type1StabMoves.Count > 0)
            {
                type1StabMoves.Sort((m1, m2) => data.MoveData[(int)m2].power.CompareTo(data.MoveData[(int)m1].power));
                ret[0] = type1StabMoves[0];
            }
            else
            {
                // Choose the highest powered non-stab move
                if (nonStabMoves.Count > 0)
                {
                    ret[0] = nonStabMoves[0];
                }
                else
                {
                    ret[0] = rand.Choice(availableMoves);
                }
            }
            availableMoves.Remove(ret[0]);
            if (availableMoves.Count <= 0)
                return ret;
            nonStabMoves = availableMoves.Where((move) => !pokemon.types.Contains(data.MoveData[(int)move].type)).ToList();
            nonStabMoves.Sort((m1, m2) => data.MoveData[(int)m2].power.CompareTo(data.MoveData[(int)m1].power));
            noPowerMoves = availableMoves.Where((move) => data.MoveData[(int)move].power <= 0).ToList();
            // Choose second move

            if (pokemon.IsSingleTyped)
            {
                // Choose the highest powered non-stab move
                if (nonStabMoves.Count > 0)
                {
                    ret[1] = nonStabMoves[0];
                }
                else
                {
                    ret[1] = rand.Choice(availableMoves);
                }
            }
            else // Dual-Typed
            {
                // Try and find the most powerful STAB move available for type 2
                var type2StabMoves = availableMoves.Where((move) => data.MoveData[(int)move].type == pokemon.types[1]).ToList();
                if(type2StabMoves.Count > 0)
                {
                    type2StabMoves.Sort((m1, m2) => data.MoveData[(int)m2].power.CompareTo(data.MoveData[(int)m1].power));
                    ret[1] = type2StabMoves[0];
                }
                else
                {
                    // Choose the highest powered non-stab move
                    if (nonStabMoves.Count > 0)
                    {
                        ret[1] = nonStabMoves[0];
                    }
                    else
                    {
                        ret[1] = rand.Choice(availableMoves);
                    }
                }
            }
            availableMoves.Remove(ret[1]);
            if (availableMoves.Count <= 0)
                return ret;
            nonStabMoves = availableMoves.Where((move) => !pokemon.types.Contains(data.MoveData[(int)move].type)).ToList();
            nonStabMoves.Sort((m1, m2) => data.MoveData[(int)m2].power.CompareTo(data.MoveData[(int)m1].power));
            noPowerMoves = availableMoves.Where((move) => data.MoveData[(int)move].power <= 0).ToList();
            // Choose third move

            if (!pokemon.IsSingleTyped && nonStabMoves.Count > 0 && data.MoveData[(int)nonStabMoves[0]].power >= 60)
            {
                ret[2] = nonStabMoves[0];
            }
            else if (noPowerMoves.Count > 0)
            {
                ret[2] = rand.Choice(noPowerMoves);
            }
            else
            {
                ret[2] = rand.Choice(availableMoves);
            }
            availableMoves.Remove(ret[2]);
            if (availableMoves.Count <= 0)
                return ret;
            nonStabMoves = availableMoves.Where((move) => !pokemon.types.Contains(data.MoveData[(int)move].type)).ToList();
            nonStabMoves.Sort((m1, m2) => data.MoveData[(int)m2].power.CompareTo(data.MoveData[(int)m1].power));
            noPowerMoves = availableMoves.Where((move) => data.MoveData[(int)move].power <= 0).ToList();
            // Choose fourth move

            if (noPowerMoves.Count > 0)
            {
                ret[3] = rand.Choice(noPowerMoves);
            }
            else
            {
                ret[3] = rand.Choice(availableMoves);
            }

            return ret;
        }
    }
}
