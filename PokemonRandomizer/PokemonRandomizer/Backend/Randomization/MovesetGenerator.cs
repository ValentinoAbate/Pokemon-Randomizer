using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using static PokemonRandomizer.Backend.DataStructures.MoveData;

namespace PokemonRandomizer.Backend.Randomization
{
    public class MovesetGenerator
    {
        private const float needSynergy = 12500;
        private const float preferSynergy = needSynergy / 2;
        private const float weakSynergy = needSynergy / 10;

        private readonly IDataTranslator dataT;
        private readonly Random rand;

        public MovesetGenerator(IDataTranslator dataT, Random rand)
        {
            this.dataT = dataT;
            this.rand = rand;
        }

        public static Move[] DefaultMoveset(PokemonBaseStats pokemon, int level)
        {
            var learnSet = pokemon.learnSet;
            var moves = new Stack<Move>(Math.Max(learnSet.Count, 4));
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

        private static Move[] EmptyMoveset() => new Move[4] { Move.None, Move.None, Move.None, Move.None };

        private static float LevelWeightScale(int learnLevel)
        {
            return MathF.Pow(learnLevel, 2);
        }

        private static float LevelWeightScaleSmall(int learnLevel)
        {
            return MathF.Pow(learnLevel, 1.5f);
        }

        public Move[] SmartMoveSet(PokemonBaseStats pokemon, int level)
        {
            bool IsStab(Move m) => pokemon.IsType(dataT.GetMoveData(m).type);
            // Initialize move choices
            var availableMoves = AvailableMoves(pokemon, level);
            // Initialize returns
            var ret = EmptyMoveset();
            if (availableMoves.Count <= 0)
                return ret;
            float PowerWeightScale(Move e) => (float)Math.Pow(dataT.GetMoveData(e).EffectivePower * (IsStab(e) && dataT.GetMoveData(e).AffectedByStab ? 1.5 : 1), 3);
            float RedundantTypeFactor(Move m)
            {
                var mType = dataT.GetMoveData(m).type;
                foreach(var m2 in ret)
                {
                    if (m2 == Move.None)
                        continue;
                    if (mType == dataT.GetMoveData(m2).type)
                        return 0.25f;
                }
                return 1;

            }
            float StabBonus(Move m) => IsStab(m) ? 2f : 1;
            float LevelWeightScale(Move e) => (float)Math.Pow(availableMoves[e], 2);
            float LevelWeightScaleSmall(Move e) => (float)Math.Pow(availableMoves[e], 1.5);
            float LevelWeightScaleLog(Move e) => (float)Math.Max(1, Math.Log(availableMoves[e]));

            // Choose first move - attempt to choose an attack move
            if(ChooseMoveForIndex(ret, 0, GetAttackMoves(availableMoves), (m) => PowerWeightScale(m) * StabBonus(m) * LevelWeightScaleLog(m), ref availableMoves))
            {
                return ret;
            }

            // Choose second move - attempt to choose another attack move
            if (ChooseMoveForIndex(ret, 1, GetAttackMoves(availableMoves), (m) => PowerWeightScale(m) * StabBonus(m) * RedundantTypeFactor(m) * LevelWeightScaleLog(m), ref availableMoves))
            {
                return ret;
            }

            // Choose third move - Attempt to choose a status move
            if(ChooseMoveForIndex(ret, 2, GetStatusMoves(availableMoves), LevelWeightScaleSmall, ref availableMoves))
            {
                return ret;
            }

            var fourthMoveChoice = new WeightedSet<Move>(availableMoves.Keys, LevelWeightScale);
            var currentMoves = ret.Where(m => m != Move.None).Select(dataT.GetMoveData);
            var metrics = new List<Func<Move, float>>();


            // Apply synergy metrics

            // Move Synergies
            void CalculateMoveSynergy(Func<MoveData, bool> currMovePred, Predicate<MoveData> moveChoicePred, float intensity)
            {
                int count = currentMoves.Count(currMovePred);
                if (count > 0)
                {
                    metrics.Add(m => (moveChoicePred(dataT.GetMoveData(m)) ? intensity : 1) * count);
                }
            }

            // Nightmare or Dream Eater + Sleep move Synergy
            CalculateMoveSynergy(m => m.effect == MoveEffect.DreamEater || m.effect == MoveEffect.StatusNightmare,
                             m => m.effect == MoveEffect.StatusSleep, needSynergy);
            // Snore or Sleep Talk + Rest Synergy
            CalculateMoveSynergy(m => m.effect == MoveEffect.SleepTalk || m.effect == MoveEffect.DamageFailUnlessAsleepFlinchChance,
                 m => m.effect == MoveEffect.Rest, needSynergy);
            // Rollout or Ice Ball + Defense Curl Synergy
            CalculateMoveSynergy(m => m.effect == MoveEffect.MultiTurnBuildup,
                             m => m.effect == MoveEffect.DefPlus1AndPrepForRoll, preferSynergy);
            // Spit Up or Swallow + Stockpile Synergy
            CalculateMoveSynergy(m => m.effect == MoveEffect.SpitUp || m.effect == MoveEffect.Swallow,
                             m => m.effect == MoveEffect.Stockpile, needSynergy);
            // Stockpile + Spit Up or Swallow Synergy
            CalculateMoveSynergy(m => m.effect == MoveEffect.Stockpile, 
                m => m.effect == MoveEffect.SpitUp || m.effect == MoveEffect.Swallow, preferSynergy);
            // Sun Move + Sun
            CalculateMoveSynergy(m => m.effect == MoveEffect.Solarbeam || m.effect == MoveEffect.RecoverHpWeather2,
                             m => m.effect == MoveEffect.WeatherSun, preferSynergy);
            // Fire Move + Sun
            CalculateMoveSynergy(m => m.type == PokemonType.FIR, m => m.effect == MoveEffect.WeatherSun, weakSynergy);
            // Rain move + Rain
            CalculateMoveSynergy(m => m.effect == MoveEffect.Thunder,
                             m => m.effect == MoveEffect.WeatherRain, preferSynergy);
            // Water Move + Rain
            CalculateMoveSynergy(m => m.type == PokemonType.WAT, m => m.effect == MoveEffect.WeatherRain, weakSynergy);
            // Weather Ball + Weather (Rain / Sun / Hail)
            CalculateMoveSynergy(m => m.effect == MoveEffect.WeatherBall,
                                 m => m.effect == MoveEffect.WeatherRain || m.effect == MoveEffect.WeatherSun 
                                     || m.effect == MoveEffect.WeatherHail, needSynergy);
            // Weather Ball + Sandstorm
            CalculateMoveSynergy(m => m.effect == MoveEffect.WeatherBall,
                                  m => m.effect == MoveEffect.WeatherSandstorm, weakSynergy);
            // Choose fourth move
            ret[3] = rand.Choice(new WeightedSet<Move>(availableMoves.Keys, m => LevelWeightScale(m) * Math.Max(1, metrics.Sum((metric) => metric(m)))));

            return ret;
        }

        public Move[] StatusTankMoveset(PokemonBaseStats pokemon, int level)
        {
            // Initialize move choices
            var availableMoves = AvailableMoves(pokemon, level);
            // Initialize returns
            var ret = EmptyMoveset();
            if (availableMoves.Count <= 0)
                return ret;

            // Choose first move - attempt to choose a damaging status move or DOT move. Favor same type

            // Choose second move - attempt to choose another damaging status move or DOT move that can overlap with the first one

            // Choose third move = attempt to choose a buff / disruption move:
            // Priority 1: healing move
            // Priority 2: def/sp.def/evasion buff
            // Priority 3: attack/sp.attack/acc debuff

            return Array.Empty<Move>();
        }

        private bool ChooseMoveForIndex(Move[] moveset, int index, IEnumerable<Move> preferredMoves, Func<Move, float> moveWeight, ref Dictionary<Move, int> availableMoves)
        {
            // Choose first move - attempt to choose an attack move
            var attackMoves = new WeightedSet<Move>(preferredMoves, moveWeight);
            moveset[index] = ChooseMove(attackMoves, availableMoves);
            availableMoves.Remove(moveset[index]);
            return availableMoves.Count <= 0;
        }

        private Move ChooseMove(WeightedSet<Move> preferredMoves, Dictionary<Move, int> allMoves)
        {
            // Preferred strategry
            if (preferredMoves.Count > 0)
            {
                return rand.Choice(preferredMoves);
            }
            // Fallback strategy
            var fallbackMoves = new WeightedSet<Move>(allMoves.Count);
            foreach (var kvp in allMoves)
            {
                fallbackMoves.Add(kvp.Key, LevelWeightScale(kvp.Value));
            }
            return rand.Choice(fallbackMoves);
        }

        private IEnumerable<Move> GetAttackMoves(Dictionary<Move, int> moves)
        {
            return moves.Keys.Where(m => !dataT.GetMoveData(m).IsStatus);
        }

        private IEnumerable<Move> GetStatusMoves(Dictionary<Move, int> moves)
        {
            return moves.Keys.Where(m => dataT.GetMoveData(m).IsStatus);
        }

        private Dictionary<Move, int> AvailableMoves(PokemonBaseStats pokemon, int level)
        {
            var availableMoves = new Dictionary<Move, int>(pokemon.learnSet.Count * 4);
            AddMovesFromPokemon(ref availableMoves, pokemon, level);
            // Add movesets from evolved from (moves down the chain)
            for (var preEvolution = pokemon; preEvolution.evolvesFrom.Count > 0;)
            {
                preEvolution = dataT.GetBaseStats(preEvolution.evolvesFrom[0].Pokemon);
                AddMovesFromPreEvolution(ref availableMoves, pokemon, level, preEvolution);
            }
            return availableMoves;
        }

        private void AddMovesFromPokemon(ref Dictionary<Move, int> availableMoves, PokemonBaseStats pokemon, int level)
        {
            AddMoves(ref availableMoves, pokemon.learnSet.Where(e => e.learnLvl <= level));
        }

        private void AddMovesFromPreEvolution(ref Dictionary<Move, int> availableMoves, PokemonBaseStats pokemon, int level, PokemonBaseStats preEvolution)
        {
            var eligibleMoves = new List<LearnSet.Entry>(preEvolution.learnSet.Count);
            var movesetLookup = pokemon.learnSet.GetMovesLookup();
            foreach(var entry in preEvolution.learnSet)
            {
                // Ignore moves past the learn level and SPLASH
                if(entry.learnLvl > level || entry.move == Move.SPLASH)
                {
                    continue;
                }
                if (availableMoves.ContainsKey(entry.move))
                {
                    if(entry.learnLvl > availableMoves[entry.move])
                    {
                        // Move is higher level, update learn level data
                        availableMoves[entry.move] = entry.learnLvl;
                    }
                    continue;
                }
                if (!movesetLookup.Contains(entry.move))
                {
                    // Pre-evo exclusive move, add to eligible moves
                    eligibleMoves.Add(entry);
                }
            }
            if (eligibleMoves.Count <= 0)
                return;
            AddMoves(ref availableMoves, eligibleMoves);
        }

        private void AddMoves(ref Dictionary<Move, int> availableMoves, IEnumerable<LearnSet.Entry> moves)
        {
            foreach (var entry in moves)
            {
                var move = entry.move;
                var learnLevel = entry.learnLvl;
                if (!availableMoves.ContainsKey(move))
                {
                    availableMoves.Add(move, learnLevel);
                }
                else if (availableMoves[move] < learnLevel)
                {
                    availableMoves[move] = learnLevel;
                }
            }
        }
    }
}
